using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NectarCollectable : MonoBehaviour
{
    private int availableNectar = 10;

    void OnTriggerEnter2D(Collider2D collider)
    {
        var queenBee = collider.gameObject.GetComponent<WorkerBee>();

        if (queenBee != null)
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one, 0.6f, 6);

            --availableNectar;

            GameplayController.Instance.AddHoneyCollected(1);

            if (availableNectar == 0)
            {
                Destroy(gameObject);
            }
        }


        var boid = collider.gameObject.GetComponent<BoidBehaviour>();

        if (boid != null)
        {
            var workerBee = boid.View.GetComponent<WorkerBee>();

            if (workerBee != null && workerBee.IsWet == false)
            {
                transform.DOKill(true);
                transform.DOPunchScale(Vector3.one, 0.6f, 6);

                --availableNectar;

                GameplayController.Instance.AddHoneyCollected(1);

                if (availableNectar == 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
