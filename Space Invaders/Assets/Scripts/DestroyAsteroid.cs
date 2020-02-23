using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class DestroyAsteroid : NetworkBehaviour
{
    public GameObject explosion;
    public GameObject rocke2Explosion;
    private GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer == false) return;
        GameObject gameConrollerObject = GameObject.FindWithTag(Utils.TagGameConroller);
        if (gameConrollerObject != null)
        {
            gameController = gameConrollerObject.GetComponent<GameController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer == false) return;
        int score = 0;
        if(other.tag == Utils.TagAsteroid || other.tag == Utils.TagBackground) {
            return; }
        if (other.tag == Utils.TagEnemy) {
            return; }
        if (other.tag == Utils.TagPlayer)
        {
            gameController.GameOverFunction();
            return;
        }
        if (other.tag == Utils.TagRocket2)
        {
            Collider[] radious = Physics.OverlapSphere(other.transform.position, 10f);
            if (radious != null)
            {
                foreach (Collider collider in radious)
                {
                    if (collider.tag == Utils.TagBackground || collider.tag == Utils.TagGameConroller || collider.tag == Utils.TagPlayer) { continue; }
                    score += Utils.getScoreByCollider(collider.tag);
                    Instantiate(explosion, collider.transform.position, collider.transform.rotation);
                    Utils.CmdDestroyObjectByID(collider.gameObject.GetComponent<NetworkIdentity>());
                    if(collider.tag == Utils.TagEnemy) { gameController.enemyKilled(); }
                }
                Instantiate(rocke2Explosion, other.transform.position, other.transform.rotation);
                gameController.addScore(score);
                return;
            }
        }
        score = Utils.AsteroidScore;
        gameController.addScore(score);
        Instantiate(explosion, other.transform.position, other.transform.rotation);
        Utils.CmdDestroyObjectByID(other.gameObject.GetComponent<NetworkIdentity>());
        Utils.CmdDestroyObjectByID(gameObject.GetComponent<NetworkIdentity>());
    }

}
