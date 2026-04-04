using UnityEngine;

/// <summary>
/// 투사체(화살, 마법 등) 생성 및 관리를 담당
/// </summary>
public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

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
        // 투사체 풀 초기화
    }

    private void Update()
    {
        // 투사체 상태 갱신
    }
}
