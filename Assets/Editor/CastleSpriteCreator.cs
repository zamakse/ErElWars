using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 메뉴: ZALIWA > Create Castle Sprites
/// 아군/적군 성 스프라이트 PNG를 픽셀아트로 생성한다.
/// 생성 위치: Assets/Sprites/Bases/
/// </summary>
public static class CastleSpriteCreator
{
    private const string OutputDir = "Assets/Sprites/Bases";
    private const int W = 64;   // 픽셀 너비
    private const int H = 96;   // 픽셀 높이
    private const int PPU = 16; // Pixels Per Unit (스케일 8과 조합 → 월드 32px = 2유닛)

    [MenuItem("ZALIWA/Create Castle Sprites")]
    public static void Run()
    {
        EnsureFolder("Assets/Sprites");
        EnsureFolder(OutputDir);

        SaveSprite("Castle_Ally",
            wall:   new Color32(90,  110, 160, 255),
            dark:   new Color32(55,  70,  110, 255),
            accent: new Color32(120, 150, 200, 255),
            gate:   new Color32(20,  20,  35,  255),
            flag:   new Color32(80,  160, 220, 255));

        SaveSprite("Castle_Enemy",
            wall:   new Color32(140, 70,  70,  255),
            dark:   new Color32(90,  40,  40,  255),
            accent: new Color32(180, 100, 100, 255),
            gate:   new Color32(20,  10,  10,  255),
            flag:   new Color32(200, 60,  60,  255));

        AssetDatabase.Refresh();

        // PPU / Sprite mode 설정
        ConfigureSprite($"{OutputDir}/Castle_Ally.png");
        ConfigureSprite($"{OutputDir}/Castle_Enemy.png");

        AssetDatabase.SaveAssets();
        Debug.Log("[CastleSpriteCreator] 완료! Assets/Sprites/Bases/ 폴더를 확인하세요.");
    }

    // ── 스프라이트 저장 ───────────────────────────────────────────────────────

    private static void SaveSprite(string name,
        Color32 wall, Color32 dark, Color32 accent, Color32 gate, Color32 flag)
    {
        Color32[] px = DrawCastle(wall, dark, accent, gate, flag);

        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels32(px);
        tex.Apply();

        string path     = $"{OutputDir}/{name}.png";
        string fullPath = Application.dataPath + "/../" + path;
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    // ── 픽셀아트 성 그리기 ────────────────────────────────────────────────────
    // 레이아웃 (64 × 96):
    //  y 80-95 : 양쪽 탑 총안 (Merlons)
    //  y 68-79 : 성벽 총안
    //  y 0-79  : 좌탑(x 0-13) / 본체(x 14-49) / 우탑(x 50-63)
    //  y 0-44  : 본체 내부 어두운 색
    //  y 0-44  : 게이트 아치 (x 22-41)

    private static Color32[] DrawCastle(
        Color32 wall, Color32 dark, Color32 accent, Color32 gate, Color32 flag)
    {
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32[] px  = new Color32[W * H];
        for (int i = 0; i < px.Length; i++) px[i] = clear;

        void Set(int x, int y, Color32 c)
        {
            if (x < 0 || x >= W || y < 0 || y >= H) return;
            px[y * W + x] = c;
        }

        void Rect(int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    Set(x, y, c);
        }

        void Line(int x0, int y0, int x1, int y1, Color32 c)
        {
            int dx = x1 - x0, dy = y1 - y0;
            int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            for (int i = 0; i <= steps; i++)
            {
                int x = x0 + Mathf.RoundToInt(dx * (float)i / steps);
                int y = y0 + Mathf.RoundToInt(dy * (float)i / steps);
                Set(x, y, c);
            }
        }

        // ── 본체 ──────────────────────────────────────────────────────────────
        Rect(14, 0, 49, 71, wall);   // 본체
        Rect(15, 1, 48, 70, wall);   // 내부 채우기

        // ── 좌 탑 ─────────────────────────────────────────────────────────────
        Rect(0, 0, 13, 79, wall);
        // 탑 그늘
        for (int y = 0; y <= 79; y++) Set(13, y, dark);

        // ── 우 탑 ─────────────────────────────────────────────────────────────
        Rect(50, 0, 63, 79, wall);
        // 탑 그늘
        for (int y = 0; y <= 79; y++) Set(50, y, dark);

        // ── 성벽 그늘 ─────────────────────────────────────────────────────────
        for (int y = 0; y <= 71; y++) Set(14, y, dark);

        // ── 총안 (Merlons): 좌 탑 y=80-95 ────────────────────────────────────
        // 패턴: ##_ ##_ ##_  (3px 블록, 1px 간격 × 4)
        int[] mTower = { 0, 3, 7, 10 };
        foreach (int mx in mTower)
        {
            Rect(mx,      80, mx + 2, 95, wall);   // 좌탑
            Rect(50 + mx, 80, 52 + mx, Mathf.Min(95, 95), wall); // 우탑
        }
        // 총안 그늘
        for (int y = 80; y <= 95; y++)
        {
            foreach (int mx in mTower)
            {
                Set(mx + 2, y, dark);
                Set(52 + mx, y, dark);
            }
        }

        // ── 총안 (Merlons): 본체 y=72-83 ─────────────────────────────────────
        int[] mWall = { 15, 19, 23, 27, 31, 35, 39, 43 };
        foreach (int mx in mWall)
            Rect(mx, 72, mx + 2, 83, wall);

        // ── 게이트 아치 ───────────────────────────────────────────────────────
        // 프레임
        Rect(22, 0, 24, 47, dark);  // 좌 프레임
        Rect(39, 0, 41, 47, dark);  // 우 프레임
        Rect(22, 44, 41, 47, dark); // 상단 프레임
        // 아치 (반원)
        int cx = 31, cy = 44, r = 9;
        for (int angle = 0; angle <= 180; angle++)
        {
            float rad = angle * Mathf.Deg2Rad;
            int ax = cx + Mathf.RoundToInt(r * Mathf.Cos(rad));
            int ay = cy - Mathf.RoundToInt(r * Mathf.Sin(rad));
            Set(ax, ay, dark);
        }
        // 게이트 내부 어둡게
        Rect(25, 0, 38, 43, gate);
        // 아치 내부
        for (int y = 35; y <= 43; y++)
        {
            for (int x = cx - r + 1; x <= cx + r - 1; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist < r) Set(x, y, gate);
            }
        }

        // ── 탑 창문 ───────────────────────────────────────────────────────────
        Rect(3,  25, 9,  36, gate);  // 좌탑 창문
        Rect(54, 25, 60, 36, gate);  // 우탑 창문
        // 창문 프레임
        for (int y = 24; y <= 37; y++) { Set(2, y, dark);  Set(10, y, dark);  }
        for (int y = 24; y <= 37; y++) { Set(53, y, dark); Set(61, y, dark);  }
        Set(3, 24, dark);  Set(9, 24, dark);  Set(3, 37, dark);  Set(9, 37, dark);
        Set(54, 24, dark); Set(60, 24, dark); Set(54, 37, dark); Set(60, 37, dark);

        // ── 본체 창 (작은 슬릿) ───────────────────────────────────────────────
        Rect(22, 52, 23, 62, gate);
        Rect(27, 52, 28, 62, gate);
        Rect(35, 52, 36, 62, gate);
        Rect(40, 52, 41, 62, gate);

        // ── 상단 하이라이트 ───────────────────────────────────────────────────
        for (int x = 1; x <= 12; x++)  Set(x, 79, accent);  // 좌탑 상단
        for (int x = 51; x <= 62; x++) Set(x, 79, accent);  // 우탑 상단
        for (int x = 15; x <= 48; x++) Set(x, 71, accent);  // 본체 상단

        // ── 기 (Flag) ─────────────────────────────────────────────────────────
        // 좌탑 기 (x=6, y=90-95)
        for (int y = 90; y <= 95; y++) Set(6, y, accent); // 깃대
        Rect(7, 92, 10, 95, flag);                        // 깃발

        // 우탑 기 (x=57, y=90-95)
        for (int y = 90; y <= 95; y++) Set(57, y, accent);
        Rect(58, 92, 61, 95, flag);

        return px;
    }

    // ── 스프라이트 임포터 설정 ────────────────────────────────────────────────

    private static void ConfigureSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType         = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = PPU;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType  = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string child  = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
