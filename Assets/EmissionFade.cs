using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using DG.Tweening;

public class EmissionFadeMachine : MonoBehaviour
{
    private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
    private List<Material> allMats = new List<Material>();
    public MeshRenderer[] renderers;
    private Color startColor;
    public float startFade;
    public Light spotLight;
    public AudioSource bulbSparkSound;
    public AudioClip bulbOffClip;
    public AudioClip bulbOnClip;

    private void Start()
    {
        // Collect all materials
        //MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                if (!allMats.Contains(mat))
                {
                    mat.EnableKeyword("_EMISSION");
                    allMats.Add(mat);
                }
            }
        }

        if (allMats.Count == 0) return;

        // Use first mat's emission as base color
        startColor = allMats[0].GetColor(Emission);

        // Start machine
        StartCoroutine(FadeMachine());
    }

    private IEnumerator FadeMachine()
    {

        yield return new WaitForSeconds(startFade);
        while (true)
        {
            bulbSparkSound.clip = bulbOnClip;
            bulbSparkSound.Play();
            //yield return new WaitForSeconds(startFade);
            //  STEP 1: Fade-in each material one by one (staggered)
            foreach (var mat in allMats)
            {
                DOTween.To(
                    () => mat.GetColor(Emission),
                    c => mat.SetColor(Emission, c),
                    startColor,
                    0.5f
                ).SetEase(Ease.OutQuad);

                //yield return new WaitForSeconds(0.2f); // delay between each
            }
            //  Also fade in spotlight intensity
            if (spotLight != null)
            {
                DOTween.To(() => spotLight.intensity, x => spotLight.intensity = x, 1.07f, 0.5f);
            }

            //  STEP 2: Wait a bit, then fade out ALL at once
            yield return new WaitForSeconds(Random.Range(5.0f, 25f)); // delay before fade-out
            bulbSparkSound.clip = bulbOffClip;
            bulbSparkSound.Play();
            yield return new WaitForSeconds(startFade);
            foreach (var mat in allMats)
            {
                bulbSparkSound.Stop();
                DOTween.To(
                    () => mat.GetColor(Emission),
                    c => mat.SetColor(Emission, c),
                    Color.black,
                    0.25f
                ).SetEase(Ease.InOutQuad);
            }
            if (spotLight != null)
            {
                DOTween.To(() => spotLight.intensity, x => spotLight.intensity = x, 0f, 0.25f);
            }
            // STEP 3: Wait before looping
            yield return new WaitForSeconds(Random.Range(2.0f, 5.5f)); // delay before next round
        }
    }
}
