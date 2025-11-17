using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    public TextMeshProUGUI overheatedMessage;

    public GameObject deathScreen;
    public TMP_Text deathText;
    public TMP_Text ammoText;
    public Slider weaponTempSlider;
    public Slider healthSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
