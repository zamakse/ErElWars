using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 라인(Ground1, Ground2, Air)의 Y 좌표와 유닛 목록을 관리한다.
/// 씬당 하나만 배치하고 LineManager.Instance로 접근한다.
/// Y 수치는 인스펙터(씬 YAML)에서만 관리한다.
/// </summary>
public class LineManager : MonoBehaviour
{
    public static LineManager Instance { get; private set; }

    [Header("라인 Y 좌표 (수치는 인스펙터에서만 관리)")]
    public float ground1Y = -1.5f;  // Ground1 라인 Y 위치
    public float ground2Y =  1.5f;  // Ground2 라인 Y 위치
    public float airY     =  3.0f;  // Air 라인 Y 위치

    // ─── 라인별 유닛 리스트 (런타임 전용) ─────────────────────────────
    private readonly List<UnitBase> ground1Units = new();
    private readonly List<UnitBase> ground2Units = new();
    private readonly List<UnitBase> airUnits     = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ─── 공개 API ─────────────────────────────────────────────────────

    /// <summary>라인 타입에 해당하는 Y 좌표 반환</summary>
    public float GetLineY(LineType lineType) => lineType switch
    {
        LineType.Ground1 => ground1Y,
        LineType.Ground2 => ground2Y,
        LineType.Air     => airY,
        _                => 0f
    };

    /// <summary>유닛을 해당 라인 목록에 등록한다. 사망 시 자동 제거.</summary>
    public void RegisterUnit(UnitBase unit)
    {
        if (unit == null) return;
        List<UnitBase> list = GetList(unit.lineType);
        if (list.Contains(unit)) return;
        list.Add(unit);
        unit.OnDeath += OnUnitDied;
    }

    /// <summary>유닛을 라인 목록에서 수동 제거한다.</summary>
    public void UnregisterUnit(UnitBase unit)
    {
        if (unit == null) return;
        GetList(unit.lineType).Remove(unit);
        unit.OnDeath -= OnUnitDied;
    }

    /// <summary>특정 라인의 유닛 목록을 반환한다 (읽기 전용).</summary>
    public IReadOnlyList<UnitBase> GetUnitsOnLine(LineType lineType)
        => GetList(lineType);

    // ─── 내부 헬퍼 ────────────────────────────────────────────────────

    private void OnUnitDied(UnitBase unit) => UnregisterUnit(unit);

    private List<UnitBase> GetList(LineType lineType) => lineType switch
    {
        LineType.Ground1 => ground1Units,
        LineType.Ground2 => ground2Units,
        LineType.Air     => airUnits,
        _                => ground1Units
    };
}
