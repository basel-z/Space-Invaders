using UnityEngine;
using System.Collections;

public class Starfield : MonoBehaviour
{
    private ParticleSystem.Particle[] points;
    private ParticleSystem ps;

    public int starsMax = 100;
    public float starSize = 1;
    public float starDistance = 10;
    public float starClipDistance = 1;
    private float starDistanceSqr;
    private float starClipDistanceSqr;


    // Use this for initialization
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        starDistanceSqr = starDistance * starDistance;
        starClipDistanceSqr = starClipDistance * starClipDistance;
    }


    private void CreateStars()
    {
        points = new ParticleSystem.Particle[starsMax];

        for (int i = 0; i < starsMax; i++)
        {
            points[i].position = Random.insideUnitSphere * starDistance + transform.position;
            points[i].color = new Color(1, 1, 1, 1);
            points[i].size = starSize;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (points == null) CreateStars();

        for (int i = 0; i < starsMax; i++)
        {

            if ((points[i].position - transform.position).sqrMagnitude > starDistanceSqr)
            {
                points[i].position = Random.insideUnitSphere.normalized * starDistance + transform.position;
            }

            if ((points[i].position - transform.position).sqrMagnitude <= starClipDistanceSqr)
            {
                float percent = (points[i].position - transform.position).sqrMagnitude / starClipDistanceSqr;
                points[i].color = new Color(1, 1, 1, percent);
                points[i].size = percent * starSize;
            }


        }

        ps.SetParticles(points, points.Length);

    }
}