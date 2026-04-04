using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 영웅 상태(체력, 스킬 쿨다운 등) UI를 관리하는 클래스
/// </summary>
public class HeroStatusUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Slider hpSlider;          // 체력 슬라이더
    public Image skillCooldownFill;  // 스킬 쿨다운 이미지
    public Text heroNameText;        // 영웅 이름 텍스트
    public Text heroLevelText;       // 영웅 레벨 텍스트

    [Header("연결된 영웅")]
    public HeroBase targetHero;  // 표시할 영웅

    private void Start()
    {
        // 영웅 상태 UI 초기화
    }

    private void Update()
    {
        // 영웅 상태 UI 갱신
        if (targetHero == null) return;
        RefreshHeroStatus();
    }

    /// <summary>
    /// 영웅 상태 UI 값 갱신
    /// </summary>
    private void RefreshHeroStatus()
    {
        if (hpSlider != null)
            hpSlider.value = targetHero.currentHp / targetHero.maxHp;

        if (heroNameText != null)
            heroNameText.text = targetHero.heroName;

        if (heroLevelText != null)
            heroLevelText.text = $"Lv. {targetHero.heroLevel}";
    }
}
