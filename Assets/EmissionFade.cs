using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EmissionFade : MonoBehaviour
{
    private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
    
    private void Start()
    {
        var mat = GetComponent<MeshRenderer>().material;
        
        Color startColor = mat.GetColor(Emission);

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(Random.Range(5.0f, 25f));
        seq.Append(
            DOTween.To(
                () => mat.GetColor(Emission),
                val => mat.SetColor(Emission, val),
                Color.black,
                Random.Range(0.1f, 0.5f)
            ).SetEase(Ease.InOutQuad)
        );
        seq.AppendInterval(Random.Range(0.1f, 2.0f));
        seq.Append(
            DOTween.To(
                () => mat.GetColor(Emission),
                val => mat.SetColor(Emission, val),
                startColor,
                Random.Range(0.1f, 2.0f)
            ).SetEase(Ease.Linear)
        );

        seq.SetLoops(-1); // Default is LoopType.Restart, so we alternate manually
    }
}