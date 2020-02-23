using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Rocket2Movment : NetworkBehaviour
{
    public float speed;
    private Vector3 origin;
    public GameObject explosion;
    public GameObject rocke2Explosion;
    private GameController gameController;
    private const float maxDistance = 20.0f;
    private void Start()
    {
        origin = transform.position;
        GameObject gameConrollerObject = GameObject.FindWithTag(Utils.TagGameConroller);
        if (gameConrollerObject != null)
        {
            gameController = gameConrollerObject.GetComponent<GameController>();
        }

        if (isServer == false) return;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogError(gameObject.name + " (Rocket2Mover.cs): No Rigidbody component was found!");
            return;
        }

        RpcInitializeVelocity(transform.forward);
    }

    [ClientRpc]
    private void RpcInitializeVelocity(Vector3 forward)
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, origin);
        if (distance > maxDistance)
        {
            int score = 0;
            Collider[] radious = Physics.OverlapSphere(transform.position, 10.0f);
            if (radious != null)
            {
                foreach (Collider collider in radious)
                {
                    if (collider.tag == Utils.TagBackground || collider.tag == Utils.TagGameConroller || collider.tag == Utils.TagPlayer) { continue; }
                    score += Utils.getScoreByCollider(collider.tag);
                    Instantiate(explosion, collider.transform.position, collider.transform.rotation);
                    Utils.CmdDestroyObjectByID(collider.gameObject.GetComponent<NetworkIdentity>());
                    if(collider.tag == Utils.TagEnemy) { gameController.enemyKilled();}
                }
                Instantiate(rocke2Explosion, transform.position, transform.rotation);
                gameController.addScore(score);
                return;
            }
        }
    }
}
