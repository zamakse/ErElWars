#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// ZALIWA > Apply BaseVisual Component
///
/// 씬에 있는 BaseHP 오브젝트에 BaseVisual을 추가하고
/// 아군(x=-8) / 적군(x=+8) X 위치를 자동으로 설정한다.
///
/// 기존 벽돌 Sprite가 있으면 SpriteRenderer를 비활성화해
/// BaseVisual이 코드로 생성한 성 외관이 보이도록 한다.
/// </summary>
public static class BaseVisualEditor
{
    [MenuItem("ZALIWA/Apply BaseVisual Component")]
    static void ApplyBaseVisual()
    {
        BaseHP[] bases = Object.FindObjectsOfType<BaseHP>();
        if (bases.Length == 0)
        {
            Debug.LogWarning("[BaseVisualEditor] 씬에 BaseHP 오브젝트가 없습니다.");
            return;
        }

        int count = 0;
        foreach (BaseHP b in bases)
        {
            Undo.RecordObject(b.gameObject, "Apply BaseVisual");

            // ── BaseVisual 컴포넌트 추가 (중복 방지) ──────────────────────────
            if (b.GetComponent<BaseVisual>() == null)
                Undo.AddComponent<BaseVisual>(b.gameObject);

            // ── X 위치 설정 (Y, Z는 유지) ─────────────────────────────────────
            Vector3 pos = b.transform.position;
            pos.x = b.baseFaction == Faction.Ally ? -8f : 8f;
            b.transform.position = pos;

            // ── 기존 벽돌 SpriteRenderer 비활성화 ─────────────────────────────
            // (BaseVisual이 자식 오브젝트로 새 SR을 생성하므로 충돌 방지)
            SpriteRenderer oldSR = b.GetComponent<SpriteRenderer>();
            if (oldSR != null)
            {
                Undo.RecordObject(oldSR, "Disable old SpriteRenderer");
                oldSR.enabled = false;
            }

            EditorUtility.SetDirty(b.gameObject);
            Debug.Log($"[BaseVisualEditor] {b.gameObject.name} 처리 완료 " +
                      $"(faction={b.baseFaction}, x={pos.x})");
            count++;
        }

        Debug.Log($"[BaseVisualEditor] 완료 — {count}개 기지 처리됨. " +
                  "씬을 저장하세요 (Ctrl+S).");
    }

    // 메뉴 항목 활성 조건: 씬에 BaseHP가 하나 이상 있을 때만
    [MenuItem("ZALIWA/Apply BaseVisual Component", validate = true)]
    static bool ValidateApplyBaseVisual()
        => Object.FindObjectOfType<BaseHP>() != null;
}
#endif
