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
    public TextMeshProUGUI slimeName, FPSText;

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

        StartCoroutine(UpdateFPSCounter());

        IEnumerator UpdateFPSCounter()
        {
            int times = 0;
            int total = 0;
            int currentAverage = 0;
            int min = int.MaxValue;
            int max = int.MinValue;
            int min15 = int.MaxValue;
            int max15 = int.MinValue;

            while(true)
            {
                int number = ((int)(1f / Time.unscaledDeltaTime));
                FPSText.text = "Current: " + number.ToString();

                total += number;
                times++;

                if(times >= 30)
                {
                    currentAverage = Mathf.RoundToInt(total / times);
                    min15 = int.MaxValue;
                    max15 = int.MinValue;
                    times = 0;
                    total = 0;
                }

                FPSText.text += "\n15s Avg: " + currentAverage;

                min15 = Mathf.Min(min15, number);
                max15 = Mathf.Max(max15, number);

                FPSText.text += "\n15s Max: " + max15;
                FPSText.text += "\n15s Min: " + min15;

                min = Mathf.Min(min, number);
                max = Mathf.Max(max, number);

                FPSText.text += "\nMax: " + max;
                FPSText.text += "\nMin: " + min;

                yield return new WaitForSeconds(0.25f);
            }
        }
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
        GameController.BlockInput(true);
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
        GameController.BlockInput(false);
    }
}
