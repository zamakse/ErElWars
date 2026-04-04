using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// 메뉴: ZALIWA > Setup Base Visuals
///
/// 씬 레이아웃 기준:
///   Base_Ally  : X=-8  / Base_Enemy: X=8
///   Ground1    : Y=-2.5 / Ground2  : Y=2.5
///
/// 피벗:
///   아군 성벽 → pivot (1.0, 0.5)  : 오른쪽 엣지 = X=-8  (성벽이 왼쪽으로 뻗음)
///   적군 성벽 → pivot (0.0, 0.5)  : 왼쪽  엣지 = X= 8  (성벽이 오른쪽으로 뻗음)
///
/// → 유닛이 기지 X에 도달했을 때 성벽 정면 벽에 딱 멈추게 됨.
/// </summary>
public static class BaseVisualSetup
{
    private const string OutputDir  = "Assets/Sprites/Bases";
    private const string AllyPath   = "Assets/Sprites/Bases/Castle_Ally.png";
    private const string EnemyPath  = "Assets/Sprites/Bases/Castle_Enemy.png";

    // ── 스프라이트 크기 ──────────────────────────────────────────────────────
    private const int W   = 80;    // 픽셀 너비  (= 2.5 월드유닛 @ PPU32)
    private const int H   = 384;   // 픽셀 높이  (= 12  월드유닛 @ PPU32) — 화면 가득
    private const int PPU = 32;

    // ── 문 위치 (y=0: 텍스처 하단, y=H: 상단) ───────────────────────────────
    // pivot이 y=0.5이므로: pixel_y = H/2 + worldY * PPU
    //   Ground1 Y=-2.5 → pixel_y = 192 - 80 = 112  (중심)
    //   Ground2 Y=+2.5 → pixel_y = 192 + 80 = 272  (중심)
    private const int G1_LO = 64,  G1_HI = 160;  // Gate1 (96px = 3 월드유닛)
    private const int G2_LO = 224, G2_HI = 320;  // Gate2

    [MenuItem("ZALIWA/Setup Base Visuals")]
    public static void Run()
    {
        EnsureFolder("Assets/Sprites");
        EnsureFolder(OutputDir);

        // 아군 성벽 (청회색)
        BuildSprite(AllyPath, pivotX: 1.0f,
            wall:   new Color32(88,  108, 158, 255),
            dark:   new Color32(48,  62,  98,  255),
            accent: new Color32(128, 152, 208, 255),
            gate:   new Color32(12,  12,  28,  255),
            flag:   new Color32(72,  160, 220, 255));

        // 적군 성벽 (적갈색)
        BuildSprite(EnemyPath, pivotX: 0.0f,
            wall:   new Color32(138, 68,  68,  255),
            dark:   new Color32(82,  36,  36,  255),
            accent: new Color32(182, 108, 108, 255),
            gate:   new Color32(22,  6,   6,   255),
            flag:   new Color32(210, 50,  50,  255));

        AssetDatabase.SaveAssets();
        AttachToScene();
    }

    // ── 스프라이트 생성 & 설정 ────────────────────────────────────────────────

    private static void BuildSprite(string path, float pivotX,
        Color32 wall, Color32 dark, Color32 accent, Color32 gate, Color32 flag)
    {
        // PNG 기록
        Color32[] px  = DrawWall(wall, dark, accent, gate, flag);
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels32(px);
        tex.Apply();
        File.WriteAllBytes(Application.dataPath + "/../" + path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        // 임포트 설정 적용
        AssetDatabase.Refresh();
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;

        imp.textureType         = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = PPU;
        imp.filterMode          = FilterMode.Point;
        imp.textureCompression  = TextureImporterCompression.Uncompressed;
        imp.alphaIsTransparency = true;

        TextureImporterSettings s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        s.spriteMeshType  = SpriteMeshType.FullRect;
        s.spriteAlignment = (int)SpriteAlignment.Custom;
        s.spritePivot     = new Vector2(pivotX, 0.5f);  // 엣지 피벗
        imp.SetTextureSettings(s);
        imp.SaveAndReimport();
    }

    // ── 씬 연결 ──────────────────────────────────────────────────────────────

    private static void AttachToScene()
    {
        var allBases = Object.FindObjectsOfType<BaseHP>();
        int attached = 0;

        foreach (BaseHP baseHP in allBases)
        {
            string path   = baseHP.baseFaction == Faction.Ally ? AllyPath : EnemyPath;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[BaseVisualSetup] 스프라이트 없음: {path}");
                continue;
            }

            SpriteRenderer sr = baseHP.GetComponent<SpriteRenderer>();
            if (sr == null) sr = baseHP.gameObject.AddComponent<SpriteRenderer>();

            sr.sprite       = sprite;
            sr.sortingOrder = -2;   // 유닛·체력바보다 뒤에

            baseHP.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(baseHP.gameObject);
            attached++;

            Debug.Log($"[BaseVisualSetup] {baseHP.name} ({baseHP.baseFaction}) 연결 완료.");
        }

        if (attached > 0)
        {
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[BaseVisualSetup] {attached}개 기지 설정 완료.");
        }
        else
        {
            Debug.LogWarning("[BaseVisualSetup] BaseHP 오브젝트를 찾지 못했습니다. " +
                             "Edit 모드에서 실행하세요.");
        }
    }

    // ── 성벽 픽셀아트 그리기 ─────────────────────────────────────────────────
    //
    //  y=0   ─ 하단
    //  y=64  ─ Gate1 하단 (세계 Y=-4.0)
    //  y=112 ─ Gate1 중심 (세계 Y=-2.5 = Ground1)
    //  y=160 ─ Gate1 상단 (세계 Y=-1.0)
    //  y=192 ─ 피벗 중심 (세계 Y= 0.0)
    //  y=224 ─ Gate2 하단 (세계 Y=+1.0)
    //  y=272 ─ Gate2 중심 (세계 Y=+2.5 = Ground2)
    //  y=320 ─ Gate2 상단 (세계 Y=+4.0)
    //  y=384 ─ 최상단

    private static Color32[] DrawWall(
        Color32 wall, Color32 dark, Color32 accent, Color32 gate, Color32 flag)
    {
        Color32[] px    = new Color32[W * H];
        Color32   clear = new Color32(0, 0, 0, 0);
        for (int i = 0; i < px.Length; i++) px[i] = clear;

        Color32 light  = Brighten(wall,  20);
        Color32 mortar = Darken(dark,    12);

        // ── 헬퍼 ──
        void Set(int x, int y, Color32 c)
        { if (x >= 0 && x < W && y >= 0 && y < H) px[y * W + x] = c; }

        void Rect(int x0, int y0, int x1, int y1, Color32 c)
        { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Set(x, y, c); }

        bool InGate(int y) =>
            (y >= G1_LO && y <= G1_HI) || (y >= G2_LO && y <= G2_HI);

        // ── 1. 전체 벽 채우기 ─────────────────────────────────────────────────
        Rect(0, 0, W - 1, H - 1, wall);

        // ── 2. 문 구멍 (투명) ─────────────────────────────────────────────────
        Rect(0, G1_LO, W - 1, G1_HI, gate);
        Rect(0, G2_LO, W - 1, G2_HI, gate);

        // ── 3. 벽돌 패턴 ──────────────────────────────────────────────────────
        const int BH = 8, BW = 20;
        for (int y = 0; y < H; y++)
        {
            if (InGate(y)) continue;
            int row    = y / BH;
            int offset = (row % 2 == 0) ? 0 : BW / 2;

            if (y % BH == 0)                                    // 수평 줄눈
            { for (int x = 0; x < W; x++) Set(x, y, mortar); continue; }

            if (y % BH == 1)                                    // 상단 하이라이트
            { for (int x = 0; x < W; x++) Set(x, y, light); }

            for (int x = offset; x < W; x += BW) Set(x, y, mortar); // 수직 줄눈
        }

        // ── 4. 문 기둥 (좌우 2px) + 상하 인방 (2px) ────────────────────────────
        void GateFrame(int lo, int hi)
        {
            for (int y = lo; y <= hi; y++)
            { Set(0,y,dark); Set(1,y,dark); Set(W-1,y,dark); Set(W-2,y,dark); }
            for (int x = 0; x < W; x++)
            { Set(x,lo-1,dark); Set(x,lo-2,dark); Set(x,hi+1,dark); Set(x,hi+2,dark); }
        }
        GateFrame(G1_LO, G1_HI);
        GateFrame(G2_LO, G2_HI);

        // ── 5. 문 상단 아치 (문 안쪽 장식) ────────────────────────────────────
        void DrawArch(int topY)
        {
            int   cx    = W / 2;
            float rx    = W / 2f - 3f;
            int   depth = 16;
            for (int x = (int)(cx - rx); x <= (int)(cx + rx); x++)
            {
                float t  = (x - cx) / rx;
                int   ay = topY - (int)(depth * (1f - t * t));
                Set(x, ay, dark); Set(x, ay - 1, dark);
            }
        }
        DrawArch(G1_HI);
        DrawArch(G2_HI);

        // ── 6. 양쪽 수직 타워 기둥 강조 ──────────────────────────────────────
        for (int y = 0; y < H; y++)
        {
            if (InGate(y)) continue;
            // 우측 그늘
            Set(W - 1, y, dark);
            Set(W - 2, y, dark);
            // 좌측 하이라이트
            if (px[y * W].a > 0) Set(0, y, light);
        }

        // ── 7. 최상단 총안 (Merlons / Crenels) ────────────────────────────────
        const int MH = 28;   // 총안 높이
        const int MW = 10;   // 총안 너비
        const int CW = 6;    // 치간 (Crenel) 너비

        Rect(0, H - MH, W - 1, H - 1, wall);  // 총안 기반

        // Crenel(빈 곳) 뚫기
        for (int x = MW; x < W; x += MW + CW)
            if (x + CW - 1 < W) Rect(x, H - MH, x + CW - 1, H - 1, clear);

        // 총안 상단·하단 줄
        for (int x = 0; x < W; x++)
        {
            if (px[(H - 1) * W + x].a > 0) { Set(x, H - 1, accent); Set(x, H - 2, light); }
            Set(x, H - MH, mortar);
        }

        // ── 8. 깃발 ───────────────────────────────────────────────────────────
        // 첫 Merlon 중앙에 깃대
        int poleX = MW / 2;
        for (int y = H - MH - 10; y < H - MH; y++) Set(poleX, y, accent);
        Rect(poleX + 1, H - MH - 10, poleX + 7, H - MH - 6, flag);

        return px;
    }

    // ── 색상 유틸 ────────────────────────────────────────────────────────────

    private static Color32 Brighten(Color32 c, int amt) => new Color32(
        (byte)Mathf.Clamp(c.r + amt, 0, 255),
        (byte)Mathf.Clamp(c.g + amt, 0, 255),
        (byte)Mathf.Clamp(c.b + amt, 0, 255), 255);

    private static Color32 Darken(Color32 c, int amt) => new Color32(
        (byte)Mathf.Clamp(c.r - amt, 0, 255),
        (byte)Mathf.Clamp(c.g - amt, 0, 255),
        (byte)Mathf.Clamp(c.b - amt, 0, 255), 255);

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}
