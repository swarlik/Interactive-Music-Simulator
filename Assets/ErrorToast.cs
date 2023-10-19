using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorToast : MonoBehaviour
{
    public static ErrorToast Instance() {
        return GameObject.Find("ErrorToast").GetComponent<ErrorToast>();
    }
    
    private CanvasGroup canvas;
    private Text errorText;
    private Button closeButton;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<CanvasGroup>();
        errorText = transform.Find("ErrorPanel/ErrorText").gameObject.GetComponent<Text>();
        closeButton = transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeButton.onClick.AddListener(() => {
            HideError();
        });

        HideError();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowError(string errorMessage) {
        Debug.LogError(errorMessage);
        errorText.text = errorMessage;
        canvas.alpha = 1.0f;
        closeButton.interactable = true;
        canvas.blocksRaycasts = true;
        canvas.interactable = true;
    }

    private void HideError() {
        canvas.alpha = 0.0f;
        closeButton.interactable = false;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }

    public IEnumerator FadeToZeroAlpha(float fadeTime, float delay, CanvasGroup canvas)
    {
        yield return new WaitForSeconds(delay);
        while (canvas.alpha > 0.0f)
        {
            canvas.alpha = canvas.alpha - (Time.deltaTime / fadeTime);
            yield return null;
        }
    }
}
