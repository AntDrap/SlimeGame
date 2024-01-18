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

    public enum Mode { Inventory, BreederLeft, BreederRight }
    public Mode currentMode;

    protected SlimeInformation currentSlime;

    public virtual void Start()
    {
        ToggleDisableOverlay(false);
    }

    public virtual void SetSlime(SlimeInformation slime, Mode mode)
    {
        currentMode = mode;
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
        switch (currentMode)
        {
            case Mode.Inventory:
                SlimeInformationPanel.instance.TogglePanel(currentSlime);
                break;
            case Mode.BreederLeft:
                BreederBehavior.instance.SetSlime(true, currentSlime);
                break;
            case Mode.BreederRight:
                BreederBehavior.instance.SetSlime(false, currentSlime);
                break;
        }
    }
}
