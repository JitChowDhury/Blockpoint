using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float moveSpeed = 5f, runSpeed = 8f;
    [SerializeField] private CharacterController charCon;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("Gun Holders")]
    [SerializeField] private Transform gunHolder;
    [SerializeField] private Transform modelGunPoint;
    [SerializeField] private GunFPS[] fpsGuns;
    [SerializeField] private GameObject[] tpsGuns;
    [SerializeField] private GunFPS[] allGuns;

    [Header("Effects")]
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private GameObject playerHitImpact;

    [Header("Player Stats")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityMod = 2.5f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject playerModel;
    public Animator anim;

    private int selectedGun;
    private int currentHealth;
    private Camera cam;

    private Vector2 mouseInput;
    private float verticalRotStore;
    private Vector3 movement, moveDir;
    private float activeMoveSpeed;

    private bool isSwapping = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        currentHealth = maxHealth;
        UIController.Instance.healthSlider.maxValue = maxHealth;
        UIController.Instance.healthSlider.value = currentHealth;

        if (photonView.IsMine)
        {
            //if local player then disable the model
            playerModel.SetActive(false);
        }
        else
        {
            gunHolder.SetParent(modelGunPoint);
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        // sync starting gun
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleLook();
        HandleMovement();
        HandleJump();
        HandleGunInput();

        HandleWeaponSwapInput();

        anim.SetFloat("speed", moveDir.magnitude);
        anim.SetBool("grounded", IsGrounded());
    }

    void LateUpdate()
    {
        //so that only local player is being effected
        if (!photonView.IsMine) return;

        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    // ------------------- LOOK --------------------
    void HandleLook()
    {
        //reads raw mouse input
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        //rotates the player around Y axis
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + mouseInput.x, 0f);
        //vertical Rotation For Up and Down
        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);
        //apply vertical rotatioon
        viewPoint.rotation = Quaternion.Euler(-verticalRotStore, transform.rotation.eulerAngles.y, 0f);
    }

    // ------------------- MOVEMENT --------------------
    void HandleMovement()
    {
        //gets raw movement Direction
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        //toggle between runspeed and movespeed
        activeMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        //save vertical velocity
        float yVel = movement.y;
        //converts movement into world direction
        movement = (transform.forward * moveDir.z + transform.right * moveDir.x).normalized * activeMoveSpeed;
        movement.y = yVel;
        //apply gravity
        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        //fix anti sticking
        if (IsGrounded() && movement.y < 0)
            movement.y = -2f;
        //apply movement to charcontroller
        charCon.Move(movement * Time.deltaTime);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayer);
    }

    // ------------------- JUMP --------------------
    void HandleJump()
    {
        if (IsGrounded() && Input.GetButtonDown("Jump"))
        {
            movement.y = jumpForce;
        }
    }

    // ------------------- SHOOT + RELOAD --------------------
    void HandleGunInput()
    {
        if (isSwapping) return; //cannot shoot while swapping

        GunFPS gun = fpsGuns[selectedGun];

        if (Input.GetMouseButton(0))
        {
            gun.TryShoot(cam, bulletImpact, playerHitImpact);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gun.Reload();
        }

        UIController.Instance.ammoText.text = gun.currentAmmo + " / " + gun.reserveAmmo;
    }

    // ------------------- WEAPON SWAP INPUT --------------------
    void HandleWeaponSwapInput()
    {
        if (isSwapping) return;

        // scroll wheel up
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            int next = (selectedGun + 1) % fpsGuns.Length;
            StartCoroutine(SwapWeapon(next));
        }

        // scroll wheel down
        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            int next = selectedGun - 1;
            if (next < 0) next = fpsGuns.Length - 1;

            StartCoroutine(SwapWeapon(next));
        }

        // number keys
        for (int i = 0; i < fpsGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()) && i != selectedGun)
            {
                StartCoroutine(SwapWeapon(i));
            }
        }
    }

    // ------------------- WEAPON SWAP ROUTINE --------------------
    private IEnumerator SwapWeapon(int newGun)
    {
        isSwapping = true;

        // Play SwapOut animation on current gun
        GunFPS oldGun = fpsGuns[selectedGun];
        float outTime = oldGun.swapOutTime;

        if (oldGun.gunAnimator != null)
            oldGun.gunAnimator.Play(oldGun.swapOutAnim, 0, 0f);

        yield return new WaitForSeconds(outTime);

        // Switch gun via RPC
        selectedGun = newGun;
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);

        GunFPS newGunScript = fpsGuns[selectedGun];
        float inTime = newGunScript.swapInTime;

        if (newGunScript.gunAnimator != null)
            newGunScript.gunAnimator.Play(newGunScript.swapInAnim, 0, 0f);

        yield return new WaitForSeconds(inTime);

        isSwapping = false;
    }

    // ------------------- GUN VISIBILITY SWITCH --------------------
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

    // ------------------- HEALTH --------------------
    [PunRPC]
    public void DealDamage(string damager, int damage)
    {
        if (!photonView.IsMine) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UIController.Instance.healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            PlayerSpawner.Instance.Die(damager);
        }
    }


    [PunRPC]
    public void SetGun(int gunToUse)
    {
        selectedGun = gunToUse;
        SwitchGun();
    }
}
