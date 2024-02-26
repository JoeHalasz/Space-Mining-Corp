using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleUIInput : MonoBehaviour
{

    public int invSlot;
    public OpenInventoryUI openInventoryUI;

    public void OnPointerDown()
    {
        openInventoryUI.OnPointerDown(invSlot);
    }

    public void OnDrag()
    {
        openInventoryUI.OnDrag(invSlot);
    }
    public void OnDrop()
    {
        openInventoryUI.OnDrop(invSlot);
    }

    public void OnPointerEnter()
    {
        openInventoryUI.OnPointerEnter(invSlot);
    }

    public void OnPointerExit()
    {
        openInventoryUI.OnPointerExit(invSlot);
    }
}
