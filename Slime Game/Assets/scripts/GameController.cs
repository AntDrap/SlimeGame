using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public LayerMask slimeMask, groundMask;
    public GameObject slimePrefab;
    public Transform[] slimeSpawnPoints;
    public int slimesToSpawnPerDay;
    public SlimeElementalData slimeElementalData;

    private Camera mainCamera;
    private Coroutine cameraMove;
    private Transform cameraOrigin;

    public SlimeBehavior currentSlimeBeingLookedAt;

    public bool inputBlocked;

    public static GameController instance;
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 120;
    }

    void Start()
    {
        mainCamera = Camera.main;
        cameraOrigin = new GameObject("cameraOrigin").transform;
        cameraOrigin.position = mainCamera.transform.position;
        cameraOrigin.rotation = mainCamera.transform.rotation;

        bool newDay = SaveManager.UpdatePlayerLogin();

        List<SlimeInformation> slimeInformation = SaveManager.GetSlimesInField();
        List<SlimeBehavior> slimeBehaviors = new List<SlimeBehavior>();

        List<Transform> spawnPoints = new List<Transform>(slimeSpawnPoints);

        for (int i = 0; i < (newDay ? slimesToSpawnPerDay : slimeInformation.Count); i++)
        {
            if(spawnPoints.Count <= 0)
            {
                spawnPoints.AddRange(slimeSpawnPoints);
            }

            Transform slimeSpawn = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
            spawnPoints.Remove(slimeSpawn);

            SlimeBehavior newSlime = Instantiate(slimePrefab, slimeSpawn.position, Quaternion.Euler(new Vector3(-90, 0, 0))).GetComponent<SlimeBehavior>();
            
            if(newDay)
            {
                newSlime.Randomize();
            }
            else
            {
                newSlime.SetGenes(slimeInformation[i]);
            }

            slimeBehaviors.Add(newSlime);
        }

        SaveManager.CreateSlimesInField(slimeBehaviors);
    }

    public void MoveCamera()
    {
        MoveCamera(cameraOrigin, true);
    }

    public void MoveCamera(Transform newTransform, bool waitForMainCamera = false)
    {
        if (cameraMove != null) { StopCoroutine(cameraMove); }
        cameraMove = StartCoroutine(MoveCameraCoroutine());

        IEnumerator MoveCameraCoroutine()
        {
            float t = 0;
            Vector3 cameraStartPosition = mainCamera.transform.position;
            Quaternion cameraStartRotation = mainCamera.transform.rotation;

            while (true && newTransform)
            {
                if(waitForMainCamera && !UIBehavior.mainCameraEnabled)
                {
                    yield return new WaitUntil(() => UIBehavior.mainCameraEnabled);
                }

                t += Time.fixedDeltaTime;
                if(newTransform)
                {
                    mainCamera.transform.position = Vector3.Lerp(cameraStartPosition, newTransform.position, t);
                    mainCamera.transform.rotation = Quaternion.Lerp(cameraStartRotation, newTransform.rotation, t);
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }

    public void AddSlimeToCollection()
    {
        if(currentSlimeBeingLookedAt)
        {
            SlimeBehavior currentSlime = currentSlimeBeingLookedAt;
            SlimeBehavior.StopLookingAtSlime();
            AddSlimeToCollection(currentSlime);
        }
    }

    public void AddSlimeToCollection(SlimeBehavior slimeBehavior)
    {
        SaveManager.AddSlimeToPlayerInventoryFromField(slimeBehavior);
    }

    void Update()
    {
        if(inputBlocked) { return; }

        if(Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, slimeMask) && hit.transform.TryGetComponent(out SlimeBehavior sb))
            {
                SlimeBehavior.LookAtSlime(sb);
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape) && currentSlimeBeingLookedAt)
        {
            SlimeBehavior.StopLookingAtSlime();
        }

        if (Input.GetMouseButtonDown(2))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
            {
                SlimeBehavior sb = Instantiate(slimePrefab, hit.point, Quaternion.Euler(new Vector3(-90, 0, 0))).GetComponent<SlimeBehavior>();
                sb.slimeInformation.Randomize();
            }
        }
    }

    public void ResetGame()
    {
        SaveManager.EraseSaveData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public static void BlockInput(bool toggle)
    {
        if(instance)
        {
            instance.inputBlocked = toggle;
        }
    }
}