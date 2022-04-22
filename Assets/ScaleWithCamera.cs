using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ScaleWithCamera : MonoBehaviour
{
    private Vector3 startingScale;

    // Start is called before the first frame update
    void Start()
    {
        startingScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        var ppc = Camera.main.GetComponent<PixelPerfectCamera>();
        float scaleOffset = 100f / (float)ppc.assetsPPU;
        transform.localScale = new Vector3(startingScale.x * scaleOffset, startingScale.y * scaleOffset, startingScale.z);
    }
}