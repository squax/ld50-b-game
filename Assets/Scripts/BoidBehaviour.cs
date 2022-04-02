
using UnityEngine;
using System.Collections;

public class BoidBehaviour : MonoBehaviour
{
    private float boidNoiseOffset;

    [SerializeField]
    private GameObject boidViewPrefab;

    public GameObject View { get { return boidViewPrefab; } }

    void Start()
    {
        // Seed some noise.
        boidNoiseOffset = Random.value * 100.0f;

        // Instantiate the view.
        boidViewPrefab = Instantiate(boidViewPrefab);
    }

    private void OnDestroy()
    {
        Destroy(boidViewPrefab);
    }

    void Update()
    {
        // Cache position/rotation.
        var currentPosition = transform.position;
        var currentRotation = transform.rotation;

        // Set view position, don't use rotation.
        boidViewPrefab.transform.position = currentPosition;

        var workerBee = boidViewPrefab.GetComponent<WorkerBee>();
        var boidSeperationAxis = Vector3.zero;
        var boidAlignment = BoidController.Instance.transform.forward;
        var boidCohesion = BoidController.Instance.transform.position;
        var boidNoise = Mathf.PerlinNoise(Time.time, boidNoiseOffset) * 2.0f - 1.0f;
        var boidVelocity = BoidController.Instance.Velocity * (1.0f + boidNoise * BoidController.Instance.VelocityVariance);

        // Find all nearby boids using a mask.
        var nearbyBoids = Physics2D.OverlapCircleAll(currentPosition, BoidController.Instance.DistanceBetweenNeighbourBoids, BoidController.Instance.SearchLayerMask);

        // Accumulates the vectors.
        foreach (var boid in nearbyBoids)
        {
            if (boid.gameObject == gameObject) continue;

            var boidTransform = boid.transform;
            boidSeperationAxis += GetSeparationVector(boidTransform);

            boidAlignment += boidTransform.forward;
            boidCohesion += boidTransform.position;
        }

        // Average out over the group.
        var average = 1.0f / nearbyBoids.Length;
        boidAlignment *= average;
        boidCohesion *= average;
        boidCohesion = (boidCohesion - currentPosition).normalized;

        // Rotate towards the target axis.
        var direction = boidSeperationAxis + boidAlignment + boidCohesion;
        var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);
        if (rotation != currentRotation)
        {
            var ip = Mathf.Exp(-BoidController.Instance.Rotation * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(rotation, currentRotation, ip);
        }

        // Moves.
        transform.position = currentPosition + transform.forward * (boidVelocity * Time.deltaTime * (workerBee.IsWet ? 0.5f : 1f));
    }

    Vector3 GetSeparationVector(Transform target)
    {
        var diff = transform.position - target.transform.position;
        var diffLen = diff.magnitude;
        var scaler = Mathf.Clamp01(1.0f - diffLen / BoidController.Instance.DistanceBetweenNeighbourBoids);
        return diff * (scaler / diffLen);
    }
}