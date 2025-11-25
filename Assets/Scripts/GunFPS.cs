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
    public string idleAnim = "Idle";
    public string reloadAnim = "Reload";
    public string swapOutAnim = "SwapOut";
    public string swapInAnim = "SwapIn";

    [Header("Weapon Swap Settings")]
    public float swapOutTime = 0.20f;
    public float swapInTime = 0.30f;

    [Header("ADS Settings")]
    public float adsZoom;
    public Transform adsInPoint;
    public float adsSpeed = 10f;
    public bool isSniper = false;
    public float finalScopeFOV = 12f;

    // public bool isScopedWeapon = false;  // sniper = true
    // [HideInInspector] public bool isADS = false;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int currentAmmo;
    public int reserveAmmo = 90;
    public float reloadTime = 1.3f;

    [Header("Effects")]
    public GameObject muzzleFlash;
    public float muzzleFlashTime = 0.05f;
    public GameObject worldHitEffect;
    public GameObject playerHitEffect;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private float nextShotTime;
    private bool isReloading = false;
    public float muzzleTimer;

    private Vector3 targetPos;
    private Quaternion targetRot;

    void Start()
    {
        currentAmmo = magazineSize;

        if (gunAnimator != null)
            gunAnimator.Play(idleAnim, 0, 0f);

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);

        // if (hipPosition == null || adsPosition == null)
        //     Debug.LogError(name + ": HipPosition or ADSPosition is not assigned!");
    }

    void Update()
    {
        HandleMuzzleFlash();
        // HandleADS();
    }

    // ----------------------------
    // MUZZLE FLASH TIMER
    // ----------------------------
    void HandleMuzzleFlash()
    {
        if (muzzleFlash != null && muzzleFlash.activeSelf)
        {
            muzzleTimer -= Time.deltaTime;
            if (muzzleTimer <= 0)
                muzzleFlash.SetActive(false);
        }
    }

    // ----------------------------
    // ADS SMOOTH MOVEMENT
    // ----------------------------
    // void HandleADS()
    // {
    //     if (isADS)
    //     {
    //         // move toward ADS
    //         transform.localPosition = Vector3.Lerp(
    //             transform.localPosition,
    //             adsPosition.localPosition,
    //             Time.deltaTime * adsSpeed
    //         );

    //         transform.localRotation = Quaternion.Lerp(
    //             transform.localRotation,
    //             adsPosition.localRotation,
    //             Time.deltaTime * adsSpeed
    //         );
    //     }
    //     else
    //     {
    //         // move toward Hip
    //         transform.localPosition = Vector3.Lerp(
    //             transform.localPosition,
    //             hipPosition.localPosition,
    //             Time.deltaTime * adsSpeed
    //         );

    //         transform.localRotation = Quaternion.Lerp(
    //             transform.localRotation,
    //             hipPosition.localRotation,
    //             Time.deltaTime * adsSpeed
    //         );
    //     }
    // }

    // ----------------------------
    // TRY SHOOT
    // ----------------------------
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

    // ----------------------------
    // SHOOT LOGIC
    // ----------------------------
    void Shoot(Camera cam, GameObject bulletImpact, GameObject playerImpact)
    {
        nextShotTime = Time.time + fireRate;
        currentAmmo--;
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);
        // Flash
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            muzzleTimer = muzzleFlashTime;
        }

        if (gunAnimator != null)
            gunAnimator.Play(shootAnim, 0, 0f);

        // Raycast
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
                    damage,
                    PhotonNetwork.LocalPlayer.ActorNumber
                );
            }
            else
            {
                Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

    // ----------------------------
    // RELOAD
    // ----------------------------
    public void Reload()
    {
        if (isReloading) return;
        if (reserveAmmo <= 0) return;
        if (currentAmmo == magazineSize) return;

        isReloading = true;
        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

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
        if (audioSource != null)
            audioSource.Stop();
        isReloading = false;
    }
}
