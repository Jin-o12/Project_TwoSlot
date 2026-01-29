using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public CeilingEnemyCTRL ceilingEnemyCTRL;
    public int damage = 100;
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime); // 시간 지나면 자동 삭제
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ceilingEnemyCTRL.TryFire();
            Destroy(gameObject); // 맞으면 총알 삭제
        }
    }
    
}
