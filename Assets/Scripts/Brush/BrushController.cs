using System;
using System.Collections;
using EasyButtons;
using UnityEngine;

public class BrushController : MonoBehaviour
{
    public static event Action<string> OnTouchInputEvent;
    public GameObject _brush;
    [SerializeField] private GameObject _hub1;
    [SerializeField] private GameObject _hub2;
    [SerializeField] private GameObject _cylinder;
    [SerializeField] private float _speed = 200f;
    [SerializeField] private float _distance = 4f;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private CameraManager _cameraManager;

    private Transform _brushTransform;
    private Transform _hub1Transform;
    private Transform _hub2Transform;
    private Transform _cylinderTransform;
    private Vector3 _cylinderScale;

    private bool _canSpin = false;
    private bool _isHub1 = true;
    private bool _isClockwise = true;

    public bool IsFirstHit { get; private set; } = true;
    public bool IsStartCalculateCombo { get; private set; } = false;
    public int CurrentHit { get; private set; }
    public int PreviousHit { get; private set; }

    void Start()
    {
        TouchInput.OnTouchInputEvent += OnTouchInput;
        _hub1Transform = _hub1.GetComponent<Transform>();
        _hub2Transform = _hub2.GetComponent<Transform>();
        _brushTransform = _brush.GetComponent<Transform>();
        _cylinderTransform = _cylinder.GetComponent<Transform>();
        _cylinderScale = _cylinderTransform.localScale;
        if (_cameraManager == null)
        {
            _cameraManager = FindObjectOfType<CameraManager>().GetComponent<CameraManager>();
        }
    }

    void OnDestroy()
    {
        TouchInput.OnTouchInputEvent -= OnTouchInput;
    }

    private void Update()
    {
        CylinderCalculate();
        Spin();
    }

    public void OnTouchInput()
    {
        if (!_canSpin) return;
        SwitchHub();
        _cameraManager.SwitchCamera(_isHub1 ? "Hub1" : "Hub2");
        OnTouchInputEvent?.Invoke(_isHub1 ? "Hub1" : "Hub2");
    }

    [Button]
    public void ShowBrush()
    {
        _brushTransform.position = new Vector3(0, -3.5f, 0);
        _hub1Transform.localPosition = Vector3.zero;
        _isClockwise = true;
        _isHub1 = true;
        _cameraManager.SwitchCamera(_isHub1 ? "Hub1" : "Hub2");
        LeanTween.move(_brush, new Vector3(0, 1f, 0), 1.5f).setEaseOutElastic().setDelay(0.5f).setOnComplete(() =>
        {
            ChangeDistanceOnShow(4f);
        });
    }

    [Button]
    public void HideBrush()
    {
        _canSpin = false;
        _hub1Transform.parent = _brushTransform;
        _hub2Transform.parent = _brushTransform;
        ChangeDistanceOnComplete(0.01f, () =>
        {
            LeanTween.move(_brush, new Vector3(0, -3.5f, 0), 0.75f).setEaseInOutBack().setOnComplete(() =>
            {
                _brushTransform.position = new Vector3(0, -3.5f, 0);
                _hub1Transform.localPosition = Vector3.zero;
            });
        });
    }

    private void Spin()
    {
        if (_isHub1)
        {
            _hub2Transform.position = _hub1Transform.position + (_hub2Transform.position - _hub1Transform.position).normalized * _distance;
            _hub2Transform.RotateAround(_hub1Transform.position, Vector3.up * (_isClockwise ? 1 : -1), _speed * Time.deltaTime);
        }
        else
        {
            _hub1Transform.position = _hub2Transform.position + (_hub1Transform.position - _hub2Transform.position).normalized * _distance;
            _hub1Transform.RotateAround(_hub2Transform.position, Vector3.up * (_isClockwise ? 1 : -1), _speed * Time.deltaTime);
        }
    }

    private void CylinderCalculate()
    {
        float distance = Vector3.Distance(_hub1Transform.position, _hub2Transform.position);

        _cylinderTransform.localScale = new Vector3(_cylinderScale.x, distance * 0.5f, _cylinderScale.z);
        _cylinderTransform.position = (_hub1Transform.position + _hub2Transform.position) / 2;

        _cylinderTransform.LookAt(_hub2Transform);
        _cylinderTransform.Rotate(0, 90, 90);
    }

    public void SwitchHub()
    {
        ChangeHub();
        ChangeDirection();
    }

    private void ChangeHub()
    {
        _isHub1 = !_isHub1;
    }

    private void ChangeDirection()
    {
        _isClockwise = !_isClockwise;
    }

    public void Respawn()
    {
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        HideBrush();
        yield return new WaitForSeconds(1.1f);
        ShowBrush();
    }

    [Button]
    public void ChangeDistanceOnShow(float distance)
    {
        LeanTween.value(gameObject, _distance, distance, 0.15f).setOnUpdate((float value) =>
        {
            _distance = value;
        }).setOnComplete(() =>
        {
            _canSpin = true;
        });
    }

    public void ChangeDistanceOnComplete(float distance, Action onComplete)
    {
        LeanTween.value(gameObject, _distance, distance, 0.15f).setOnUpdate((float value) =>
        {
            _distance = value;
        });
        LeanTween.delayedCall(0.2f, onComplete);
    }

    public void SetParticleColor(Color color)
    {
        var main = _particleSystem.main;
        main.startColor = color;
    }

    public void ShowParticle(Vector3 position)
    {
        ParticleSystem particleSystem = Instantiate(_particleSystem, position, Quaternion.identity);
        particleSystem.Play();
        Destroy(particleSystem.gameObject, 1f);
    }

    public void SetIsFirstHit(bool isFirstHit)
    {
        IsFirstHit = isFirstHit;
    }

    public void SetIsStartCalculateCombo(bool isStartCalculateCombo)
    {
        IsStartCalculateCombo = isStartCalculateCombo;
    }

    public void SetCurrentHit(int hit)
    {
        CurrentHit = hit;
    }

    public void SetPreviousHit(int hit)
    {
        PreviousHit = hit;
    }

}
