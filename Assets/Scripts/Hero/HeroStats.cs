using UnityEngine;

/// <summary>
/// 영웅의 스탯 및 성장을 관리하는 컴포넌트
/// </summary>
public class HeroStats : MonoBehaviour
{
    [Header("기본 스탯 성장치")]
    public float hpPerLevel = 20f;        // 레벨당 체력 증가량
    public float attackPerLevel = 3f;     // 레벨당 공격력 증가량
    public float defensePerLevel = 1f;    // 레벨당 방어력 증가량

    [Header("현재 스탯")]
    public float defense = 0f;  // 방어력

    private HeroBase heroBase;

    private void Awake()
    {
        heroBase = GetComponent<HeroBase>();
    }

    private void Start()
    {
        // 스탯 초기화
    }

    /// <summary>
    /// 레벨업 시 스탯 증가 처리
    /// </summary>
    public void OnLevelUp()
    {
        heroBase.maxHp += hpPerLevel;
        heroBase.currentHp = heroBase.maxHp;
        heroBase.attackDamage += attackPerLevel;
        defense += defensePerLevel;
    }
}
