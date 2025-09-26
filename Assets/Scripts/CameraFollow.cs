using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                   
    public Vector3 offset = new Vector3(4f, 1.5f, -10f);
    public float followSpeed = 5f;
    private float maxX;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = target.position + offset;

        
        if (desired.x < maxX) desired.x = maxX;
        maxX = Mathf.Max(maxX, desired.x);

        Vector3 smoothed = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        transform.position = new Vector3(smoothed.x, smoothed.y, offset.z);
    }
}