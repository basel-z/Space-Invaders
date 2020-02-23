using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnectionHandling : NetworkBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject PlayerPrefab2;

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer == false) return; 
        if (PlayerPrefab == null) throw new MissingReferenceException();
        CmdIncAmntOfPlayers();
        CmdSpawnPlayerUnit();
    }

    [Command]
    void CmdIncAmntOfPlayers()
    {
        Utils.amountOfPlayers++;
    }

    [Command]
    void CmdSpawnPlayerUnit()
    {
        GameObject playerUnit = 
            Utils.amountOfPlayers % 2 == 0 
            ? Instantiate(PlayerPrefab) 
            : Instantiate(PlayerPrefab2);

        NetworkServer.SpawnWithClientAuthority(playerUnit, connectionToClient);
    }

}
