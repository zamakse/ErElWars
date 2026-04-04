using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// 메뉴: ZALIWA > Setup Warrior Animation
/// 실행하면 Individual Sprite PNG를 읽어 AnimationClip 4개와
/// AnimatorController를 자동 생성한다.
/// 생성 위치: Assets/Animations/Warrior/
/// </summary>
public static class WarriorAnimationSetup
{
    private const string OutputDir   = "Assets/Animations/Warrior";
    private const string SpritesRoot = "Assets/Sprites/Units/Warrior/Individual Sprite";
    private const float  Fps         = 12f;

    [MenuItem("ZALIWA/Setup Warrior Animation")]
    public static void Run()
    {
        EnsureFolder("Assets/Animations");
        EnsureFolder("Assets/Animations/Warrior");

        AnimationClip idleClip   = CreateClip("idle",         "Warrior_Idle_",   6,  loop: true);
        AnimationClip walkClip   = CreateClip("Run",          "Warrior_Run_",    8,  loop: true);
        AnimationClip attackClip = CreateClip("Attack",       "Warrior_Attack_", 12, loop: false);
        AnimationClip dieClip    = CreateClip("Death-Effect", "Warrior_Death_",  11, loop: false);

        SaveClip(idleClip,   "Warrior_Idle");
        SaveClip(walkClip,   "Warrior_Walk");
        SaveClip(attackClip, "Warrior_Attack");
        SaveClip(dieClip,    "Warrior_Die");

        CreateAnimatorController(idleClip, walkClip, attackClip, dieClip);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WarriorAnimationSetup] 완료! Assets/Animations/Warrior/ 폴더를 확인하세요.");
    }

    // ── 내부 헬퍼 ──────────────────────────────────────────────────────────────

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string child  = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static AnimationClip CreateClip(
        string folder, string prefix, int frameCount, bool loop)
    {
        var clip = new AnimationClip { frameRate = Fps };

        // 루프 설정
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // SpriteRenderer.sprite 커브 바인딩
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keyframes = new ObjectReferenceKeyframe[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            string spritePath = $"{SpritesRoot}/{folder}/{prefix}{i + 1}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
                Debug.LogWarning($"[WarriorAnimationSetup] 스프라이트 없음: {spritePath}");

            keyframes[i] = new ObjectReferenceKeyframe
            {
                time  = i / Fps,
                value = sprite
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        return clip;
    }

    private static void SaveClip(AnimationClip newClip, string name)
    {
        string path     = $"{OutputDir}/{name}.anim";
        string fullPath = Application.dataPath + "/../" + path;

        if (File.Exists(fullPath))
        {
            // 기존 에셋 in-place 업데이트 → GUID 보존 (프리팹 참조 유지)
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                existing.ClearCurves();

                // PPtrCurve(스프라이트 키프레임) 복사
                foreach (EditorCurveBinding binding in
                         AnimationUtility.GetObjectReferenceCurveBindings(newClip))
                {
                    AnimationUtility.SetObjectReferenceCurve(
                        existing, binding,
                        AnimationUtility.GetObjectReferenceCurve(newClip, binding));
                }

                // 루프 등 클립 설정 복사
                AnimationUtility.SetAnimationClipSettings(
                    existing,
                    AnimationUtility.GetAnimationClipSettings(newClip));

                EditorUtility.SetDirty(existing);
                return;
            }
        }

        AssetDatabase.CreateAsset(newClip, path);
    }

    private static void CreateAnimatorController(
        AnimationClip idle, AnimationClip walk,
        AnimationClip attack, AnimationClip die)
    {
        string path     = $"{OutputDir}/WarriorAnimator.controller";
        string fullPath = Application.dataPath + "/../" + path;

        if (File.Exists(fullPath))
        {
            // 기존 컨트롤러 in-place 업데이트 → GUID 보존 (프리팹 참조 유지)
            AnimatorController existing =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null)
            {
                foreach (ChildAnimatorState cs in
                         existing.layers[0].stateMachine.states)
                {
                    switch (cs.state.name)
                    {
                        case "Idle":   cs.state.motion = idle;   break;
                        case "Walk":   cs.state.motion = walk;   break;
                        case "Attack": cs.state.motion = attack; break;
                        case "Die":    cs.state.motion = die;    break;
                    }
                }
                EditorUtility.SetDirty(existing);
                Debug.Log($"[WarriorAnimationSetup] AnimatorController 업데이트: {path}");
                return;
            }
        }

        // ── 최초 생성 ────────────────────────────────────────────────────────
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // int 파라미터 하나로 상태 전환 (0=Idle, 1=Walk, 2=Attack, 3=Die)
        controller.AddParameter("State", AnimatorControllerParameterType.Int);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        // 상태 생성
        AnimatorState idleState   = sm.AddState("Idle");
        AnimatorState walkState   = sm.AddState("Walk");
        AnimatorState attackState = sm.AddState("Attack");
        AnimatorState dieState    = sm.AddState("Die");

        idleState.motion   = idle;
        walkState.motion   = walk;
        attackState.motion = attack;
        dieState.motion    = die;

        sm.defaultState = idleState;

        // ── Any State → Die ─────────────────────────────────────────────────
        var toDie = sm.AddAnyStateTransition(dieState);
        toDie.AddCondition(AnimatorConditionMode.Equals, 3, "State");
        toDie.hasExitTime         = false;
        toDie.duration            = 0f;
        toDie.canTransitionToSelf = false;

        // ── Idle → Walk ──────────────────────────────────────────────────────
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Equals, 1, "State");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration    = 0f;

        // ── Walk → Idle ──────────────────────────────────────────────────────
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration    = 0f;

        // ── Idle → Attack ────────────────────────────────────────────────────
        var idleToAttack = idleState.AddTransition(attackState);
        idleToAttack.AddCondition(AnimatorConditionMode.Equals, 2, "State");
        idleToAttack.hasExitTime = false;
        idleToAttack.duration    = 0f;

        // ── Walk → Attack ────────────────────────────────────────────────────
        var walkToAttack = walkState.AddTransition(attackState);
        walkToAttack.AddCondition(AnimatorConditionMode.Equals, 2, "State");
        walkToAttack.hasExitTime = false;
        walkToAttack.duration    = 0f;

        // ── Attack → Idle (클립 끝나면 자동 복귀) ───────────────────────────
        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime    = 1f;  // 정규화 시간 1.0 = 클립 끝
        attackToIdle.duration    = 0f;
        attackToIdle.AddCondition(AnimatorConditionMode.NotEqual, 3, "State"); // Die 아닐 때만

        Debug.Log($"[WarriorAnimationSetup] AnimatorController 생성: {path}");
    }
}
