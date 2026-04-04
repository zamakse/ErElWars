/// <summary>
/// 전투 라인 타입: 지상1, 지상2, 공중
/// </summary>
public enum LineType
{
    Ground1,
    Ground2,
    Air
}

/// <summary>
/// 진영 구분: 아군, 적군
/// </summary>
public enum Faction
{
    Ally,
    Enemy
}

/// <summary>
/// 유닛 타입: 딜러, 기병, 탱커
/// 상성 관계: 딜러 → 탱커, 기병 → 딜러, 탱커 → 기병 (각 150% 데미지)
/// </summary>
public enum UnitType
{
    Dealer,   // 딜러  (탱커에게 150% 데미지)
    Cavalry,  // 기병  (딜러에게 150% 데미지)
    Tanker    // 탱커  (기병에게 150% 데미지)
}

/// <summary>
/// 상성 배율 계산 유틸리티.
/// 모든 수치는 이 클래스에서만 관리한다.
/// </summary>
public static class AffinitySystem
{
    private const float StrongMultiplier = 1.5f;
    private const float NormalMultiplier = 1.0f;

    /// <summary>
    /// 공격자 타입이 방어자 타입에 적용할 데미지 배율을 반환한다.
    /// </summary>
    public static float GetMultiplier(UnitType attacker, UnitType defender)
    {
        if (attacker == UnitType.Cavalry && defender == UnitType.Dealer)  return StrongMultiplier;
        if (attacker == UnitType.Tanker  && defender == UnitType.Cavalry) return StrongMultiplier;
        if (attacker == UnitType.Dealer  && defender == UnitType.Tanker)  return StrongMultiplier;
        return NormalMultiplier;
    }
}
