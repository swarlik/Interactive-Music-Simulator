using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroSceneConfig : MonoBehaviour
{
    private static string descriptionPlaceholder = 
        "<SAMPLE> This is an interactive music demo mini-game that demonstrates how a score responds to gameplay in real-time!\nThis is a vertical remixing based score implemented in FMOD.";

    [Tooltip("Composer Name to display.")]
    public string composerName = "<COMPOSER>";
    [TextArea(5,10)]
    public string description = descriptionPlaceholder;
    [Tooltip("Preview video to play in the background (leave blank for no video).")]
    public string backgroundVideoUrl = "";
}
