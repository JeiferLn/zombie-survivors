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
        StartCoroutine(ReturnToPlayerWhenCentered());
    }

    private IEnumerator ReturnToPlayerWhenCentered()
    {
        yield return new WaitForSeconds(2f);
        
        SetCameraTarget(playerTarget);
        
        float threshold = 0.1f;
        while (cineCam != null && playerTarget != null)
        {
            Vector3 cameraPos = cineCam.State.RawPosition;
            Vector3 targetPos = playerTarget.position;
            
            float distance = Vector2.Distance(
                new Vector2(cameraPos.x, cameraPos.y),
                new Vector2(targetPos.x, targetPos.y)
            );
            
            if (distance <= threshold)
            {
                break;
            }
            
            yield return null;
        }

        PlayerController.Instance.SetBlocked(false);
        isFocusing = false;
    }
}
