using DG.Tweening;
using Squax.Patterns;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameplayController : UnitySingleton<GameplayController>
{
    public enum GameState
    {
        TitleScreen,
        Summer,
        Winter,
        GameOver
    }

    private GameState currentGameState = GameState.TitleScreen;

    [SerializeField]
    private GameObject titleScreen;

    [SerializeField]
    private Renderer titleScreenRenderer;

    [SerializeField]
    private GameObject gameplayScreen;

    [SerializeField]
    private Transform queenBee;

    [SerializeField]
    private Transform portableHive;

    [SerializeField]
    private Transform casowlyFlowerFinder;

    [SerializeField]
    private Transform sunFinder;

    [SerializeField]
    private TextMeshPro scoreLabel;

    [SerializeField]
    private TextMeshProUGUI infoHibernationLabel;

    [SerializeField]
    private TextMeshProUGUI topLeftLabel;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject workerBeeBoidPrefab;

    [SerializeField]
    private GameObject collectablePrefab;

    [SerializeField]
    private GameObject angryCloudPrefab;

    [SerializeField]
    private GameObject[] flowerPrefabs;

    [SerializeField]
    private GameObject[] treePrefabs;

    [Header("World Generation")]
    [SerializeField]
    private float startingSpawnRadius = 2;

    [SerializeField]
    private int totalTreeClusters = 12;

    [SerializeField]
    private int totalFlowerClusters = 3;

    [SerializeField]
    private int totalFlowerCollectables = 10;

    [SerializeField]
    private float sesssionLength = 60f;

    [SerializeField]
    private string[] gameHelp;

    [SerializeField]
    private string[] randomBeeFacts;

    private GameObject dynamicLevelLayer;
    private List<Transform> collectables = new List<Transform>();
    private Transform closestCollectable = null;

    private int totalHoneyReserves = 0;
    private float timer = 60f;
    private int currentYear = 0;
    private int totalCollectables;

    void Start()
    {
        totalCollectables = totalTreeClusters * totalTreeClusters * totalFlowerClusters * 10;

        ChangeScreenState(GameState.Winter);
    }

    public void AddHoneyCollected(int amount)
    {
        totalHoneyReserves += amount;

        scoreLabel.text = totalHoneyReserves.ToString("#,##0");

        scoreLabel.transform.DOKill(true);
        scoreLabel.transform.DOPunchPosition(new Vector3(0, 0.08f, 0), 0.3f);
    }

    void BuildRandomWorld()
    {
        if(dynamicLevelLayer != null)
        {
            Destroy(dynamicLevelLayer);
        }

        dynamicLevelLayer = new GameObject("Content Generation");
        dynamicLevelLayer.transform.parent = gameplayScreen.transform;

        for (int n = 0; n < 10; ++n)
        {
            var boid = BoidController.Instance.CreateBoid(Vector2.zero + Random.insideUnitCircle, workerBeeBoidPrefab);

            boid.transform.parent = dynamicLevelLayer.transform;
        }

        Vector2 lastTreePosition = Vector2.zero;
        collectables.Clear();

        for (int n = 0; n < totalTreeClusters; ++n)
        {
            var position = Random.insideUnitCircle * startingSpawnRadius;

            var tree = Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)]);
            tree.transform.parent = dynamicLevelLayer.transform;
            tree.transform.position = position;

            for (int i = 0; i < totalFlowerClusters; ++i)
            {
                var flower = Instantiate(flowerPrefabs[Random.Range(0, flowerPrefabs.Length)]);
                flower.transform.parent = tree.transform;
                flower.transform.localPosition = Random.insideUnitCircle;

                for (int y = 0; y < totalFlowerCollectables; ++y)
                {
                    var collectable = Instantiate(collectablePrefab);
                    collectable.transform.parent = flower.transform;
                    collectable.transform.localPosition = Random.insideUnitCircle;

                    collectables.Add(collectable.transform);
                }
            }

            if(n > 0)
            {
                var stormCloud = Instantiate(angryCloudPrefab);
                stormCloud.transform.parent = dynamicLevelLayer.transform;
                stormCloud.transform.position = lastTreePosition + (position - lastTreePosition) * 0.5f;
            }

            lastTreePosition = position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(currentGameState)
        {
            case GameState.TitleScreen:
                UpdateTitleScreen();
                break;

            case GameState.Summer:
                UpdateSummer();
                break;

            case GameState.Winter:
                UpdateWinter();
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (currentGameState)
        {
            case GameState.Summer:
                FixedUpdateSummer();
                break;
        }
    }

    void UpdateTitleScreen()
    {
    }

    void UpdateSummer()
    {
        timer -= Time.deltaTime;

        if(timer <= 0)
        {
            ChangeScreenState(GameState.Winter);

            return;
        }

        topLeftLabel.text = "" + timer.ToString("#,##0");

        if (Input.GetMouseButton(0))
        {
            // Queen bee movement.
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = UnityEngine.Camera.main.nearClipPlane;
            var worldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(mousePos);
            worldPosition.z = 0;

            queenBee.DOKill();
            queenBee.DOMove(worldPosition, 1.0f);

            BoidController.Instance.transform.position = queenBee.position;
        }

        // Find a target for casowly.
        var minDistance = float.MaxValue;
        closestCollectable = null;
        foreach (var collectable in collectables)
        {
            if (collectable == null) continue;

            var delta = casowlyFlowerFinder.position - collectable.position;

            if(delta.sqrMagnitude < minDistance)
            {
                minDistance = delta.sqrMagnitude;
                closestCollectable = collectable;
            }
        }
    }

    void UpdateWinter()
    {
    }

    public void Hibernate()
    {
        ChangeScreenState(GameState.Summer);
    }

    void FixedUpdateSummer()
    {
        if (closestCollectable != null)
        {
            var delta = closestCollectable.position - casowlyFlowerFinder.position;
            casowlyFlowerFinder.position = Vector3.Lerp(casowlyFlowerFinder.position, queenBee.position + delta.normalized * 0.64f, Time.deltaTime);
        }
        else
        {
            casowlyFlowerFinder.position = Vector3.Lerp(casowlyFlowerFinder.position, queenBee.position, Time.deltaTime);
        }

        var sunDelta = Vector3.zero - queenBee.position;

        sunFinder.position = Vector3.Lerp(sunFinder.position, portableHive.position + sunDelta.normalized, Time.deltaTime);

        portableHive.position = Vector3.Lerp(portableHive.position, queenBee.position, Time.deltaTime);
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(queenBee.position.x, queenBee.position.y, Camera.main.transform.position.z), Time.deltaTime);
    }

    private void ChangeScreenState(GameState newState)
    {
        if (currentGameState == newState) return;

        currentGameState = newState;

        if (currentGameState == GameState.Summer)
        {
            BuildRandomWorld();

            ++currentYear;

            timer = sesssionLength + 3f;

            gameplayScreen.SetActive(true);

            topLeftLabel.color = Color.black;

            titleScreenRenderer.material.DOColor(new Color(0, 0, 0, 0), 1.0f).SetDelay(2f).OnComplete(() =>
            {
                titleScreen.SetActive(false);
            });
        }

        if (currentGameState == GameState.Winter)
        {
            var text = "";

            if (currentYear < gameHelp.Length)
            {
                text = gameHelp[currentYear];
            }
            else
            {
                text = randomBeeFacts[Random.Range(0, randomBeeFacts.Length)];
            }

            int requiredToSurvive = (totalCollectables / 10) + (currentYear * 100);

            topLeftLabel.text = "<b>Year " + currentYear + "</b><br>" + totalHoneyReserves.ToString("#,##0") + " Honey in Storage.<br>Required to Survive: " + requiredToSurvive.ToString("#,##0");
            topLeftLabel.color = Color.white;

            if (currentYear > 0)
            {
                if (requiredToSurvive > totalHoneyReserves)
                {
                    // Game over.
                    ChangeScreenState(GameState.GameOver);

                    return;
                }

                totalHoneyReserves -= requiredToSurvive;
            }

            infoHibernationLabel.text = text;

            titleScreen.SetActive(true);

            titleScreenRenderer.material.DOColor(new Color(1, 1, 1, 1), 1.0f).OnComplete(() =>
            {
                gameplayScreen.SetActive(false);
            });
        }

        if (currentGameState == GameState.GameOver)
        {
            totalHoneyReserves = 0;
            currentYear = 0;

            topLeftLabel.text = "You ran out of Honey.<br>The hive didn't survive the Winter.";
            infoHibernationLabel.text = "<b>Game Over</b><br>Thanks for playing!";

            titleScreen.SetActive(true);

            titleScreenRenderer.material.DOColor(new Color(1, 1, 1, 1), 1.0f).OnComplete(() =>
            {
                gameplayScreen.SetActive(false);
            });
        }
    }
}