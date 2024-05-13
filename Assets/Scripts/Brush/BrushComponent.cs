using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushComponent : MonoBehaviour
{
    private IngameManager _ingameManager;
    private MaterialPropertyBlock _propertyBlock;
    private BrushController _brushController;

    void Start()
    {
        _ingameManager = GameObject.Find("IngameManager").GetComponent<IngameManager>();
        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetColor("_Color", _ingameManager.Colored);
        BrushController.OnTouchInputEvent += OnTouchInput;
        _brushController = GetComponentInParent<BrushController>();
    }

    void OnDestroy()
    {
        BrushController.OnTouchInputEvent -= OnTouchInput;
    }

    private void OnTouchInput(string obj)
    {
        if ((obj == "Hub1" && obj == name) || (obj == "Hub2" && obj == name))
        {
            if (Physics.SphereCast(transform.position, 1.1f, Vector3.down, out RaycastHit hit, 1.5f, LayerMask.GetMask("Tile")))
            {
                transform.SetParent(hit.transform);
            }
            else
            {
                if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.Solo)
                {
                    GameManager.Instance.AudioManager.PlayBrushHitWallEffect();
                    GameManager.Instance.UpdateGameState(GameState.Lose);
                }
                else if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.PVE)
                {
                    _ingameManager.Respawn();
                }
            }

            if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.Solo)
            {
                CalculateCombo();
            }
        }
    }

    private void CalculateCombo()
    {
        if (!_brushController.IsStartCalculateCombo)
        {
            _brushController.SetIsStartCalculateCombo(true);
            return;
        }

        if (_brushController.IsFirstHit)
        {
            _brushController.SetIsFirstHit(false);
            _brushController.SetPreviousHit(_brushController.CurrentHit);
        }
        else
        {
            if (_brushController.CurrentHit > _brushController.PreviousHit)
            {
                _ingameManager.AddBonusScore(5); // 5 bonus combo hit score
                GameManager.Instance.UIManager.ShowComboHit();
                _brushController.SetPreviousHit(_brushController.CurrentHit);
            }
            else
            {
                _brushController.SetPreviousHit(0);
                _brushController.SetIsFirstHit(true);
            }
        }

        _brushController.SetCurrentHit(0);
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
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                Renderer renderer = collisionGameObject.GetComponent<Renderer>();
                renderer.GetPropertyBlock(propertyBlock);
                Color color = propertyBlock.GetColor("_Color");

                if (color == _ingameManager.Uncolored)
                {
                    collisionGameObject.GetComponent<Renderer>().SetPropertyBlock(_propertyBlock);
                    _brushController.ShowParticle(collisionGameObject.transform.position);
                    GameManager.Instance.AudioManager.PlayBrushHitEffect();
                }

                if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.Solo)
                {
                    _ingameManager.AddHitScore(1);
                    _ingameManager.Rubbers.Remove(parent);
                }
                else if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.PVE)
                {

                }
            }
        }
        else if (collisionGameObject.CompareTag("Wall"))
        {
            if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.Solo)
            {
                GameManager.Instance.AudioManager.PlayBrushHitWallEffect();
                GameManager.Instance.UpdateGameState(GameState.Lose);
            }
            else if (GameManager.Instance.DataManager.SaveData.CurrentGameMode == GameMode.PVE)
            {
                _ingameManager.Respawn();
            }
        }
    }
}
