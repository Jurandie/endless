using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // O Player
    public Vector3 offset;     // Distância entre player e câmera
    public float smoothSpeed = 0.125f; // Suavização do movimento

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target); // Faz a câmera olhar para o Player
    }
}
