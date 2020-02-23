using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class Boundary : NetworkBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == Utils.TagEnemy)
        {
            return;
        }
        if (other.tag != Utils.TagPlayer)
        {
            Utils.CmdDestroyObjectByID(other.GetComponent<NetworkIdentity>());
        }
    }
}
