using UnityEngine;
using System;

/// <summary>
/// 마나(자원) 자동 생성, 소비, 상태 관리 매니저.
/// UI 레이어는 OnManaChanged 이벤트를 구독해 상태를 수신한다.
/// </summary>
public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance { get; private set; }

    [Header("마나 설정")]
    public float maxMana = 100f;     // 최대 마나
    public float startMana = 20f;    // 시작 마나
    public float manaRegenRate = 5f; // 초당 자동 생성량

    public float CurrentMana { get; private set; }
    public float MaxMana => maxMana;

    // ─── 이벤트 (UI는 이 이벤트로만 마나 상태를 수신) ─────────────────────────
    /// <summary>마나 변경 이벤트 (현재 마나, 최대 마나)</summary>
    public event Action<float, float> OnManaChanged;

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
        CurrentMana = Mathf.Clamp(startMana, 0f, maxMana);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    private void Update()
    {
        RegenerateMana();
    }

    /// <summary>
    /// 시간 경과에 따른 마나 자동 생성
    /// </summary>
    private void RegenerateMana()
    {
        if (CurrentMana >= maxMana) return;

        float prev = CurrentMana;
        CurrentMana = Mathf.Min(CurrentMana + manaRegenRate * Time.deltaTime, maxMana);

        if (!Mathf.Approximately(prev, CurrentMana))
            OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    /// <summary>
    /// 마나 소비 시도. 마나가 부족하면 false 반환하고 소비하지 않는다.
    /// </summary>
    public bool SpendMana(float amount)
    {
        if (CurrentMana < amount) return false;

        CurrentMana -= amount;
        OnManaChanged?.Invoke(CurrentMana, maxMana);
        return true;
    }

    /// <summary>
    /// 마나 직접 추가 (보상, 스킬 등)
    /// </summary>
    public void AddMana(float amount)
    {
        CurrentMana = Mathf.Min(CurrentMana + amount, maxMana);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    /// <summary>현재 마나가 amount 이상인지 확인 (소비 없음)</summary>
    public bool HasEnoughMana(float amount) => CurrentMana >= amount;
}
