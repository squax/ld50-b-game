using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    [SerializeField]
    private Vector3 scaleTo;

    [SerializeField]
    private float time;

    [SerializeField]
    private Ease easing;

    void Start()
    {
        transform.DOScale(scaleTo, time).SetEase(easing).SetLoops(-1, LoopType.Yoyo);
    }
}
