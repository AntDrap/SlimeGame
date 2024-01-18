using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class SlimeBehavior : MonoBehaviour
{
    public SlimeInformation slimeInformation;
    public GameObject childSlimePrefab;
    public SkinnedMeshRenderer meshRenderer;
    public Transform cameraPosition;
    public LayerMask groundMask;

    public bool displaySlime;

    public float bounciness = 10000, solidity = 500;

    public SpringJoint[] springs;
    public List<Vector3> originalAnchors;
    public List<Vector3> originalPositions;

    private Material slimeMaterial;
    public Rigidbody rigidbody;

    private bool beingLookedAt = false;
    private const float REPEL_FORCE = 20;

    void Start()
    {
        slimeMaterial = meshRenderer.material;
        rigidbody = GetComponent<Rigidbody>();

        UpdateAppearence();

        if(!displaySlime)
        {
            StartCoroutine(HoppingCoroutine());
        }
    }

    public void SetSpringOrigins()
    {
        springs = GetComponents<SpringJoint>();
        originalAnchors = new List<Vector3>();
        originalPositions = new List<Vector3>();

        foreach (SpringJoint s in springs)
        {
            originalAnchors.Add(s.connectedAnchor);
            originalPositions.Add(s.connectedBody.transform.localPosition);
        }
    }

    public void Randomize() => slimeInformation.Randomize();

    public static void LookAtSlime(SlimeBehavior slimeBehavior)
    {
        if (GameController.instance.currentSlimeBeingLookedAt) { return; }
        GameController.instance.currentSlimeBeingLookedAt = slimeBehavior;
        UIBehavior.instance.EnableViewPanel(slimeBehavior.slimeInformation);
        GameController.instance.MoveCamera(slimeBehavior.cameraPosition);

        slimeBehavior.beingLookedAt = true;
    }

    public static void StopLookingAtSlime()
    {
        if(GameController.instance.currentSlimeBeingLookedAt == null) { return; }

        GameController.instance.currentSlimeBeingLookedAt.beingLookedAt = false;
        GameController.instance.currentSlimeBeingLookedAt = null;
        GameController.instance.MoveCamera();
    }

    public void SetGenes(SlimeInformation genetics)
    {
        slimeInformation = genetics;
        UpdateAppearence();
    }

    public void UpdateAppearence()
    {
        if(!slimeMaterial)
        {
            slimeMaterial = meshRenderer.material;
        }

        slimeMaterial.SetColor("_" + nameof(SlimeInformation.topColor), slimeInformation.GetTopColor());
        slimeMaterial.SetColor("_" + nameof(SlimeInformation.bottomColor), slimeInformation.GetBottomColor());

        for (int i = 0; i < Enum.GetValues(typeof(SlimeInformation.SkinTexture)).Length; i++)
        {
            SlimeInformation.SkinTexture skinTexture = (SlimeInformation.SkinTexture)i;

            if (!slimeInformation.skinTexture.Equals(skinTexture))
            {
                slimeMaterial.DisableKeyword("_" + nameof(SlimeInformation.skinTexture).ToUpper() + "_" + skinTexture.ToString().ToUpper());
                continue;
            }

            slimeMaterial.EnableKeyword("_" + nameof(SlimeInformation.skinTexture).ToUpper() + "_" + skinTexture.ToString().ToUpper());
        }

        slimeMaterial.SetTexture("_" + nameof(SlimeInformation.slimeTexture), GameController.instance.slimeElementalData.GetFace(slimeInformation.slimeTexture));

        transform.localScale = Vector3.one * slimeInformation.size;

        for(int i = 0; i < springs.Length; i++)
        {
            springs[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            springs[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            springs[i].spring = solidity;
            springs[i].connectedAnchor = originalAnchors[i];
            springs[i].connectedBody.transform.localPosition = originalPositions[i];
        }
    }

    private IEnumerator HoppingCoroutine()
    {
        while(true)
        {
            yield return new WaitWhile(() => beingLookedAt);
            yield return new WaitUntil(() => Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, 3 * slimeInformation.size, groundMask));
            yield return new WaitForSeconds(UnityEngine.Random.Range(1, 4));

            int rotationDirection = UnityEngine.Random.Range(0, 2);
            rotationDirection = rotationDirection <= 0 ? -1 : 1;
            float rotationTime = UnityEngine.Random.Range(0f, 2f);

            while(rotationTime > 0)
            {
                yield return new WaitWhile(() => beingLookedAt);
                rotationTime -= Time.fixedDeltaTime;
                transform.Rotate(0, 0, rotationDirection * 60 * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            for (int i = 0; i < springs.Length; i++)
            {
                springs[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
                springs[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                springs[i].spring = solidity;
                springs[i].connectedAnchor = originalAnchors[i];
                springs[i].connectedBody.transform.localPosition = originalPositions[i];
            }

            Vector3 hopDirection = -transform.right;
            hopDirection.y = UnityEngine.Random.Range(1f,3f);
            hopDirection.Normalize();

            rigidbody.AddForce(hopDirection * bounciness);
        }
    }

    public void CreateChild(SlimeBehavior otherSlime)
    {
        Vector3 spawnPos = transform.position + otherSlime.transform.position;
        spawnPos /= 2;
        spawnPos.y += slimeInformation.size * 5;
        SlimeBehavior newChild = Instantiate(childSlimePrefab, spawnPos, Quaternion.Euler(new Vector3(-90,0,0))).GetComponent<SlimeBehavior>();
        newChild.SetGenes(new SlimeInformation(slimeInformation, otherSlime.slimeInformation));
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == gameObject.layer && other.transform.parent != transform && other.transform.parent)
        {
            rigidbody.AddForce((transform.position - other.transform.parent.position) * REPEL_FORCE * slimeInformation.size);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SlimeBehavior)), CanEditMultipleObjects]
    internal class SlimeBehaviorEditor : Editor
    {
        public SlimeBehavior otherSlime;

        public override void OnInspectorGUI()
        {
            SlimeBehavior slime = (SlimeBehavior)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Set Springs"))
            {
                slime.SetSpringOrigins();
                EditorUtility.SetDirty(slime);
            }

            if (GUILayout.Button("Randomize Appearence"))
            {
                slime.slimeInformation.Randomize();
                slime.UpdateAppearence();
                EditorUtility.SetDirty(slime);
            }

            if (GUILayout.Button("Update Appearence"))
            {
                slime.UpdateAppearence();
                EditorUtility.SetDirty(slime);
            }

            if (GUILayout.Button("Get Genetics From Material"))
            {
                Material slimeMaterial = slime.meshRenderer.sharedMaterial;

                slime.slimeInformation.topColor = SlimeInformation.ConvertColor(slimeMaterial.GetColor("_" + nameof(SlimeInformation.topColor)));
                slime.slimeInformation.bottomColor = SlimeInformation.ConvertColor(slimeMaterial.GetColor("_" + nameof(SlimeInformation.bottomColor)));

                slime.slimeInformation.skinTexture = (SlimeInformation.SkinTexture) slimeMaterial.GetFloat("_" + nameof(SlimeInformation.skinTexture).ToUpper());

                EditorUtility.SetDirty(slime);
            }

            EditorGUILayout.BeginHorizontal("box");

            otherSlime = (SlimeBehavior) EditorGUILayout.ObjectField(otherSlime, typeof(SlimeBehavior), true);

            if (GUILayout.Button("Create Child"))
            {
                slime.CreateChild(otherSlime);

                EditorUtility.SetDirty(slime);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

#endif
}

[Serializable]
public class SlimeInformation
{
    public string slimeName;
    public string slimeTexture;

    public const float COLOR_MUTATION_PERCENT = 0.20f;
    public const float FLOAT_MUTATION_PERCENT = 0.20f;

    public enum SkinTexture { Smooth, Rough, Wavy, Rocky}

    public enum SlimeElement { Fire, Water, Earth, Air, Sun, Moon }
    public SlimeElement elementOne, elementTwo;

    public float[] topColor = new float[] { 1, 1, 1 };
    public float[] bottomColor = new float[] { 1, 1, 1 };

    public float size;

    public SkinTexture skinTexture;

    private Dictionary<string, StatRange> statRanges = new Dictionary<string, StatRange>()
    {
        {nameof(size), new StatRange(1, 0.5f, 2.5f, 0.2f) },
    };

    private static Dictionary<SlimeElement, SlimeElement> enemyElements = new Dictionary<SlimeElement, SlimeElement>
    {
        { SlimeElement.Fire, SlimeElement.Water },
        { SlimeElement.Water, SlimeElement.Fire },
        { SlimeElement.Earth, SlimeElement.Air },
        { SlimeElement.Air, SlimeElement.Earth },
        { SlimeElement.Sun, SlimeElement.Moon },
        { SlimeElement.Moon, SlimeElement.Sun },
    };

    private static Dictionary<SlimeElement, List<SlimeElement>> allyElements = new Dictionary<SlimeElement, List<SlimeElement>>
    {
        { SlimeElement.Fire, new List<SlimeElement>() { SlimeElement.Air, SlimeElement.Sun } },
        { SlimeElement.Water, new List<SlimeElement>() { SlimeElement.Earth, SlimeElement.Moon } },
        { SlimeElement.Earth, new List<SlimeElement>() { SlimeElement.Water, SlimeElement.Moon } },
        { SlimeElement.Air, new List<SlimeElement>() { SlimeElement.Fire, SlimeElement.Sun } },
        { SlimeElement.Sun, new List<SlimeElement>() { SlimeElement.Fire, SlimeElement.Air } },
        { SlimeElement.Moon, new List<SlimeElement>() { SlimeElement.Water, SlimeElement.Earth } },
    };

    private const float SAME_ELEMENT_CHANCE = 40;
    private const float ALLY_ELEMENT_CHANCE = 35;
    private const float NEUTRAL_ELEMENT_CHANCE = 20;

    public struct StatRange
    {
        public float defaultValue;
        public float min;
        public float max;
        public float mutation;
        private const int round = 100;

        public StatRange(float defaultValue, float min, float max, float mutation)
        {
            this.defaultValue = defaultValue;
            this.min = min;
            this.max = max;
            this.mutation = mutation;
        }

        public float Clamp(float c)
        {
            float num = UnityEngine.Random.Range(MathF.Max(min, c * (1 - mutation)), MathF.Min(max, c * (1 + mutation)));
            num = Mathf.Round(num * round) / round;
            return num;
        }

        public float GetRandom()
        {
            float num = UnityEngine.Random.Range(min, max);
            num = Mathf.Round(num * round) / round;
            return num;
        }
        public float GenerateFloat(float float1, float float2)
        {
            float t = UnityEngine.Random.Range(0f, 1f);
            float num = Mathf.Lerp(float1, float2, t);
            num = Mathf.Round(num * round) / round;
            return Clamp(num);
        }
    }

    public void Randomize()
    {
        Tuple<SlimeElement, float[]>[] slimeElements = DetermineElement();

        elementOne = slimeElements[0].Item1;
        elementTwo = slimeElements[1].Item1;
        topColor = ConvertColor(GameController.instance.slimeElementalData.GetElementInfo(elementOne).GetColor());
        bottomColor = ConvertColor(GameController.instance.slimeElementalData.GetElementInfo(elementTwo).GetColor());

        slimeTexture = GameController.instance.slimeElementalData.GetFace(elementOne, elementTwo);
        skinTexture = (SkinTexture)(UnityEngine.Random.Range(0, 4));

        size = statRanges[nameof(size)].GetRandom();

        slimeName = GameController.instance.slimeElementalData.GetName(elementOne, elementTwo);
    }

    public SlimeInformation()
    {
        topColor = new float[] { 1, 1, 1 };
        bottomColor = new float[] { 1, 1, 1 };

        skinTexture = SkinTexture.Smooth;

        size = statRanges[nameof(size)].defaultValue;
    }

    public SlimeInformation(SlimeInformation baseGenetics)
    {
        ChangeGenetics(baseGenetics);
    }

    public SlimeInformation(SlimeInformation parentOne, SlimeInformation parentTwo)
    {
        Tuple<SlimeElement, float[]>[] slimeElements = DetermineElement(parentOne, parentTwo);

        int chosenNum = UnityEngine.Random.Range(0, 4);
        elementOne = slimeElements[0].Item1;
        topColor = slimeElements[0].Item2;

        chosenNum = UnityEngine.Random.Range(0, 4);
        elementTwo = slimeElements[1].Item1;
        bottomColor = slimeElements[1].Item2;

        slimeTexture = (UnityEngine.Random.Range(0,2) == 0 ? parentOne.slimeTexture : parentTwo.slimeTexture);

        size = statRanges[nameof(size)].GenerateFloat(parentOne.size, parentTwo.size);

        skinTexture = new SkinTexture[] { parentOne.skinTexture, parentTwo.skinTexture }[UnityEngine.Random.Range(0, 2)];

        slimeName = GameController.instance.slimeElementalData.GetName(elementOne, elementTwo);
    }

    public void ChangeGenetics(SlimeInformation baseGenetics)
    {
        topColor = baseGenetics.topColor;
        bottomColor = baseGenetics.bottomColor;
        
        skinTexture = baseGenetics.skinTexture;

        size = baseGenetics.size;
    }

    public Tuple<SlimeElement, SlimeElement> GetElement()
    {
        return new Tuple<SlimeElement, SlimeElement>(elementOne, elementTwo);
    }

    public static float[] ConvertColor(Color color)
    {
        return new float[] { color.r, color.g, color.b };
    }

    public Color GetTopColor()
    {
        return new Color(topColor[0], topColor[1], topColor[2]);
    }

    public Color GetBottomColor()
    {
        return new Color(bottomColor[0], bottomColor[1], bottomColor[2]);
    }

    public static Dictionary<SlimeElement, List<float[]>> allElementsDictionary;
    public static List<SlimeElement> allElementsList;
    public static List<Tuple<SlimeElement, float[]>> allElementTuples;

    public static Dictionary<List<SlimeElement>, List<Tuple<SlimeElement, SlimeElement>>> alliedPairs;
    public static Dictionary<List<SlimeElement>, List<Tuple<SlimeElement, SlimeElement>>> neutralPairs;
    public static Dictionary<List<SlimeElement>, List<Tuple<SlimeElement, SlimeElement>>> enemiedPairs;

    private static Tuple<SlimeElement, float[]>[] DetermineElement()
    {
        if(allElementTuples == null)
        {
            allElementTuples = new List<Tuple<SlimeElement, float[]>>();

            for(int i = 0; i < Enum.GetNames(typeof(SlimeElement)).Length; i++)
            {
                allElementTuples.Add(new Tuple<SlimeElement, float[]>((SlimeElement)i, null));
            }
        }

        return DetermineElement(allElementTuples);
    }

    private static Tuple<SlimeElement, float[]>[] DetermineElement(SlimeInformation parentOne, SlimeInformation parentTwo)
    {
        List<Tuple<SlimeElement, float[]>> outArray = new List<Tuple<SlimeElement, float[]>>
        {
            new Tuple<SlimeElement, float[]>(parentOne.elementOne, parentOne.topColor),
            new Tuple<SlimeElement, float[]>(parentOne.elementTwo, parentOne.bottomColor),
            new Tuple<SlimeElement, float[]>(parentTwo.elementOne, parentTwo.topColor),
            new Tuple<SlimeElement, float[]>(parentTwo.elementTwo, parentTwo.bottomColor)
        };

        return DetermineElement(outArray);
    }

    private static Tuple<SlimeElement, float[]>[] DetermineElement(List<Tuple<SlimeElement, float[]>> elements)
    {
        Tuple<SlimeElement, float[]>[] outArray = new Tuple<SlimeElement, float[]>[2];

        Dictionary<SlimeElement, List<float[]>> tempDictionary;
        List<SlimeElement> slimeElements;

        if (allElementTuples == null)
        {
            allElementTuples = new List<Tuple<SlimeElement, float[]>>();

            for (int i = 0; i < Enum.GetNames(typeof(SlimeElement)).Length; i++)
            {
                allElementTuples.Add(new Tuple<SlimeElement, float[]>((SlimeElement)i, null));
            }
        }

        bool isAllElements = elements.Count == allElementTuples.Count;

        if (isAllElements && allElementsDictionary != null && allElementsList != null)
        {
            tempDictionary = allElementsDictionary;
            slimeElements = allElementsList;
        }
        else
        {
            tempDictionary = new Dictionary<SlimeElement, List<float[]>>();
            slimeElements = new List<SlimeElement>();

            foreach (Tuple<SlimeElement, float[]> tuple in elements)
            {
                if (!slimeElements.Contains(tuple.Item1))
                {
                    slimeElements.Add(tuple.Item1);
                }

                if (tempDictionary.ContainsKey(tuple.Item1))
                {
                    tempDictionary[tuple.Item1].Add(tuple.Item2);
                }
                else
                {
                    tempDictionary.Add(tuple.Item1, new List<float[]>() { tuple.Item2 });
                }
            }

            if(isAllElements)
            {
                allElementsDictionary = tempDictionary;
                allElementsList = slimeElements;
            }
        }

        int random = UnityEngine.Random.Range(0, 100);

        Tuple<SlimeElement, SlimeElement> chosenElements = new Tuple<SlimeElement, SlimeElement>(SlimeElement.Fire, SlimeElement.Fire);

        if (random <= SAME_ELEMENT_CHANCE || slimeElements.Count <= 1)
        {
            chosenElements = createSameElementPair();
        }
        else if (random <= (SAME_ELEMENT_CHANCE + ALLY_ELEMENT_CHANCE))
        {
            chosenElements = createDifferentElementPair(0);
        }
        else if (random <= (SAME_ELEMENT_CHANCE + ALLY_ELEMENT_CHANCE + NEUTRAL_ELEMENT_CHANCE))
        {
            chosenElements = createDifferentElementPair(1);
        }
        else
        {
            chosenElements = createDifferentElementPair(2);
        }

        if(random % 2 == 1)
        {
            chosenElements = new Tuple<SlimeElement, SlimeElement>(chosenElements.Item2, chosenElements.Item1);
        }

        outArray[0] = new Tuple<SlimeElement, float[]>(chosenElements.Item1, tempDictionary[chosenElements.Item1][UnityEngine.Random.Range(0, tempDictionary[chosenElements.Item1].Count)]);
        outArray[1] = new Tuple<SlimeElement, float[]>(chosenElements.Item2, tempDictionary[chosenElements.Item2][UnityEngine.Random.Range(0, tempDictionary[chosenElements.Item2].Count)]);

        return outArray;

        Tuple<SlimeElement, SlimeElement> createSameElementPair()
        {
            SlimeElement chosenElement = tempDictionary.ElementAt(UnityEngine.Random.Range(0, tempDictionary.Count)).Key;
            return new Tuple<SlimeElement, SlimeElement>(chosenElement, chosenElement);
        }

        Tuple<SlimeElement, SlimeElement> createDifferentElementPair(int type)
        {
            Tuple<SlimeElement, SlimeElement> pair = GetPair(slimeElements, type);

            if(pair != null) { return pair; }

            return type == 0 ? createSameElementPair() : createDifferentElementPair(type - 1);
        }
    }

    private static bool IsElementAllied(Tuple<SlimeElement, SlimeElement> elementTuple) { return allyElements[elementTuple.Item1].Contains(elementTuple.Item2); }
    private static bool IsElementNeutral(Tuple<SlimeElement, SlimeElement> elementTuple) { return !IsElementAllied(elementTuple) && !IsElementEnemies(elementTuple); }
    private static bool IsElementEnemies(Tuple<SlimeElement, SlimeElement> elementTuple) { return enemyElements[elementTuple.Item1] == elementTuple.Item2; }

    private static Tuple<SlimeElement, SlimeElement> GetPair(List<SlimeElement> slimeElements, int type)
    {
        List<Tuple<SlimeElement, SlimeElement>> pairList = GetPairs(slimeElements, type);
        return pairList.Count > 0 ? pairList[UnityEngine.Random.Range(0, pairList.Count)] : null;
    }

    private static List<Tuple<SlimeElement, SlimeElement>> GetPairs(List<SlimeElement> slimeElements, int type)
    {
        Dictionary<List<SlimeElement>, List<Tuple<SlimeElement, SlimeElement>>> dictionaryToWorkWith;

        switch (type)
        {
            case 0:
                dictionaryToWorkWith = alliedPairs;
                break;
            case 1:
                dictionaryToWorkWith = neutralPairs;
                break;
            default:
                dictionaryToWorkWith = enemiedPairs;
                break;
        }

        if (dictionaryToWorkWith == null) { dictionaryToWorkWith = new Dictionary<List<SlimeElement>, List<Tuple<SlimeElement, SlimeElement>>>(); }
        if (dictionaryToWorkWith.ContainsKey(slimeElements)) { return enemiedPairs[slimeElements]; }

        List<Tuple<SlimeElement, SlimeElement>> pairList = new List<Tuple<SlimeElement, SlimeElement>>();

        foreach (SlimeElement elementOne in slimeElements)
        {
            foreach (SlimeElement elementTwo in slimeElements)
            {
                SlimeElement tempElementOne = elementOne > elementTwo ? elementOne : elementTwo;
                SlimeElement tempElementTwo = elementOne > elementTwo ? elementTwo : elementOne;

                Tuple<SlimeElement, SlimeElement> newTuple = new Tuple<SlimeElement, SlimeElement>(tempElementOne, tempElementTwo);

                bool check = false;

                switch(type)
                {
                    case 0:
                        check = IsElementAllied(newTuple);
                        break;
                    case 1:
                        check = IsElementNeutral(newTuple);
                        break;
                    case 2:
                        check = IsElementEnemies(newTuple);
                        break;
                }

                if (!pairList.Contains(newTuple) && check)
                {
                    pairList.Add(newTuple);
                }
            }
        }

        dictionaryToWorkWith.Add(slimeElements, pairList);

        return pairList;
    }
}