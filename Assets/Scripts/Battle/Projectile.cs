using UnityEngine;

/// <summary>
/// 원거리 유닛의 투사체.
/// 발사 시 목표 ITargetable을 향해 이동하고, 도달하면 데미지를 가한다.
/// ProjectileManager.Launch()에서 생성되며, 충돌 또는 목표 사망 시 자동 파괴된다.
/// </summary>
public class Projectile : MonoBehaviour
{
    private const float Speed         = 8f;   // 투사체 이동 속도
    private const float MaxLifetime   = 5f;   // 최대 생존 시간 (초)
    private const float ArrivalRadius = 0.3f; // 목표 도달 판정 반경

    private ITargetable target;
    private float       damage;
    private float       lifetime;

    /// <summary>발사 초기화. ProjectileManager.Launch()에서 호출.</summary>
    public void Init(ITargetable target, float damage)
    {
        this.target  = target;
        this.damage  = damage;
        this.lifetime = 0f;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        // 수명 초과 시 자동 파괴
        if (lifetime >= MaxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 목표가 죽었거나 사라졌으면 파괴
        if (target == null || !target.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        // 목표 방향으로 이동 (GetTransform 결과를 캐싱해 null 안전 보장)
        Transform targetTransform = target.GetTransform();
        if (targetTransform == null) { Destroy(gameObject); return; }

        Vector3 dir = (targetTransform.position - transform.position).normalized;
        transform.position += dir * Speed * Time.deltaTime;

        // 목표 도달 판정
        float dist = Vector3.Distance(transform.position, targetTransform.position);
        if (dist <= ArrivalRadius)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
