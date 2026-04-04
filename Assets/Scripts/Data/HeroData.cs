using UnityEngine;

/// <summary>
/// 영웅 데이터를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "HeroData", menuName = "ZALIWA/Data/HeroData")]
public class HeroData : ScriptableObject
{
    [Header("기본 정보")]
    public string heroId;       // 영웅 고유 ID
    public string heroName;     // 영웅 이름
    public Sprite heroPortrait; // 영웅 초상화
    public GameObject prefab;   // 영웅 프리팹

    [Header("기본 스탯")]
    public float baseHp = 300f;          // 기본 체력
    public float baseAttackDamage = 25f; // 기본 공격력
    public float baseAttackSpeed = 1f;   // 기본 공격 속도
    public float baseMoveSpeed = 2.5f;   // 기본 이동 속도

    [Header("스킬 정보")]
    public string skillName;         // 스킬 이름
    public string skillDescription;  // 스킬 설명
    public float skillCooldown = 10f; // 스킬 쿨다운 (초)
    public float skillDamage = 50f;   // 스킬 데미지

    [Header("부활 설정")]
    public float reviveDelay = 5f;     // 부활 대기 시간
    public float reviveHpRatio = 0.3f; // 부활 체력 비율
}
