using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Movement : NetworkBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;
    public GameObject cameras;
    public float cameraSwitchRate;
    public Text rocket2Text;
    private float camSwitchTime;
    public Canvas canvas;
    public RawImage AstroPrefab;
    public RawImage masterRocket1Prefab;
    public RawImage masterRocket2Prefab;
    public RawImage masterRocket3Prefab;
    public RawImage masterRocketExtraPrefab;
    public Text welcomePrefab;
    public Image redImage;
    public Text redMessage;
    public Text welcomeMessagePrefab;
    public InputField mainInputFieldPrefab;

    private GameObject _Astro;
    private GameObject _masterRocket1;
    private GameObject _masterRocket2;
    private GameObject _masterRocket3;
    private GameObject _masterRocketExtra;
    private GameObject _welcome;
    private GameObject _welcomeMessage;
    private GameObject _redMessage;
    private GameObject _redPicture;
    private GameObject mainInputField;
    private bool showWelcomeMessage;

    public GameObject rocket1;
    public Transform rocket1Shot;
    public GameObject rocket2;
    public Transform rocket2Shot;
    public float fireRate;
    private float nextFire;
    private float shotElapsedTime;
    private bool canShootRocket2;
    private int masterRocketsCount;
    private GameController gameController;
    private bool shouldStart = true;
    private float extraSpeeedTime;
    private float radious;
    private float waitSeconds;
    private Image image;
    private AudioSource dangerAudioSource;
    private Text dangerText;
    private bool shouldPlayDanger;

    private string helpMeMessagePress1 = "Help me I'm dying!";
    private string comeHerePress2 = "come here";
    private string HurayPress3 = "Huray :)";
    private bool userTyping;
    private bool sendMessageOnce;

    private Color[] colors = new Color[] { Color.blue, Color.cyan, Color.magenta, Color.red, Color.white, Color.yellow };
    private Color chatColor;

    public override void OnStartAuthority()
    {
        Start();
    }

    void loadUI()
    {
        GameObject canvasObject = Instantiate(canvas).gameObject;
        RectTransform rTransform = canvasObject.GetComponent<RectTransform>();
        _Astro = Instantiate(AstroPrefab.gameObject);
        _Astro.transform.SetParent(rTransform, false);

        _masterRocket1 = Instantiate(masterRocket1Prefab.gameObject);
        _masterRocket1.transform.SetParent(rTransform, false);

        _masterRocket2 = Instantiate(masterRocket2Prefab.gameObject);
        _masterRocket2.transform.SetParent(rTransform, false);

        _masterRocket3 = Instantiate(masterRocket3Prefab.gameObject);
        _masterRocket3.transform.SetParent(rTransform, false);

        _masterRocketExtra = Instantiate(masterRocketExtraPrefab.gameObject);
        _masterRocketExtra.transform.SetParent(rTransform, false);

        _welcome = Instantiate(welcomePrefab.gameObject);
        _welcome.transform.SetParent(rTransform, false);

        _redMessage = Instantiate(redMessage.gameObject);
        _redMessage.transform.SetParent(rTransform, false);

        _redPicture = Instantiate(redImage.gameObject);
        _redPicture.transform.SetParent(rTransform, false);

        _welcomeMessage = Instantiate(welcomeMessagePrefab.gameObject);
        _welcomeMessage.transform.SetParent(rTransform, false);

        mainInputField = Instantiate(mainInputFieldPrefab.gameObject);
        mainInputField.transform.SetParent(rTransform, false);

        chatColor = getRandomColor();
    }

    private Color getRandomColor() { return colors[Random.Range(0, colors.Length)]; }

    private void InitializeRedAlertVariables()
    {
        waitSeconds = 0.0f;
        radious = Utils.getGameBoundaryRadius(GameObject.FindGameObjectWithTag(Utils.TagBackground));
        image = _redPicture.GetComponent<Image>();
        dangerText = _redMessage.GetComponent<Text>();
        dangerText.text = "";
        Color tmpColor = image.color;
        tmpColor.a = 0f;
        image.color = tmpColor;
        shouldPlayDanger = false;
    }

    private void Awake()
    {
        GameObject.FindGameObjectWithTag(Utils.TagNetworkScript).GetComponent<NetworkScript>().GetComponent<AudioListener>().enabled = false;
    }

    private void Start()
    {
        if (hasAuthority == false) return;
        if (shouldStart == false) return;
        shouldStart = false;
        loadUI();
        shotElapsedTime = 0.0f;
        InitCameras();
        InitRocketsGUI();
        _Astro.GetComponent<RawImage>().enabled = true;
        _welcome.GetComponent<Text>().text = "Welcome to BE in space";
        _welcomeMessage.GetComponent<Text>().text = "You're trying to take over the universe\n all 8 planets will send space ships that follows you\n and try to crash you to stop your plan\n So... lets show them what you got\n\n Controls:\nMouse: Left for normal rockets, Right for master(available for short time) and move mouse for rotation\nKeyboard: t for typing a message,1 2 3 for quick messages\n AWSD for normal movments,tab for changing camera view:)\n HIT ENTER TO BEGIN";
        showWelcomeMessage = true;
        GameObject gameConrollerObject = GameObject.FindWithTag(Utils.TagGameConroller);
        if (gameConrollerObject != null)
        {
            gameController = gameConrollerObject.GetComponent<GameController>();
        }
        InitializeRedAlertVariables();
        userTyping = false;
        sendMessageOnce = false;
        mainInputField.SetActive(false);
        gameController.setShowMessage(true);
    }


    private void Update()
    {
        if (hasAuthority == false) return;
        if (showWelcomeMessage)
        {
            WelcomeMessage();
        }
        else if(!userTyping)
        {
            HandleSwitchingActiveCamera();
            if (gameController.isGameOver == false) HandleShooting();
            HandleDistanceFromBoundary();
            HandleQuickMessage();
            if (Input.GetKeyDown(KeyCode.T)) { userTyping = true; }
        }
        else if (userTyping)
        {
            HandleRegularMessage();
        }
    }

    private void HandleRegularMessage()
    {
        mainInputField.SetActive(true);
        mainInputField.GetComponent<InputField>().Select();
        mainInputField.GetComponent<InputField>().ActivateInputField();
        mainInputField.GetComponent<InputField>().onEndEdit.AddListener(getMessageFromUi);
    }

    private void getMessageFromUi(string message)
    {
        if(message.Length == 0) { mainInputField.SetActive(false); userTyping = false; }
        mainInputField.GetComponent<InputField>().DeactivateInputField();
        if (userTyping)
        {
            HandleSendingMessage(message);
            mainInputField.SetActive(false);
        }
        userTyping = false;
    }

    private void HandleSendingMessage(string message)
    {
        if (isServer == true)
            RpcPostMessage("Host", message);
        else CmdUpdateMessageForAllDammit("Client", message);
    }

    [Command]
    private void CmdUpdateMessageForAllDammit(string name, string message)
    {
        RpcPostMessage(name, message);
    }

    [ClientRpc]
    public void RpcPostMessage(string name, string message)
    {
        string output = name + ": " + message;
        if (message.EndsWith(".") == false)
            output = output + ".";

        ConsoleOutput.Instance.PostMessage(output, chatColor);
    }

    private void HandleQuickMessage()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            HandleSendingMessage(helpMeMessagePress1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){
            HandleSendingMessage(comeHerePress2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)){
            HandleSendingMessage(HurayPress3);
        }
    }

    private void FixedUpdate()
    {
        if (hasAuthority == false) return;
        if (hasAuthority == false) return;
        if (userTyping) return;
        if (showWelcomeMessage == true) return;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogError(gameObject.name + " (" + typeof(Movement).Name + "): No Rigidbody component was found!");
            return;
        }

        MovementProtocol(rigidbody);
    }

    private void MovementProtocol(Rigidbody rigidbody)
    {
        rigidbody.freezeRotation = true;
        if (gameController.isGameOver) { rigidbody.velocity = Vector3.zero; }

        if (gameController.isGameOver == false) { HandleMovement(rigidbody); }
        HandleRotation(rigidbody);
    }
    
    private void HandleMovement(Rigidbody rigidbody)
    {
        float _moveSpeed = 0;
        float verticalDirection = Input.GetAxis("Vertical");
        float horizontalDirection = Input.GetAxis("Horizontal");
        if (gameController.getSpeedGift())
        {
            extraSpeeedTime += Time.deltaTime;
            _moveSpeed = moveSpeed + 45f;
            if(extraSpeeedTime > 10) { gameController.setSpeedGift(false); }
        }
        else
        {
            extraSpeeedTime = 0f;
            _moveSpeed = moveSpeed;
        }
        Vector3 moveAmount = _moveSpeed * (verticalDirection * rigidbody.transform.forward + horizontalDirection * rigidbody.transform.right);
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        if (moveAmount == new Vector3(0, 0, 0))
        {
            particleSystem.enableEmission = false;
        }
        else
            particleSystem.enableEmission = true;

        // handle movement locally
        rigidbody.velocity = moveAmount;

        // ask the server to handle this unit's movement as well.
        CmdHandleMovement(moveAmount, transform.position);
    }

    [Command]
    private void CmdHandleMovement(Vector3 newMoveAmount, Vector3 newPosition)
    {
        HandleMovementHelper(newMoveAmount, newPosition);
        RpcHandleMovement(newMoveAmount, newPosition);
    }

    [ClientRpc]
    private void RpcHandleMovement(Vector3 newMoveAmount, Vector3 newPosition)
    {
        if (hasAuthority == true) return;
        HandleMovementHelper(newMoveAmount, newPosition);
    }

    private void HandleMovementHelper(Vector3 newMoveAmount, Vector3 newPosition)
    {
        transform.position = newPosition;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = newMoveAmount;
    }


    private void HandleRotation(Rigidbody rigidbody)
    {
        float upRotationInput = Input.GetAxis("Mouse Y");
        float roundRotationInput = Input.GetAxis("Mouse X");

        float xRotation = upRotationInput * rotationSpeed;
        float yRotation = roundRotationInput * rotationSpeed;

        // Rotate body locally
        rigidbody.transform.Rotate(xRotation, yRotation, 0.0f);
        
        // Update Rotation for everybody else
        CmdHandleRotation(rigidbody.transform.rotation);
    }

    [Command]
    private void CmdHandleRotation(Quaternion newRotation)
    {
        HandleRotationHelper(newRotation);
        RpcHandleRotation(newRotation);
    }

    [ClientRpc]
    private void RpcHandleRotation(Quaternion newRotation)
    {
        if (hasAuthority == true) return;
        HandleRotationHelper(newRotation);
    }

    private void HandleRotationHelper(Quaternion newRotation)
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.rotation = newRotation;
    }


    private void HandleShooting()
    {
        AudioSource audioData;
        shotElapsedTime += Time.deltaTime;
        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            CmdSpawnNormalRocket(rocket1Shot.position, rocket1Shot.rotation);
            audioData = GetComponent<AudioSource>();
            audioData.Play(0);
        }
        if (shotElapsedTime < 7 && canShootRocket2 && masterRocketsCount != 0)
        {
            if (Input.GetButton("Fire2") && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                CmdSpawnMasterRocket(rocket2Shot.position, rocket2Shot.rotation);
                audioData = GetComponent<AudioSource>();
                audioData.Play(0);
                turnOffRocketsImage();
                masterRocketsCount--;
            }
        }
        else if (shotElapsedTime > 10 && !canShootRocket2)
        {
            shotElapsedTime = 0.0f;
            rocket2Text.color = new Color(1, 0, 0);
            rocket2Text.text = "";
            _masterRocket1.GetComponent<RawImage>().enabled = true;
            _masterRocket2.GetComponent<RawImage>().enabled = true;
            _masterRocket3.GetComponent<RawImage>().enabled = true;
            masterRocketsCount = 3;
            canShootRocket2 = true;
            GetComponents<AudioSource>()[2].Play();
        }
        else
        {
            if (canShootRocket2) { shotElapsedTime = 0.0f; }
            rocket2Text.color = new Color(0.67f, 0.67f, 0.19f);
            rocket2Text.text = "";
            if (_masterRocket1 == null) { Debug.LogError("_masterRocket1 is null"); }
            if (_masterRocket1.GetComponent<RawImage>() == null) { Debug.LogError("_masterRocket1.RawImage is null"); }
            _masterRocket1.GetComponent<RawImage>().enabled = false;
            _masterRocket2.GetComponent<RawImage>().enabled = false;
            _masterRocket3.GetComponent<RawImage>().enabled = false;
            canShootRocket2 = false;
        }
        if (gameController.getExtraRocketStatus())
        {
            _masterRocketExtra.GetComponent<RawImage>().enabled = true;
            if (Input.GetButton("Fire2") && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                CmdSpawnMasterRocket(rocket2Shot.position, rocket2Shot.rotation);
                audioData = GetComponent<AudioSource>();
                audioData.Play(0);
                _masterRocketExtra.GetComponent<RawImage>().enabled = false;
                gameController.setExtraRocket(false);
            }
        }
    }

    [Command]
    private void CmdSpawnNormalRocket(Vector3 startingPostion, Quaternion startingRotation)
    {
        GameObject normalRocket = Instantiate(rocket1, startingPostion, startingRotation);
        NetworkServer.Spawn(normalRocket);
    }

    [Command]
    private void CmdSpawnMasterRocket(Vector3 startingPosition, Quaternion startingRotation)
    {
        GameObject masterRocket = Instantiate(rocket2, startingPosition, startingRotation);
        NetworkServer.Spawn(masterRocket);
    }

    private void InitCameras()
    {
        camSwitchTime = Time.time;
        SetActiveCameras();
    }

    private void SetActiveCameras()
    {
        if (cameras == null) { Debug.LogError(typeof(Movement).Name + ": Start() Function was initiated with null"); return; }
        foreach (Transform child in cameras.transform)
        {
            if (child.gameObject.tag == PlayerGameObject.Tags.cameras.GetValue())
            {
                SetCameraStatus(child.gameObject, true);
            }
            else
            {
                SetCameraStatus(child.gameObject, false);
            }
        }
    }

    private void SetCameraStatus(GameObject cameraHolder, bool isEnabled)
    {
        Camera c = cameraHolder.GetComponent<Camera>();
        if (c == null) throw new MissingComponentException("No camera component was found!");
        c.enabled = isEnabled;

        AudioListener listener = cameraHolder.GetComponent<AudioListener>();
        if (listener == null) throw new MissingComponentException("No Audio Listener provided for Camera GameObject!");
        listener.enabled = isEnabled;

        cameraHolder.SetActive(isEnabled);
    }

    private void HandleSwitchingActiveCamera()
    {
        if (Input.GetKey(KeyCode.Tab) == false) return;
        if (Time.time <= camSwitchTime) return;

        camSwitchTime = Time.time + cameraSwitchRate;
        PlayerGameObject.Tags.cameras.Next();
        SetActiveCameras();
    }

    private void turnOffRocketsImage()
    {
        if (masterRocketsCount == 3)
        {
            _masterRocket3.GetComponent<RawImage>().enabled = false;
            return;
        }
        else if (masterRocketsCount == 2)
        {
            _masterRocket2.GetComponent<RawImage>().enabled = false;
            return;
        }
        else
            _masterRocket1.GetComponent<RawImage>().enabled = false;
    }

    private void InitRocketsGUI()
    {
        rocket2Text.color = new Color(1, 0, 0);
        rocket2Text.text = "";
        _masterRocket1.GetComponent<RawImage>().enabled = true;
        _masterRocket2.GetComponent<RawImage>().enabled = true;
        _masterRocket3.GetComponent<RawImage>().enabled = true;
        _masterRocketExtra.GetComponent<RawImage>().enabled = false;
        masterRocketsCount = 3;
        canShootRocket2 = true;
    }

    private void WelcomeMessage()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            _Astro.GetComponent<RawImage>().enabled = false;
            _welcome.GetComponent<Text>().text = "";
            _welcomeMessage.GetComponent<Text>().text = "";
            showWelcomeMessage = false;
            gameController.setShowMessage(false);
        }
    }

    private void HandleDistanceFromBoundary()
    {
        dangerAudioSource = GetComponents<AudioSource>()[1];
        float distance = Vector3.Distance(new Vector3(0, 0, 0), gameObject.transform.position);
        distance = radious - distance;
        if (distance > 0 && distance < 100f) // should alert
        {
            if (!shouldPlayDanger)
            {
                dangerAudioSource.Play();
                shouldPlayDanger = true;
            }
            dangerText.text = "You're too close to the edge! STAY AWAY";
            dangerAudioSource.enabled = true;
            if (waitSeconds == 0)
            {
                Color tmpColor = image.color;
                tmpColor.a = 0.5f;
                image.color = tmpColor;
            }
            waitSeconds += Time.deltaTime;
            if (waitSeconds >= 0.3f)
            {
                Color tmpColor = image.color;
                tmpColor.a = 0f;
                image.color = tmpColor;
                waitSeconds = 0;
            }
        }
        else if (distance <= 0) { dangerText.text = ""; Destroy(gameObject); gameController.GameOverFunction(); }
        else
        {
            dangerText.text = "";
            Color tmpColor = image.color;
            tmpColor.a = 0f;
            image.color = tmpColor;
            dangerAudioSource.Stop();
            shouldPlayDanger = false;
        }
    }
}
