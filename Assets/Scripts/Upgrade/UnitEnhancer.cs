using UnityEngine;

/// <summary>
/// 유닛 강화(스탯 업그레이드) 시스템을 처리하는 클래스
/// </summary>
public class UnitEnhancer : MonoBehaviour
{
    [Header("강화 설정")]
    public int enhanceLevel = 0;        // 현재 강화 레벨
    public int maxEnhanceLevel = 10;    // 최대 강화 레벨
    public float statBonusPerLevel = 0.1f; // 레벨당 스탯 보너스 비율

    private UnitBase unitBase;

    private void Awake()
    {
        unitBase = GetComponent<UnitBase>();
    }

    private void Start()
    {
        // 강화 시스템 초기화
    }

    /// <summary>
    /// 유닛을 한 단계 강화
    /// </summary>
    public bool Enhance()
    {
        if (enhanceLevel >= maxEnhanceLevel) return false;

        enhanceLevel++;
        ApplyEnhancement();
        return true;
    }

    /// <summary>
    /// 강화 스탯 적용
    /// </summary>
    private void ApplyEnhancement()
    {
        unitBase.attackDamage *= (1f + statBonusPerLevel);
        unitBase.maxHp *= (1f + statBonusPerLevel);
        unitBase.currentHp = unitBase.maxHp;
    }
}
