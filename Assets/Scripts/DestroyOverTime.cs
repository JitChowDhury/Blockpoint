using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1.5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
