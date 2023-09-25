using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnClickCanvasSwap : MonoBehaviour
{
    public CanvasGroup[] canvasesToHide;
    public CanvasGroup[] canvasesToShow;
    public float fadeTime;

    public void OnClick() {
        Debug.Log("Clicked canvas swap");
        foreach (CanvasGroup canvasToHide in canvasesToHide) {
            StartCoroutine(fadeCanvasToAlpha(canvasToHide, 1.0f, 0.0f, fadeTime));
            canvasToHide.interactable = false;
            canvasToHide.blocksRaycasts = false;
        }

        foreach (CanvasGroup canvasToShow in canvasesToShow) {
            StartCoroutine(fadeCanvasToAlpha(canvasToShow, 0.0f, 1.0f, fadeTime));
            canvasToShow.interactable = true;
            canvasToShow.blocksRaycasts = true;
        }
    }

    private IEnumerator fadeCanvasToAlpha(CanvasGroup canvas, float startAlpha, float endAlpha, float fadeTime) {
        float alpha = startAlpha;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / fadeTime)
        {
            alpha = Mathf.Lerp(alpha, endAlpha, t);
            canvas.alpha = alpha;
            yield return null;
        }
    }
}
