using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SlimeInventorySlotBehavior : MonoBehaviour, IPointerClickHandler
{
    public Image elementOne, elementTwo, slimeImageTop, slimeImagebottom, disabledOverlay;
    public TextMeshProUGUI slimeName;
    public Sprite[] elementIcons;

    protected SlimeInformation currentSlime;

    public virtual void Start()
    {
        ToggleDisableOverlay(false);
    }

    public virtual void SetSlime(SlimeInformation slime)
    {
        currentSlime = slime;
        slimeName.text = slime.slimeName;
        if (slime.elementOne == slime.elementTwo)
        {
            elementOne.gameObject.SetActive(false);
        }
        else
        {
            elementOne.gameObject.SetActive(true);
            elementOne.sprite = elementIcons[(int)slime.elementOne];
        }

        elementTwo.sprite = elementIcons[(int)slime.elementTwo];

        slimeImageTop.color = slime.GetTopColor();
        slimeImagebottom.color = slime.GetBottomColor();
    }

    public virtual void ToggleDisableOverlay(bool toggle)
    {
        if(!disabledOverlay) { return; }
        disabledOverlay.gameObject.SetActive(toggle);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        SlimeInformationPanel.instance.SetSlime(currentSlime);
    }
}
