using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    public GameObject gameBackground;
    public GameObject Planets;
    public GameObject AsteroidPrefab;
    public GameObject EnemyPrefab;
    public float asteroidSpawnWaitSeconds;
    public float enemyIntervalSpawnWaitSeconds = 4;
    public Canvas canvas;
    public Text scoreTextPrefab;
    public Text gameOverTextPrefab;
    public Text RestartTextPrefab;
    public Text enemyCounttPrefab;
    private GameObject _enemyCount;
    public RawImage winImage;
    private GameObject _winImage;
    public Text levelText;
    private GameObject _level;
    public Text win;
    private GameObject _winning;
    public GameObject chat;
    private GameObject _chat;
    private float showLevelFor4Sec;
    private GameObject _scoreText;
    private GameObject _gameOverText;
    private GameObject _RestartText;

    private GameObject circule;
    private bool gameOver;
    private bool restart;

    [SyncVar(hook = "updateScoreGUI")]
    private int score;

    private int maxAllowedLevels;
    private bool shouldAdvanceLevel;

    [SyncVar(hook = "showLevelText")]
    private int level;
    private bool playersInGameExist;
    private bool escape;

    private bool extraRocket;

    [SyncVar]
    private bool speedGift;

    private GameObject AsteroidsHolder;
    [SyncVar]
    private Vector3 _AsteroidDirection;

    public Vector3 AsteroidDirection { get { return _AsteroidDirection; } }
    private Vector3 startSpawn;

    [SyncVar(hook = "showEnemiesCount")]
    private int enemiesAlive;

    [SyncVar]
    public bool isGameOver;

    private uint[] playersIndex = new uint[10]{0,0,0,0,0,0,0,0,0,0};



    private float scoreCounter;
    private bool showMessgae;
    void loadGUI()
    {
        GameObject canvasObject = Instantiate(canvas).gameObject;
        RectTransform rTransform = canvasObject.GetComponent<RectTransform>();

        _scoreText = Instantiate(scoreTextPrefab.gameObject);
        _scoreText.transform.SetParent(rTransform, false);

        _gameOverText = Instantiate(gameOverTextPrefab.gameObject);
        _gameOverText.transform.SetParent(rTransform, false);

        _RestartText = Instantiate(RestartTextPrefab.gameObject);
        _RestartText.transform.SetParent(rTransform, false);

        _level = Instantiate(levelText.gameObject);
        _level.transform.SetParent(rTransform, false);

        _winning = Instantiate(win.gameObject);
        _winning.transform.SetParent(rTransform, false);

        _winImage = Instantiate(winImage.gameObject);
        _winImage.transform.SetParent(rTransform, false);

        _chat = Instantiate(chat.gameObject);
        _chat.transform.SetParent(rTransform, false);

        _enemyCount = Instantiate(enemyCounttPrefab.gameObject);
        _enemyCount.transform.SetParent(rTransform, false);

        if (isServer)
        {
            enemiesAlive = 0;
            score = 0;
        }
    }

    public void playGiftSound()
    {
        GetComponents<AudioSource>()[1].Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
        scoreCounter = 0;
        if (Planets == null || gameBackground == null || AsteroidPrefab == null) throw new MissingReferenceException();
        // specific to the LOCAL PLAYER
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        loadGUI();

        // specific to the LOCAL PLAYER (For Now)
        updateScoreGUI(score);
        showEnemiesCount(enemiesAlive);
        gameOver = false;
        restart = false;
        escape = true;
        _gameOverText.GetComponent<Text>().text = "";
        _RestartText.GetComponent<Text>().text = "";
        _winImage.GetComponent<RawImage>().enabled = false;
        _winning.GetComponent<Text>().text = "";
        if (isServer == false)
        {
            AsteroidsHolder = new GameObject("Asteroid Holder");
            CollectAsteroids();
        }

        // everything below is specific to the SERVER
        if (isServer == false) return;
        AsteroidsHolder = new GameObject("Asteroid Holder");

        SpawnAsteroids();

        // okay, let's initiate these planets locations as the SERVER
        InitiatePlanetLocationsOnServerAndUpdateForClients();
        maxAllowedLevels = Planets.transform.childCount;
        level = 1;
        shouldAdvanceLevel = false;
        playersInGameExist = false;
        StartCoroutine (LevelSystem());
    }

    private void SpawnAsteroids()
    {
        (_AsteroidDirection, startSpawn) = decideAsteroidDirection();
        StartCoroutine(SpawnAsteroidsHelper());
    }

    private void CollectAsteroids()
    {
        GameObject[] spawnedAsteroids = GameObject.FindGameObjectsWithTag(Utils.TagAsteroid);
        foreach (GameObject o in spawnedAsteroids)
        {
            o.transform.parent = AsteroidsHolder.transform;
        }
    }

    private (Vector3, Vector3) decideAsteroidDirection()
    {
        float radius = Utils.getBackgroundRadius(gameBackground);
        Vector3 startSpawn = Utils.getRandomDirection() * radius;
        while (startSpawn.x == 35.0f && startSpawn.y == 35.0f && startSpawn.z == 35.0f)
            startSpawn = Utils.getRandomDirection() * radius;
        return (
            (new Vector3(35.0f, 35.0f, 35.0f) - startSpawn).normalized,
             startSpawn);
    }

    private void InitiatePlanetLocationsOnServerAndUpdateForClients()
    {
        float radius = Utils.getGameBoundaryRadius(gameBackground) + 25.0f;
        for (int i = 0, size = Planets.transform.childCount; i < size; ++i)
        {
            GameObject planet = Planets.transform.GetChild(i).gameObject;
            // generate random location
            Vector3 direction = Utils.getRandomDirection();

            // assign random location
            planet.transform.position = direction * radius;
            planet.SetActive(false);
            // let's update ALL of the clients, to change their planet locations according to the SERVER.
            RpcUpdatePlanetLocationsOnClient(i, planet.transform.position, planet.transform.rotation, planet.activeSelf);
        }
    }

    [ClientRpc]
    private void RpcUpdatePlanetLocationsOnClient(int childPlanetIndex, Vector3 planetPosition, Quaternion planetRotation, bool planetIsActive)
    {
        Transform planetTransform = Planets.transform.GetChild(childPlanetIndex);
        planetTransform.position = planetPosition;
        planetTransform.rotation = planetRotation;
        planetTransform.gameObject.SetActive(planetIsActive);
    }

    IEnumerator LevelSystem()
    {
        if (isServer == false) throw new Utils.AttemptedUnauthorizedAccessLevelSystemException("hasAuthority = " + hasAuthority + ", isLocalPlayer = " + isLocalPlayer + ", isServer = " + isServer + ".");
        yield return new WaitUntil(() => playersInGameExist);
        while (level <= maxAllowedLevels)
        {
            shouldAdvanceLevel = false;
            SpawnPlanets();
            showLevelFor4Sec = 0f;
            StartCoroutine(SpawnLevel(level));
            yield return new WaitUntil(() => shouldAdvanceLevel == true);
            ++level;
        }
        // Win Game! => Define Behaviour!
        Win();
    }

    private void Win()
    {
        //_winImage.GetComponent<RawImage>().enabled = true;
        _winning.GetComponent<Text>().text = "Thats great :)\n You just won the game!";
        GetComponents<AudioSource>()[2].Play();
        GameObject[] players = GameObject.FindGameObjectsWithTag(Utils.TagPlayer);
        foreach (GameObject player in players)
        {
            GameObject fireworks = FindChildGameObjectWithTag(player, Utils.TagFireworks);
            if (fireworks == null) { Debug.LogError("Failed to find fireworks for the player"); break; }
            fireworks.SetActive(true);
        }
    }

    GameObject FindChildGameObjectWithTag(GameObject parent, string tag)
    {
        foreach (Transform o in parent.transform)
        {
            if (o.tag == tag) return o.gameObject;
        }

        return null;
    }

    void showLevelText(int newLevel)
    {
        StartCoroutine(showLevelTextS(newLevel));
    }

    IEnumerator showLevelTextS(int newLevel)
    {
        if (newLevel <= maxAllowedLevels)
            _level.GetComponent<Text>().text = "LEVEL " + newLevel + "!";
        yield return new WaitForSeconds(4.0f);
        _level.GetComponent<Text>().text = "";
    }

    IEnumerator SpawnLevel(int level)
    {
        enemiesAlive = 0;
        // spawn some enemies
        foreach (Transform child in Planets.transform)
        {
            if (child.gameObject.activeSelf == false) continue;
            int enemiesToAdd = level;// * 3;
            StartCoroutine (SpawnEnemiesFromPlanet(child, enemiesToAdd));
        }
        yield return new WaitUntil(() => enemiesAlive == 0);
        shouldAdvanceLevel = true;
    }
    
    IEnumerator SpawnEnemiesFromPlanet(Transform planet, int amount)
    {
        for (int i = 0; i < amount; ++i)
        {
            CmdSpawnEnemy(planet.position, Quaternion.identity);
            enemiesAlive++;
            yield return new WaitForSeconds(enemyIntervalSpawnWaitSeconds);
        }
    }

    [Command]
    private void CmdSpawnEnemy(Vector3 startingPosition, Quaternion startingRotation)
    {
        GameObject newEnemy = Instantiate(EnemyPrefab, startingPosition, startingRotation);
        NetworkServer.Spawn(newEnemy);
    }

    void SpawnPlanets()
    {
        int currentLevel = level;
        if (currentLevel <= 0 || currentLevel > maxAllowedLevels) return;
        for (int i = 0, size = Planets.transform.childCount; i < size; ++i)
        {
            Transform planetTransform = Planets.transform.GetChild(i);
            if (currentLevel == 0) break;
            planetTransform.gameObject.SetActive(true);
            currentLevel--;
            RpcUpdatePlanetLocationsOnClient(i, planetTransform.position, planetTransform.rotation, planetTransform.gameObject.activeSelf);
        }
    }

    private void Update()
    {
        playersInGameExist = GameObject.FindGameObjectWithTag(Utils.TagPlayer) != null;
        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        if (escape)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                escape = false;
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                escape = true;
            }
        }
        scoreCounter += Time.deltaTime;
        if (scoreCounter > 1) { _scoreText.GetComponent<Text>().color = Color.white; _scoreText.GetComponent<Text>().fontSize -= 5; }
    }
    void showEnemiesCount(int newValue)
    {
        _enemyCount.GetComponent<Text>().text = "Enemies left: " + newValue;
    }

    IEnumerator SpawnAsteroidsHelper()
    {
        float distance = 12.0f;
        float radius = Utils.getBackgroundRadius(gameBackground);
        Vector3 direction = _AsteroidDirection;

        // spawn the first 800
        for (int i = 0; i < 750; i += 5)
        {
            Vector3 inc = new Vector3(direction.x * i, direction.y * i, direction.z * i);
            if (Random.value <= 0.5)
                CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + inc, Quaternion.identity);

            CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + inc + Random.insideUnitSphere * distance, Quaternion.identity);
            CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + inc + Random.insideUnitSphere * distance, Quaternion.identity);
            CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + inc + Random.insideUnitSphere * distance, Quaternion.identity);

            if (Random.value <= 0.3)
                CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + inc + Random.insideUnitSphere * distance, Quaternion.identity);
        }

        // spawn endless Asteroids from startSpawn
        while (true)
        {
            if (Random.value <= 0.5)
                CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn, Quaternion.identity);

            CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + Random.insideUnitSphere * distance, Quaternion.identity);
            CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + Random.insideUnitSphere * distance, Quaternion.identity);
            CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + Random.insideUnitSphere * distance, Quaternion.identity);


            if (Random.value <= 0.3)
                CmdInstantiateAsteroidOnServerThenUpdateClient(startSpawn + Random.insideUnitSphere * distance, Quaternion.identity);

            RpcUpdateAsteroidHolder();
            yield return new WaitForSeconds(asteroidSpawnWaitSeconds);
        }
    }

    [ClientRpc]
    private void RpcUpdateAsteroidHolder()
    {
        AsteroidsHolder.name = "Asteroid Holder (" + AsteroidsHolder.transform.childCount + ")";
    }

    [Command]
    private void CmdInstantiateAsteroidOnServerThenUpdateClient(Vector3 startingPosition, Quaternion startingRotation)
    {
        if (isServer == false) throw new System.Exception("Unauthorized Access to instantiating Asteroids");
        GameObject asteroid = Instantiate(AsteroidPrefab, startingPosition, startingRotation);
        asteroid.transform.parent = AsteroidsHolder.transform;
        NetworkServer.Spawn(asteroid);
        RpcInstantiateAsteroid(asteroid.GetComponent<NetworkIdentity>(), asteroid.transform.position, asteroid.transform.rotation);
    }

    [ClientRpc]
    private void RpcInstantiateAsteroid(NetworkIdentity asteroidIdentity, Vector3 startingPosition, Quaternion startingRotation)
    {
        if (asteroidIdentity == null) throw new MissingComponentException("Error: Asteroid did not have NetworkIdentity");
        asteroidIdentity.gameObject.transform.parent = AsteroidsHolder.transform;
        asteroidIdentity.gameObject.transform.position = startingPosition;
        asteroidIdentity.gameObject.transform.rotation = startingRotation;
    }


    //please dont rename this function
    void updateScoreGUI(int newScore)
    {
        score = newScore;
        _scoreText.GetComponent<Text>().text = "Combined Score: " + score;
        _scoreText.GetComponent<Text>().color = Color.yellow;
        _scoreText.GetComponent<Text>().fontSize += 5;
        scoreCounter = 0;
    }

    public void addScore (int newScore)
    {

        if (!isServer)
            CmdSetScore(newScore);
        else
            score += newScore;
    }

    [Command]
    private void CmdSetScore(int newScore)
    {
        score += newScore;
    }

    public void GameOverFunction()
    {
        if (isGameOver) return;
        if (isServer == true)
        {
            GameOverFunctionHelper();
            RpcGameOverFunction();
        }
        else
        {
            CmdGameOverFunction();
        }
    }

    [Command]
    private void CmdGameOverFunction()
    {
        GameOverFunctionHelper();
        RpcGameOverFunction();
    }

    [ClientRpc]
    private void RpcGameOverFunction()
    {
        GameOverFunctionHelper();
    }

    private void GameOverFunctionHelper()
    {
        _level.SetActive(false);
        _enemyCount.SetActive(false);
        _gameOverText.GetComponent<Text>().text = "Game Over!";
        //_RestartText.GetComponent<Text>().text = "Press 'R' for restart";
        //restart = true;
        gameOver = true;
        AudioListener audioListener = GetComponent<AudioListener>();
        audioListener.enabled = true;
        AudioSource[] allAudioSources;
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource audioS in allAudioSources)
        {
            audioS.Stop();
        }
        AudioSource audioData = GetComponent<AudioSource>();
        audioData.Play();

        // most important update of all:
        isGameOver = true;
    }

    public void setExtraRocket(bool status) {
        extraRocket = status;
        if (isServer == true && status == true)
            RpcUpdateExtraRocketStatus(status);
    }

    [ClientRpc]
    private void RpcUpdateExtraRocketStatus(bool status)
    {
        extraRocket = status;
    }

    public bool getExtraRocketStatus() { return extraRocket; }
    public bool getSpeedGift() { return speedGift; }
    public void enemyKilled() {
        if (isServer == false)
            CmdDecreaseEnemies();
        else
            enemiesAlive--;
    }

    [Command]
    private void CmdDecreaseEnemies()
    {
        enemiesAlive--;
    }
    
    public void setSpeedGift(bool status) {
        if (isServer == false)
            CmdUpdateSpeedGiftStatus(status);
        else
            speedGift = status;
    }

    [Command]
    private void CmdUpdateSpeedGiftStatus(bool status)
    {
        speedGift = status;
    }

    public void setShowMessage(bool status) { showMessgae = status; }
    public bool getShowMessage() {return showMessgae; }
}
