using UnityEngine;

public enum RandomSpawnerShape
{
    Box,
    Sphere,
}

// Used mostly for testing to provide stuff to fly around and into.
public class RandomAreaSpawner : MonoBehaviour
{
    [Header("General settings:")]

    [Tooltip("Prefab to spawn.")]
    public Transform AsteroidPrefab;

    [Tooltip("Shape to spawn the prefabs in.")]
    public RandomSpawnerShape SpawnShape = RandomSpawnerShape.Sphere;

    [Tooltip("Multiplier for the spawn shape in each axis.")]
    public Vector3 ShapeModifiers = Vector3.one;

    [Tooltip("How many prefab to spawn.")]
    public int AsteroidCount = 50;

    [Tooltip("Distance from the center of the gameobject that prefabs will spawn")]
    public float SpawnRange = 1000.0f;

    [Tooltip("Should prefab have a random rotation applied to it.")]
    public bool HasRandomRotation = true;

    [Tooltip("Random min/max scale to apply.")]
    public Vector2 ScaleRange = new Vector2(1.0f, 3.0f);

    [Header("Rigidbody settings:")]

    [Tooltip("Apply a velocity from 0 to this value in a random direction.")]
    public float Velocity = 0.0f;

    [Tooltip("Apply an angular velocity (deg/s) from 0 to this value in a random direction.")]
    public float AngularVelocity = 0.0f;

    [Tooltip("If true, raise the mass of the object based on its scale.")]
    public bool ScaleMass = true;

    void Start()
    {
        if (AsteroidPrefab != null)
        {
            for (int i = 0; i < AsteroidCount; i++)
                CreateAsteroid();
        }
    }

    private void CreateAsteroid()
    {
        Vector3 spawnPos = Vector3.zero;
         
        // Create random position based on specified shape and range.
        if (SpawnShape == RandomSpawnerShape.Box)
        {
            spawnPos.x = Random.Range(-SpawnRange, SpawnRange) * ShapeModifiers.x;
            spawnPos.y = Random.Range(-SpawnRange, SpawnRange) * ShapeModifiers.y;
            spawnPos.z = Random.Range(-SpawnRange, SpawnRange) * ShapeModifiers.z;
        }
        else if (SpawnShape == RandomSpawnerShape.Sphere)
        {
            spawnPos = Random.insideUnitSphere * SpawnRange;
            spawnPos.x *= ShapeModifiers.x;
            spawnPos.y *= ShapeModifiers.y;
            spawnPos.z *= ShapeModifiers.z;
        }

        // Offset position to match position of the parent gameobject.
        spawnPos += transform.position;

        // Apply a random rotation if necessary.
        Quaternion spawnRot = (HasRandomRotation) ? Random.rotation : Quaternion.identity;

        // Create the object and set the parent to this gameobject for scene organization.
        Transform t = Instantiate(AsteroidPrefab, spawnPos, spawnRot) as Transform;
        t.SetParent(transform);

        // Apply scaling.
        float scale = Random.Range(ScaleRange.x, ScaleRange.y);
        t.localScale = Vector3.one * scale;

        // Apply rigidbody values.
        Rigidbody r = t.GetComponent<Rigidbody>();
        if (r)
        {
            if (ScaleMass)
                r.mass *= scale * scale * scale;

            r.AddRelativeForce(Random.insideUnitSphere * Velocity, ForceMode.VelocityChange);
            r.AddRelativeTorque(Random.insideUnitSphere * AngularVelocity * Mathf.Deg2Rad, ForceMode.VelocityChange);
        }
    }

    public void CreateNewAstroid()
    {
        CreateAsteroid();
    }
}
