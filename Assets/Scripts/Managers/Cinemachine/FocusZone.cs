using UnityEngine;

public class FocusZone : MonoBehaviour
{
    public Transform focusPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CameraControl.Instance.focusTarget = focusPoint;
        CameraControl.Instance.ShowFocusView();
        Destroy(gameObject);
    }
}
