using UnityEngine;

/// <summary>
/// 배경 스프라이트 2장을 이어붙여 무한 좌→우 스크롤.
///
/// ── 씬 배치 방법 ───────────────────────────────────────────────────────────
/// 1. 빈 GameObject "BackgroundScroller" 생성 → 이 스크립트 추가.
///
/// 2. 자식 GameObject 2개 생성:
///      BG1 — SpriteRenderer 추가, 배경 스프라이트 할당
///            Position: (0, 0, 10)   ← z=10 으로 유닛 뒤에 배치
///      BG2 — SpriteRenderer 추가, 동일 스프라이트 할당
///            Position: (bgWidth, 0, 10)   ← bgWidth = 스프라이트 월드 너비
///
/// 3. Inspector에서 bg1, bg2 슬롯에 각 Transform 할당.
///
/// 4. bgWidth
///      · 0 으로 두면 BG1 의 SpriteRenderer.bounds.size.x 로 자동 계산.
///      · 카메라 orthographicSize 기준 화면 너비: size * aspect * 2
///        예) size=5, aspect=16:9 → 약 17.78 유닛
///      · 스프라이트 PPU(PixelsPerUnit) 이 100 이고 이미지가 1920px 이면
///        월드 너비 = 1920 / 100 = 19.2 → bgWidth 에 19.2 입력.
///
/// 5. scrollSpeed: 음수 = 오른쪽→왼쪽, 양수 = 왼쪽→오른쪽.
///    배경이 유닛 이동과 반대 방향이 되도록 조절. (권장: -1 ~ -3)
/// ───────────────────────────────────────────────────────────────────────────
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("배경 오브젝트")]
    [Tooltip("씬의 BG1 Transform")]
    [SerializeField] private Transform bg1;
    [Tooltip("씬의 BG2 Transform")]
    [SerializeField] private Transform bg2;

    [Header("스크롤 설정")]
    [Tooltip("스크롤 속도 (월드 단위/초). 음수 = 왼쪽 방향, 양수 = 오른쪽 방향")]
    [SerializeField] private float scrollSpeed = -2f;

    [Tooltip("배경 스프라이트 한 장의 월드 너비. 0 이면 BG1 의 SpriteRenderer 에서 자동 계산.")]
    [SerializeField] private float bgWidth = 0f;

    private float tileWidth;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        tileWidth = ResolveTileWidth();
    }

    private void Update()
    {
        if (bg1 == null || bg2 == null) return;

        Vector3 delta = Vector3.right * (scrollSpeed * Time.deltaTime);
        bg1.position += delta;
        bg2.position += delta;

        WrapIfNeeded(bg1, bg2);
        WrapIfNeeded(bg2, bg1);
    }

    /// <summary>
    /// target이 화면 밖으로 나갔으면 other의 반대편으로 재배치한다.
    /// </summary>
    private void WrapIfNeeded(Transform target, Transform other)
    {
        if (mainCam == null) return;

        float camX        = mainCam.transform.position.x;
        float halfTile    = tileWidth * 0.5f;

        if (scrollSpeed < 0f)
        {
            // 왼쪽으로 이동 → 오른쪽 끝이 카메라 왼쪽 화면 밖으로 나가면 재배치
            float screenLeft = camX - mainCam.orthographicSize * mainCam.aspect - halfTile;
            if (target.position.x < screenLeft)
            {
                float newX = other.position.x + tileWidth;
                target.position = new Vector3(newX, target.position.y, target.position.z);
            }
        }
        else
        {
            // 오른쪽으로 이동 → 왼쪽 끝이 카메라 오른쪽 화면 밖으로 나가면 재배치
            float screenRight = camX + mainCam.orthographicSize * mainCam.aspect + halfTile;
            if (target.position.x > screenRight)
            {
                float newX = other.position.x - tileWidth;
                target.position = new Vector3(newX, target.position.y, target.position.z);
            }
        }
    }

    private float ResolveTileWidth()
    {
        if (bgWidth > 0f) return bgWidth;

        SpriteRenderer sr = bg1 != null ? bg1.GetComponent<SpriteRenderer>() : null;
        if (sr != null && sr.sprite != null)
            return sr.bounds.size.x;

        // 최후 수단: 카메라 화면 너비
        if (mainCam != null)
        {
            float w = mainCam.orthographicSize * mainCam.aspect * 2f;
            Debug.LogWarning($"[BackgroundScroller] bgWidth 자동 계산 실패 — 카메라 너비 사용: {w:F2}");
            return w;
        }

        Debug.LogError("[BackgroundScroller] bg1 또는 Camera.main 이 없어 tileWidth를 계산할 수 없습니다.");
        return 10f;
    }
}
