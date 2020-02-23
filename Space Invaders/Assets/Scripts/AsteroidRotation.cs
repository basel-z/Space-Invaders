using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class AsteroidRotation : MonoBehaviour
{
    public int speed;
    public int tumble;

    private SphereCollider sCollider;
    private Vector3 offset;

    void Start()
    {
        sCollider = GetComponent<SphereCollider>();
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        GameController gc = GameObject.FindWithTag(Utils.TagGameConroller).GetComponent<GameController>(); ;
        Vector3 direction = gc.AsteroidDirection;
        rigidbody.velocity = direction * speed;

        offset = Utils.getRandomDirection() * Random.Range(1.0f, 30.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 center = sCollider.transform.TransformPoint(sCollider.bounds.center);
        transform.Rotate(center + offset, Time.deltaTime * tumble);
    }
}
