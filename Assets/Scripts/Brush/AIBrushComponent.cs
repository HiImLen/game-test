using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrushComponent : MonoBehaviour
{
    private IngameManager _ingameManager;
    private MaterialPropertyBlock _propertyBlock;
    private AIBrushController _AIBrushController;

    void Start()
    {
        _ingameManager = GameObject.Find("IngameManager").GetComponent<IngameManager>();
        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetColor("_Color", _ingameManager.Uncolored);
        AIBrushController.OnSpinEvent += OnSpin;
        _AIBrushController = GetComponentInParent<AIBrushController>();
    }

    void OnDestroy()
    {
        AIBrushController.OnSpinEvent -= OnSpin;
    }

    private void OnSpin(string obj)
    {
        if ((obj == "Hub1" && obj == name) || (obj == "Hub2" && obj == name))
        {
            if (Physics.SphereCast(transform.position, 1.1f, Vector3.down, out RaycastHit hit, 1.5f, LayerMask.GetMask("Tile")))
            {
                transform.SetParent(hit.transform);
            }
            else
            {
                if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.PVE)
                {
                    _ingameManager.AIRespawn();
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_ingameManager.IsGameStarted) return;
        GameObject collisionGameObject = collision.gameObject;
        if (collisionGameObject.CompareTag("Rubber"))
        {
            GameObject parent = collisionGameObject.transform.parent.gameObject;
            if (_ingameManager.Rubbers.Contains(parent))
            {
                if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.PVE)
                {
                    MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                    Renderer renderer = collisionGameObject.GetComponent<Renderer>();
                    renderer.GetPropertyBlock(propertyBlock);
                    Color color = propertyBlock.GetColor("_Color");

                    if (color == _ingameManager.Colored)
                    {
                        collisionGameObject.GetComponent<Renderer>().SetPropertyBlock(_propertyBlock);
                        _AIBrushController.ShowParticle(collisionGameObject.transform.position);
                    }
                }
            }
        }
    }
}
