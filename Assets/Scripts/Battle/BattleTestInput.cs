using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 전투 테스트용 키보드 입력 처리 스크립트.
/// - Space : Ground1 아군 소환
/// - Q     : Ground2 아군 소환
/// - E     : Ground1 적군 소환
/// - R     : Ground2 적군 소환
/// BattleManager 오브젝트에 붙여서 사용한다.
/// </summary>
public class BattleTestInput : MonoBehaviour
{
    [Header("소환기 연결")]
    public UnitSpawner allySpawner;   // UnitSpawner_Ally 오브젝트 연결
    public UnitSpawner enemySpawner;  // UnitSpawner_Enemy 오브젝트 연결

    [Header("소환할 유닛 데이터")]
    public UnitData allyUnitData;    // TestSoldier_Data 연결
    public UnitData enemyUnitData;   // TestEnemy_Data 연결

    [Header("디버그 설정")]
    public bool showDebugLog = true;

    private void Start()
    {
        Debug.Log($"[BattleTestInput] 초기화 확인\n" +
                  $"  allySpawner  : {(allySpawner  != null ? allySpawner.name  : "❌ null")}\n" +
                  $"  enemySpawner : {(enemySpawner != null ? enemySpawner.name : "❌ null")}\n" +
                  $"  allyUnitData : {(allyUnitData  != null ? allyUnitData.unitName  : "❌ null")}\n" +
                  $"  enemyUnitData: {(enemyUnitData != null ? enemyUnitData.unitName : "❌ null")}\n" +
                  $"  ManaManager  : {(ManaManager.Instance != null ? "✅ OK" : "❌ null")}\n" +
                  $"  LineManager  : {(LineManager.Instance  != null ? "✅ OK" : "❌ null")}");
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 아군 소환
        if (keyboard.spaceKey.wasPressedThisFrame)
            TrySpawn(allySpawner, allyUnitData, LineType.Ground1, "아군");

        if (keyboard[Key.Q].wasPressedThisFrame)
            TrySpawn(allySpawner, allyUnitData, LineType.Ground2, "아군");

        // 적군 소환
        if (keyboard[Key.E].wasPressedThisFrame)
            TrySpawn(enemySpawner, enemyUnitData, LineType.Ground1, "적군");

        if (keyboard[Key.R].wasPressedThisFrame)
            TrySpawn(enemySpawner, enemyUnitData, LineType.Ground2, "적군");
    }

    private void TrySpawn(UnitSpawner spawner, UnitData unitData, LineType line, string label)
    {
        if (spawner == null)
        {
            Debug.LogError($"[BattleTestInput] {label} 소환기가 null입니다.");
            return;
        }
        if (unitData == null)
        {
            Debug.LogError($"[BattleTestInput] {label} UnitData가 null입니다.");
            return;
        }

        if (showDebugLog)
            Debug.Log($"[BattleTestInput] {label} 소환 요청: {unitData.unitName} / 라인: {line} | 마나 {ManaManager.Instance?.CurrentMana:F1}");

        bool success = spawner.TrySpawnUnit(unitData, line);

        if (showDebugLog)
        {
            if (success)
                Debug.Log($"[BattleTestInput] ✅ {label}({line}) 소환 성공 | 남은 마나: {ManaManager.Instance?.CurrentMana:F1}");
            else
                Debug.LogWarning($"[BattleTestInput] ❌ {label}({line}) 소환 실패 | 마나 부족");
        }
    }

    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 17,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = Color.white }
        };

        // 마나 표시
        float mana    = ManaManager.Instance != null ? ManaManager.Instance.CurrentMana : 0f;
        float maxMana = ManaManager.Instance != null ? ManaManager.Instance.MaxMana     : 0f;
        GUI.Label(new Rect(10, 10, 300, 28), $"마나: {mana:F0} / {maxMana:F0}", labelStyle);

        // 라인 선택 UI (좌측)
        float boxX  = 10f;
        float boxY  = 42f;
        float lineH = 28f;

        // 배경
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(boxX - 4, boxY - 4, 244f, lineH * 5 + 8f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(boxX, boxY, 240f, lineH), "=== 라인 선택 ===", labelStyle);

        // 아군 키 (파란색)
        GUI.color = new Color(0.5f, 0.75f, 1f, 1f);
        GUI.Label(new Rect(boxX, boxY + lineH,     240f, lineH),
            $"[Space]  아군 Ground1  (비용: {(allyUnitData != null ? allyUnitData.manaCost.ToString() : "?")})", labelStyle);
        GUI.Label(new Rect(boxX, boxY + lineH * 2, 240f, lineH),
            $"[Q]      아군 Ground2  (비용: {(allyUnitData != null ? allyUnitData.manaCost.ToString() : "?")})", labelStyle);

        // 적군 키 (빨간색)
        GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
        GUI.Label(new Rect(boxX, boxY + lineH * 3, 240f, lineH),
            $"[E]      적군 Ground1  (비용: {(enemyUnitData != null ? enemyUnitData.manaCost.ToString() : "?")})", labelStyle);
        GUI.Label(new Rect(boxX, boxY + lineH * 4, 240f, lineH),
            $"[R]      적군 Ground2  (비용: {(enemyUnitData != null ? enemyUnitData.manaCost.ToString() : "?")})", labelStyle);

        GUI.color = Color.white;
    }
}
