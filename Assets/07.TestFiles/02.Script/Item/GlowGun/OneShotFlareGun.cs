using UnityEngine;

public class OneShotFlareGun : MonoBehaviour
{
    [Header("Projectile")]
    public Rigidbody flareProjectile;
    public Transform muzzle;
    public float shootForce = 35f;
    public ForceMode forceMode = ForceMode.Impulse;

    [Header("FX")]
    public GameObject muzzleParticles;
    public AudioSource audioSource;
    public AudioClip shotSound;
    public Animation anim; // 레거시 애니메이션이면 사용, 없으면 null 가능

    bool _used;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (anim == null) anim = GetComponent<Animation>();
    }

    public void FireOnce()
    {
        if (_used) return;
        _used = true;

        if (anim != null) anim.CrossFade("Shoot");
        if (audioSource != null && shotSound != null) audioSource.PlayOneShot(shotSound);

        Transform origin = muzzle != null ? muzzle : transform;
        Vector3 pos = origin.position;
        Quaternion rot = origin.rotation;

        if (flareProjectile != null)
        {
            Rigidbody b = Instantiate(flareProjectile, pos, rot);
            b.AddForce(origin.forward * shootForce, forceMode);
        }

        if (muzzleParticles != null)
            Instantiate(muzzleParticles, pos, rot);
    }
}
