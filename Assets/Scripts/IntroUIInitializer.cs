using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroUIInitializer : MonoBehaviour
{
    public IntroSceneConfig config;
    public VideoPlayer backgroundVideoPlayer;
    public Text composerLabel;
    public Text descriptionLabel;

    // Start is called before the first frame update
    void Start()
    {
        if (config == null) {
            Debug.LogError("Pls send config pls");
            return;
        }

        composerLabel.text = "Music Composed and Implemented by " + config.composerName + ".";
        descriptionLabel.text = config.description;
        if (config.backgroundVideoUrl != "") {
            backgroundVideoPlayer.url = config.backgroundVideoUrl;
            backgroundVideoPlayer.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
