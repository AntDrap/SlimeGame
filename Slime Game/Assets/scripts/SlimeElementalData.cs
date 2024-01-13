using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "SlimeElementalData", menuName = "SlimeElementalData")]
public class SlimeElementalData : ScriptableObject
{
    private Dictionary<SlimeInformation.SlimeElement, SlimeElementInfo> elementDictionary;
    private Dictionary<string, Texture2D> faceDictionary;
    public Texture2D[] neutralTextures;
    public SlimeElementInfo[] elementInfos;

    public Texture2D GetFace(string faceName)
    {
        if(faceDictionary == null)
        {
            faceDictionary = new Dictionary<string, Texture2D>();

            foreach(Texture2D texture2D in neutralTextures)
            {
                faceDictionary.Add(texture2D.name, texture2D);
            }

            if (elementDictionary == null)
            {
                elementDictionary = new Dictionary<SlimeInformation.SlimeElement, SlimeElementInfo>();

                foreach (SlimeElementInfo sei in elementInfos)
                {
                    elementDictionary.Add(sei.element, sei);
                }
            }

            foreach (KeyValuePair<SlimeInformation.SlimeElement, SlimeElementInfo> kvp in elementDictionary)
            {
                foreach (Texture2D texture2D in kvp.Value.textures)
                {
                    faceDictionary.Add(texture2D.name, texture2D);
                }
            }
        }

        return faceDictionary.ContainsKey(faceName) ? faceDictionary[faceName] : faceDictionary.ElementAt(0).Value;
    }

    public string GetFace(SlimeInformation.SlimeElement elementOne, SlimeInformation.SlimeElement elementTwo)
    {
        List<Texture2D> faces = new List<Texture2D>(neutralTextures);
        faces.AddRange(GetElementInfo(elementTwo).textures);
        faces.AddRange(GetElementInfo(elementTwo).textures);
        return faces[UnityEngine.Random.Range(0, faces.Count)].name;
    }

    public string GetName(SlimeInformation.SlimeElement elementOne, SlimeInformation.SlimeElement elementTwo)
    {
        return GetElementInfo(elementOne).GetFirstName() + " " + GetElementInfo(elementTwo).GetLastName();
    }

    public SlimeElementInfo GetElementInfo(SlimeInformation.SlimeElement element)
    {
        if(elementDictionary == null)
        {
            elementDictionary = new Dictionary<SlimeInformation.SlimeElement, SlimeElementInfo>();

            foreach(SlimeElementInfo sei in elementInfos)
            {
                elementDictionary.Add(sei.element, sei);
            }
        }

        return elementDictionary[element];
    }

    [Serializable]
    public struct SlimeElementInfo
    {
        public SlimeInformation.SlimeElement element;
        public Texture2D[] textures;
        public Color[] colors;
        public string[] firstName;
        public string[] lastName;

        public Color GetColor()
        {
            return colors[UnityEngine.Random.Range(0, colors.Length)];
        }

        public Color GetColor(int i)
        {
            return colors[i];
        }

        public string GetFirstName()
        {
            return firstName[UnityEngine.Random.Range(0, firstName.Length)];
        }

        public string GetLastName()
        {
            return lastName[UnityEngine.Random.Range(0, lastName.Length)];
        }
    }
}
