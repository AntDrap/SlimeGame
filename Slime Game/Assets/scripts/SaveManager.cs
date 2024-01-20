using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class SaveManager
{
    private static string gameSavePath = string.Concat(Application.persistentDataPath, "/GameSave.json");
    private static string fieldSavePath = string.Concat(Application.persistentDataPath, "/FieldState.json");

    private static PlayerInformation playerInformation;
    private static FieldInformation fieldInformation;

    public static void EraseSaveData()
    {
        fieldInformation = new FieldInformation();
        playerInformation = new PlayerInformation();
        SaveData();
    }

    public static FieldInformation GetFieldInformation()
    {
        if (fieldInformation == null) { LoadData(); }
        return fieldInformation;
    }

    public static PlayerInformation GetPlayerInformation()
    {
        if(playerInformation == null) { LoadData(); }
        return playerInformation;
    }

    public static List<SlimeInformation> GetPlayerSlimes() { return GetPlayerInformation().GetSlimes(); }

    public static bool AddSlimeToPlayerInventory(SlimeInformation slime)
    {
        bool returnValue = GetPlayerInformation().AddSlimeToInventory(slime);
        SaveData();
        return returnValue;
    }

    public static bool RemoveSlimeFromPlayerInventory(SlimeInformation slime)
    {
        bool returnValue = GetPlayerInformation().RemoveSlimeFromInventory(slime);
        SaveData();
        return returnValue;
    }

    public static bool ChangeMoney(int amount)
    {
        bool returnValue = GetPlayerInformation().ChangeMoney(amount);
        SaveData();
        return returnValue;
    }

    public static void LoadData()
    {
        if(!File.Exists(gameSavePath) || !File.Exists(fieldSavePath))
        {
            EraseSaveData();
        }
        else
        {
            try
            {
                playerInformation = JsonConvert.DeserializeObject<PlayerInformation>(File.ReadAllText(gameSavePath));
                fieldInformation = JsonConvert.DeserializeObject<FieldInformation>(File.ReadAllText(fieldSavePath));
            }
            catch
            {
                EraseSaveData();
            }
        }
    }

    public static void SaveData()
    {
        File.WriteAllText(gameSavePath, JsonConvert.SerializeObject(playerInformation, Formatting.Indented));
        File.WriteAllText(fieldSavePath, JsonConvert.SerializeObject(fieldInformation, Formatting.Indented));
    }

    public static bool UpdatePlayerLogin() => GetPlayerInformation().UpdatePlayerLogin();
    public static void CreateSlimesInField(List<SlimeBehavior> slimes) => GetFieldInformation().CreateSlimes(slimes);
    public static void AddSlimeToPlayerInventoryFromField(SlimeBehavior slime)
    {
        GetFieldInformation().RemoveSlimeFromField(slime);
        AddSlimeToPlayerInventory(slime.slimeInformation);
    }
    public static List<SlimeInformation> GetSlimesInField() => GetFieldInformation().GetSlimesInField();
}

[Serializable]
public class PlayerInformation
{
    [JsonProperty]
    private int money = 0;
    [JsonProperty]
    private DateTime lastPlayerLogin = DateTime.MinValue;
    [JsonProperty]
    private int slimeCapacity = 100;
    [JsonProperty]
    private List<SlimeInformation> slimesOwned = new List<SlimeInformation>();

    public bool AddSlimeToInventory(SlimeInformation slime)
    {
        if(slimesOwned.Count >= slimeCapacity) { return false; }
        SlimeInformationPanel.instance.TogglePanel(slime);
        slimesOwned.Add(slime);
        return true;
    }
    public bool RemoveSlimeFromInventory(SlimeInformation slime)
    {
        if (!slimesOwned.Contains(slime)) { return false; }
        slimesOwned.Remove(slime);
        return true;
    }

    public SlimeInformation GetSlime(int index)
    {
        return slimesOwned[index];
    }

    public List<SlimeInformation> GetSlimes()
    {
        return slimesOwned;
    }

    public bool UpdatePlayerLogin()
    {
        DateTime newTime = DateTime.Now;
        if(lastPlayerLogin.Day != newTime.Day || lastPlayerLogin.Month != newTime.Month || lastPlayerLogin.Year != newTime.Year)
        {
            lastPlayerLogin = newTime;
            return true;
        }

        return false;
    }

    public bool ChangeMoney(int amount)
    {
        if(amount > 0 || money >= Mathf.Abs(amount))
        {
            money = Mathf.Clamp(money + amount, 0, int.MaxValue);
            return true;
        }

        return false;
    }
}

[Serializable]
public class FieldInformation
{
    [JsonProperty]
    private List<SlimeInformation> slimesInField = new List<SlimeInformation>();

    private Dictionary<SlimeBehavior, SlimeInformation> slimesCurrentlyInGame;

    public SlimeInformation GetSlime(int index) => slimesInField[index];
    public List<SlimeInformation> GetSlimesInField() => slimesInField;

    public void RemoveSlimeFromField(SlimeBehavior slime)
    {
        if(slimesCurrentlyInGame.ContainsKey(slime))
        {
            slimesInField.Remove(slimesCurrentlyInGame[slime]);
            slimesCurrentlyInGame.Remove(slime);
        }

        GameObject.Destroy(slime.gameObject);
    }

    public void CreateSlimes(List<SlimeBehavior> slimes)
    {
        slimesCurrentlyInGame = new Dictionary<SlimeBehavior, SlimeInformation>();
        slimesInField = new List<SlimeInformation>();

        foreach (SlimeBehavior slime in slimes)
        {
            slimesInField.Add(slime.slimeInformation);
            slimesCurrentlyInGame.Add(slime, slime.slimeInformation);
        }

        SaveManager.SaveData();
    }
}