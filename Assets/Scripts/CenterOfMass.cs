using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CenterOfMass : MonoBehaviour
{
    [SerializeField] private Vector3 _newCenterOfMass;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.centerOfMass = _newCenterOfMass;
        }
        else
        {
            Debug.LogError("Rigidbody component not found on this GameObject.");
        }
    }
}
