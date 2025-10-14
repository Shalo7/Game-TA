using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public UIButtonEventComms buttonEventComms;


    public void OnPointerDown(PointerEventData eventData)
    {
        buttonEventComms.OnClickEvent(gameObject?.GetComponent<Button>());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonEventComms.OnHoveredEventStart(gameObject?.GetComponent<Button>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonEventComms.OnExitHoveredEvent(gameObject?.GetComponent<Button>());
    }
}
