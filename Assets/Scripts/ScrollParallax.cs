using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ScrollParallax : MonoBehaviour
{
    private Material spriteMaterial;

    private Vector2 offset;
    private Vector3 lastCameraPosition;
    private Vector3 startingScale;

    // Start is called before the first frame update
    void Start()
    {
        spriteMaterial = GetComponent<Renderer>().material;
        startingScale = transform.localScale;
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

        var ppc = Camera.main.GetComponent<PixelPerfectCamera>();
        float scaleOffset = 100f / (float)ppc.assetsPPU;
        transform.localScale = new Vector3(startingScale.x * scaleOffset, startingScale.y * scaleOffset, startingScale.z);
    }
}
