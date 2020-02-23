using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class DestroyEnemy : NetworkBehaviour
{
    public GameObject explosion;
    public GameObject rocke2Explosion;
    public GameObject woodBox;
    private GameController gameController;

    private const float giftProbability = 0.4f;

    private void Start()
    {
        if (isServer == false) return;
        GameObject gameConrollerObject = GameObject.FindWithTag(Utils.TagGameConroller);
        if(gameConrollerObject != null)
        {
            gameController = gameConrollerObject.GetComponent<GameController>();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer == false) return;
        int score = 0;
        if (other.tag == Utils.TagBackground || other.tag == Utils.TagWoodBox || other.tag == Utils.TagEnemy|| other.tag == Utils.TagAsteroid)
        {
            return;
        }
        if(other.tag == Utils.TagPlayer)
        {
            gameController.GameOverFunction();
            return;
        }
        if (other.tag == Utils.TagRocket2)
        {
           Collider[] radious =  Physics.OverlapSphere(other.transform.position, 10f);
            if(radious!= null)
            {
                foreach (Collider collider in radious) {
                    if (collider.tag == Utils.TagBackground || collider.tag == Utils.TagGameConroller || collider.tag == Utils.TagPlayer) {continue;}
                    score += Utils.getScoreByCollider(collider.tag);
                    Instantiate(explosion, collider.transform.position, collider.transform.rotation);
                    Utils.CmdDestroyObjectByID(collider.gameObject.GetComponent<NetworkIdentity>());
                    if(collider.tag == Utils.TagEnemy) { gameController.enemyKilled(); SpawnGiftWithProbability(); }
                }
                Instantiate(rocke2Explosion, other.transform.position, other.transform.rotation);
                gameController.addScore(score);
                return;
            }
        }
        score = Utils.getScoreByCollider(tag);
        gameController.addScore(score);
        Instantiate(explosion, other.transform.position, other.transform.rotation);
        SpawnGiftWithProbability();
        Utils.CmdDestroyObjectByID(other.gameObject.GetComponent<NetworkIdentity>());
        Utils.CmdDestroyObjectByID(gameObject.GetComponent<NetworkIdentity>());
        gameController.enemyKilled();
    }

    public void SpawnGiftWithProbability()
    {
        CmdSpawnGiftWithProbability();
    }

    [Command]
    private void CmdSpawnGiftWithProbability()
    {
        if (Random.value <= giftProbability)
        {
            int randomGift = Random.Range(1, 4);
            GameObject gift = Instantiate(woodBox, transform.position, transform.rotation);
            HandleGiftColoring(gift, randomGift);
            gift.SendMessage("onStart", randomGift);
            NetworkServer.Spawn(gift);

            RpcInitGiftBoxProperly(gift.GetComponent<NetworkIdentity>(), randomGift);
        }
    }

    [ClientRpc]
    private void RpcInitGiftBoxProperly(NetworkIdentity giftNetworkIdentity, int giftType)
    {
        GameObject gift = giftNetworkIdentity.gameObject;
        HandleGiftColoring(gift, giftType);
        gift.SendMessage("onStart", giftType);
    }

    private void HandleGiftColoring(GameObject gift, int giftType)
    {
        if (giftType == 1)  // extra master rocket
            ChooseColorForLights(gift, Color.red);
        else if (giftType == 2) //extra score
            ChooseColorForLights(gift, Color.yellow);
        else
            ChooseColorForLights(gift, Color.blue);
    }

    private void ChooseColorForLights(GameObject gift, Color color)
    {
        Light[] lights = gift.GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            light.color = color;
        }
    }
}
