using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.DORotate(new Vector3(0, 0, 360f), 1.0f, RotateMode.WorldAxisAdd).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }
}
