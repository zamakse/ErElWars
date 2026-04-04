using UnityEngine;

/// <summary>
/// 성(기지) 유닛. UnitBase를 상속하므로
/// UnitCombat의 Physics2D.OverlapCircleAll에 자동으로 탐지된다.
///
/// 특성:
///   - 이동 없음 (UnitMover 미부착)
///   - 공격 없음 (UnitCombat 미부착)
///   - 피격만 수신 → HP 0 시 BattleManager.OnBaseDestroyed(team) 호출
///   - Awake에서 BoxCollider2D, BaseVisual 자동 추가
/// </summary>
public class BaseUnit : UnitBase
{
    // ─── 씬 전역 참조 (UnitMover.CheckBaseReached 등에서 사용) ──────────────
    public static BaseUnit AllyBase;
    public static BaseUnit EnemyBase;

    [Header("기지 설정")]
    [Tooltip("0 = 아군 기지, 1 = 적군 기지")]
    public int   team   = 0;
    [Tooltip("UnitData가 없을 때 사용할 기본 HP")]
    public float baseHp = 1000f;

    // ─────────────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        // faction을 base.Awake() 이전에 확정
        faction = (team == 0) ? Faction.Ally : Faction.Enemy;

        // base.Awake() — data == null이면 Initialize 미실행 (의도적)
        base.Awake();

        // 수동 스탯 초기화 (UnitData 불필요)
        // data가 있으면 UnitData.baseHp 우선, 없으면 직렬화 필드 사용
        float hp   = (data != null) ? data.baseHp : baseHp;
        maxHp      = hp;
        currentHp  = hp;
        attackDamage = 0f;
        defense      = 0f;
        attackSpeed  = 0f;
        moveSpeed    = 0f;
        attackRange  = 0f;
        lineType     = LineType.Ground1; // 라인 시스템 미사용이므로 임의값

        // 정적 참조 등록
        if (faction == Faction.Ally)  AllyBase  = this;
        else                          EnemyBase = this;

        // BoxCollider2D 자동 보장 (Physics2D 탐지에 필요)
        if (GetComponent<Collider2D>() == null)
        {
            var col  = gameObject.AddComponent<BoxCollider2D>();
            col.size   = new Vector2(2.0f, 3.0f);
            col.offset = new Vector2(0f, 1.5f); // 성 본체 중심
        }

        // BaseVisual 자동 추가 (없는 경우에만)
        if (GetComponent<BaseVisual>() == null)
            gameObject.AddComponent<BaseVisual>();
    }

    private void OnDestroy()
    {
        if (AllyBase  == this) AllyBase  = null;
        if (EnemyBase == this) EnemyBase = null;
    }

    /// <summary>
    /// 성 HP가 0이 되면 호출됨.
    /// 파괴 애니메이션 없이 BattleManager에 게임 종료를 알린다.
    /// </summary>
    protected override void Die()
    {
        // 이벤트 발동 (BaseVisual이 HP바를 숨기는 데 사용)
        FireOnDeath();

        // 게임 종료 처리
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnBaseDestroyed(team);
        else
            Debug.LogError("[BaseUnit] BattleManager.Instance가 null입니다.");

        // 성 오브젝트는 씬에 남겨 둠 (visual continuity)
        // Destroy는 호출하지 않음
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = (faction == Faction.Ally)
            ? new Color(0.2f, 0.4f, 1f, 0.5f)
            : new Color(1f, 0.2f, 0.2f, 0.5f);
        Gizmos.DrawWireCube(
            transform.position + new Vector3(0f, 1.5f, 0f),
            new Vector3(2f, 3f, 0f));
    }
#endif
}
