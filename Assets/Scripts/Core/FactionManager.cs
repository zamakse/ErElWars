using UnityEngine;

/// <summary>
/// 진영(아군/적군) 정보 및 관계를 관리하는 매니저
/// </summary>
public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }

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
        // 진영 초기화
    }

    private void Update()
    {
        // 진영 상태 갱신
    }
}
