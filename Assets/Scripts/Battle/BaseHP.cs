using UnityEngine;
using System;

/// <summary>
/// 기지(Base) 체력을 관리하는 컴포넌트.
/// 유닛이 기지 위치에 도달하면 UnitMover에서 TakeDamage를 호출한다.
/// 정적 참조(AllyBase / EnemyBase)로 씬 어디서든 접근 가능.
/// </summary>
[System.Obsolete("BaseHP는 더 이상 사용되지 않습니다. BaseUnit(UnitBase 상속)으로 교체하세요. Editor 메뉴: ZALIWA > Replace Bases with BaseUnit")]
public class BaseHP : MonoBehaviour, ITargetable
{
    // ─── 정적 참조 (씬당 진영별 하나씩) ──────────────────────────────
    public static BaseHP AllyBase;
    public static BaseHP EnemyBase;

    /// <summary>어느 기지든 파괴되면 발동하는 정적 이벤트 (파괴된 기지의 진영 전달)</summary>
    public static event Action<Faction> OnAnyBaseDestroyed;

    [Header("기지 설정")]
    public Faction baseFaction = Faction.Ally;  // 이 기지의 진영
    public float   maxHp       = 1000f;         // 기지 최대 체력

    public float CurrentHp { get; private set; }

    // ─── ITargetable ─────────────────────────────────────────────────
    public bool      IsAlive        => CurrentHp > 0f;
    public Transform GetTransform() => transform;
    /// <inheritdoc cref="ITargetable.IsEnemy"/>
    public bool IsEnemy(int myTeam) => (int)baseFaction != myTeam;

    // ─── 이벤트 ──────────────────────────────────────────────────────
    /// <summary>기지 피격 이벤트 (base, 남은 HP)</summary>
    public event Action<BaseHP, float> OnDamaged;
    /// <summary>기지 파괴 이벤트</summary>
    public event Action<BaseHP> OnDestroyed;

    private void Awake()
    {
        CurrentHp = maxHp;

        // 진영에 따라 정적 참조 등록
        if (baseFaction == Faction.Ally)
        {
            AllyBase = this;
            Debug.Log($"[BaseHP] 아군 기지 등록 (HP={maxHp})");
        }
        else
        {
            EnemyBase = this;
            Debug.Log($"[BaseHP] 적군 기지 등록 (HP={maxHp})");
        }
    }

    private void OnDestroy()
    {
        if (AllyBase  == this) AllyBase  = null;
        if (EnemyBase == this) EnemyBase = null;
    }

    /// <summary>
    /// 기지에 데미지를 가한다.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (CurrentHp <= 0f) return;

        CurrentHp = Mathf.Max(0f, CurrentHp - amount);
        OnDamaged?.Invoke(this, CurrentHp);

        string factionStr = baseFaction == Faction.Ally ? "아군" : "적군";
        Debug.Log($"[BaseHP] {factionStr} 기지 피격 -{amount} → 남은 HP={CurrentHp}");

        if (CurrentHp <= 0f)
        {
            OnDestroyed?.Invoke(this);
            OnAnyBaseDestroyed?.Invoke(baseFaction); // GameManager 등에서 게임 오버 처리
            Debug.Log($"[BaseHP] {factionStr} 기지 파괴!");
        }
    }
}
