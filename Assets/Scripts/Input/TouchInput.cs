using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchInput : MonoBehaviour, IPointerDownHandler
{
    public static event System.Action OnTouchInputEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnTouchInputEvent?.Invoke();
    }
}