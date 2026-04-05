#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// ZALIWA > Clean And Reset Scene
///
/// 실행 순서:
///   1. Line_Ground1/2, BG_Ground1/2 — 유닛 컴포넌트 제거 + SpriteRenderer.sprite null 초기화
///   2. Base_Ally / Base_Enemy 계열 오브젝트 전부 삭제
///   3. Base_Ally (x=-7) / Base_Enemy (x=+7) 새로 생성
///      구성: BaseUnit (먼저) → BaseHP (다음) → BoxCollider2D (size 2×6)
///      루트에 SpriteRenderer 없음 — 외형은 BaseUnit.Awake()가 BaseVisual 자동 추가
///   4. UnitSpawner / BattleManager / ManaManager 는 일절 건드리지 않음
///   5. EditorUtility.SetDirty + 씬 저장
/// </summary>
public static class SceneCleaner
{
    // ── BG 오브젝트 이름 (정확 일치) ─────────────────────────────────────────
    private static readonly string[] BgObjectNames =
    {
        "Line_Ground1", "Line_Ground2", "BG_Ground1", "BG_Ground2"
    };

    // ── 기지 삭제 키워드 ──────────────────────────────────────────────────────
    private static readonly string[] BaseKeywords =
    {
        "Base_Ally", "Base_Enemy", "AllyBase", "EnemyBase"
    };

    // ── 위치 / 수치 ──────────────────────────────────────────────────────────
    // orthographicSize=5 → 가로범위 ≈ ±8.89 (16:9)
    // 성 자식 오브젝트(탑 등)가 추가로 좌우로 뻗어나가므로 x=±6 으로 여유 확보
    private const float AllyX  = -6f;
    private const float EnemyX =  6f;
    private const float BaseHp = 1000f;

    // ── 스폰 포인트 X 위치 ────────────────────────────────────────────────────
    private const float AllySpawnX  = -5f;
    private const float EnemySpawnX =  5f;

    [MenuItem("ZALIWA/Clean And Reset Scene")]
    static void CleanAndResetScene()
    {
        if (!EditorUtility.DisplayDialog(
            "씬 초기화 확인",
            "다음 작업을 수행합니다:\n" +
            "① Line/BG 오브젝트 유닛 컴포넌트 제거 + Sprite null 초기화\n" +
            "② 기존 기지 오브젝트 전부 삭제\n" +
            "③ Base_Ally(x=-6) / Base_Enemy(x=+6) 새로 생성\n" +
            "④ SpawnPoint_Ally x=-5 / SpawnPoint_Enemy x=+5 로 이동\n\n" +
            "UnitSpawner / BattleManager / ManaManager 는 유지됩니다.\n\n" +
            "계속하시겠습니까?",
            "확인", "취소"))
        {
            return;
        }

        // ── 1. BG 오브젝트 정리 ───────────────────────────────────────────────
        CleanBgObjects();

        // ── 2. 기존 기지 전부 삭제 ───────────────────────────────────────────
        DeleteAllBases();

        // ── 3. 기지 새로 생성 ────────────────────────────────────────────────
        GameObject ally  = CreateBase("Base_Ally",  team: 0, posX: AllyX);
        GameObject enemy = CreateBase("Base_Enemy", team: 1, posX: EnemyX);

        // ── 4. 스폰 포인트 이동 ──────────────────────────────────────────────
        MoveSpawnPoints();

        // ── 5. Dirty 마킹 + 씬 저장 ─────────────────────────────────────────
        EditorUtility.SetDirty(ally);
        EditorUtility.SetDirty(enemy);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[SceneCleaner] 씬 초기화 완료.");
    }

    [MenuItem("ZALIWA/Clean And Reset Scene", validate = true)]
    static bool ValidateCleanAndResetScene()
        => UnityEngine.SceneManagement.SceneManager.GetActiveScene().IsValid();

    // =========================================================================
    // 0. 스폰 포인트 이동
    // =========================================================================

    /// <summary>
    /// SpawnPoint_Ally_Ground1/2 → x=AllySpawnX(-5)
    /// SpawnPoint_Enemy_Ground1/2 → x=EnemySpawnX(+5)
    /// Y, Z 는 유지.
    /// </summary>
    static void MoveSpawnPoints()
    {
        MoveSpawnPoint("SpawnPoint_Ally_Ground1",  AllySpawnX);
        MoveSpawnPoint("SpawnPoint_Ally_Ground2",  AllySpawnX);
        MoveSpawnPoint("SpawnPoint_Enemy_Ground1", EnemySpawnX);
        MoveSpawnPoint("SpawnPoint_Enemy_Ground2", EnemySpawnX);
    }

    static void MoveSpawnPoint(string objName, float newX)
    {
        GameObject go = GameObject.Find(objName);
        if (go == null)
        {
            Debug.LogWarning($"[SceneCleaner] '{objName}' 를 찾지 못했습니다.");
            return;
        }

        Undo.RecordObject(go.transform, $"SceneCleaner - move {objName}");
        Vector3 pos = go.transform.position;
        pos.x = newX;
        go.transform.position = pos;
        EditorUtility.SetDirty(go);
        Debug.Log($"[SceneCleaner] '{objName}' → x={newX}");
    }

    // =========================================================================
    // 1. BG 오브젝트 정리
    // =========================================================================

    /// <summary>
    /// Line_Ground1/2, BG_Ground1/2 의 유닛 컴포넌트를 제거하고
    /// SpriteRenderer.sprite 를 null 로 초기화한다.
    ///
    /// RequireComponent 충돌 방지를 위해 의존 순서대로 제거:
    ///   UnitVisual → UnitCombat → UnitMover → UnitHealthBar → UnitBase → Animator
    /// SpriteRenderer 와 Transform 은 유지.
    /// </summary>
    static void CleanBgObjects()
    {
        var scene   = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        int cleaned = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (t == null || t.gameObject == null) continue;
                if (!IsBgObjectName(t.gameObject.name)) continue;

                GameObject go = t.gameObject;

                // 의존 순서대로 제거 (RequireComponent 에러 방지)
                TryRemove<UnitVisual>   (go);
                TryRemove<UnitCombat>   (go);
                TryRemove<UnitMover>    (go);
                TryRemove<UnitHealthBar>(go);
                TryRemove<UnitBase>     (go);
                TryRemove<Animator>     (go);

                // SpriteRenderer.sprite null 초기화
                // (런타임에 잘못 생성된 단색 스프라이트가 남아있을 경우 대비)
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Undo.RecordObject(sr, "SceneCleaner - clear bg sprite");
                    sr.sprite = null;
                    EditorUtility.SetDirty(sr);
                }

                Debug.Log($"[SceneCleaner] '{go.name}' 정리 완료.");
                cleaned++;
            }
        }

        if (cleaned == 0)
            Debug.Log("[SceneCleaner] 정리할 BG 오브젝트를 찾지 못했습니다.");
    }

    static bool IsBgObjectName(string name)
    {
        foreach (string bg in BgObjectNames)
            if (name == bg) return true;
        return false;
    }

    static void TryRemove<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) return;
        Undo.DestroyObjectImmediate(comp);
    }

    // =========================================================================
    // 2. 기존 기지 전부 삭제
    // =========================================================================

    /// <summary>
    /// BaseKeywords 를 이름에 포함하는 오브젝트를 씬에서 전부 찾아 삭제.
    /// 자식 오브젝트를 먼저 수집한 뒤 역순으로 삭제하여 부모 삭제 시 중복 접근을 방지.
    /// </summary>
    static void DeleteAllBases()
    {
        var scene   = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var toDelete = new List<GameObject>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (t == null || t.gameObject == null) continue;
                if (IsBaseObject(t.gameObject.name))
                    toDelete.Add(t.gameObject);
            }
        }

        // 역순 삭제 (자식 → 부모 순서로 처리해 참조 문제 방지)
        for (int i = toDelete.Count - 1; i >= 0; i--)
        {
            if (toDelete[i] == null) continue;
            Debug.Log($"[SceneCleaner] 기지 삭제: '{toDelete[i].name}'");
            Undo.DestroyObjectImmediate(toDelete[i]);
        }
    }

    static bool IsBaseObject(string name)
    {
        foreach (string kw in BaseKeywords)
            if (name.Contains(kw)) return true;
        return false;
    }

    // =========================================================================
    // 3. 기지 생성
    // =========================================================================

    /// <summary>
    /// 기지 오브젝트를 새로 생성한다.
    ///
    /// 컴포넌트 추가 순서 (ITargetable 탐지 우선순위를 결정):
    ///   ① BaseUnit (UnitBase 상속) — UnitCombat 이 이 컴포넌트의 TakeDamage 를 호출
    ///      → BaseUnit.Die() → BattleManager.OnBaseDestroyed() 로 이어짐
    ///   ② BaseHP (Obsolete, UnitSpawner.GetSpawnX 호환성 유지)
    ///   ③ BoxCollider2D — Physics2D 탐지에 필요 (size 1.5×4)
    ///
    /// 루트에 SpriteRenderer 를 추가하지 않는다.
    /// 외형(성 모양)은 런타임에 BaseUnit.Awake() 가 BaseVisual 을 자동 추가해 생성.
    /// </summary>
    static GameObject CreateBase(string objName, int team, float posX)
    {
        var go = new GameObject(objName);
        Undo.RegisterCreatedObjectUndo(go, $"Create {objName}");
        go.transform.position = new Vector3(posX, 0f, 0f);

        // ① BaseUnit — GetComponentInParent<ITargetable>() 에서 먼저 반환됨
        var bu   = Undo.AddComponent<BaseUnit>(go);
        bu.team   = team;
        bu.baseHp = BaseHp;

        // ② BaseHP — UnitSpawner.GetSpawnX() 의 BaseHP.AllyBase / EnemyBase 참조용
#pragma warning disable CS0618
        var bh = Undo.AddComponent<BaseHP>(go);
        bh.baseFaction = (team == 0) ? Faction.Ally : Faction.Enemy;
        bh.maxHp       = BaseHp;
#pragma warning restore CS0618

        // ③ BoxCollider2D (BaseUnit.Awake 에서도 추가하지만 에디터 시점에 미리 설정)
        // size (2.0, 6.0): Y=-3 ~ +3 범위 → Ground1(Y=-2.5)·Ground2(Y=+2.5) 모두 커버
        var col    = Undo.AddComponent<BoxCollider2D>(go);
        col.size   = new Vector2(2.0f, 6.0f);
        col.offset = new Vector2(0f,   0f);

        string label = team == 0 ? "아군" : "적군";
        Debug.Log($"[SceneCleaner] {label} 기지 '{objName}' 생성 완료 (x={posX})");
        return go;
    }
}
#endif
