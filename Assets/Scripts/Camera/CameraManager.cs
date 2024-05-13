using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform _hub1Transform;
    [SerializeField] private Transform _hub2Transform;
    private CinemachineVirtualCamera _virtualCamera;

    void Awake()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void SwitchCamera(string hubName)
    {
        if (hubName == "Hub1")
        {
            _virtualCamera.Follow = _hub1Transform;
            _virtualCamera.LookAt = _hub1Transform;
        }
        else if (hubName == "Hub2")
        {
            _virtualCamera.Follow = _hub2Transform;
            _virtualCamera.LookAt = _hub2Transform;
        }
    }
}
