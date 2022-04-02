using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDrawOrder : MonoBehaviour
{
    [SerializeField]
    private string sortingLayerID = "Overlay";

    // Start is called before the first frame update
    void Start()
    {
        var renderer = GetComponent<Renderer>();

        renderer.sortingLayerID = SortingLayer.GetLayerValueFromID(SortingLayer.GetLayerValueFromName(sortingLayerID));
    }
}
