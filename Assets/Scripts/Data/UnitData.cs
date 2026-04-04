using UnityEngine;

/// <summary>
/// 유닛 데이터를 정의하는 ScriptableObject.
/// 유닛의 모든 수치는 이 에셋에서 관리하며 코드에 하드코딩하지 않는다.
/// </summary>
[CreateAssetMenu(fileName = "UnitData", menuName = "ZALIWA/Data/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("기본 정보")]
    public string unitId;       // 유닛 고유 ID
    public string unitName;     // 유닛 이름
    public Sprite unitIcon;     // 유닛 아이콘
    public GameObject prefab;   // 유닛 프리팹

    [Header("라인 설정")]
    public LineType lineType = LineType.Ground1; // 소속 라인

    [Header("유닛 타입 (상성 시스템)")]
    public UnitType unitType = UnitType.Dealer;  // 유닛 타입

    [Header("기본 스탯")]
    public float baseHp = 100f;           // 기본 체력
    public float baseAttackDamage = 10f;  // 기본 공격력
    public float baseDefense = 0f;        // 기본 방어력 (데미지 감소 고정값)
    public float baseAttackSpeed = 1f;    // 기본 공격 속도 (초당 공격 횟수)
    public float baseMoveSpeed = 2f;      // 기본 이동 속도
    public float attackRange = 1.5f;      // 공격 사거리

    [Header("소환 정보")]
    public float manaCost = 10f;  // 소환 마나 비용
}
