using UnityEngine;

/// <summary>
/// 영웅 부활 로직을 담당하는 컴포넌트
/// </summary>
public class HeroRevive : MonoBehaviour
{
    [Header("부활 설정")]
    public float reviveDelay = 5f;     // 부활 대기 시간 (초)
    public float reviveHpRatio = 0.3f; // 부활 시 회복 체력 비율

    private HeroBase heroBase;
    private bool isReviving = false;

    private void Awake()
    {
        heroBase = GetComponent<HeroBase>();
    }

    private void Start()
    {
        // 부활 시스템 초기화
    }

    /// <summary>
    /// 부활 시퀀스를 시작
    /// </summary>
    public void TriggerRevive()
    {
        if (isReviving) return;
        isReviving = true;
        // 부활 대기 코루틴 시작
        StartCoroutine(ReviveCoroutine());
    }

    private System.Collections.IEnumerator ReviveCoroutine()
    {
        // 부활 대기
        yield return new WaitForSeconds(reviveDelay);

        // 체력 회복 후 부활
        heroBase.currentHp = heroBase.maxHp * reviveHpRatio;
        isReviving = false;

        // 부활 이펙트 재생
    }
}
