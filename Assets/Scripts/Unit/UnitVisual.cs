using UnityEngine;

/// <summary>
/// Warrior 유닛의 Animator 상태 전환.
///
/// Animator 파라미터: int "State"
///   0 = Idle | 1 = Walk | 2 = Attack | 3 = Die
///
/// 공격 감지: UnitCombat.OnAttackExecuted 이벤트로 구동 (폴링 없음).
/// 공격 애니메이션은 attackAnimDuration 동안 재생 후 자동 복귀.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(UnitBase))]
public class UnitVisual : MonoBehaviour
{
    [Tooltip("Die 애니메이션 재생 후 오브젝트 파괴까지의 대기 시간.\n" +
             "Death-Effect 11프레임 / 12fps = 약 0.917초.")]
    [SerializeField] private float dieAnimDuration = 0.917f;

    [Tooltip("Attack 애니메이션 클립 길이.\n" +
             "Attack 12프레임 / 12fps = 1.0초.")]
    [SerializeField] private float attackAnimDuration = 1.0f;

    private Animator   animator;
    private UnitBase   unitBase;
    private UnitCombat unitCombat;
    private Vector3    prevPosition;
    private bool       isDead       = false;
    private float      attackTimer  = 0f;

    private static readonly int StateParam = Animator.StringToHash("State");

    private const int StateIdle   = 0;
    private const int StateWalk   = 1;
    private const int StateAttack = 2;
    private const int StateDie    = 3;

    private void Awake()
    {
        animator   = GetComponent<Animator>();
        unitBase   = GetComponent<UnitBase>();
        unitCombat = GetComponent<UnitCombat>();
    }

    private void Start()
    {
        prevPosition = transform.position;

        unitBase.destroyDelay = dieAnimDuration;
        unitBase.OnDeath     += HandleDeath;

        if (unitCombat != null)
            unitCombat.OnAttackExecuted += HandleAttack;

        // Prefab에 Sprite가 없을 때 런타임 폴백 스프라이트 생성
        EnsureFallbackSprite();
    }

    /// <summary>
    /// SpriteRenderer.sprite 가 null 이면 32×32 흰색 텍스처를 생성해 할당.
    /// 아군 #3380FF / 적군 #FF3333 색상 적용.
    /// localScale 을 절댓값 최대 (1,1,1) 로 클램프해 Prefab 의 과대 스케일(8,8,4 등)을 보정.
    ///
    /// 호출 시점: Start() — Initialize() 가 Awake~Start 사이에 호출되므로 faction 확정된 상태.
    /// 방어 조건: UnitBase 가 없는 오브젝트(배경 등)에는 동작하지 않음.
    /// </summary>
    private void EnsureFallbackSprite()
    {
        // UnitBase 가 없는 오브젝트에는 적용하지 않음 (방어적 처리)
        if (GetComponent<UnitBase>() == null) return;

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite != null) return;

        // ── 32×32 흰색 스프라이트 생성 ──────────────────────────────────────
        var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp
        };
        Color white = Color.white;
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                tex.SetPixel(x, y, white);
        tex.Apply();

        sr.sprite   = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        sr.drawMode = SpriteDrawMode.Simple;

        // ── 색상: #3380FF(아군) / #FF3333(적군) ─────────────────────────────
        // #3380FF ≈ (0.200, 0.502, 1.000)
        // #FF3333 ≈ (1.000, 0.200, 0.200)
        sr.color = unitBase.faction == Faction.Ally
            ? new Color(0.200f, 0.502f, 1.000f)
            : new Color(1.000f, 0.200f, 0.200f);

        // ── 스케일 클램프: 절댓값 최대 1 (Prefab scale 8,8,4 → 1,1,1) ───────
        // 음수 부호(적군 반전)는 유지하고 크기만 제한.
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(
            Mathf.Sign(s.x) * Mathf.Min(Mathf.Abs(s.x), 1f),
            Mathf.Sign(s.y) * Mathf.Min(Mathf.Abs(s.y), 1f),
            Mathf.Sign(s.z) * Mathf.Min(Mathf.Abs(s.z), 1f)
        );
    }

    private void OnDestroy()
    {
        if (unitBase != null)
            unitBase.OnDeath -= HandleDeath;

        if (unitCombat != null)
            unitCombat.OnAttackExecuted -= HandleAttack;
    }

    private void Update()
    {
        if (isDead) return;

        // prevPosition은 항상 갱신 (공격 중 이동 감지 오류 방지)
        bool isMoving = Vector3.Distance(transform.position, prevPosition) > 0.001f;
        prevPosition  = transform.position;

        // Attack 우선: TryAttack() 호출 시 타이머 세트
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            animator.SetInteger(StateParam, StateAttack);
            return;
        }

        animator.SetInteger(StateParam, isMoving ? StateWalk : StateIdle);
    }

    private void HandleAttack()
    {
        if (isDead) return;
        attackTimer = attackAnimDuration;
    }

    private void HandleDeath(UnitBase _)
    {
        isDead      = true;
        attackTimer = 0f;
        animator.SetInteger(StateParam, StateDie);
    }
}
