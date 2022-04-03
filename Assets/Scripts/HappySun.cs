using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HappySun : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        var boid = collider.gameObject.GetComponent<BoidBehaviour>();

        if (boid != null)
        {
            var workerBee = boid.View.GetComponent<WorkerBee>();

            if (workerBee != null && workerBee.IsWet == true)
            {
                AudioManager.Instance.PlayOneShot("Heal", Random.value);

                workerBee.IsWet = false;
            }
        }
    }
}
