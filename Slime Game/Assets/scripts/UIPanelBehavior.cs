using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelBehavior : MonoBehaviour
{
    public virtual void TogglePanel(bool toggle)
    {
        gameObject.SetActive(toggle);
    }
}
