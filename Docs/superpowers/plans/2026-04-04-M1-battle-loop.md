# M1 전투 루프 완성 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** EnemyAI가 자동으로 유닛을 스폰하고, 플레이어가 UI 버튼으로 유닛을 소환해 전투 → 기지 파괴 → 승패 판정까지 완전히 동작하는 전투 루프를 완성한다.

**Architecture:** UnitMover/UnitCombat/BaseUnit은 이미 완료. 남은 작업은 (1) 구식 BaseHP 참조 제거, (2) EnemyAI 웨이브 스폰 구현, (3) BattleUI 모바일 버튼, (4) 원거리 투사체 시스템, (5) 공중 유닛 호버링 5개다. 각 태스크는 독립적으로 테스트 가능하며 Unity Play Mode에서 씬 실행으로 검증한다.

**Tech Stack:** Unity 2022+ (URP 2D), C#, ScriptableObject, Physics2D, Unity UI (UGUI), Coroutine

---

## 파일 맵

| 파일 | 작업 |
|------|------|
| `Assets/Scripts/Battle/UnitSpawner.cs` | 수정 — BaseHP → BaseUnit 참조 교체 |
| `Assets/Scripts/UI/BattleUI.cs` | 수정 — BaseHP 참조 교체 + 유닛 소환 버튼 추가 |
| `Assets/Scripts/Data/StageData.cs` | 수정 — WaveData에 UnitData 직접 참조 추가 |
| `Assets/Scripts/AI/EnemyAI.cs` | 전면 재작성 — 웨이브 기반 자동 스폰 |
| `Assets/Scripts/Battle/Projectile.cs` | 신규 생성 — 투사체 이동·충돌 |
| `Assets/Scripts/Battle/ProjectileManager.cs` | 재작성 — 투사체 풀 관리 |
| `Assets/Scripts/Unit/UnitCombat.cs` | 수정 — 원거리 공격 시 ProjectileManager 사용 |
| `Assets/Scripts/Unit/UnitAir.cs` | 재작성 — 호버링 애니메이션 구현 |

---

## Task 1: BaseHP → BaseUnit 마이그레이션

`BaseHP`는 `[Obsolete]` 처리된 구식 클래스다. `UnitSpawner`와 `BattleUI`가 아직 이를 참조하고 있어 경고가 발생한다. `BaseUnit.AllyBase` / `BaseUnit.EnemyBase`로 교체한다.

**Files:**
- Modify: `Assets/Scripts/Battle/UnitSpawner.cs`
- Modify: `Assets/Scripts/UI/BattleUI.cs`

- [ ] **Step 1: UnitSpawner.cs — BaseHP 참조 2곳 교체**

`GetSpawnX()` 메서드에서 `BaseHP.AllyBase` → `BaseUnit.AllyBase`, `BaseHP.EnemyBase` → `BaseUnit.EnemyBase`로 교체한다.

```csharp
// Assets/Scripts/Battle/UnitSpawner.cs
// GetSpawnX() 메서드 전체 교체

private float GetSpawnX()
{
    if (spawnerFaction == Faction.Ally)
    {
        float baseX = BaseUnit.AllyBase != null
            ? BaseUnit.AllyBase.transform.position.x
            : transform.position.x;
        return baseX + spawnOffsetFromBase;
    }
    else
    {
        float baseX = BaseUnit.EnemyBase != null
            ? BaseUnit.EnemyBase.transform.position.x
            : transform.position.x;
        return baseX - spawnOffsetFromBase;
    }
}
```

- [ ] **Step 2: BattleUI.cs — BaseHP 참조 전체 교체**

`Update()`와 `OnGUI()`에서 `BaseHP.AllyBase` → `BaseUnit.AllyBase`, `BaseHP.EnemyBase` → `BaseUnit.EnemyBase`, `.CurrentHp` → `.currentHp`, `.maxHp` → `.maxHp`로 교체한다.

```csharp
// Assets/Scripts/UI/BattleUI.cs
// Update() 내 기지 HP 텍스트 갱신 부분

if (allyBaseText != null && BaseUnit.AllyBase != null)
    allyBaseText.text = $"아군 기지\n{BaseUnit.AllyBase.currentHp:0}/{BaseUnit.AllyBase.maxHp:0}";

if (enemyBaseText != null && BaseUnit.EnemyBase != null)
    enemyBaseText.text = $"적군 기지\n{BaseUnit.EnemyBase.currentHp:0}/{BaseUnit.EnemyBase.maxHp:0}";
```

```csharp
// OnGUI() 내 아군 기지 HP 표시 부분 교체
if (BaseUnit.AllyBase != null)
{
    float ratio  = BaseUnit.AllyBase.maxHp > 0f
        ? BaseUnit.AllyBase.currentHp / BaseUnit.AllyBase.maxHp : 0f;
    float rightX = 10f;
    GUI.color = new Color(0.25f, 0.45f, 1f, 0.9f);
    GUI.Box(new Rect(rightX, 10, 160, 42),
        $"아군 기지  {BaseUnit.AllyBase.currentHp:0} / {BaseUnit.AllyBase.maxHp:0}", style);
    GUI.color = new Color(0.45f, 0.08f, 0.08f, 0.9f);
    GUI.DrawTexture(new Rect(rightX, 56, 160, 10), Texture2D.whiteTexture);
    GUI.color = new Color(0.15f, 0.78f, 0.15f, 0.9f);
    GUI.DrawTexture(new Rect(rightX, 56, 160f * ratio, 10), Texture2D.whiteTexture);
}

// 적군 기지
if (BaseUnit.EnemyBase != null)
{
    float ratio  = BaseUnit.EnemyBase.maxHp > 0f
        ? BaseUnit.EnemyBase.currentHp / BaseUnit.EnemyBase.maxHp : 0f;
    float rightX = Screen.width - 170f;
    GUI.color = new Color(1f, 0.28f, 0.28f, 0.9f);
    GUI.Box(new Rect(rightX, 10, 160, 42),
        $"적군 기지  {BaseUnit.EnemyBase.currentHp:0} / {BaseUnit.EnemyBase.maxHp:0}", style);
    GUI.color = new Color(0.45f, 0.08f, 0.08f, 0.9f);
    GUI.DrawTexture(new Rect(rightX, 56, 160, 10), Texture2D.whiteTexture);
    GUI.color = new Color(0.15f, 0.78f, 0.15f, 0.9f);
    GUI.DrawTexture(new Rect(rightX, 56, 160f * ratio, 10), Texture2D.whiteTexture);
}
```

- [ ] **Step 3: Play Mode 확인**

씬 실행 → Console에 `BaseHP` 관련 Obsolete 경고가 없어야 한다. 기지 HP가 OnGUI에 표시되고, 유닛이 기지를 공격할 때 HP가 줄어들면 OK.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Battle/UnitSpawner.cs Assets/Scripts/UI/BattleUI.cs
git commit -m "refactor: BaseHP 구식 참조를 BaseUnit으로 교체"
```

---

## Task 2: StageData.WaveData — UnitData 직접 참조로 개선

현재 `WaveData.enemyUnitId`는 문자열이라 EnemyAI가 UnitData를 찾을 방법이 없다. ScriptableObject 직접 참조로 교체한다.

**Files:**
- Modify: `Assets/Scripts/Data/StageData.cs`

- [ ] **Step 1: WaveData 구조체 수정**

`enemyUnitId` (string)를 `unitData` (UnitData)로 교체한다.

```csharp
// Assets/Scripts/Data/StageData.cs 전체

using UnityEngine;

/// <summary>
/// 스테이지 데이터를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "ZALIWA/Data/StageData")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public int    stageId;    // 스테이지 번호
    public string stageName;  // 스테이지 이름
    public Sprite stageImage; // 스테이지 배경 이미지

    [Header("적 웨이브 설정")]
    public WaveData[] waves;  // 웨이브 데이터 배열

    [Header("보상")]
    public int goldReward; // 클리어 골드 보상
    public int expReward;  // 클리어 경험치 보상
}

/// <summary>
/// 웨이브 1개 단위 데이터.
/// unitData: 소환할 적 유닛 ScriptableObject를 직접 연결한다.
/// </summary>
[System.Serializable]
public class WaveData
{
    [Tooltip("소환할 적 유닛 ScriptableObject")]
    public UnitData unitData;     // 소환할 적 유닛 데이터
    public int   spawnCount    = 5;   // 총 소환 수
    public float spawnInterval = 3f;  // 유닛 간 소환 간격 (초)
    public float startDelay    = 0f;  // 웨이브 시작 전 대기 시간 (초)
}
```

- [ ] **Step 2: 커밋**

```bash
git add Assets/Scripts/Data/StageData.cs
git commit -m "feat: WaveData에 UnitData 직접 참조 추가 (enemyUnitId string 제거)"
```

---

## Task 3: EnemyAI 웨이브 스폰 구현

StageData의 웨이브를 순서대로 실행하는 자동 스폰 로직을 구현한다. StageData가 없을 때는 기본 무한 루프 스폰으로 폴백한다.

**Files:**
- Modify: `Assets/Scripts/AI/EnemyAI.cs`

- [ ] **Step 1: EnemyAI.cs 전면 재작성**

```csharp
// Assets/Scripts/AI/EnemyAI.cs 전체

using System.Collections;
using UnityEngine;

/// <summary>
/// 적 유닛 자동 스폰 AI.
/// StageData가 연결되면 웨이브 순서대로 진행.
/// StageData가 없으면 fallbackUnitData를 fallbackInterval 간격으로 무한 스폰.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("적 소환기 (EnemyFaction UnitSpawner 연결)")]
    public UnitSpawner enemySpawner;

    [Header("스테이지 데이터 (없으면 폴백 모드 동작)")]
    public StageData stageData;

    [Header("폴백 모드 (StageData 미연결 시 사용)")]
    public UnitData  fallbackUnitData;
    public float     fallbackInterval = 4f;  // 폴백 스폰 간격(초)

    private void Start()
    {
        if (enemySpawner == null)
        {
            Debug.LogError("[EnemyAI] enemySpawner가 연결되지 않았습니다.");
            return;
        }

        if (stageData != null && stageData.waves != null && stageData.waves.Length > 0)
            StartCoroutine(RunWaves());
        else
            StartCoroutine(RunFallback());
    }

    // ─── 웨이브 모드 ──────────────────────────────────────────────────────────

    /// <summary>StageData 웨이브를 순서대로 실행한다.</summary>
    private IEnumerator RunWaves()
    {
        foreach (WaveData wave in stageData.waves)
        {
            if (wave.unitData == null)
            {
                Debug.LogWarning("[EnemyAI] WaveData.unitData가 null입니다. 이 웨이브를 건너뜁니다.");
                continue;
            }

            // 웨이브 시작 전 대기
            if (wave.startDelay > 0f)
                yield return new WaitForSeconds(wave.startDelay);

            // 지정 횟수만큼 스폰
            for (int i = 0; i < wave.spawnCount; i++)
            {
                enemySpawner.TrySpawnUnit(wave.unitData);

                if (i < wave.spawnCount - 1)
                    yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        // 모든 웨이브 완료 후 마지막 유닛 종류로 무한 스폰
        WaveData lastWave = stageData.waves[stageData.waves.Length - 1];
        if (lastWave.unitData != null)
            yield return RunInfinite(lastWave.unitData, lastWave.spawnInterval);
    }

    // ─── 폴백 모드 ───────────────────────────────────────────────────────────

    /// <summary>StageData 없을 때 단순 무한 루프 스폰.</summary>
    private IEnumerator RunFallback()
    {
        if (fallbackUnitData == null)
        {
            Debug.LogWarning("[EnemyAI] fallbackUnitData가 null입니다. 스폰을 중단합니다.");
            yield break;
        }

        yield return RunInfinite(fallbackUnitData, fallbackInterval);
    }

    // ─── 공통 무한 스폰 루프 ─────────────────────────────────────────────────

    private IEnumerator RunInfinite(UnitData unitData, float interval)
    {
        while (true)
        {
            enemySpawner.TrySpawnUnit(unitData);
            yield return new WaitForSeconds(Mathf.Max(0.5f, interval));
        }
    }
}
```

- [ ] **Step 2: Inspector 설정**

Unity Editor에서:
1. `EnemyAI` 컴포넌트가 붙은 오브젝트 선택
2. `enemySpawner` 필드에 씬의 `UnitSpawner_Enemy` 오브젝트 연결
3. `fallbackUnitData` 필드에 `TestEnemy_Data` ScriptableObject 연결
4. `stageData`는 비워둔다 (폴백 모드로 테스트)

- [ ] **Step 3: Play Mode 확인**

씬 실행 → 게임 시작 후 `fallbackInterval`(기본 4초)마다 적 유닛이 자동으로 스폰되어 아군 기지를 향해 전진하면 OK.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/AI/EnemyAI.cs
git commit -m "feat: EnemyAI 웨이브 기반 자동 스폰 구현 (StageData 연동 + 폴백 모드)"
```

---

## Task 4: BattleUI 유닛 소환 버튼 추가

키보드 테스트용 `BattleTestInput` 대신 모바일 터치로 동작하는 유닛 소환 버튼을 BattleUI에 추가한다. OnGUI 기반으로 간단하게 구현하고, 나중에 Canvas UI로 교체한다.

**Files:**
- Modify: `Assets/Scripts/UI/BattleUI.cs`

- [ ] **Step 1: BattleUI.cs에 소환 슬롯 필드 추가**

클래스 상단에 다음 필드를 추가한다.

```csharp
// BattleUI.cs — 클래스 필드 추가 (기존 필드 아래)

[Header("유닛 소환 슬롯 (최대 8개)")]
public UnitData[]   slotUnits;     // 각 슬롯에 배치할 UnitData
public UnitSpawner  allySpawner;   // 아군 소환기 연결
```

- [ ] **Step 2: OnGUI에 소환 버튼 추가**

기존 `OnGUI()` 메서드 안에 다음 코드를 추가한다. 기존 기지 HP 표시 코드 아래에 넣는다.

```csharp
// BattleUI.cs — OnGUI() 안 기존 코드 아래에 추가

// ─── 유닛 소환 버튼 (하단 중앙) ───────────────────────────────────────
if (slotUnits == null || slotUnits.Length == 0) return;

float btnW    = 80f;
float btnH    = 80f;
float gap     = 8f;
int   count   = Mathf.Min(slotUnits.Length, 8);
float totalW  = count * btnW + (count - 1) * gap;
float startX  = (Screen.width - totalW) * 0.5f;
float startY  = Screen.height - btnH - 20f;

GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
{
    fontSize  = 11,
    fontStyle = FontStyle.Bold,
    alignment = TextAnchor.LowerCenter
};

for (int i = 0; i < count; i++)
{
    UnitData unit = slotUnits[i];
    if (unit == null) continue;

    float   btnX      = startX + i * (btnW + gap);
    Rect    btnRect   = new Rect(btnX, startY, btnW, btnH);
    bool    canAfford = ManaManager.Instance != null &&
                        ManaManager.Instance.HasEnoughMana(unit.manaCost);

    GUI.color = canAfford ? Color.white : new Color(1f, 1f, 1f, 0.4f);

    string label = $"{unit.unitName}\n{unit.manaCost}마나";
    if (GUI.Button(btnRect, label, btnStyle) && canAfford && allySpawner != null)
        allySpawner.TrySpawnUnit(unit);
}

GUI.color = Color.white;
```

- [ ] **Step 3: Inspector 설정**

1. `BattleUI` 컴포넌트 선택
2. `slotUnits` 배열에 테스트 유닛 데이터 1~2개 연결 (`TestSoldier_Data` 등)
3. `allySpawner` 필드에 `UnitSpawner_Ally` 오브젝트 연결

- [ ] **Step 4: Play Mode 확인**

씬 실행 → 화면 하단에 유닛 버튼이 표시된다. 마나가 충분하면 버튼 클릭 시 유닛이 소환된다. 마나 부족 시 버튼이 반투명해진다.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/UI/BattleUI.cs
git commit -m "feat: BattleUI에 모바일 호환 유닛 소환 버튼 슬롯 추가"
```

---

## Task 5: Projectile + ProjectileManager 구현

원거리 유닛(attackRange > 2f)이 직접 TakeDamage를 호출하는 대신, 투사체를 발사하도록 한다.

**Files:**
- Create: `Assets/Scripts/Battle/Projectile.cs`
- Modify: `Assets/Scripts/Battle/ProjectileManager.cs`
- Modify: `Assets/Scripts/Unit/UnitCombat.cs`

- [ ] **Step 1: Projectile.cs 생성**

```csharp
// Assets/Scripts/Battle/Projectile.cs (신규 생성)

using UnityEngine;

/// <summary>
/// 원거리 유닛의 투사체.
/// 발사 시 목표 ITargetable을 향해 이동하고, 도달하면 데미지를 가한다.
/// ProjectileManager.Launch()에서 생성되며, 충돌 또는 목표 사망 시 자동 파괴된다.
/// </summary>
public class Projectile : MonoBehaviour
{
    private const float Speed         = 8f;   // 투사체 이동 속도
    private const float MaxLifetime   = 5f;   // 최대 생존 시간 (초)
    private const float ArrivalRadius = 0.3f; // 목표 도달 판정 반경

    private ITargetable target;
    private float       damage;
    private float       lifetime;

    /// <summary>발사 초기화. ProjectileManager.Launch()에서 호출.</summary>
    public void Init(ITargetable target, float damage)
    {
        this.target  = target;
        this.damage  = damage;
        this.lifetime = 0f;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        // 수명 초과 시 자동 파괴
        if (lifetime >= MaxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 목표가 죽었거나 사라졌으면 파괴
        if (target == null || !target.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        // 목표 방향으로 이동
        Vector3 dir = (target.GetTransform().position - transform.position).normalized;
        transform.position += dir * Speed * Time.deltaTime;

        // 목표 도달 판정
        float dist = Vector3.Distance(transform.position, target.GetTransform().position);
        if (dist <= ArrivalRadius)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
```

- [ ] **Step 2: ProjectileManager.cs 재작성**

```csharp
// Assets/Scripts/Battle/ProjectileManager.cs 전체

using UnityEngine;

/// <summary>
/// 투사체 생성을 담당하는 매니저.
/// Launch()를 호출하면 originPos에서 target을 향해 투사체가 발사된다.
/// projectilePrefab이 없으면 런타임에 기본 흰 사각형 오브젝트를 생성한다.
/// </summary>
public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("투사체 프리팹 (없으면 기본 흰 점 사용)")]
    public GameObject projectilePrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 투사체를 발사한다.
    /// originPos에서 생성되어 target을 향해 이동, 도달 시 damage를 가한다.
    /// </summary>
    public void Launch(Vector3 originPos, ITargetable target, float damage)
    {
        if (target == null || !target.IsAlive) return;

        GameObject go;
        if (projectilePrefab != null)
        {
            go = Instantiate(projectilePrefab, originPos, Quaternion.identity);
            Projectile proj = go.GetComponent<Projectile>();
            if (proj == null) proj = go.AddComponent<Projectile>();
            proj.Init(target, damage);
        }
        else
        {
            go = CreateDefaultProjectile(originPos);
            go.GetComponent<Projectile>().Init(target, damage);
        }
    }

    /// <summary>projectilePrefab 미설정 시 노란 점 투사체 GameObject를 직접 생성해 반환한다.</summary>
    private GameObject CreateDefaultProjectile(Vector3 pos)
    {
        GameObject go = new GameObject("Projectile_Default");
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

        // Texture2D.whiteTexture로 단색 스프라이트 생성 (외부 에셋 불필요)
        var sr    = go.AddComponent<SpriteRenderer>();
        var tex   = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
        sr.color  = Color.yellow;

        go.AddComponent<Projectile>();
        return go;
    }
}
```

- [ ] **Step 3: UnitCombat.cs — 원거리 공격 분기 추가**

`TryAttack()` 메서드에서 `attackRange > 2f`이면 ProjectileManager를 통해 투사체 발사, 아니면 기존 즉시 데미지 방식을 유지한다.

```csharp
// UnitCombat.cs — TryAttack() 메서드 교체

private const float RangedThreshold = 2f; // 이 값 초과이면 원거리 유닛 취급

private void TryAttack(ITargetable target)
{
    if (attackTimer > 0f) return;

    float damage = unitBase.attackDamage;

    // 상성 배율 적용 (기지에는 미적용)
    if (target is UnitBase enemyUnit && !(target is BaseUnit))
        damage *= AffinitySystem.GetMultiplier(unitBase.unitType, enemyUnit.unitType);

    // 원거리 유닛: 투사체 발사 / 근접 유닛: 즉시 데미지
    if (unitBase.attackRange > RangedThreshold && ProjectileManager.Instance != null)
        ProjectileManager.Instance.Launch(transform.position, target, damage);
    else
        target.TakeDamage(damage);

    attackTimer = 1f / Mathf.Max(0.01f, unitBase.attackSpeed);
    OnAttackExecuted?.Invoke();
}
```

- [ ] **Step 4: Inspector 설정**

씬의 `ProjectileManager` 오브젝트에 컴포넌트 추가 또는 기존 컴포넌트 확인. `projectilePrefab`은 비워두면 기본 노란 점이 사용된다.

- [ ] **Step 5: Play Mode 확인**

원거리 유닛(attackRange > 2f) 데이터를 가진 유닛이 씬에 있을 때: 적을 향해 노란 점이 날아가고 도달 시 데미지가 들어가면 OK. 근접 유닛은 변화 없이 기존처럼 동작해야 한다.

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Battle/Projectile.cs \
        Assets/Scripts/Battle/ProjectileManager.cs \
        Assets/Scripts/Unit/UnitCombat.cs
git commit -m "feat: 원거리 투사체 시스템 구현 (Projectile + ProjectileManager)"
```

---

## Task 6: UnitAir 호버링 구현

공중 라인(Air) 유닛이 사인파로 Y축을 진동하며 비행하는 것처럼 보이게 한다.

**Files:**
- Modify: `Assets/Scripts/Unit/UnitAir.cs`

- [ ] **Step 1: UnitAir.cs 재작성**

```csharp
// Assets/Scripts/Unit/UnitAir.cs 전체

using UnityEngine;

/// <summary>
/// 공중 유닛 전용 호버링 컴포넌트.
/// UnitMover의 이동과 독립적으로 Y축을 사인파로 진동시킨다.
/// 이 컴포넌트는 Air 라인 유닛 프리팹에만 부착한다.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitAir : MonoBehaviour
{
    [Header("호버링 설정")]
    [Tooltip("Y축 진동 진폭 (유닛 크기에 맞게 조정)")]
    public float amplitude = 0.18f;
    [Tooltip("진동 주기 (값이 클수록 빠르게 움직임)")]
    public float frequency = 2.2f;

    private float baseY;     // 라인 기준 Y 좌표
    private float timeOffset; // 유닛마다 위상 차이를 줘서 동기화 방지

    private void Start()
    {
        baseY      = transform.position.y;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void LateUpdate()
    {
        // UnitMover가 X를 움직인 뒤 LateUpdate에서 Y를 보정
        Vector3 pos = transform.position;
        pos.y = baseY + Mathf.Sin(Time.time * frequency + timeOffset) * amplitude;
        transform.position = pos;
    }
}
```

- [ ] **Step 2: Play Mode 확인**

Air 라인 유닛 프리팹에 `UnitAir` 컴포넌트를 추가하고 씬 실행 → 유닛이 좌우로 이동하면서 Y축으로 미세하게 진동하면 OK.

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Unit/UnitAir.cs
git commit -m "feat: 공중 유닛 호버링 애니메이션 구현 (UnitAir)"
```

---

## M1 최종 확인

- [ ] 씬 실행 후 게임 시작되면 적 유닛이 자동으로 스폰된다
- [ ] 하단 UI 버튼을 눌러 아군 유닛을 소환할 수 있다
- [ ] 아군/적 유닛이 충돌하면 자동으로 전투가 시작된다
- [ ] 어느 한 쪽 유닛이 기지에 도달하면 기지 HP가 줄어든다
- [ ] 기지 HP가 0이 되면 승/패 화면이 표시되고 R키로 재시작된다
- [ ] Console에 error가 없고, Obsolete warning이 없다
