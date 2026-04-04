using UnityEngine;

/// <summary>
/// 스테이지 진행 및 전환을 관리하는 매니저
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

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
        // 스테이지 초기화
    }

    private void Update()
    {
        // 스테이지 클리어 조건 체크
    }
}
