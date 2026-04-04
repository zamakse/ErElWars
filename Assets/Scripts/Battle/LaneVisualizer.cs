using UnityEngine;

/// <summary>
/// Ground1 / Ground2 라인의 이름 레이블을 화면에 표시한다.
/// 실제 바닥선 시각화는 씬의 Line_Ground1 / Line_Ground2 오브젝트가 담당한다.
/// </summary>
public class LaneVisualizer : MonoBehaviour
{
    private void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float g1Y = LineManager.Instance != null ? LineManager.Instance.ground1Y : -1.5f;
        float g2Y = LineManager.Instance != null ? LineManager.Instance.ground2Y :  1.5f;

        // 화면 왼쪽 가장자리 안쪽에 레이블 배치 (월드 → 스크린 좌표 변환)
        float worldLeftX = -cam.orthographicSize * cam.aspect + 0.4f;
        Vector3 s1 = cam.WorldToScreenPoint(new Vector3(worldLeftX, g1Y, 0f));
        Vector3 s2 = cam.WorldToScreenPoint(new Vector3(worldLeftX, g2Y, 0f));

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = new Color(0.95f, 0.90f, 0.75f, 0.9f) }
        };

        // OnGUI Y축: 위→아래. WorldToScreenPoint Y축: 아래→위 → 변환 필요
        GUI.Label(new Rect(s1.x, Screen.height - s1.y - 10f, 90f, 20f), "Ground 1", style);
        GUI.Label(new Rect(s2.x, Screen.height - s2.y - 10f, 90f, 20f), "Ground 2", style);
    }
}
