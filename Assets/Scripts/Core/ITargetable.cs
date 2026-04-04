using UnityEngine;

/// <summary>
/// 공격 가능한 타깃의 공통 인터페이스.
/// UnitBase(유닛)와 BaseHP(기지)가 모두 구현하므로
/// UnitCombat은 타깃 종류에 무관하게 동일한 코드로 탐지·공격한다.
/// </summary>
public interface ITargetable
{
    bool      IsAlive { get; }
    void      TakeDamage(float damage);
    Transform GetTransform();

    /// <summary>myTeam: 0 = 아군(Faction.Ally), 1 = 적군(Faction.Enemy)</summary>
    bool IsEnemy(int myTeam);
}
