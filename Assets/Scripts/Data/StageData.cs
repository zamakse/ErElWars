using UnityEngine;

/// <summary>
/// 스테이지 데이터를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "ZALIWA/Data/StageData")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public int stageId;          // 스테이지 번호
    public string stageName;     // 스테이지 이름
    public Sprite stageImage;    // 스테이지 배경 이미지

    [Header("적 웨이브 설정")]
    public WaveData[] waves;     // 웨이브 데이터 배열

    [Header("보상")]
    public int goldReward;       // 클리어 골드 보상
    public int expReward;        // 클리어 경험치 보상
}

/// <summary>
/// 웨이브 데이터 구조체
/// </summary>
[System.Serializable]
public class WaveData
{
    public string enemyUnitId;   // 소환할 적 유닛 ID
    public int spawnCount;       // 소환 수
    public float spawnInterval;  // 소환 간격 (초)
    public float startDelay;     // 웨이브 시작 전 대기 시간
}
