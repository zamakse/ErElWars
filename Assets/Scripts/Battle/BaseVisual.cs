using UnityEngine;

/// <summary>
/// BaseUnit에 자동 부착되어 성(Castle) 외관을 코드로 조립한다.
/// Unity 기본 SpriteRenderer + 단색 스프라이트만 사용.
/// UnitBase.OnDamaged / OnDeath 이벤트로 HP바를 갱신한다.
///
/// 구조 (localY=0 = 지면):
///   Gate   y=0~0.7  어두운 문
///   Body   y=0~3.0  파란/빨간 본체  (2.0 × 3.0)
///   Tower  y=3.0~4.5 좌우 탑       (0.7 × 1.5)
///   Flag   y=4.5~5.1 깃발          (0.3 × 0.6)
///   HP bar y≈5.4
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class BaseVisual : MonoBehaviour
{
    // ── 크기 (월드 단위) ──────────────────────────────────────────────────────
    private const float BodyW  = 2.0f;
    private const float BodyH  = 3.0f;
    private const float TowerW = 0.7f;
    private const float TowerH = 1.5f;
    private const float FlagW  = 0.3f;
    private const float FlagH  = 0.6f;
    private const float GateW  = 0.5f;
    private const float GateH  = 0.7f;
    private const float BarW   = 2.4f;
    private const float BarH   = 0.20f;

    // ── 색상 (스펙 hex 값 그대로) ─────────────────────────────────────────────
    // #2244AA
    private static readonly Color AllyBody  = new Color(0.133f, 0.267f, 0.667f);
    // #112288
    private static readonly Color AllyTower = new Color(0.067f, 0.133f, 0.533f);
    // #44AAFF
    private static readonly Color AllyFlag  = new Color(0.267f, 0.667f, 1.000f);
    // #1a1a44 (HP바 배경)
    private static readonly Color AllyBarBG = new Color(0.100f, 0.100f, 0.267f);

    // #AA2222
    private static readonly Color EnemyBody  = new Color(0.667f, 0.133f, 0.133f);
    // #881111
    private static readonly Color EnemyTower = new Color(0.533f, 0.067f, 0.067f);
    // #FF6600
    private static readonly Color EnemyFlag  = new Color(1.000f, 0.400f, 0.000f);
    // #441a1a (HP바 배경)
    private static readonly Color EnemyBarBG = new Color(0.267f, 0.100f, 0.100f);

    // #111111
    private static readonly Color GateColor = new Color(0.067f, 0.067f, 0.067f);
    // HP바 전경 (공통)
    private static readonly Color BarFG     = new Color(0.180f, 0.820f, 0.180f);

    // ─────────────────────────────────────────────────────────────────────────
    private UnitBase  unitBase;
    private Transform fgBar;
    private float     fgFullW;

    private void Awake()
    {
        unitBase = GetComponent<UnitBase>();

        Sprite sq    = CreateWhiteSprite();
        bool   isAlly = unitBase.faction == Faction.Ally;

        BuildCastle(sq, isAlly);
        BuildHealthBar(sq, isAlly);
    }

    private void Start()
    {
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

    // ─── 성 조립 ─────────────────────────────────────────────────────────────

    private void BuildCastle(Sprite sq, bool isAlly)
    {
        Color body  = isAlly ? AllyBody  : EnemyBody;
        Color tower = isAlly ? AllyTower : EnemyTower;
        Color flag  = isAlly ? AllyFlag  : EnemyFlag;

        // 본체 (pivot at bottom-center: 0, 0)
        MakePart("Body",    sq, body,     0f,  BodyH  * 0.5f,  BodyW,  BodyH,  -1);

        // 좌탑 — 본체 좌상단
        float towerX = -(BodyW * 0.5f - TowerW * 0.5f);
        float towerY =   BodyH + TowerH * 0.5f;
        MakePart("Tower_L", sq, tower, towerX, towerY, TowerW, TowerH, -1);

        // 우탑 — 본체 우상단
        MakePart("Tower_R", sq, tower, -towerX, towerY, TowerW, TowerH, -1);

        // 깃발 — 우탑 위 (스펙: "우탑 위")
        float flagY = BodyH + TowerH + FlagH * 0.5f;
        MakePart("Flag",    sq, flag,  -towerX, flagY,  FlagW,  FlagH,  0);

        // 문 — 본체 하단 중앙
        MakePart("Gate",    sq, GateColor, 0f, GateH * 0.5f, GateW, GateH, -1);
    }

    private void MakePart(string partName, Sprite sprite, Color color,
                          float lx, float ly, float w, float h, int order)
    {
        var go = new GameObject(partName);
        go.transform.SetParent(transform, worldPositionStays: false);
        go.transform.localPosition = new Vector3(lx, ly, 0f);
        go.transform.localScale    = new Vector3(w,  h,  1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.color        = color;
        sr.sortingOrder = order;
        // 스펙: SortingLayer "Units". 레이어가 없으면 Default로 폴백됨.
        sr.sortingLayerName = "Units";
    }

    // ─── HP바 ────────────────────────────────────────────────────────────────

    private void BuildHealthBar(Sprite sq, bool isAlly)
    {
        float barY = BodyH + TowerH + FlagH + 0.35f + BarH * 0.5f;

        // 배경
        var bg = new GameObject("BaseHP_BG");
        bg.transform.SetParent(transform, worldPositionStays: false);
        bg.transform.localPosition = new Vector3(0f, barY, 0f);
        bg.transform.localScale    = new Vector3(BarW, BarH, 1f);
        var bgSR = bg.AddComponent<SpriteRenderer>();
        bgSR.sprite           = sq;
        bgSR.color            = isAlly ? AllyBarBG : EnemyBarBG;
        bgSR.sortingOrder     = 10;
        bgSR.sortingLayerName = "Units";

        // 전경
        var fg = new GameObject("BaseHP_FG");
        fg.transform.SetParent(transform, worldPositionStays: false);
        fg.transform.localPosition = new Vector3(0f, barY, -0.01f);
        fg.transform.localScale    = new Vector3(BarW, BarH, 1f);
        var fgSR = fg.AddComponent<SpriteRenderer>();
        fgSR.sprite           = sq;
        fgSR.color            = BarFG;
        fgSR.sortingOrder     = 11;
        fgSR.sortingLayerName = "Units";

        fgBar  = fg.transform;
        fgFullW = BarW;
    }

    private void RefreshBar()
    {
        if (fgBar == null || unitBase == null) return;
        float ratio = unitBase.maxHp > 0f
            ? Mathf.Clamp01(unitBase.currentHp / unitBase.maxHp)
            : 0f;

        Vector3 scale = fgBar.localScale;
        scale.x = fgFullW * ratio;
        fgBar.localScale = scale;

        // 왼쪽 정렬
        Vector3 pos = fgBar.localPosition;
        pos.x = (fgFullW * 0.5f) * (ratio - 1f);
        fgBar.localPosition = pos;
    }

    private void OnUnitDamaged(UnitBase _, float __) => RefreshBar();
    private void OnUnitDied(UnitBase _)
    {
        if (fgBar != null) fgBar.gameObject.SetActive(false);
    }

    // ─── 흰색 스프라이트 ──────────────────────────────────────────────────────

    private static Sprite CreateWhiteSprite()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp
        };
        Color w = Color.white;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, w);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
