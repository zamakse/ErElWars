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

    [Header("유닛 소환 슬롯 (최대 8개)")]
    public UnitData[] slotUnits;    // 각 슬롯에 배치할 UnitData
    public UnitSpawner allySpawner; // 아군 소환기 연결

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
        if (allyBaseText != null && BaseUnit.AllyBase != null)
            allyBaseText.text = $"아군 기지\n{BaseUnit.AllyBase.currentHp:0}/{BaseUnit.AllyBase.maxHp:0}";

        if (enemyBaseText != null && BaseUnit.EnemyBase != null)
            enemyBaseText.text = $"적군 기지\n{BaseUnit.EnemyBase.currentHp:0}/{BaseUnit.EnemyBase.maxHp:0}";
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
        if (BaseUnit.AllyBase != null)
        {
            float ratio   = BaseUnit.AllyBase.maxHp > 0f
                ? BaseUnit.AllyBase.currentHp / BaseUnit.AllyBase.maxHp : 0f;
            float rightX  = 10f;
            GUI.color = new Color(0.25f, 0.45f, 1f, 0.9f);
            GUI.Box(new Rect(rightX, 10, 160, 42),
                $"아군 기지  {BaseUnit.AllyBase.currentHp:0} / {BaseUnit.AllyBase.maxHp:0}", style);
            GUI.color = new Color(0.45f, 0.08f, 0.08f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160, 10), Texture2D.whiteTexture);
            GUI.color = new Color(0.15f, 0.78f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160f * ratio, 10), Texture2D.whiteTexture);
        }

        // 적군 기지 HP (우측 상단)
        if (BaseUnit.EnemyBase != null)
        {
            float ratio  = BaseUnit.EnemyBase.maxHp > 0f
                ? BaseUnit.EnemyBase.currentHp / BaseUnit.EnemyBase.maxHp : 0f;
            float rightX = Screen.width - 170f;
            GUI.color = new Color(1f, 0.28f, 0.28f, 0.9f);
            GUI.Box(new Rect(rightX, 10, 160, 42),
                $"적군 기지  {BaseUnit.EnemyBase.currentHp:0} / {BaseUnit.EnemyBase.maxHp:0}", style);
            GUI.color = new Color(0.45f, 0.08f, 0.08f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160, 10), Texture2D.whiteTexture);
            GUI.color = new Color(0.15f, 0.78f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(rightX, 56, 160f * ratio, 10), Texture2D.whiteTexture);
        }

        GUI.color = Color.white;

        // ─── 유닛 소환 버튼 (하단 중앙) ───────────────────────────────────────
        if (slotUnits == null || slotUnits.Length == 0) return;

        float btnW    = 80f;
        float btnH    = 80f;
        float gap     = 8f;
        int   count   = Mathf.Min(slotUnits.Length, 8);
        float totalW  = count * btnW + (count - 1) * gap;
        float startX  = (Screen.width - totalW) * 0.5f;
        float startY  = Screen.height - btnH - 20f;

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerCenter
        };

        for (int i = 0; i < count; i++)
        {
            UnitData unit = slotUnits[i];
            if (unit == null) continue;

            float   btnX      = startX + i * (btnW + gap);
            Rect    btnRect   = new Rect(btnX, startY, btnW, btnH);
            bool    canAfford = ManaManager.Instance != null &&
                                ManaManager.Instance.HasEnoughMana(unit.manaCost);

            GUI.color = canAfford ? Color.white : new Color(1f, 1f, 1f, 0.4f);

            string label = $"{unit.unitName}\n{unit.manaCost}마나";
            if (GUI.Button(btnRect, label, btnStyle) && canAfford && allySpawner != null)
                allySpawner.TrySpawnUnit(unit);
        }

        GUI.color = Color.white;
    }

    private void OnRetreatButtonClicked()
    {
        // 후퇴 로직 연결
    }
}
