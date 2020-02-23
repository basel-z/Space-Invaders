using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;

public class RocketAndScoreGift : NetworkBehaviour
{
    public GameObject explosion;

    private float sw;
    private GameController gameController;
    private bool shouldDestry;
    private int giftType;// 1 for extra master rocket, 2 for additional 25 score, 3 for 10 seconds 
    // Start is called before the first frame update
    void Start()
    {
        GameObject gameConrollerObject = GameObject.FindWithTag(Utils.TagGameConroller);
        if (gameConrollerObject != null)
        {
            gameController = gameConrollerObject.GetComponent<GameController>();
        }
        sw = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Utils.TagPlayer)
        {
            if (giftType == 1)  // extra master rocket
            {
                gameController.setExtraRocket(true);
            }
            else if (giftType == 2) //extra score
            {
                gameController.addScore(25);
            }
            else
            {
                gameController.setSpeedGift(true);
            }
            gameController.playGiftSound();
            Utils.CmdDestroyObjectByID(GetComponent<NetworkIdentity>());
        }
    }
    // Update is called once per frame
    void Update()
    {
        sw += Time.deltaTime;
        if (sw > 8)
        {
            Instantiate(explosion, transform.position,transform.rotation);
            Destroy(gameObject);
        }
    }
    void onStart(int gift)
    {
        giftType = gift;
    }
}
