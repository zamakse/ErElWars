#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// ZALIWA > Replace Bases with BaseUnit
///
/// 실행 순서:
///   1. 씬에서 "AllyBase", "EnemyBase" 이름 오브젝트 OR BaseHP 컴포넌트를 탐색
///   2. 기존 BaseHP / BaseVisual 컴포넌트 제거 (Obsolete 정리)
///   3. BaseUnit 컴포넌트 추가, team / baseHp 설정
///   4. X 위치 정렬: 아군 x=-8, 적군 x=+8
///   5. BoxCollider2D 없으면 추가 (Physics2D 탐지에 필요)
///   6. 씬 Dirty 마킹 → Ctrl+S로 저장 유도
/// </summary>
public static class BaseReplacer
{
    private const float AllyX  = -8f;
    private const float EnemyX =  8f;
    private const float BaseHp = 1000f;

    [MenuItem("ZALIWA/Replace Bases with BaseUnit")]
    static void ReplaceBases()
    {
        // ── 중복 실행 방지: BaseUnit이 이미 존재하면 안내 후 종료 ──────────
#pragma warning disable CS0618
        BaseUnit[] existing = Object.FindObjectsOfType<BaseUnit>();
#pragma warning restore CS0618
        if (existing.Length > 0)
        {
            EditorUtility.DisplayDialog(
                "이미 적용됨",
                $"씬에 BaseUnit이 이미 {existing.Length}개 존재합니다.\n" +
                "중복 실행을 방지하기 위해 작업을 중단합니다.\n\n" +
                "기지를 완전히 초기화하려면\n" +
                "ZALIWA > Clean And Reset Scene 을 사용하세요.",
                "확인");
            return;
        }

        int replaced = 0;

        // ── 아군 기지 처리 ──────────────────────────────────────────────
        replaced += ProcessBase("AllyBase",  team: 0, posX: AllyX);

        // ── 적군 기지 처리 ──────────────────────────────────────────────
        replaced += ProcessBase("EnemyBase", team: 1, posX: EnemyX);

        if (replaced == 0)
            Debug.LogWarning("[BaseReplacer] 처리할 기지 오브젝트를 찾지 못했습니다. " +
                             "씬에 'AllyBase' 또는 'EnemyBase' 이름의 오브젝트가 있는지 확인하세요.");
        else
            Debug.Log($"[BaseReplacer] 완료 — {replaced}개 기지 교체됨. Ctrl+S로 씬을 저장하세요.");
    }

    [MenuItem("ZALIWA/Replace Bases with BaseUnit", validate = true)]
    static bool ValidateReplaceBases()
        => UnityEngine.SceneManagement.SceneManager.GetActiveScene().IsValid();

    // ─── 내부 처리 ──────────────────────────────────────────────────────────

    static int ProcessBase(string objName, int team, float posX)
    {
        // 이름으로 탐색 먼저, 없으면 BaseHP 컴포넌트로 진영 탐색
        GameObject go = GameObject.Find(objName);

        if (go == null)
        {
            // 이름이 다를 경우 BaseHP.baseFaction 기준으로 탐색
#pragma warning disable CS0618
            foreach (BaseHP bp in Object.FindObjectsOfType<BaseHP>())
#pragma warning restore CS0618
            {
                bool isAlly = (bp.baseFaction == Faction.Ally);
                if ((team == 0 && isAlly) || (team == 1 && !isAlly))
                {
                    go = bp.gameObject;
                    break;
                }
            }
        }

        if (go == null) return 0;

        Undo.RecordObject(go, $"Replace {objName} with BaseUnit");

        // ── 기존 컴포넌트 정리 ────────────────────────────────────────────
#pragma warning disable CS0618
        BaseHP oldHP = go.GetComponent<BaseHP>();
#pragma warning restore CS0618
        if (oldHP != null) Undo.DestroyObjectImmediate(oldHP);

        BaseVisual oldVisual = go.GetComponent<BaseVisual>();
        if (oldVisual != null) Undo.DestroyObjectImmediate(oldVisual);

        // 기존 SpriteRenderer (벽돌 스프라이트) 비활성화
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Undo.RecordObject(sr, "Disable old SpriteRenderer");
            sr.enabled = false;
        }

        // ── BaseUnit 추가 및 설정 ────────────────────────────────────────
        BaseUnit baseUnit = go.GetComponent<BaseUnit>();
        if (baseUnit == null)
            baseUnit = Undo.AddComponent<BaseUnit>(go);

        baseUnit.team   = team;
        baseUnit.baseHp = BaseHp;

        // ── X 위치 설정 (Y, Z 유지) ──────────────────────────────────────
        Vector3 pos = go.transform.position;
        pos.x = posX;
        go.transform.position = pos;

        // ── BoxCollider2D 보장 ────────────────────────────────────────────
        if (go.GetComponent<Collider2D>() == null)
        {
            var col  = Undo.AddComponent<BoxCollider2D>(go);
            col.size   = new Vector2(2.0f, 3.0f);
            col.offset = new Vector2(0f, 1.5f);
        }

        EditorUtility.SetDirty(go);

        string label = team == 0 ? "아군" : "적군";
        Debug.Log($"[BaseReplacer] {label} 기지({go.name}) 교체 완료 (x={posX}, team={team})");
        return 1;
    }
}
#endif
