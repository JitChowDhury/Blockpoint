using UnityEngine;
using Photon.Pun;

public class GunFPS : MonoBehaviourPun
{
    [Header("Gun Settings")]
    public bool isAutomatic = false;
    public int damage = 20;
    public float range = 100f;
    public float fireRate = 0.1f;

    [Header("Animations")]
    public Animator gunAnimator;
    public string shootAnim = "Shoot";
    public string reloadAnim = "Reload";
    public string idleAnim = "Idle";

    [Header("Ammo")]
    public int magazineSize = 30;
    public int currentAmmo;
    public int reserveAmmo = 90;
    public float reloadTime = 1.3f;

    [Header("Effects")]
    public GameObject muzzleFlash;         // GameObject flash
    public float muzzleFlashTime = 0.05f;  // Duration flash is ON
    public GameObject worldHitEffect;
    public GameObject playerHitEffect;

    private float nextShotTime;
    private bool isReloading = false;
    private float muzzleTimer;

    void Start()
    {
        gunAnimator.Play(idleAnim, 0, 0f);
        currentAmmo = magazineSize;
        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    void Update()
    {
        // Turn off muzzle flash after a short duration
        if (muzzleFlash != null && muzzleFlash.activeSelf)
        {
            muzzleTimer -= Time.deltaTime;
            if (muzzleTimer <= 0)
                muzzleFlash.SetActive(false);
        }
    }

    // --------------------------------------------------------------------
    // Called by PlayerController when shooting
    // --------------------------------------------------------------------
    public void TryShoot(Camera cam, GameObject bulletImpact, GameObject playerImpact)
    {
        if (!photonView.IsMine) return;
        if (isReloading) return;
        if (Time.time < nextShotTime) return;

        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        Shoot(cam, bulletImpact, playerImpact);
    }

    // --------------------------------------------------------------------
    // Shooting Logic
    // --------------------------------------------------------------------
    void Shoot(Camera cam, GameObject bulletImpact, GameObject playerImpact)
    {
        nextShotTime = Time.time + fireRate;
        currentAmmo--;

        // Muzzle Flash
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            muzzleTimer = muzzleFlashTime;
        }

        if (gunAnimator != null)
            gunAnimator.Play(shootAnim, 0, 0f);

        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                PhotonNetwork.Instantiate(playerImpact.name, hit.point, Quaternion.identity);

                hit.collider.GetComponent<PhotonView>().RPC(
                    "DealDamage",
                    RpcTarget.All,
                    photonView.Owner.NickName,
                    damage
                );
            }
            else
            {
                Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }


    public void Reload()
    {
        if (isReloading) return;
        if (reserveAmmo <= 0) return;
        if (currentAmmo == magazineSize) return;

        isReloading = true;
        if (gunAnimator != null)
            gunAnimator.Play(reloadAnim, 0, 0f);

        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        int needed = magazineSize - currentAmmo;
        int loadAmount = Mathf.Min(needed, reserveAmmo);

        currentAmmo += loadAmount;
        reserveAmmo -= loadAmount;
        isReloading = false;
    }
}
