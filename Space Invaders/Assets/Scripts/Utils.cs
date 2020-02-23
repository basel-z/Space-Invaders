using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;

public class Utils
{
    [SyncVar]
    public static int amountOfPlayers = 0;
    public const string TagBackground = "Circular_Background";
    public const string TagPlayer = "Player";
    public const string TagAsteroid = "Asteroid";
    public const string TagEnemy = "Enemy";
    public const string TagRocket2 = "Rocket2";
    public const string TagGameConroller = "GameController";
    public const string TagWoodBox = "woodBox";
    public const string TagSpotLight1 = "spotLight1";
    public const string TagSpotLight2 = "spotLight2";
    public const string TagPontLight = "pointLight";
    public const string TagRedScreen= "redScreen";
    public const string TagNetworkScript = "NetworkScript";
    public const string TagFireworks = "Fireworks";
    public const int AsteroidScore = 1;
    public const int EnemyScore = 10;

    public static int getScoreByCollider(string tag)
    {
        if (tag == Utils.TagAsteroid)
        {
            return Utils.AsteroidScore;
        }
        if (tag == Utils.TagEnemy)
        {
            return Utils.EnemyScore;
        }
        return 0;
    }
    // returns the game's radius, though gameBackground should be the Circular_Background
    public static float getGameBoundaryRadius(GameObject gameBackground)
    {
        if (gameBackground == null) { Debug.LogError("(" + typeof(Utils).Name + "): No game background component provided."); throw new MissingComponentException(); }
        SphereCollider bgSphere = gameBackground.GetComponent<SphereCollider>();
        if (bgSphere == null) { Debug.LogError("(" + typeof(Utils).Name + "): Missing SphereCollider for bg component."); throw new MissingComponentException(); }

        float scale = getUniformScale(gameBackground);
        float localRadius = bgSphere.radius;
        return scale * localRadius;
    }
    public static float getBackgroundRadius(GameObject gameBackground)
    {
        return getUniformScale(gameBackground) / 2.0f;
    }

    public static Vector3 getRandomDirection() => new Vector3(
            (Random.value + 0.01f) * (Random.value < 0.5 ? 1 : -1),
            (Random.value + 0.01f) * (Random.value < 0.5 ? 1 : -1),
            (Random.value + 0.01f) * (Random.value < 0.5 ? 1 : -1)
        ).normalized;

    [Command]
    public static void CmdDestroyObjectByID(NetworkIdentity netIdentity)
    {
        if (netIdentity == null) throw new System.Exception("Server attempted to destroy a null network identity!");
        GameObject obj = NetworkServer.FindLocalObject(netIdentity.netId);
        NetworkServer.Destroy(obj);
    }

    [System.Serializable]
    public class NonEqualScaleProvidedException : System.Exception
    {
        public NonEqualScaleProvidedException() { }
        public NonEqualScaleProvidedException(string message) : base(message) { }
        public NonEqualScaleProvidedException(string message, System.Exception inner) : base(message, inner) { }
        protected NonEqualScaleProvidedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    private static float getUniformScale(GameObject _object)
    {
        if (_object == null) { Debug.LogError("(class=" + typeof(Utils).Name + ", GameObject=" + _object.name + "): No _object component provided."); throw new MissingComponentException(); }
        float x = _object.transform.lossyScale.x;
        float y = _object.transform.lossyScale.y;
        float z = _object.transform.lossyScale.z;
        if (x != y || x != z) throw new NonEqualScaleProvidedException("(class=" + typeof(Utils).Name + ", GameObject=" + _object.name + "): x = " + x + ", y = " + y + ", z = " + z);
        float scale = x;
        return scale;
    }

    [System.Serializable]
    public class AttemptedUnauthorizedAccessLevelSystemException : System.Exception
    {
        public AttemptedUnauthorizedAccessLevelSystemException() {}
        public AttemptedUnauthorizedAccessLevelSystemException(string message) : base(message) {}
        public AttemptedUnauthorizedAccessLevelSystemException(string message, System.Exception innerException) : base(message, innerException) {}
        protected AttemptedUnauthorizedAccessLevelSystemException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}

public class ImmutableDoublyLinkedList<T>
{
    private int currentItem;
    private readonly int itemsSize;
    private List<T> list;

    public ImmutableDoublyLinkedList(int index = 0, params T[] items)
    {
        list = new List<T>();
        currentItem = index;
        itemsSize = items.Length;
        for (int i = 0; i < itemsSize; ++i)
        {
            list.Add(items[i]);
        }
    }

    public T GetValue()
    {
        return list[currentItem];
    }

    public void Next()
    {
        ++currentItem;
        if (currentItem == itemsSize)
        {
            currentItem = 0;
        }
    }
}
