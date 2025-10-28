using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float speed;

    void Update()
    {
        transform.Rotate(0, Time.deltaTime * speed, 0);
    }
}