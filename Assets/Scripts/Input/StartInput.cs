using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartInput : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.UpdateGameState(GameState.Ingame);
        GameManager.Instance.DisableGO(gameObject);
    }
}
