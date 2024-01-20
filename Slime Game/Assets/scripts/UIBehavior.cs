using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBehavior : MonoBehaviour
{
    public GameObject mainGameUIHolder;
    public TextMeshProUGUI FPSText;

    public UIPanelBehavior[] UIPanels;

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
        DisablePanel();
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) { DisablePanel(); }
    }

    public void EnableInfoPanel(SlimeInformation slime)
    {
        EnablePanel(null);
        SlimeInformationPanel.instance.TogglePanel(slime);
    }

    public void EnableViewPanel(SlimeInformation slime)
    {
        EnablePanel(null);
        ToggleMainCamera(true);
        SlimeViewerBehavior.instance.TogglePanel(slime);
    }

    public void EnablePanel(UIPanelBehavior panelToEnable)
    {
        ToggleMainCamera(false);
        GameController.BlockInput(true);
        mainGameUIHolder.SetActive(false);

        foreach (UIPanelBehavior panel in UIPanels)
        {
            if (panel != panelToEnable)
            {
                panel.TogglePanel(false);
                continue;
            }

            panel.TogglePanel(true);
        }
    }

    public void DisablePanel()
    {
        ToggleMainCamera(true);
        StopLookingAtSlime();
        GameController.BlockInput(false);
        mainGameUIHolder.SetActive(true);

        foreach (UIPanelBehavior panel in UIPanels)
        {
            panel.TogglePanel(false);
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

    public void StopLookingAtSlime() => SlimeBehavior.StopLookingAtSlime();

    public void CatchSlime() => GameController.instance.AddSlimeToCollection();

    public void ToggleMainUI(bool toggle)
    {
        mainGameUIHolder.SetActive(toggle);
        ToggleMainCamera(toggle);
    }
}
