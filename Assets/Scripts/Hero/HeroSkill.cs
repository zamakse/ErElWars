using UnityEngine;

/// <summary>
/// 영웅 스킬 발동 및 쿨다운을 관리하는 컴포넌트
/// </summary>
public class HeroSkill : MonoBehaviour
{
    [Header("스킬 설정")]
    public float skillCooldown = 10f;  // 스킬 쿨다운 (초)
    public float skillDamage = 50f;    // 스킬 데미지

    private float cooldownTimer = 0f;  // 현재 쿨다운 타이머

    private void Start()
    {
        // 스킬 초기화
    }

    private void Update()
    {
        // 쿨다운 감소
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 스킬 사용 가능 여부 반환
    /// </summary>
    public bool CanUseSkill() => cooldownTimer <= 0f;

    /// <summary>
    /// 스킬 발동
    /// </summary>
    public void UseSkill()
    {
        if (!CanUseSkill()) return;

        // 스킬 효과 구현
        cooldownTimer = skillCooldown;
    }
}
