using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private Camera cam;
    public Transform unit;
    public Vector3 offset;
    public TMP_Text nameText;

    void Start()
    {

        cam = Camera.main;

        if (unit == null)
            unit = transform.parent;
    }

    void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;
        if (cam == null) return;

        // Look at camera
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        // Follow player
        transform.position = unit.position + offset;
    }
}
