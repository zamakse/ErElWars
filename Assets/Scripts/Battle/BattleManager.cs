using UnityEngine;
using System;

/// <summary>
/// 전투 시작/종료 및 전체 전투 흐름을 관리하는 매니저.
/// BaseUnit.Die()가 OnBaseDestroyed(team)을 호출 →
/// OnAnyBaseDestroyed 정적 이벤트 발동 → GameManager가 구독해 게임 오버 처리.
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    /// <summary>
    /// 기지가 파괴될 때 발동.
    /// team: 0 = 아군 기지 파괴(패배), 1 = 적군 기지 파괴(승리)
    /// </summary>
    public static event Action<int> OnAnyBaseDestroyed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 전투 초기화
    }

    private void Update()
    {
        // 전투 상태 갱신
    }

    /// <summary>
    /// BaseUnit.Die()에서 호출.
    /// team: 0 = 아군 기지, 1 = 적군 기지
    /// </summary>
    public void OnBaseDestroyed(int team)
    {
        string label = team == 0 ? "아군" : "적군";
        Debug.Log($"[BattleManager] {label} 기지 파괴! (team={team})");
        OnAnyBaseDestroyed?.Invoke(team);
    }
}
