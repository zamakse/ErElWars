using UnityEngine;

/// <summary>
/// 유닛 이동 로직.
/// - UnitCombat이 IsFighting = true 로 설정하면 이동 중단.
/// - 기지(BaseUnit) 좌표에 도달하면 영구 정지 (isAtBase 래치).
/// - 적 감지 / 전투 판단은 UnitCombat 담당.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitMover : MonoBehaviour
{
    private UnitBase unitBase;
    private float    directionX;
    private bool     isAtBase = false;

    /// <summary>UnitCombat이 전투 상태일 때 true → 이동 정지.</summary>
    public bool IsFighting { get; set; }

    private void Awake()
    {
        unitBase = GetComponent<UnitBase>();
    }

    private void Start()
    {
        directionX = unitBase.faction == Faction.Ally ? 1f : -1f;

        if (unitBase.faction == Faction.Enemy)
        {
            Vector3 s = transform.localScale;
            s.x *= -1f;
            transform.localScale = s;
        }
    }

    private void Update()
    {
        if (isAtBase)   return;
        if (IsFighting) return;

        transform.Translate(
            Vector2.right * directionX * unitBase.moveSpeed * Time.deltaTime,
            Space.World);

        CheckBaseReached();
    }

    private void CheckBaseReached()
    {
        // BaseUnit 정적 참조 사용 (BaseHP 대체)
        BaseUnit targetBase = unitBase.faction == Faction.Ally
            ? BaseUnit.EnemyBase
            : BaseUnit.AllyBase;

        if (targetBase == null) return;

        float baseX   = targetBase.transform.position.x;
        bool  reached = unitBase.faction == Faction.Ally
            ? transform.position.x >= baseX
            : transform.position.x <= baseX;

        if (reached) isAtBase = true;
    }

    public bool  IsAtBase   => isAtBase;
    public float DirectionX => directionX;
}
