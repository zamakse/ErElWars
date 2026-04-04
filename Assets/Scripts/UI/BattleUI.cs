using UnityEngine;
using UnityEngine.UI;
// v3

/// <summary>
/// 전투 화면 UI(마나바, 기지 HP 등)를 관리하는 클래스.
/// Canvas Text가 연결되지 않은 경우 OnGUI로 기지 HP를 화면에 표시한다.
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("마나 UI")]
    public Slider manaSlider;      // 마나 슬라이더
    public Text   waveText;        // 웨이브 표시 텍스트
    public Button retreatButton;   // 후퇴 버튼

    [Header("기지 HP 텍스트 (Canvas 사용 시 연결)")]
    public Text allyBaseText;      // 아군 기지 HP 텍스트
    public Text enemyBaseText;     // 적군 기지 HP 텍스트

    private void Start()
    {
        if (retreatButton != null)
            retreatButton.onClick.AddListener(OnRetreatButtonClicked);
    }

    private void Update()
    {
        // 마나 슬라이더 갱신
        if (ManaManager.Instance != null && manaSlider != null)
            manaSlider.value = ManaManager.Instance.CurrentMana / ManaManager.Instance.MaxMana;

        // 기지 HP 텍스트 갱신 (Text 컴포넌트가 연결된 경우)
        if (allyBaseText != null && BaseHP.AllyBase != null)
            allyBaseText.text = $"아군 기지\n{BaseHP.AllyBase.CurrentHp:0}/{BaseHP.AllyBase.maxHp:0}";

        if (enemyBaseText != null && BaseHP.EnemyBase != null)
            enemyBaseText.text = $"적군 기지\n{BaseHP.EnemyBase.CurrentHp:0}/{BaseHP.EnemyBase.maxHp:0}";
    }

    /// <summary>
    /// Canvas Text 미연결 시 OnGUI로 기지 HP 표시 (개발·디버그용)
    /// </summary>
    private void OnGUI()
    {
        if (allyBaseText != null && enemyBaseText != null) return; // Text 연결 시 생략

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize  = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.white;

        // 아군 기지 HP (좌측 상단)
        if (BaseHP.AllyBase != null)
        {
            float ratio   = BaseHP.AllyBase.maxHp > 0f
                ? BaseHP.AllyBase.CurrentHp / BaseHP.AllyBase.maxHp : 0f;
            float rightX  = 10f;
            GUI.color = new Color(0.25f, 0.45f, 1f, 0.9f);
            GUI.Box(new Rect(rightX, 10, 160, 42),
                $"아군 기지  {BaseHP.AllyBase.CurrentHp:0} / {BaseHP.AllyBase.maxHp:0}", style);
            GUI.color = new Color(0.45f, 0.08f, 0.08f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160, 10), Texture2D.whiteTexture);
            GUI.color = new Color(0.15f, 0.78f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160f * ratio, 10), Texture2D.whiteTexture);
        }

        // 적군 기지 HP (우측 상단)
        if (BaseHP.EnemyBase != null)
        {
            float ratio  = BaseHP.EnemyBase.maxHp > 0f
                ? BaseHP.EnemyBase.CurrentHp / BaseHP.EnemyBase.maxHp : 0f;
            float rightX = Screen.width - 170f;
            GUI.color = new Color(1f, 0.28f, 0.28f, 0.9f);
            GUI.Box(new Rect(rightX, 10, 160, 42),
                $"적군 기지  {BaseHP.EnemyBase.CurrentHp:0} / {BaseHP.EnemyBase.maxHp:0}", style);
            GUI.color = new Color(0.45f, 0.08f, 0.08f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160, 10), Texture2D.whiteTexture);
            GUI.color = new Color(0.15f, 0.78f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160f * ratio, 10), Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
    }

    private void OnRetreatButtonClicked()
    {
        // 후퇴 로직 연결
    }
}
