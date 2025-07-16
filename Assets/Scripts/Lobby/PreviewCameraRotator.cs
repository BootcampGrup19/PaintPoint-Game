using UnityEngine;

public class PreviewCameraRotator : MonoBehaviour
{
    public Transform target;
    public float speed = 20f;
    void LateUpdate()
    {
        if (!target) return;
        transform.LookAt(target);
        target.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
    }
}
