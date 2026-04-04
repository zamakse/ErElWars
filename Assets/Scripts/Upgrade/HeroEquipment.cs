using UnityEngine;

/// <summary>
/// 영웅 장비 착용 및 장비 효과를 관리하는 컴포넌트
/// </summary>
public class HeroEquipment : MonoBehaviour
{
    [Header("장비 슬롯")]
    public string weaponId;   // 무기 ID
    public string armorId;    // 방어구 ID
    public string accessoryId; // 악세서리 ID

    private HeroBase heroBase;
    private HeroStats heroStats;

    private void Awake()
    {
        heroBase = GetComponent<HeroBase>();
        heroStats = GetComponent<HeroStats>();
    }

    private void Start()
    {
        // 장비 초기화
    }

    /// <summary>
    /// 장비 착용 및 스탯 적용
    /// </summary>
    public void EquipItem(string slot, string itemId)
    {
        // 기존 장비 해제 후 새 장비 적용
        switch (slot)
        {
            case "weapon":
                weaponId = itemId;
                break;
            case "armor":
                armorId = itemId;
                break;
            case "accessory":
                accessoryId = itemId;
                break;
        }
        // 장비 스탯 반영
        ApplyEquipmentStats();
    }

    /// <summary>
    /// 장착된 장비 스탯 반영
    /// </summary>
    private void ApplyEquipmentStats()
    {
        // 장비 데이터 기반 스탯 적용 구현
    }
}
