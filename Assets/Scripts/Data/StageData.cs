using UnityEngine;

/// <summary>
/// 스테이지 데이터를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "ZALIWA/Data/StageData")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public int    stageId;    // 스테이지 번호
    public string stageName;  // 스테이지 이름
    public Sprite stageImage; // 스테이지 배경 이미지

    [Header("적 웨이브 설정")]
    public WaveData[] waves;  // 웨이브 데이터 배열

    [Header("보상")]
    public int goldReward; // 클리어 골드 보상
    public int expReward;  // 클리어 경험치 보상
}

/// <summary>
/// 웨이브 1개 단위 데이터.
/// unitData: 소환할 적 유닛 ScriptableObject를 직접 연결한다.
/// </summary>
[System.Serializable]
public class WaveData
{
    [Tooltip("소환할 적 유닛 ScriptableObject")]
    public UnitData unitData;     // 소환할 적 유닛 데이터
    public int   spawnCount    = 5;   // 총 소환 수
    public float spawnInterval = 3f;  // 유닛 간 소환 간격 (초)
    public float startDelay    = 0f;  // 웨이브 시작 전 대기 시간 (초)
}
