using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float moveSpeed = 5f, runSpeed = 8f;
    [SerializeField] private CharacterController charCon;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private GameObject playerHitImpact;

    [SerializeField] private float maxHeatValue = 10f, coolRate = 4f, overheatCoolRate = 5f;
    [SerializeField] private float muzzleDisplayTime;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject playerModel;

    [Header("Weapon Holders")]
    [SerializeField] private Transform gunHolder;      // FPS holder
    [SerializeField] private Transform modelGunPoint;  // TPS holder (Adjuster for remote)

    [Header("Guns")]
    [SerializeField] private Gun[] fpsGuns;           // FPS guns (for local player)
    [SerializeField] private GameObject[] tpsGuns;    // TPS guns (for remote players)
    [SerializeField] private Gun[] allGuns;           // For shooting logic (same as fps guns)

    public Animator anim;

    private int selectedGun;
    private int currentHealth;

    private float heatCounter;
    private bool isOverHeated;
    private float shotCounter;
    private float muzzleCounter;

    private float activeMoveSpeed;
    private float verticalRotStore;
    private Vector3 moveDir, movement;
    private Vector2 mouseInput;

    private Camera cam;
    private float jumpForce = 12f, gravityMod = 2.5f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        UIController.Instance.weaponTempSlider.maxValue = maxHeatValue;
        currentHealth = maxHealth;

        if (photonView.IsMine)
        {
            playerModel.SetActive(false);  // hide 3rd person model for local
            UIController.Instance.healthSlider.maxValue = maxHealth;
            UIController.Instance.healthSlider.value = currentHealth;
        }
        else
        {
            // Remote player → Attach FPS gunHolder to TPS model point
            gunHolder.SetParent(modelGunPoint);
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        // SwitchGun();
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Mouse Look
        mouseInput = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        ) * mouseSensitivity;

        transform.rotation = Quaternion.Euler(
            0f,
            transform.rotation.eulerAngles.y + mouseInput.x,
            0f
        );

        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        viewPoint.rotation = Quaternion.Euler(-verticalRotStore, transform.rotation.eulerAngles.y, 0f);

        // Movement
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        activeMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        float yVel = movement.y;
        movement = (transform.forward * moveDir.z + transform.right * moveDir.x).normalized * activeMoveSpeed;
        movement.y = yVel;

        bool isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayer);

        if (isGrounded && Input.GetButtonDown("Jump"))
            movement.y = jumpForce;

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        charCon.Move(movement * Time.deltaTime);

        // Muzzle Flash hide timer
        if (fpsGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0) fpsGuns[selectedGun].muzzleFlash.SetActive(false);
        }

        // Shooting Heat Logic
        if (!isOverHeated)
        {
            if (Input.GetMouseButtonDown(0)) Shoot();

            if (Input.GetMouseButton(0) && fpsGuns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0) Shoot();
            }

            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                heatCounter = 0;
                isOverHeated = false;
                UIController.Instance.overheatedMessage.gameObject.SetActive(false);
            }
        }

        heatCounter = Mathf.Clamp(heatCounter, 0f, maxHeatValue);
        UIController.Instance.weaponTempSlider.value = heatCounter;

        // Gun Switching
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun = (selectedGun + 1) % fpsGuns.Length;
            // SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;
            if (selectedGun < 0) selectedGun = fpsGuns.Length - 1;
            // SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }

        for (int i = 0; i < fpsGuns.Length; i++)
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                // SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }

        anim.SetBool("grounded", isGrounded);
        anim.SetFloat("speed", moveDir.magnitude);
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;

        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC(
                    "DealDamage",
                    RpcTarget.All,
                    photonView.Owner.NickName,
                    fpsGuns[selectedGun].shotDamage
                );
            }
            else
            {
                Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
            }
        }

        shotCounter = fpsGuns[selectedGun].timeBetweenShots;

        heatCounter += fpsGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeatValue)
        {
            heatCounter = maxHeatValue;
            isOverHeated = true;
            UIController.Instance.overheatedMessage.gameObject.SetActive(true);
        }

        fpsGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    void SwitchGun()
    {
        // Disable all FPS & TPS guns
        foreach (var g in fpsGuns) g.gameObject.SetActive(false);
        foreach (var g in tpsGuns) g.SetActive(false);

        // Local = FPS guns
        if (photonView.IsMine)
        {
            fpsGuns[selectedGun].gameObject.SetActive(true);
            fpsGuns[selectedGun].muzzleFlash.SetActive(false);
        }
        else
        {
            // Remote = TPS guns
            tpsGuns[selectedGun].SetActive(true);
        }
    }

    [PunRPC]
    public void DealDamage(string damager, int damageAmount)
    {
        if (!photonView.IsMine) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth == 0)
            PlayerSpawner.Instance.Die(damager);

        UIController.Instance.healthSlider.value = currentHealth;
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if (gunToSwitchTo < allGuns.Length)
        {
            selectedGun = gunToSwitchTo;
            SwitchGun();
        }
    }
}
