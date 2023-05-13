using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIMouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIMouseHoverManager.instance.overUIElement = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIMouseHoverManager.instance.overUIElement = false;
    }
}
