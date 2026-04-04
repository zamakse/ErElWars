using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드맵 화면 UI(스테이지 선택 등)를 관리하는 클래스
/// </summary>
public class WorldMapUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Button[] stageButtons;  // 스테이지 선택 버튼 배열

    private void Start()
    {
        // 월드맵 UI 초기화
        InitializeStageButtons();
    }

    private void Update()
    {
        // 월드맵 UI 갱신
    }

    /// <summary>
    /// 스테이지 버튼 초기화 및 이벤트 등록
    /// </summary>
    private void InitializeStageButtons()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stageIndex = i; // 클로저 캡처를 위해 로컬 변수 사용
            stageButtons[i].onClick.AddListener(() => OnStageButtonClicked(stageIndex));
        }
    }

    /// <summary>
    /// 스테이지 버튼 클릭 처리
    /// </summary>
    private void OnStageButtonClicked(int stageIndex)
    {
        // 해당 스테이지로 진입
    }
}
