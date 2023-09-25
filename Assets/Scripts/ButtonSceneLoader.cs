using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    public Button button;
    public string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(delegate {
            SceneManager.LoadScene(sceneName);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
