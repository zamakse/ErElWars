using UnityEngine;

/// <summary>
/// 투사체 생성을 담당하는 매니저.
/// Launch()를 호출하면 originPos에서 target을 향해 투사체가 발사된다.
/// projectilePrefab이 없으면 런타임에 기본 노란 점 오브젝트를 생성한다.
/// </summary>
public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("투사체 프리팹 (없으면 기본 노란 점 사용)")]
    public GameObject projectilePrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 투사체를 발사한다.
    /// originPos에서 생성되어 target을 향해 이동, 도달 시 damage를 가한다.
    /// </summary>
    public void Launch(Vector3 originPos, ITargetable target, float damage)
    {
        if (target == null || !target.IsAlive) return;

        GameObject go;
        if (projectilePrefab != null)
        {
            go = Instantiate(projectilePrefab, originPos, Quaternion.identity);
            Projectile proj = go.GetComponent<Projectile>();
            if (proj == null) proj = go.AddComponent<Projectile>();
            if (proj == null) { Debug.LogError("[ProjectileManager] Projectile 컴포넌트를 가져올 수 없습니다."); Destroy(go); return; }
            proj.Init(target, damage);
        }
        else
        {
            go = CreateDefaultProjectile(originPos);
            Projectile proj = go.GetComponent<Projectile>();
            if (proj == null) { Debug.LogError("[ProjectileManager] Projectile 컴포넌트를 가져올 수 없습니다."); Destroy(go); return; }
            proj.Init(target, damage);
        }
    }

    /// <summary>projectilePrefab 미설정 시 노란 점 투사체 GameObject를 직접 생성해 반환한다.</summary>
    private GameObject CreateDefaultProjectile(Vector3 pos)
    {
        GameObject go = new GameObject("Projectile_Default");
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

        // Texture2D로 단색 스프라이트 생성 (외부 에셋 불필요)
        var sr    = go.AddComponent<SpriteRenderer>();
        var tex   = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
        sr.color  = Color.yellow;

        go.AddComponent<Projectile>();
        return go;
    }
}
