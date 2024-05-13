using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;

public class AIBrushController : MonoBehaviour
{
    public static event Action<string> OnSpinEvent;
    public GameObject _brush;
    [SerializeField] private GameObject _hub1;
    [SerializeField] private GameObject _hub2;
    [SerializeField] private GameObject _cylinder;
    [SerializeField] private float _speed = 200f;
    [SerializeField] private float _distance = 4f;
    [SerializeField] private Vector3 _startPosition;
    [SerializeField] private ParticleSystem _particleSystem;

    private IngameManager _ingameManager;
    private Transform _brushTransform;
    private Transform _hub1Transform;
    private Transform _hub2Transform;
    private Transform _cylinderTransform;
    private Vector3 _cylinderScale;
    MaterialPropertyBlock propBlock;

    private bool _isHub1 = true;
    private bool _isClockwise = true;
    private float _timePassed = 0f;
    private bool _isSpinning = false;
    [SerializeField] private float _timeToSpin = 1f;

    void Start()
    {
        _ingameManager = GameObject.Find("IngameManager").GetComponent<IngameManager>();
        _hub1Transform = _hub1.GetComponent<Transform>();
        _hub2Transform = _hub2.GetComponent<Transform>();
        _brushTransform = _brush.GetComponent<Transform>();
        _cylinderTransform = _cylinder.GetComponent<Transform>();
        _cylinderScale = _cylinderTransform.localScale;
        propBlock = new MaterialPropertyBlock();
        propBlock.SetColor("_Color", _ingameManager.Uncolored);
        // Set color of the brush by get components in children
        for (int i = 0; i < _brushTransform.childCount; i++)
        {
            _brushTransform.GetChild(i).GetComponent<Renderer>().SetPropertyBlock(propBlock);
        }
    }

    private void Update()
    {
        _timePassed += Time.deltaTime;
        CalculateWhenToSpin();
        CylinderCalculate();
        Spin();
    }

    public void SetIngameManager(IngameManager ingameManager)
    {
        _ingameManager = ingameManager;
    }

    public void OnSpin()
    {
        SwitchHub();
        OnSpinEvent?.Invoke(_isHub1 ? "Hub1" : "Hub2");
    }

    [Button]
    public void ShowBrush()
    {
        _brushTransform.position = _startPosition;
        _hub1Transform.localPosition = Vector3.zero;
        _isClockwise = true;
        _isHub1 = true;
        LeanTween.move(_brush, new Vector3(_startPosition.x, 1f, _startPosition.z), 1.5f).setEaseOutElastic().setDelay(0.5f).setOnComplete(() =>
        {
            ChangeDistanceOnComplete(4f, () =>
            {
                _timePassed = 0f;
                _isSpinning = false;
            });
        });
    }

    [Button]
    public void HideBrush()
    {
        _hub1Transform.parent = _brushTransform;
        _hub2Transform.parent = _brushTransform;
        _isSpinning = true;
        ChangeDistanceOnComplete(0.01f, () =>
        {
            LeanTween.move(_brush, new Vector3(_startPosition.x, -3.5f, _startPosition.z), 0.75f).setEaseInOutBack().setOnComplete(() =>
            {
                _brushTransform.position = new Vector3(0, -3.5f, 0);
                _hub1Transform.localPosition = Vector3.zero;
            });
        });
    }

    private void CalculateWhenToSpin()
    {
        if (!_ingameManager.IsGameStarted) return;
        if (_isSpinning) return;
        Vector3[] rubberPositions = new Vector3[_ingameManager.Rubbers.Count];
        rubberPositions = _ingameManager.Rubbers.ConvertAll(rubber => rubber.transform.position).ToArray();

        // Remove positions that color is Ingamemanager.Uncolored
        for (int i = 0; i < rubberPositions.Length; i++)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            Renderer renderer = _ingameManager.Rubbers[i].GetComponent<Renderer>();
            renderer.GetPropertyBlock(propertyBlock);
            Color color = propertyBlock.GetColor("_Color");

            if (color == _ingameManager.Uncolored)
            {
                rubberPositions[i] = Vector3.zero;
            }
        }

        // Get the position of the current spinning hub
        Vector3 currentHubPosition = _isHub1 ? _hub1Transform.position : _hub2Transform.position;

        float nearestDistanceSqr = float.MaxValue;
        Vector3 nearestRubber = Vector3.zero;

        // Find the nearest tile that color is Ingamemanager.Colored
        MaterialPropertyBlock coloredPropertyBlock = new MaterialPropertyBlock();
        coloredPropertyBlock.SetColor("_Color", _ingameManager.Colored);

        // Create a dictionary to map rubber positions to their corresponding renderers
        Dictionary<Vector3, Renderer> rubberRenderers = new Dictionary<Vector3, Renderer>();
        foreach (var rubber in _ingameManager.Rubbers)
        {
            rubberRenderers[rubber.transform.position] = rubber.GetComponent<Renderer>();
        }

        foreach (Vector3 rubberPosition in rubberPositions)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            Renderer renderer = rubberRenderers[rubberPosition];
            renderer.GetPropertyBlock(propertyBlock);

            if (propertyBlock.GetColor("_Color") == coloredPropertyBlock.GetColor("_Color")) continue;
            float distanceSqr = (currentHubPosition - rubberPosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestRubber = rubberPosition;
            }
        }

        StartCoroutine(SpinCoroutine(!_isHub1 ? _hub1Transform : _hub2Transform, nearestRubber));
    }

    IEnumerator SpinCoroutine(Transform spinningHub, Vector3 nearestRubber)
    {
        _isSpinning = true;
        float nearestDistance = float.MaxValue;
        float distance = Vector3.Distance(spinningHub.position, nearestRubber);

        for (int i = 0; i < _timeToSpin / 0.1f; i++)
        {
            if (distance < nearestDistance)
                nearestDistance = distance;
            yield return new WaitForSeconds(0.1f);
        }

        while (true)
        {
            double tolerance = 0.2;
            if (Math.Abs(distance - nearestDistance) < tolerance)
            {
                OnSpin();
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        _isSpinning = false;
        _timePassed = 0f;
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

    [Button]
    public void ChangeDistanceOnShow(float distance)
    {
        LeanTween.value(gameObject, _distance, distance, 0.15f).setOnUpdate((float value) =>
        {
            _distance = value;
        });
    }

    public void ChangeDistanceOnComplete(float distance, Action onComplete, float delay = 0.2f)
    {
        LeanTween.value(gameObject, _distance, distance, 0.15f).setOnUpdate((float value) =>
        {
            _distance = value;
        });
        LeanTween.delayedCall(delay, onComplete);
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

    public void Respawn()
    {
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        HideBrush();
        // Stop counting time when respawning
        StopCoroutine(nameof(SpinCoroutine));
        yield return new WaitForSeconds(1.1f);
        ShowBrush();
    }
}
