using UnityEngine;
using System;

/// <summary>
/// 모든 유닛의 기반 클래스.
/// 수치는 UnitData ScriptableObject에서 초기화하며 코드에 하드코딩하지 않는다.
/// </summary>
public class UnitBase : MonoBehaviour, ITargetable
{
    [Header("유닛 데이터 (ScriptableObject)")]
    public UnitData data; // 인스펙터에서 직접 할당하거나 UnitSpawner가 주입

    [Header("런타임 진영/라인 정보")]
    public Faction faction;   // 진영 (UnitSpawner가 설정)
    public LineType lineType; // 소속 라인 (UnitData에서 복사)

    // ─── 런타임 스탯 (UnitData에서 초기화, 외부 읽기 가능) ───────────────────
    [HideInInspector] public float    maxHp;
    [HideInInspector] public float    currentHp;
    [HideInInspector] public float    attackDamage;
    [HideInInspector] public float    defense;
    [HideInInspector] public float    attackSpeed;
    [HideInInspector] public float    moveSpeed;
    [HideInInspector] public float    attackRange;
    [HideInInspector] public UnitType unitType;   // 상성 시스템용 유닛 타입

    // ─── 이벤트 (UI 레이어는 이벤트로만 상태를 수신) ─────────────────────────
    /// <summary>피격 이벤트 (unit, 남은 HP)</summary>
    public event Action<UnitBase, float> OnDamaged;
    /// <summary>사망 이벤트</summary>
    public event Action<UnitBase> OnDeath;

    /// <summary>
    /// Die() 호출 후 실제 파괴까지의 대기 시간(초).
    /// UnitVisual이 Start에서 Die 애니메이션 길이로 설정한다.
    /// </summary>
    [HideInInspector] public float destroyDelay = 0f;

    private bool initialized = false;

    protected virtual void Awake()
    {
        // Prefab scale(8,8,4) 등 과대 스케일 보정 — BaseUnit(성)은 scale 건드리지 않음
        if (!(this is BaseUnit) && transform.localScale.x > 2f)
            transform.localScale = Vector3.one;

        // 씬에 직접 배치된 유닛은 Awake 시점에 data가 이미 할당돼 있으면 초기화
        if (data != null && !initialized)
            Initialize(data, faction);
    }

    protected virtual void Start() { }

    protected virtual void Update() { }

    /// <summary>
    /// UnitSpawner 또는 씬 초기화 시 호출.
    /// UnitData 수치를 런타임 필드에 복사한다.
    /// </summary>
    public void Initialize(UnitData unitData, Faction unitFaction)
    {
        if (unitData == null)
        {
            Debug.LogWarning($"[UnitBase] {name}: UnitData가 null입니다.");
            return;
        }

        data = unitData;
        faction = unitFaction;

        maxHp = unitData.baseHp;
        currentHp = unitData.baseHp;
        attackDamage = unitData.baseAttackDamage;
        defense = unitData.baseDefense;
        attackSpeed = unitData.baseAttackSpeed;
        moveSpeed = unitData.baseMoveSpeed;
        attackRange = unitData.attackRange;
        lineType    = unitData.lineType;
        unitType    = unitData.unitType;

        initialized = true;
    }

    /// <summary>
    /// 데미지 수신. 방어력(defense)을 차감한 뒤 최소 1 적용.
    /// </summary>
    public virtual void TakeDamage(float rawDamage)
    {
        float finalDamage = Mathf.Max(1f, rawDamage - defense);
        currentHp = Mathf.Max(0f, currentHp - finalDamage);

        OnDamaged?.Invoke(this, currentHp);

        if (currentHp <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject, destroyDelay);
    }

    // ─── ITargetable ─────────────────────────────────────────────────────────
    public bool      IsAlive        => currentHp > 0f;
    public Transform GetTransform() => transform;
    /// <inheritdoc cref="ITargetable.IsEnemy"/>
    public bool IsEnemy(int myTeam) => (int)faction != myTeam;

    // ─── 파생 클래스에서 OnDeath를 발동할 수 있도록 노출 (BaseUnit에서 사용) ───
    protected void FireOnDeath() => OnDeath?.Invoke(this);

    // ─── 스탯 조정 메서드 (UnitEnhancer, UnitFrenzy 등에서 사용) ──────────────

    public void ModifyAttackDamage(float multiplier) => attackDamage *= multiplier;
    public void ModifyMoveSpeed(float multiplier) => moveSpeed *= multiplier;
    public void ModifyDefense(float delta) => defense += delta;

    public void ModifyMaxHp(float multiplier)
    {
        float ratio = (maxHp > 0f) ? currentHp / maxHp : 1f;
        maxHp *= multiplier;
        currentHp = maxHp * ratio;
    }

    /// <summary>체력을 직접 회복 (부활, 힐 스킬 등)</summary>
    public void Heal(float amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 에디터에서 공격 사거리 시각화
        float range = (data != null) ? data.attackRange : attackRange;
        if (range <= 0f) return;
        Gizmos.color = faction == Faction.Ally ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
#endif
}
