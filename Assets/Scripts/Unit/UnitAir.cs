using UnityEngine;

/// <summary>
/// 공중 유닛 전용 동작을 처리하는 컴포넌트
/// </summary>
public class UnitAir : MonoBehaviour
{
    [Header("공중 유닛 설정")]
    public float flyHeight = 3f;  // 비행 높이
    public float hoverAmplitude = 0.2f;  // 호버링 진폭
    public float hoverFrequency = 1f;    // 호버링 주기

    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.position;
        // 공중 유닛 초기화
    }

    private void Update()
    {
        // 호버링 애니메이션 처리
    }
}
