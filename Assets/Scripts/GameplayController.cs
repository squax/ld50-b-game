using DG.Tweening;
using Squax.Patterns;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private TMPWobbleEffect infoHibernationLabel;

    [SerializeField]
    private TextMeshProUGUI topLeftLabel;

    [SerializeField]
    private TextMeshProUGUI buttonLabel;

    [SerializeField]
    private Button button;

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

    [SerializeField]
    private GameObject scoreItem;

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

    [Header("Audio")]
    [SerializeField]
    private AudioClip winter;

    [SerializeField]
    private AudioClip summer;

    private GameObject dynamicLevelLayer;
    private List<Transform> collectables = new List<Transform>();
    private Transform closestCollectable = null;
    private AudioSource audioBG;

    private int totalHoneyReserves = 0;
    private float timer = 60f;
    private int currentYear = 0;
    private int totalCollectables;

    void Start()
    {
        audioBG = GetComponent<AudioSource>();
        totalCollectables = totalTreeClusters * totalTreeClusters * totalFlowerClusters * 10;

        GameObjectPoolManager.Instance.Register("Score", scoreItem, transform, 100);

        ChangeScreenState(GameState.Winter);
    }

    public void AddHoneyCollected(int amount, Vector3 position)
    {
        totalHoneyReserves += amount;

        scoreLabel.text = totalHoneyReserves.ToString("#,##0");

        scoreLabel.transform.DOKill(true);
        scoreLabel.transform.DOPunchPosition(new Vector3(0, 0.08f, 0), 0.3f);

        var scoreSpawn = GameObjectPoolManager.Instance.Spawn("Score", position + new Vector3(Random.Range(-0.16f, 0.16f), 0.32f, 0f), Quaternion.identity, true, 1.0f);

        if(scoreSpawn != null)
        {
            scoreSpawn.transform.DOKill();
            scoreSpawn.transform.localScale = Vector3.one;

            scoreSpawn.transform.DOPunchScale(Vector3.one, 0.4f, 3, 0.3f).OnComplete(()=>
            {
                scoreSpawn.transform.DOScale(Vector3.zero, 0.3f).SetDelay(0.2f);
            });

            var tmp = scoreSpawn.GetComponent<TextMeshPro>();

            if(tmp != null)
            {
                tmp.text = amount + "";
            }
        }
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

            float arc = 360f / (float)totalFlowerClusters;
            float angle = 0f;
            for (int i = 0; i < totalFlowerClusters; ++i)
            {
                var flower = Instantiate(flowerPrefabs[Random.Range(0, flowerPrefabs.Length)]);
                flower.transform.parent = tree.transform;
                //flower.transform.localPosition = Random.insideUnitCircle;

                var dir = new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle));

                flower.transform.localPosition = dir;

                angle += arc;

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
        AudioManager.Instance.PlayOneShot("Heal", 1f);

        infoHibernationLabel.SetText("Loading..");

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
            audioBG.clip = summer;
            audioBG.Play();

            infoHibernationLabel.SetText("");
            button.gameObject.SetActive(false);

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
            audioBG.clip = winter;
            audioBG.Play();

            button.gameObject.SetActive(true);
            buttonLabel.text = "End Hibernation";

            button.transform.DOKill(true);
            button.transform.localRotation = Quaternion.identity;
            button.transform.DOPunchRotation(new Vector3(0, 0, 2), 1.6f).SetDelay(2f).SetLoops(-1, LoopType.Yoyo);

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

            topLeftLabel.text = "<b>Winter: Year " + currentYear + "</b><br><size=80%>" + totalHoneyReserves.ToString("#,##0") + " Honey in Storage.</size><br><size=60%>Required: " + requiredToSurvive.ToString("#,##0") + "</size>";
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

                scoreLabel.text = totalHoneyReserves.ToString("#,##0");
            }

            infoHibernationLabel.SetText(text);

            titleScreen.SetActive(true);

            titleScreenRenderer.material.DOColor(new Color(1, 1, 1, 1), 1.0f).OnComplete(() =>
            {
                gameplayScreen.SetActive(false);
            });
        }

        if (currentGameState == GameState.GameOver)
        {
            audioBG.clip = winter;
            audioBG.Play();

            buttonLabel.text = "Replay?";

            totalHoneyReserves = 0;
            currentYear = 0;

            topLeftLabel.text = "You ran out of Honey.<br><size=80%>The hive didn't survive the Winter.</size>";
            infoHibernationLabel.SetText("<b><size=150%>Game Over</size></b><br>Thanks for playing!<br><size=80%>Game Maker: <color=orange>@squaxcom</color></size>");
            scoreLabel.text = "Hi B,<br>Follow Owl";

            titleScreen.SetActive(true);

            titleScreenRenderer.material.DOColor(new Color(1, 1, 1, 1), 1.0f).OnComplete(() =>
            {
                gameplayScreen.SetActive(false);
            });
        }
    }
}
