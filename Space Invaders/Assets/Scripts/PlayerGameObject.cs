using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note: This class represents the Player GameObject (and all of its children in the hierarchy).
public class PlayerGameObject : MonoBehaviour
{
    public class Tags
    {
        // Note: Tags should be consistent in the code the same way they are consisntent
        // in the unity editor
        public const string InitialCamera = "InitialCamera";
        public const string SecondaryCamera = "SecondaryCamera";
        public const string thirdCamera = "ThirdCam";

        public static ImmutableDoublyLinkedList<string> cameras = new ImmutableDoublyLinkedList<string>(0, InitialCamera, SecondaryCamera,thirdCamera);
    }
}
