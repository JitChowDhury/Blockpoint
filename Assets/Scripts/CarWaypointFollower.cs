using UnityEngine;
using Photon.Pun;

public class CarWaypointFollower : MonoBehaviourPun
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float turnSpeed = 5f;
    public float reachDistance = 3f;

    private int currentIndex = 0;

    void Update()
    {
        // Only MASTER moves the car
        if (!PhotonNetwork.IsMasterClient) return;

        if (waypoints.Length == 0) return;

        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Rotate toward waypoint
        Vector3 targetPos = waypoints[currentIndex].position;
        Vector3 dir = (targetPos - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);

        // Reached waypoint?
        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist < reachDistance)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Length)
                currentIndex = 0; // loop
        }
    }
}
