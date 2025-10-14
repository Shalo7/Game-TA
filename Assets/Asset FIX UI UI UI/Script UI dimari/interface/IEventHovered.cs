using System;
using UnityEngine;
using UnityEngine.UI;

public interface IEventHovered
{
    public event Action<Button> OnHoveredButton;
    public event Action<Button> OnExitButton;
    public event Action<Button> OnClickButton;
}
