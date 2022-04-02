using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerBee : MonoBehaviour
{
    private float lastPositionX;
    private SpriteRenderer spriteRenderer;

    public bool IsWet { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lastPositionX != transform.position.x)
        {
            spriteRenderer.flipX = transform.position.x < lastPositionX;

            lastPositionX = transform.position.x;
        }

        if(IsWet == true)
        {
            spriteRenderer.color = Color.black;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
}
