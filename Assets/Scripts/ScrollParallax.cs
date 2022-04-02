using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollParallax : MonoBehaviour
{
    private Material spriteMaterial;

    private Vector2 offset;
    private Vector3 lastCameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        spriteMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        var cameraPosition = Camera.main.transform.position;

        var delta = cameraPosition - lastCameraPosition;

        if (delta.magnitude > 0)
        {
            offset = offset + new Vector2(delta.x, delta.y);

            spriteMaterial.mainTextureOffset = offset;

            lastCameraPosition = cameraPosition;
        }
    }
}
