using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryCloud : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer angry;

    [SerializeField]
    private SpriteRenderer sleeping;

    private bool isSleeping = true;
    private float sleepFor = 6f;

    private void Awake()
    {
        angry.enabled = false;
        sleeping.enabled = true;
    }

    private void Update()
    {
        if (isSleeping == false)
        {
            sleepFor -= Time.deltaTime;

            if(sleepFor < 0)
            {
                angry.enabled = false;
                sleeping.enabled = true;
                isSleeping = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        var queenBee = collider.gameObject.GetComponent<WorkerBee>();

        if (queenBee != null && isSleeping == true)
        {
            angry.enabled = true;
            sleeping.enabled = false;
            isSleeping = false;

            sleepFor = 6f;

            AudioManager.Instance.PlayOneShot("Cloud-Wakeup", 1.0f);

            return;
        }

        if (isSleeping == false)
        {
            var boid = collider.gameObject.GetComponent<BoidBehaviour>();

            if (boid != null)
            {
                var workerBee = boid.View.GetComponent<WorkerBee>();

                if (workerBee != null && workerBee.IsWet == false)
                {
                    AudioManager.Instance.PlayOneShot("Hurt", Random.value);

                    workerBee.IsWet = true;
                }
            }
        }
    }
}
