using System;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonEventComms : MonoBehaviour, IEventHovered
{
    public event Action<Button> OnHoveredButton;
    public event Action<Button> OnExitButton;
    public event Action<Button> OnClickButton;

    public void OnHoveredEventStart(Button butt)
    {
        OnHoveredButton?.Invoke(butt);
    }

    public void OnExitHoveredEvent(Button butt)
    {
        OnExitButton?.Invoke(butt);
    }

    public void OnClickEvent(Button butt)
    {
        OnClickButton?.Invoke(butt);
    }
}
