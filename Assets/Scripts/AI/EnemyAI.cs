using System.Collections;
using UnityEngine;

/// <summary>
/// 적 유닛 자동 스폰 AI.
/// StageData가 연결되면 웨이브 순서대로 진행.
/// StageData가 없으면 fallbackUnitData를 fallbackInterval 간격으로 무한 스폰.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("적 소환기 (EnemyFaction UnitSpawner 연결)")]
    public UnitSpawner enemySpawner;

    [Header("스테이지 데이터 (없으면 폴백 모드 동작)")]
    public StageData stageData;

    [Header("폴백 모드 (StageData 미연결 시 사용)")]
    public UnitData  fallbackUnitData;
    public float     fallbackInterval = 4f;  // 폴백 스폰 간격(초)

    private void Start()
    {
        if (enemySpawner == null)
        {
            Debug.LogError("[EnemyAI] enemySpawner가 연결되지 않았습니다.");
            return;
        }

        if (stageData != null && stageData.waves != null && stageData.waves.Length > 0)
            StartCoroutine(RunWaves());
        else
            StartCoroutine(RunFallback());
    }

    // ─── 웨이브 모드 ──────────────────────────────────────────────────────────

    /// <summary>StageData 웨이브를 순서대로 실행한다.</summary>
    private IEnumerator RunWaves()
    {
        foreach (WaveData wave in stageData.waves)
        {
            if (wave.unitData == null)
            {
                Debug.LogWarning("[EnemyAI] WaveData.unitData가 null입니다. 이 웨이브를 건너뜁니다.");
                continue;
            }

            // 웨이브 시작 전 대기
            if (wave.startDelay > 0f)
                yield return new WaitForSeconds(wave.startDelay);

            // 지정 횟수만큼 스폰
            for (int i = 0; i < wave.spawnCount; i++)
            {
                enemySpawner.TrySpawnUnit(wave.unitData);

                if (i < wave.spawnCount - 1)
                    yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        // 모든 웨이브 완료 후 마지막 유닛 종류로 무한 스폰
        if (stageData.waves.Length > 0)
        {
            WaveData lastWave = stageData.waves[stageData.waves.Length - 1];
            if (lastWave.unitData != null)
                yield return RunInfinite(lastWave.unitData, lastWave.spawnInterval);
        }
    }

    // ─── 폴백 모드 ───────────────────────────────────────────────────────────

    /// <summary>StageData 없을 때 단순 무한 루프 스폰.</summary>
    private IEnumerator RunFallback()
    {
        if (fallbackUnitData == null)
        {
            Debug.LogWarning("[EnemyAI] fallbackUnitData가 null입니다. 스폰을 중단합니다.");
            yield break;
        }

        yield return RunInfinite(fallbackUnitData, fallbackInterval);
    }

    // ─── 공통 무한 스폰 루프 ─────────────────────────────────────────────────

    private IEnumerator RunInfinite(UnitData unitData, float interval)
    {
        while (true)
        {
            enemySpawner.TrySpawnUnit(unitData);
            yield return new WaitForSeconds(Mathf.Max(0.5f, interval));
        }
    }
}
