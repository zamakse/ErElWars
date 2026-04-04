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

    private float baseY;      // 라인 기준 Y 좌표
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
