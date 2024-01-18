using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlimeViewerBehavior : UIPanelBehavior
{
    public TextMeshProUGUI slimeName;
    public Image elementOne, elementTwo;
    public Sprite[] elementIcons;

    public static SlimeViewerBehavior instance;

    private void Awake()
    {
        instance = this;
    }

    public virtual void TogglePanel(SlimeInformation slime)
    {
        base.TogglePanel(true);

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
    }
}
