using System;
using System.ComponentModel;
using UnityEngine.UI;
namespace UnityEngine.Networking
{
    /// <summary>
    /// An extension for the NetworkManager that displays a default HUD for controlling the network state of the game.
    /// <para>This component also shows useful internal state for the networking system in the inspector window of the editor. It allows users to view connections, networked objects, message handlers, and packet statistics. This information can be helpful when debugging networked games.</para>
    /// </summary>
    [AddComponentMenu("Network/NetworkManagerHUD")]
    [RequireComponent(typeof(NetworkManager))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("The high level API classes are deprecated and will be removed in the future.")]
    public class NetworkScript : MonoBehaviour
    {
        /// <summary>
        /// The NetworkManager associated with this HUD.
        /// </summary>
        public NetworkManager manager;
        /// <summary>
        /// Whether to show the default control HUD at runtime.
        /// </summary>
        [SerializeField] public bool showGUI = true;
        /// <summary>
        /// The horizontal offset in pixels to draw the HUD runtime GUI at.
        /// </summary>
        [SerializeField] public int offsetX;
        /// <summary>
        /// The vertical offset in pixels to draw the HUD runtime GUI at.
        /// </summary>
        [SerializeField] public int offsetY;

        // Runtime variable
        bool m_ShowServer;
        public Canvas canvas;
        public RawImage _bg;
        private GameObject background;
        public RawImage logo;
        private GameObject beInSpaceLogo;
        public RawImage playerAndEnemy;
        private GameObject _playerAndEnemy;
        public GUIStyle style;
        void loadUI()
        {
            GameObject canvasObject = Instantiate(canvas).gameObject;
            RectTransform rTransform = canvasObject.GetComponent<RectTransform>();
            background = Instantiate(_bg.gameObject);
            background.transform.SetParent(rTransform, false);

            beInSpaceLogo = Instantiate(logo.gameObject);
            beInSpaceLogo.transform.SetParent(rTransform, false);

            _playerAndEnemy= Instantiate(playerAndEnemy.gameObject);
            _playerAndEnemy.transform.SetParent(rTransform, false);
            
        }
        private void Start()
        {
            loadUI();
        }
        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }
        void Update()
        {
            if (!showGUI)
                return;
            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        destroyStuff();
                        manager.StartServer();
                    }
                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        destroyStuff();
                        manager.StartHost();
                    }
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    destroyStuff();
                    manager.StartClient();
                }
            }
            if (NetworkServer.active)
            {
                if (manager.IsClientConnected())
                {
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        manager.StopHost();
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        manager.StopServer();
                    }
                }
            }
        }
        void destroyStuff()
        {
            Destroy(background);
            Destroy(beInSpaceLogo);
            Destroy(_playerAndEnemy);
        }
        void OnGUI()
        {
            if (!showGUI)
            {
                return;
            }
            int xpos = 10 + offsetX;
            int ypos = 40 + offsetY;
            const int spacing = 24;

            bool noConnection = (manager.client == null || manager.client.connection == null ||
                manager.client.connection.connectionId == -1);

            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (noConnection)
                {
                    if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Single player Or Host",style))
                        {
                            destroyStuff();
                            manager.StartHost();
                        }
                        ypos += 2*spacing;
                    }

                    if (GUI.Button(new Rect(xpos, ypos, 105, 20), "Guest ",style))
                    {
                        destroyStuff();
                        manager.StartClient();
                    }

                    manager.networkAddress = GUI.TextField(new Rect(xpos + 100, ypos, 95, 20), manager.networkAddress);
                    ypos += spacing;

                    if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // cant be a server in webgl build
                        GUI.Box(new Rect(xpos, ypos, 200, 25), "(  WebGL cannot be server  )");
                        ypos += spacing;
                    }
                }
                else
                {
                    GUI.Label(new Rect(xpos, ypos, 200, 20), "Connecting to " + manager.networkAddress + ":" + manager.networkPort + "..");
                    ypos += spacing;


                    if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Cancel Connection Attempt"))
                    {
                        manager.StopClient();
                    }
                }
            }
            else
            {
                if (NetworkServer.active)
                {
                    string serverMsg = "Server: port=" + manager.networkPort;
                    if (manager.useWebSockets)
                    {
                        serverMsg += " (Using WebSockets)";
                    }
                    GUI.Label(new Rect(xpos, ypos, 300, 20), serverMsg);
                    ypos += spacing;
                }
                if (manager.IsClientConnected())
                {
                    GUI.Label(new Rect(xpos, ypos, 300, 20), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
                    ypos += spacing;
                }
            }

            if (manager.IsClientConnected() && !ClientScene.ready)
            {
                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Client Ready"))
                {
                    ClientScene.Ready(manager.client.connection);

                    if (ClientScene.localPlayers.Count == 0)
                    {
                        ClientScene.AddPlayer(0);
                    }
                }
                ypos += spacing;
            }

            if (NetworkServer.active || manager.IsClientConnected())
            {
                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop (X)"))
                {
                    manager.StopHost();
                }
                ypos += spacing;
            }

            if (!NetworkServer.active && !manager.IsClientConnected() && noConnection)
            {
                ypos += 10;

                if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    GUI.Box(new Rect(xpos - 5, ypos, 220, 25), "(WebGL cannot use Match Maker)");
                    return;
                }
            }
        }
    }
}
