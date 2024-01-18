using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreederBehavior : UIPanelBehavior
{
    public SlimeInformation leftSlime, rightSlime;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private RectTransform leftDisplay, rightDisplay;
    [SerializeField] private SlimeImage leftSlimeImage, rightSlimeImage;
    [Serializable] private struct SlimeImage { public Image topImage, bottomImage; }

    private Dictionary<SlimeInformation, SlimeInventorySlotBehavior> leftSlimeSlots, rightSlimeSlots;

    public static BreederBehavior instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        leftSlimeSlots = new Dictionary<SlimeInformation, SlimeInventorySlotBehavior>();
        rightSlimeSlots = new Dictionary<SlimeInformation, SlimeInventorySlotBehavior>();
    }

    public void BreedSlimes()
    {
        if(leftSlime == null || rightSlime == null || leftSlime == rightSlime) { return; }

        SlimeInformation newSlime = new SlimeInformation(leftSlime, rightSlime);
        SaveManager.AddSlimeToPlayerInventory(newSlime);
        UIBehavior.instance.EnableInfoPanel(newSlime);
    }

    public void SetSlime(bool isLeftSlime, SlimeInformation slime)
    {
        if(slime == null)
        {
            if(isLeftSlime)
            {
                leftSlime = null;
            }
            else
            {
                rightSlime = null;
            }

            (isLeftSlime ? leftSlimeImage : rightSlimeImage).topImage.color = Color.clear;
            (isLeftSlime ? leftSlimeImage : rightSlimeImage).bottomImage.color = Color.clear;
            UpdateDisplay();
        }
        else if(slime == leftSlime || slime == rightSlime)
        {
            SlimeInformation temp = null;

            if (isLeftSlime && slime == rightSlime)
            {
                temp = rightSlime;
                SetSlime(!isLeftSlime, null);
            }
            else if (!isLeftSlime && slime == leftSlime)
            {
                temp = leftSlime;
                SetSlime(!isLeftSlime, null);
            }

            SetSlime(isLeftSlime, temp);
        }
        else
        {
            if (isLeftSlime)
            {
                leftSlime = slime;
            }
            else
            {
                rightSlime = slime;
            }

            (isLeftSlime ? leftSlimeImage : rightSlimeImage).topImage.color = slime.GetTopColor();
            (isLeftSlime ? leftSlimeImage : rightSlimeImage).bottomImage.color = slime.GetBottomColor();
            UpdateDisplay();
        }
    }

    public void UpdateDisplay()
    {
        foreach (KeyValuePair<SlimeInformation, SlimeInventorySlotBehavior> keyValuePair in leftSlimeSlots)
        {
            keyValuePair.Value.ToggleDisableOverlay(false);
        }

        foreach (KeyValuePair<SlimeInformation, SlimeInventorySlotBehavior> keyValuePair in rightSlimeSlots)
        {
            keyValuePair.Value.ToggleDisableOverlay(false);
        }

        if (rightSlime != null && leftSlimeSlots.ContainsKey(rightSlime)) { leftSlimeSlots[rightSlime].ToggleDisableOverlay(true); }
        if (rightSlime != null && rightSlimeSlots.ContainsKey(rightSlime)) { rightSlimeSlots[rightSlime].ToggleDisableOverlay(true); }
        if (leftSlime != null && rightSlimeSlots.ContainsKey(leftSlime)) { rightSlimeSlots[leftSlime].ToggleDisableOverlay(true); }
        if (leftSlime != null && leftSlimeSlots.ContainsKey(leftSlime)) { leftSlimeSlots[leftSlime].ToggleDisableOverlay(true); }
    }

    public override void TogglePanel(bool toggle)
    {
        base.TogglePanel(toggle);

        List<SlimeInformation> slimeInformations = SaveManager.GetPlayerSlimes();
        leftSlimeSlots = new Dictionary<SlimeInformation, SlimeInventorySlotBehavior>();
        rightSlimeSlots = new Dictionary<SlimeInformation, SlimeInventorySlotBehavior>();

        for (int i = 0; i < 2; i++)
        {
            int index = 0;

            RectTransform panel = (i == 0 ? leftDisplay : rightDisplay);

            Dictionary<SlimeInformation, SlimeInventorySlotBehavior> slimeSlots = (i == 0 ? leftSlimeSlots : rightSlimeSlots);

            foreach (SlimeInformation slimeInformation in slimeInformations)
            {
                SlimeInventorySlotBehavior slot = null;

                if (panel.childCount > index)
                {
                    panel.GetChild(index).gameObject.SetActive(true);
                    slot = panel.GetChild(index).GetComponent<SlimeInventorySlotBehavior>();
                }
                else
                {
                    slot = Instantiate(slotPrefab, panel).GetComponent<SlimeInventorySlotBehavior>();
                }

                slot.SetSlime(slimeInformation, i == 0 ? SlimeInventorySlotBehavior.Mode.BreederLeft : SlimeInventorySlotBehavior.Mode.BreederRight);

                slimeSlots.Add(slimeInformation, slot);

                index++;
            }

            while (index < panel.childCount)
            {
                index++;
                panel.GetChild(index).gameObject.SetActive(false);
            }
        }

        SetSlime(true, null);
        SetSlime(false, null);
    }
}
