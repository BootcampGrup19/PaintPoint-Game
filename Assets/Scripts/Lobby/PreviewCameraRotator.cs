using UnityEngine;

public class PreviewCameraRotator : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 50f;

    private float currentRotationDirection = 0f;
    public bool inCustomization = true;

    void Update()
    {
        if (target != null && currentRotationDirection != 0f)
        {
            transform.LookAt(target);
            target.Rotate(Vector3.up, rotationSpeed * currentRotationDirection * Time.deltaTime, Space.World);
        }
    }
    void LateUpdate()
    {
        if(inCustomization && target != null)
        {
            transform.LookAt(target);
            target.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    public void RotateRightCharacter()
    {
        currentRotationDirection = 1f;
    }
    public void RotateLeftCharacter()
    {
        currentRotationDirection = -1f;
    }
    public void StopRotate()
    {
        currentRotationDirection = 0f;
    }
}
