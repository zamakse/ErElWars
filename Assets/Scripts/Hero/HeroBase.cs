using UnityEngine;

/// <summary>
/// 영웅 캐릭터의 기반이 되는 베이스 클래스
/// </summary>
public class HeroBase : UnitBase
{
    [Header("영웅 정보")]
    public string heroName;   // 영웅 이름
    public int heroLevel = 1; // 영웅 레벨

    protected HeroStats heroStats;
    protected HeroSkill heroSkill;

    protected override void Awake()
    {
        base.Awake();
        heroStats = GetComponent<HeroStats>();
        heroSkill = GetComponent<HeroSkill>();
    }

    protected override void Start()
    {
        base.Start();
        // 영웅 초기화
    }

    protected override void Update()
    {
        base.Update();
        // 영웅 상태 갱신
    }

    protected override void Die()
    {
        // 영웅 사망 시 부활 처리로 연결. 부활 컴포넌트가 없으면 일반 사망 처리
        HeroRevive revive = GetComponent<HeroRevive>();
        if (revive != null)
            revive.TriggerRevive();
        else
            base.Die(); // OnDeath 이벤트 발생 후 Destroy
    }
}
