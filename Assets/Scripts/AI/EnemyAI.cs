using UnityEngine;

/// <summary>
/// 적 유닛의 AI 행동 패턴을 제어하는 컴포넌트
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("AI 설정")]
    public float detectionRange = 10f;  // 적 감지 범위
    public float chaseRange = 15f;      // 추적 포기 범위

    private UnitBase unitBase;
    private Transform targetTransform;  // 현재 타겟

    // AI 상태 정의
    private enum AIState { Idle, Chase, Attack, Retreat }
    private AIState currentState = AIState.Idle;

    private void Awake()
    {
        unitBase = GetComponent<UnitBase>();
    }

    private void Start()
    {
        // AI 초기화
    }

    private void Update()
    {
        // AI 상태 머신 갱신
        UpdateAIState();
    }

    /// <summary>
    /// AI 상태 머신 업데이트
    /// </summary>
    private void UpdateAIState()
    {
        switch (currentState)
        {
            case AIState.Idle:
                // 주변 탐색
                break;
            case AIState.Chase:
                // 타겟 추적
                break;
            case AIState.Attack:
                // 공격 수행
                break;
            case AIState.Retreat:
                // 후퇴
                break;
        }
    }
}
