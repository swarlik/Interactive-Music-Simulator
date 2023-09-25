using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/**
 * Sets and vends all configurable parameters to set up the demo
 * environment
 *
 * @author Sanchit Malhotra
 */
public class GameConfig : MonoBehaviour
{
    private static string composerPlaceholder = "<COMPOSER NAME>";
    private static string instructionsPlaceholder = 
        "<SAMPLE> Hear music react to gameplay footage in real time! Switch between zones and hear how the music adapts to the environment chosen. Change the intensity slider to add or subtract more layers to the music dynamically. Trigger an ending to hear a musically appropriate ending depending on the outcome chosen. The Restart Gameplay resets the footage and restarts the music, as triggering an ending will stop the music. Enjoy!";
    private static string aboutThisGameTextPlaceholder = 
        "<SAMPLE> Gameplay footage used from <GAME>. All gameplay footage was used for demonstrative purposes only and belongs to <COMPANY>.";
    private static string subtitleTextPlaceholder = "<SAMPLE> Vertical Remixing Score; Implemented in FMOD.";

    /*** Public variables ***/
    
    // Composer & instructions
    [Header("Composer & Instructions")]
    public string composerName = composerPlaceholder;
    [TextArea(3,10)]
    public string subtitleText = subtitleTextPlaceholder;
    [TextArea(15,20)]
    public string instructionsText = instructionsPlaceholder;
    [TextArea(5,10)]
    public string aboutThisGameText = aboutThisGameTextPlaceholder;
    
    [System.Serializable]
    public struct FMODParameterSetting {
        [Tooltip("Label for this parameter setting (ex. Low, Medium, High).")]
        public string label;
        [Tooltip("Value the parameter should be set to for this setting.")]
        public float value;
        [Tooltip("If controlling video, index (0-based) of the video to switch to when choosing this setting.")]
        public int videoUrlIndex;
    }

    [System.Serializable]
    public struct FMODParameter {
        [Tooltip("Whether to include this parameter dropdown in the controls.")]
        public bool enabled;
        [Tooltip("Whether or not this parameter affects which video clip is playing.")]
        public bool changesVideo;
        [Tooltip("Name of the FMOD parameter this dropdown controls.")]
        public string parameterName;
        [Tooltip("Label for the dropdown")]
        public string parameterLabel;
        [Tooltip("Index (0-based) of the parameter setting that should be selected on start.")]
        public int initialSettingIndex;
        public FMODParameterSetting[] parameterSettings;
    }

    [System.Serializable]
    public struct FMODParameter4 {
        public bool enabled;
        public string parameterName;
        public FMODParameterSetting parameterSetting1;
        public FMODParameterSetting parameterSetting2;
        public FMODParameterSetting parameterSetting3;
        public FMODParameterSetting parameterSetting4;
    }

    [System.Serializable]
    public struct VideoDelay {
        [Tooltip("Index (0-based) of the video clip transitioing from.")]
        public int fromIndex;
        [Tooltip("Index (0-based) of the video clip transitioning to.")]
        public int toIndex;
        [Tooltip("Delay time (in seconds) to wait before initiating this transition")]
        public float delayTimeSeconds;
    }

    // FMOD Parameter Config
    [Header("Configure FMOD Parameters")]
    public FMODParameter fmodParameter1;
    public FMODParameter fmodParameter2;

    [Header("Video URLs")]
    public string[] videoUrls;
    [Tooltip("Crossfade time when switching videos (seconds)")]
    public float videoCrossfadeTime;
    [Tooltip("Delay time before switching to a new video")]
    public VideoDelay[] videoDelays;

    // FMOD Ending Config
    [Header("Configure Game End Controls")]
    public bool showGameEndButtons = true;

    [Tooltip("Parameter that controls stopping the playing event.")]
    [InspectorName("Parameter Name")]
    public string gameEndParameterName;
    
    [InspectorName("Positive Text")]
    public string gameEndPositiveButtonText = "Win";
    
    [Tooltip("Value of the parameter that yields a positive outcome.")]
    [InspectorName("Positive Value")]
    public float gameEndPositiveButtonValue;
    
    [InspectorName("Negative Text")]
    public string gameEndNegativeButtonText = "Lose";
    
    [Tooltip("Value of the parameter that yields a negative outcome.")]
    [InspectorName("Negative Value")]
    public float gameEndNegativeButtonValue;

    [Tooltip("Video clip to play for positive game end.")]
    public string gameEndPositiveVideoUrl;

    [Tooltip("Video clip to play for negative game end.")]
    public string gameEndNegativeVideoUrl;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
