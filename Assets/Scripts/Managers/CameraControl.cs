using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    public static CameraControl Instance;

    public Transform playerTarget;
    public CinemachineCamera cineCam;
    public Transform focusTarget;

    bool isFocusing;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetCameraTarget(playerTarget);
    }

    public void SetCameraTarget(Transform newTarget)
    {
        if (cineCam == null) return;

        cineCam.Follow = newTarget;
        cineCam.LookAt = newTarget;
    }

    public void ShowFocusView()
    {
        if (isFocusing) return;

        isFocusing = true;
        PlayerController.Instance.SetBlocked(true);

        SetCameraTarget(focusTarget);
        StartCoroutine(ReturnToPlayerAfterDelay());
    }

    private IEnumerator ReturnToPlayerAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        SetCameraTarget(playerTarget);
        PlayerController.Instance.SetBlocked(false);

        isFocusing = false;
    }
}
