
using UnityEngine;
using System.Collections;
using Squax.Patterns;

public class BoidController : UnitySingleton<BoidController>
{
    public LayerMask SearchLayerMask;

    [Range(0.1f, 10.0f)]
    public float DistanceBetweenNeighbourBoids = 2.0f;

    [Range(0.1f, 20.0f)]
    public float Velocity = 6.0f;

    [Range(0.0f, 0.9f)]
    public float VelocityVariance = 0.5f;

    [Range(0.1f, 20.0f)]
    public float Rotation = 4.0f;

    public GameObject CreateBoid(Vector2 position, GameObject boidPrefab)
    {
        var boid = Instantiate(boidPrefab, position, Quaternion.identity) as GameObject;
        return boid;
    }
}