using UnityEngine;

public class CameraFollowSimple : MonoBehaviour
{
    public Transform target;     // asignar el transform del jugador en el inspector
    public Vector3 offset = new Vector3(0f, 0f, -10f); // separación cámara-jugador

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}
