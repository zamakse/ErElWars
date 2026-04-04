using UnityEngine;

/// <summary>
/// 유닛 광전사 모드(Frenzy) 버프 시스템을 처리하는 컴포넌트
/// </summary>
public class UnitFrenzy : MonoBehaviour
{
    [Header("광전사 설정")]
    public float frenzyDuration = 5f;         // 광전사 지속 시간 (초)
    public float frenzyAttackMultiplier = 2f; // 공격력 배율
    public float frenzySpeedMultiplier = 1.5f; // 이동속도 배율

    private UnitBase unitBase;
    private bool isFrenzy = false;
    private float frenzyTimer = 0f;

    private void Awake()
    {
        unitBase = GetComponent<UnitBase>();
    }

    private void Start()
    {
        // 광전사 시스템 초기화
    }

    private void Update()
    {
        // 광전사 타이머 처리
        if (isFrenzy)
        {
            frenzyTimer -= Time.deltaTime;
            if (frenzyTimer <= 0f)
                EndFrenzy();
        }
    }

    /// <summary>
    /// 광전사 모드 활성화
    /// </summary>
    public void ActivateFrenzy()
    {
        if (isFrenzy) return;

        isFrenzy = true;
        frenzyTimer = frenzyDuration;
        unitBase.attackDamage *= frenzyAttackMultiplier;
        unitBase.moveSpeed *= frenzySpeedMultiplier;
    }

    /// <summary>
    /// 광전사 모드 종료
    /// </summary>
    private void EndFrenzy()
    {
        isFrenzy = false;
        unitBase.attackDamage /= frenzyAttackMultiplier;
        unitBase.moveSpeed /= frenzySpeedMultiplier;
    }
}
