using UnityEngine;
using System;

/// <summary>
/// 유닛 소환기.
/// - ManaManager에서 마나를 소비하고 UnitData.prefab을 지정 위치에 생성한다.
/// - 각 진영당 하나씩 씬에 배치한다 (아군 소환기 / 적군 소환기).
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("소환기 설정")]
    public Faction spawnerFaction;   // 이 소환기의 진영 (Ally / Enemy)

    [Header("스폰 X 오프셋")]
    [Tooltip("기지 X로부터 전방으로 떨어진 거리 (기지 바로 앞 스폰)")]
    [SerializeField] private float spawnOffsetFromBase = 1.5f;

    [Header("라인별 Y 기준 위치")]
    [Tooltip("Ground1 라인 Y 기준점")]
    public Transform spawnPointGround1;
    [Tooltip("Ground2 라인 Y 기준점")]
    public Transform spawnPointGround2;
    [Tooltip("Air 라인 Y 기준점")]
    public Transform spawnPointAir;

    // ─── 이벤트 ───────────────────────────────────────────────────────────────
    /// <summary>소환 성공 이벤트 (생성된 UnitBase, 사용된 UnitData)</summary>
    public event Action<UnitBase, UnitData> OnUnitSpawned;
    /// <summary>마나 부족으로 소환 실패 이벤트</summary>
    public event Action<UnitData> OnSpawnFailed;

    /// <summary>
    /// 유닛 소환 시도 (라인 자동 결정: unitData.lineType 사용).
    /// </summary>
    public bool TrySpawnUnit(UnitData unitData)
        => TrySpawnUnit(unitData, unitData != null ? unitData.lineType : LineType.Ground1);

    /// <summary>
    /// 유닛 소환 시도 (라인 직접 지정).
    /// lineOverride가 unitData.lineType보다 우선하며,
    /// 소환 위치와 유닛 lineType이 모두 lineOverride로 설정된다.
    /// </summary>
    public bool TrySpawnUnit(UnitData unitData, LineType lineOverride)
    {
        Debug.Log($"[UnitSpawner:{spawnerFaction}] TrySpawnUnit 진입: {(unitData != null ? unitData.unitName : "null")} / 라인: {lineOverride}");

        if (unitData == null)
        {
            Debug.LogError("[UnitSpawner] unitData가 null입니다.");
            return false;
        }

        if (unitData.prefab == null)
        {
            Debug.LogError($"[UnitSpawner] '{unitData.unitName}': prefab이 할당되지 않았습니다. " +
                           $"UnitData 에셋의 Prefab 필드를 확인하세요.");
            return false;
        }

        // ManaManager 존재 확인
        if (ManaManager.Instance == null)
        {
            Debug.LogError("[UnitSpawner] ManaManager.Instance가 null입니다. 씬에 ManaManager가 있는지 확인하세요.");
            return false;
        }

        Debug.Log($"[UnitSpawner:{spawnerFaction}] 마나 확인: 현재 {ManaManager.Instance.CurrentMana:F1} / 필요 {unitData.manaCost}");

        // 마나 소비 시도
        if (!ManaManager.Instance.SpendMana(unitData.manaCost))
        {
            Debug.LogWarning($"[UnitSpawner:{spawnerFaction}] 마나 부족으로 소환 실패.");
            OnSpawnFailed?.Invoke(unitData);
            return false;
        }

        // lineOverride 기준으로 소환 위치 결정
        Vector3 spawnPos = GetSpawnPosition(lineOverride);
        Debug.Log($"[UnitSpawner:{spawnerFaction}] 소환 위치: {spawnPos} (라인: {lineOverride})");

        // 유닛 생성
        GameObject go = Instantiate(unitData.prefab, spawnPos, Quaternion.identity);
        Debug.Log($"[UnitSpawner:{spawnerFaction}] Instantiate 완료: {go.name}");

        UnitBase unit = go.GetComponent<UnitBase>();
        if (unit == null)
        {
            Debug.LogError($"[UnitSpawner] '{unitData.unitName}' prefab에 UnitBase 컴포넌트가 없습니다.");
            Destroy(go);
            return false;
        }

        // 스탯 초기화 후 lineOverride 적용 (UnitCombat 라인 필터링에 사용됨)
        unit.Initialize(unitData, spawnerFaction);
        unit.lineType = lineOverride;
        Debug.Log($"[UnitSpawner:{spawnerFaction}] 초기화 완료: {unitData.unitName} | 라인={lineOverride} | HP {unit.currentHp}/{unit.maxHp}");

        // LineManager에 유닛 등록
        LineManager.Instance?.RegisterUnit(unit);

        OnUnitSpawned?.Invoke(unit, unitData);
        return true;
    }

    /// <summary>
    /// LineType에 해당하는 소환 위치를 반환.
    /// X = 자기 진영 기지 위치 + spawnOffsetFromBase (기지 바로 앞)
    /// Y = 라인 기준점 트랜스폼의 Y값 (LineManager 라인 Y와 동기화)
    /// </summary>
    private Vector3 GetSpawnPosition(LineType lineType)
    {
        Transform point = lineType switch
        {
            LineType.Ground1 => spawnPointGround1,
            LineType.Ground2 => spawnPointGround2,
            LineType.Air     => spawnPointAir,
            _                => null
        };

        float spawnY = point != null ? point.position.y : transform.position.y;
        float spawnX = GetSpawnX();
        return new Vector3(spawnX, spawnY, 0f);
    }

    /// <summary>
    /// 기지 X 위치 기준으로 스폰 X를 계산한다.
    /// 아군: 기지X + offset (기지 오른쪽), 적군: 기지X − offset (기지 왼쪽)
    /// 기지를 찾지 못하면 소환기 자신의 X를 fallback으로 사용한다.
    /// </summary>
    private float GetSpawnX()
    {
        if (spawnerFaction == Faction.Ally)
        {
            float baseX = BaseHP.AllyBase != null
                ? BaseHP.AllyBase.transform.position.x
                : transform.position.x;
            return baseX + spawnOffsetFromBase;
        }
        else
        {
            float baseX = BaseHP.EnemyBase != null
                ? BaseHP.EnemyBase.transform.position.x
                : transform.position.x;
            return baseX - spawnOffsetFromBase;
        }
    }
}
