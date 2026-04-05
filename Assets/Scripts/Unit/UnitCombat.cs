using UnityEngine;

/// <summary>
/// 유닛 전투 로직 — Update 폴링 기반 2-상태 머신 (MOVING / FIGHTING).
///
/// 탐지 흐름:
///   Physics2D.OverlapCircleAll(attackRange) → GetComponentInParent&lt;ITargetable&gt;()
///   IsAlive &amp;&amp; IsEnemy(myTeam) → 교전 대상 등록
///
/// BaseUnit(기지)은 UnitBase를 상속하므로 별도 처리 없이 자동 탐지됨.
/// lineType 필터 없음 — 범위 내 모든 적과 교전.
/// UnitMover가 없어도 null 체크로 안전하게 동작.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitCombat : MonoBehaviour
{
    [SerializeField] private LayerMask unitLayerMask = ~0;

    private UnitBase  unitBase;
    private UnitMover unitMover;
    private float     attackTimer;
    private int       myTeam;

    private const float RangedThreshold = 2f; // 이 값 초과이면 원거리 유닛 취급

    private enum CombatState { Moving, Fighting }
    private CombatState state = CombatState.Moving;

    /// <summary>공격이 실제로 발동될 때 호출됩니다 (UnitVisual 등이 구독).</summary>
    public event System.Action OnAttackExecuted;

    private void Awake()
    {
        unitBase  = GetComponent<UnitBase>();
        unitMover = GetComponent<UnitMover>();
    }

    private void Start()
    {
        myTeam = (int)unitBase.faction;
    }

    private void Update()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        // 성(기지) 콜라이더와의 접촉 여유를 위해 탐지 반경에 0.5f 를 추가
        float range = (unitBase.attackRange > 0f ? unitBase.attackRange : 1.5f) + 0.5f;

        ITargetable target = FindNearestTarget(range);

        if ( target != null && state == CombatState.Moving)   EnterFighting();
        if ( target == null && state == CombatState.Fighting) EnterMoving();

        if (state == CombatState.Fighting && target != null)
            TryAttack(target);
    }

    // ─── 상태 전환 ───────────────────────────────────────────────────────────

    private void EnterFighting()
    {
        state = CombatState.Fighting;
        if (unitMover != null) unitMover.IsFighting = true;
    }

    private void EnterMoving()
    {
        state = CombatState.Moving;
        if (unitMover != null) unitMover.IsFighting = false;
    }

    // ─── 탐지 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Physics2D.OverlapCircleAll로 범위 내 ITargetable을 탐색.
    /// BaseUnit(UnitBase 상속)은 자동으로 감지됨 — 별도 폴백 불필요.
    /// </summary>
    private ITargetable FindNearestTarget(float range)
    {
        ITargetable nearest     = null;
        float       nearestDist = float.MaxValue;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, range, unitLayerMask);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)       continue;
            if (hit.transform.IsChildOf(transform)) continue;

            ITargetable t = hit.GetComponentInParent<ITargetable>();
            if (t == null || !t.IsAlive || !t.IsEnemy(myTeam)) continue;

            float dist = Vector2.Distance(transform.position, t.GetTransform().position);
            if (dist < nearestDist) { nearestDist = dist; nearest = t; }
        }

        return nearest;
    }

    // ─── 공격 ────────────────────────────────────────────────────────────────

    private void TryAttack(ITargetable target)
    {
        if (attackTimer > 0f) return;

        float damage = unitBase.attackDamage;

        // 일반 유닛: 상성 배율 적용. 기지(BaseUnit)는 상성 제외.
        if (target is UnitBase enemyUnit && !(target is BaseUnit))
            damage *= AffinitySystem.GetMultiplier(unitBase.unitType, enemyUnit.unitType);

        // 원거리 유닛: 투사체 발사 / 근접 유닛: 즉시 데미지
        if (unitBase.attackRange > RangedThreshold && ProjectileManager.Instance != null)
            ProjectileManager.Instance.Launch(transform.position, target, damage);
        else
            target.TakeDamage(damage);

        attackTimer = 1f / Mathf.Max(0.01f, unitBase.attackSpeed);
        OnAttackExecuted?.Invoke();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (unitBase == null) return;
        float range = unitBase.attackRange > 0f ? unitBase.attackRange
                    : (unitBase.data != null ? unitBase.data.attackRange : 1.5f);
        Gizmos.color = unitBase.faction == Faction.Ally
            ? new Color(0f, 1f, 1f, 0.4f)
            : new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
#endif
}
