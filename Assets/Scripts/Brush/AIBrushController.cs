using System;
using System.Collections;
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
    [SerializeField] private float _nearestThreshold = 1.2f;
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
        if (_timePassed >= _timeToSpin)
        {
            _timePassed = 0f;
            CalculateWhenToSpin();
        }
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
            ChangeDistanceOnShow(4f);
        });
    }

    [Button]
    public void HideBrush()
    {
        _hub1Transform.parent = _brushTransform;
        _hub2Transform.parent = _brushTransform;
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
        Vector3[] rubberPositions = new Vector3[_ingameManager.Rubbers.Count];
        rubberPositions = _ingameManager.Rubbers.ConvertAll(rubber => rubber.transform.position).ToArray();

        // Remove positions that color is Ingamemanager.Uncolored
        for (int i = 0; i < rubberPositions.Length; i++)
        {
            Renderer renderer = _ingameManager.Rubbers[i].GetComponent<Renderer>();
            renderer.GetPropertyBlock(propBlock);
            Color color = propBlock.GetColor("_Color");

            if (color == _ingameManager.Uncolored)
            {
                rubberPositions[i] = Vector3.zero;
            }
        }

        // Get the position of the current spinning hub
        Vector3 currentHubPosition = _isHub1 ? _hub2Transform.position : _hub1Transform.position;

        float nearestDistance = float.MaxValue;
        Vector3 nearestRubber = Vector3.zero;

        // Find the nearest tile that color is Ingamemanager.Colored
        foreach (Vector3 rubberPosition in rubberPositions)
        {
            float distance = Vector3.Distance(currentHubPosition, rubberPosition);
            if (distance < nearestDistance && distance < _nearestThreshold)
            {
                nearestDistance = distance;
                nearestRubber = rubberPosition;
            }
        }

        // Check if the current hub is on the nearest tile based on some distance threshold
        float distanceToNearestTile = Vector3.Distance(currentHubPosition, nearestRubber);
        if (distanceToNearestTile < _nearestThreshold)
        {
            if (Physics.SphereCast(currentHubPosition, 1.1f, Vector3.down, out RaycastHit hit, 1.5f, LayerMask.GetMask("Tile")))
            {
                OnSpin();
            }
        }
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
}
