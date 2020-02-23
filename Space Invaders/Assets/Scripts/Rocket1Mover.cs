using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Rocket1Mover : NetworkBehaviour
{
    public float speed;
    private Vector3 origin;

    private const float maxDistance = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        origin = transform.position;
        
        if (isServer == false) return;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogError(gameObject.name + " (Rocket1Mover.cs): No Rigidbody component was found!");
            return;
        }

        RpcInitializeVelocity(transform.up);
    }

    [ClientRpc]
    private void RpcInitializeVelocity(Vector3 up)
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = up * speed;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, origin);
        if (distance > maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
