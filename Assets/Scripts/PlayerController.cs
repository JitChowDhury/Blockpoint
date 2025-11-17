using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("View / Camera")]
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float mouseSensitivity = 1f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private CharacterController charCon;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityMod = 2.5f;

    [Header("Effects")]
    public GameObject bulletImpact;
    public GameObject playerHitImpact;

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject playerModel;
    private int currentHealth;

    [Header("Weapons")]
    [SerializeField] private Transform gunHolder;      // FPS for local
    [SerializeField] private Transform modelGunPoint;  // TPS for remote players

    [SerializeField] private GunFPS[] fpsGuns;         // Local FPS guns
    [SerializeField] private GameObject[] tpsGuns;     // Remote TPS guns
    [SerializeField] private GunFPS[] allGuns;         // For reference  
    private int selectedGun = 0;

    public Animator anim;

    private Camera cam;
    private float activeMoveSpeed;
    private float verticalRotStore;
    private Vector3 movement, moveDir;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        currentHealth = maxHealth;
        UIController.Instance.healthSlider.maxValue = maxHealth;

        if (photonView.IsMine)
        {
            playerModel.SetActive(false);     // Local uses FPS gun
            UIController.Instance.healthSlider.value = currentHealth;
        }
        else
        {
            // Remote uses TPS gun
            gunHolder.SetParent(modelGunPoint);
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
    }


    void Update()
    {
        if (!photonView.IsMine) return;

        HandleLook();
        HandleMovement();
        HandleJump();
        HandleGunInput();
        HandleGunSwitching();

        anim.SetFloat("speed", moveDir.magnitude);
        anim.SetBool("grounded", IsGrounded());
    }


    private void LateUpdate()
    {
        if (!photonView.IsMine) return;

        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    // ------------------ LOOK ----------------------

    void HandleLook()
    {
        Vector2 mouseInput = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        ) * mouseSensitivity;

        transform.rotation = Quaternion.Euler(0f,
            transform.rotation.eulerAngles.y + mouseInput.x,
            0f);

        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        viewPoint.rotation =
            Quaternion.Euler(-verticalRotStore, transform.rotation.eulerAngles.y, 0f);
    }

    // ------------------ MOVEMENT ----------------------

    void HandleMovement()
    {
        moveDir = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        activeMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        float yVel = movement.y;
        movement = (transform.forward * moveDir.z +
                    transform.right * moveDir.x).normalized * activeMoveSpeed;
        movement.y = yVel;

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

        if (IsGrounded() && movement.y < 0)
            movement.y = -2f;

        charCon.Move(movement * Time.deltaTime);
    }

    void HandleJump()
    {
        if (IsGrounded() && Input.GetButtonDown("Jump"))
            movement.y = jumpForce;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayer);
    }

    // ------------------ GUN SYSTEM ----------------------

    void HandleGunInput()
    {
        GunFPS gun = fpsGuns[selectedGun];

        if (Input.GetMouseButton(0))
        {
            gun.TryShoot(cam, bulletImpact, playerHitImpact);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gun.Reload();
        }

        UIController.Instance.ammoText.text =
            gun.currentAmmo + " / " + gun.reserveAmmo;
    }

    void HandleGunSwitching()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun++;
            if (selectedGun >= fpsGuns.Length) selectedGun = 0;
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;
            if (selectedGun < 0) selectedGun = fpsGuns.Length - 1;
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }

        for (int i = 0; i < fpsGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }
        }
    }

    void SwitchGun()
    {
        foreach (var g in fpsGuns) g.gameObject.SetActive(false);
        foreach (var g in tpsGuns) g.SetActive(false);

        if (photonView.IsMine)
        {
            fpsGuns[selectedGun].gameObject.SetActive(true);
        }
        else
        {
            tpsGuns[selectedGun].SetActive(true);
        }
    }

    // ------------------ DAMAGE / HEALTH ----------------------

    [PunRPC]
    public void DealDamage(string damager, int damageAmount)
    {
        if (!photonView.IsMine) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UIController.Instance.healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            PlayerSpawner.Instance.Die(damager);
        }
    }

    // ------------------ RPC SWITCH GUN ----------------------

    [PunRPC]
    public void SetGun(int gunToUse)
    {
        selectedGun = gunToUse;
        SwitchGun();
    }
}
