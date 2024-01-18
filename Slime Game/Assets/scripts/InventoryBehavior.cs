using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryBehavior : UIPanelBehavior
{
    public RectTransform inventoryPanel;
    public GameObject inventorySlotPrefab;

    public override void TogglePanel(bool toggle)
    {
        base.TogglePanel(toggle);

        int index = 0;

        foreach (SlimeInformation slimeInformation in SaveManager.GetPlayerSlimes())
        {
            if (inventoryPanel.childCount > index)
            {
                inventoryPanel.GetChild(index).gameObject.SetActive(true);
                inventoryPanel.GetChild(index).GetComponent<SlimeInventorySlotBehavior>().SetSlime(slimeInformation, SlimeInventorySlotBehavior.Mode.Inventory);
            }
            else
            {
                Instantiate(inventorySlotPrefab, inventoryPanel).GetComponent<SlimeInventorySlotBehavior>().SetSlime(slimeInformation, SlimeInventorySlotBehavior.Mode.Inventory);
            }

            index++;
        }

        while (index < inventoryPanel.childCount)
        {
            index++;
            inventoryPanel.GetChild(index).gameObject.SetActive(false);
        }
    }
}
