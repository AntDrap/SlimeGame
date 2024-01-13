using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBehavior : MonoBehaviour
{
    public GameObject mainGameUIHolder;
    public GameObject slimeUIHolder;
    public Image elementOne, elementTwo;
    public TextMeshProUGUI slimeName;

    public GameObject inventoryUIHolder, inventorySlotPrefab;
    public RectTransform inventoryPanel;

    public Sprite[] elementIcons;

    public static UIBehavior instance;

    public static bool mainCameraEnabled;

    private static int cameraDefaultMask;
    private static CameraClearFlags cameraClearFlags;
    private static Camera mainCamera;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CloseSlimeUI();
        CloseInventory();
    }

    public static void ToggleMainCamera(bool toggle)
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
            cameraClearFlags = mainCamera.clearFlags;
            cameraDefaultMask = mainCamera.cullingMask;
        }

        mainCameraEnabled = toggle;
        mainCamera.clearFlags = toggle ? cameraClearFlags : CameraClearFlags.Nothing;
        mainCamera.cullingMask = toggle ? cameraDefaultMask : 0;
    }

    public void OpenSlimeUI(SlimeBehavior slime)
    {
        mainGameUIHolder.SetActive(false);
        slimeUIHolder.gameObject.SetActive(true);
        slimeName.text = slime.slimeInformation.slimeName;
        if(slime.slimeInformation.elementOne == slime.slimeInformation.elementTwo)
        {
            elementOne.gameObject.SetActive(false);
        }
        else
        {
            elementOne.gameObject.SetActive(true);
            elementOne.sprite = elementIcons[(int)slime.slimeInformation.elementOne];
        }

        elementTwo.sprite = elementIcons[(int)slime.slimeInformation.elementTwo];
    }

    public void StopLookingAtSlime()
    {
        SlimeBehavior.StopLookingAtSlime();
    }

    public void CloseSlimeUI()
    {
        slimeUIHolder.gameObject.SetActive(false);
        ToggleMainUI(true);
    }

    public void CatchSlime()
    {
        GameController.instance.AddSlimeToCollection();
    }

    public void ToggleMainUI(bool toggle)
    {
        mainGameUIHolder.SetActive(toggle);
        ToggleMainCamera(toggle);
    }

    public void OpenInventory()
    {
        GameController.instance.inputBlocked = true;
        ToggleMainUI(false);
        inventoryUIHolder.SetActive(true);
        ToggleMainCamera(false);

        int index = 0;

        foreach(SlimeInformation slimeInformation in SaveManager.GetPlayerSlimes())
        {
            if(inventoryPanel.childCount > index)
            {
                inventoryPanel.GetChild(index).gameObject.SetActive(true);
                inventoryPanel.GetChild(index).GetComponent<SlimeInventorySlotBehavior>().SetSlime(slimeInformation);
            }
            else
            {
                Instantiate(inventorySlotPrefab, inventoryPanel).GetComponent<SlimeInventorySlotBehavior>().SetSlime(slimeInformation);
            }

            index++;
        }

        while(index < inventoryPanel.childCount)
        {
            index++;
            inventoryPanel.GetChild(index).gameObject.SetActive(false);
        }
    }

    public void CloseInventory()
    {
        inventoryUIHolder.SetActive(false);
        ToggleMainUI(true);
        GameController.instance.inputBlocked = false;
    }
}
