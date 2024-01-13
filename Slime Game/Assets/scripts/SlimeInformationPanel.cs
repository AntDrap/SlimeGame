using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlimeInformationPanel : MonoBehaviour
{
    public Image elementOne, elementTwo;
    public Camera slimeCamera;
    public SlimeBehavior displaySlime;
    public TextMeshProUGUI slimeDescription, slimeName;
    public static SlimeInformationPanel instance;
    public Sprite[] elementIcons;

    private string[] slimeSizeText = new string[]
    {
        "Very Small",
        "Small",
        "Medium",
        "Large",
        "Very Large"
    };

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetSlime(SlimeInformation slime)
    {
        gameObject.SetActive(true);
        UIBehavior.instance.ToggleMainUI(false);
        slimeCamera.gameObject.SetActive(true);

        slimeName.text = slime.slimeName;
        displaySlime.transform.position += Vector3.up;
        displaySlime.SetGenes(slime);

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

        slimeDescription.text = "";

        float slimeSize = slime.size;
        slimeSize *= 2;
        int intSlimeSize = Mathf.RoundToInt(slimeSize) - 1;

        slimeDescription.text += "Size: " + slimeSizeText[intSlimeSize] + "\n";
        slimeDescription.text += "Texture: " + slime.skinTexture.ToString();
    }

    private void OnDisable()
    {
        if(!UIBehavior.instance.inventoryUIHolder.activeSelf)
        {
            UIBehavior.instance.ToggleMainUI(true);
        }

        UIBehavior.ToggleMainCamera(true);
    }

    private void OnEnable()
    {
        UIBehavior.ToggleMainCamera(false);
    }
}
