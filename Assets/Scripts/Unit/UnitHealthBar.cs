using UnityEngine;

/// <summary>
/// 유닛 머리 위에 체력바를 SpriteRenderer로 표시한다.
/// Canvas 없이 월드 스페이스에 직접 렌더링.
/// UnitBase의 OnDamaged 이벤트를 구독해 실시간 갱신.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitHealthBar : MonoBehaviour
{
    [Header("체력바 크기 (월드 스페이스 기준)")]
    [SerializeField] private float barWorldWidth  = 1.2f;   // 너비 (월드 단위)
    [SerializeField] private float barWorldHeight = 0.12f;  // 높이 (월드 단위)
    [SerializeField] private float barOffsetY     = 0.9f;   // 유닛 중심에서 위쪽 오프셋 (월드 단위)

    private const int SortOrderBG = 10;
    private const int SortOrderFG = 11;

    private UnitBase  unitBase;
    private Transform fgTransform;
    private float     fullLocalWidth; // 로컬 스페이스 기준 전체 너비

    private void Awake()
    {
        unitBase = GetComponent<UnitBase>();
        CreateBar();
    }

    private void Start()
    {
        if (unitBase == null) return;
        unitBase.OnDamaged += OnUnitDamaged;
        unitBase.OnDeath   += OnUnitDied;
    }

    private void OnDestroy()
    {
        if (unitBase != null)
        {
            unitBase.OnDamaged -= OnUnitDamaged;
            unitBase.OnDeath   -= OnUnitDied;
        }
    }

    // ─── 체력바 생성 ─────────────────────────────────────────────────

    private void CreateBar()
    {
        // 부모 스케일로 월드 크기 → 로컬 크기 변환 (절댓값: 적군 반전 대응)
        float parentScale = Mathf.Abs(transform.localScale.x);
        fullLocalWidth    = barWorldWidth  / parentScale;
        float localH      = barWorldHeight / parentScale;
        float localY      = barOffsetY     / parentScale;

        Sprite sprite = CreateWhiteSprite();

        // 배경 (어두운 빨강)
        GameObject bg = new GameObject("HP_BG");
        bg.transform.SetParent(transform);
        bg.transform.localRotation = Quaternion.identity;
        bg.transform.localPosition = new Vector3(0f, localY, -0.01f);
        bg.transform.localScale    = new Vector3(fullLocalWidth, localH, 1f);
        SpriteRenderer bgSR = bg.AddComponent<SpriteRenderer>();
        bgSR.sprite       = sprite;
        bgSR.color        = new Color(0.55f, 0.08f, 0.08f, 1f);
        bgSR.sortingOrder = SortOrderBG;

        // 전경 (초록)
        GameObject fg = new GameObject("HP_FG");
        fg.transform.SetParent(transform);
        fg.transform.localRotation = Quaternion.identity;
        fg.transform.localPosition = new Vector3(0f, localY, -0.02f);
        fg.transform.localScale    = new Vector3(fullLocalWidth, localH, 1f);
        SpriteRenderer fgSR = fg.AddComponent<SpriteRenderer>();
        fgSR.sprite       = sprite;
        fgSR.color        = new Color(0.15f, 0.75f, 0.15f, 1f);
        fgSR.sortingOrder = SortOrderFG;

        fgTransform = fg.transform;
    }

    /// <summary>
    /// 4×4 흰색 텍스처로 Sprite 생성 (UnitVisual과 동일한 방식)
    /// </summary>
    private static Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp
        };
        Color white = Color.white;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    // ─── 체력바 갱신 ─────────────────────────────────────────────────

    private void UpdateBar()
    {
        if (unitBase == null || fgTransform == null) return;
        float ratio = unitBase.maxHp > 0f
            ? Mathf.Clamp01(unitBase.currentHp / unitBase.maxHp)
            : 0f;

        // 너비 스케일 조정
        Vector3 scale = fgTransform.localScale;
        scale.x = fullLocalWidth * ratio;
        fgTransform.localScale = scale;

        // 왼쪽 정렬 유지: 스케일이 줄어들 때 왼쪽 고정
        // 아군(정방향): 오른쪽부터 줄어듦 / 적군(반전): 왼쪽부터 줄어듦 (시각적으로 자연스러운 연출)
        Vector3 pos = fgTransform.localPosition;
        pos.x = (fullLocalWidth / 2f) * (ratio - 1f);
        fgTransform.localPosition = pos;
    }

    private void OnUnitDamaged(UnitBase unit, float currentHp) => UpdateBar();

    private void OnUnitDied(UnitBase unit)
    {
        // 사망 직전 체력바 숨김
        if (fgTransform != null)
            fgTransform.gameObject.SetActive(false);
    }
}
