using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Linq;
using static VerticalRemixingConfig;
using static VerticalRemixingPlayer;

public class VerticalUIController : MonoBehaviour
{
    public VerticalRemixingPlayer player;
    public VideoPlayerController videoPlayerController;

    public Dropdown layersDropdown;
    public LayerToggles layerToggles;
    public Text nextSectionText;

    public Button restartButton;
    public Button stopButton;
    public Button outroButton;

    public Slider musicSlider;
    public Slider videoSlider;

    private VerticalRemixingConfig currentConfig;

    // Start is called before the first frame update
    void Start()
    {
        currentConfig = VerticalSetupManager.CURRENT_CONFIG;

        // setupLayersDropdown();

        player.Init(currentConfig);
        layerToggles.Setup(currentConfig.layers.Length, (index, isOn) => {
            Debug.Log($"Layer {index} changed {isOn}");
            player.ToggleLayer(index, isOn);
        });

        restartButton.onClick.AddListener(() => {
            player.StartPlayback(currentConfig, layerToggles.GetActiveLayers());
        });

        stopButton.onClick.AddListener(() => {
            player.StopPlayback();
        });

        outroButton.onClick.AddListener(() => {
            player.GoToOutro();
        });

        musicSlider.value = currentConfig.musicVolume;
        musicSlider.onValueChanged.AddListener((float value) => {
            player.SetVolume(value);
            currentConfig.musicVolume = value;
        });

        videoSlider.value = currentConfig.videoVolume;
        videoSlider.onValueChanged.AddListener((float value) => {
            videoPlayerController.setVolume(value);
            currentConfig.videoVolume = value;
        });

        if (currentConfig.videoFilePath != null && currentConfig.videoFilePath != "") {
            videoPlayerController.startVideo(
                FilePathUtils.LocalPathToFullPath(currentConfig.videoFilePath), currentConfig.videoVolume);
        }

        player.StartPlayback(currentConfig, layerToggles.GetActiveLayers());
    }

    // Update is called once per frame
    void Update()
    {
        // layersDropdown.interactable = !player.IsFading() && 
        //     (player.GetCurrentSegment() == Segment.Layers || player.GetCurrentSegment() == Segment.None);
        restartButton.interactable = !player.IsPlaying();
        stopButton.interactable = player.IsPlaying();
        outroButton.interactable = 
            currentConfig.HasOutro() &&
            player.IsPlaying() && 
            player.GetCurrentSegment() != Segment.Outro && 
            !player.IsFading();
        nextSectionText.text = GetPlayingText();
    }

    private string GetPlayingText() {
        if (!player.IsPlaying()) {
            return "Stopped";
        }

        if (player.GetCurrentSegment() == Segment.Intro) {
            return "Playing Intro";
        }

        if (player.GetCurrentSegment() == Segment.Outro) {
            return "Playing Outro";
        }

        return "Playing Layers";
    }


    /// TEST CODE

    private static string[] TEST_FILES = {
        "Builds/Latest/_TestAudio/Layering/L4 Bass Omni A.wav",
        "Builds/Latest/_TestAudio/Layering/L2 Choir Pad.wav",
        "Builds/Latest/_TestAudio/Layering/L3 Kora Outside.wav", 
        "Builds/Latest/_TestAudio/Layering/L6 Shaker.wav",
        "Builds/Latest/_TestAudio/Layering/L1 Sanchit Vox.wav", 
    };

    private static float[] LENGTHS = {
        25.263f,
        114.0f, 
        12.632f,
        12.632f,
        114.0f
    };

    public void createSampleConfig(Action<VerticalRemixingConfig> onComplete) {
        VerticalRemixingConfig config = new VerticalRemixingConfig();
        config.hasReverb = true;
        config.layeringMode = LayeringMode.Additive;
        List<Fadeable> layers = new List<Fadeable>();
        for (int i = 0; i < TEST_FILES.Length; i++) {
            Fadeable layer = new Fadeable();
            layer.file = TEST_FILES[i];
            layer.loopLength = LENGTHS[i];
            layer.fadeInTime = 4.0f;
            layer.fadeOutTime = 4.0f;
            layers.Add(layer);
        }
        config.layers = layers.ToArray();
        LoadFiles(() => {
            onComplete(config);
        });
    }

    private void LoadFiles(Action onComplete) {
        int loadCounter = 0;
        Action callback = () => {
            loadCounter++;
            if (loadCounter == TEST_FILES.Length) {
                onComplete();
            }
        };

        foreach (String file in TEST_FILES) {
            StartCoroutine(
                AudioCache.Instance().LoadClip(FilePathUtils.LocalPathToFullPath(file), callback, (error) => {
                    Debug.Log(error);
                }));
        }
    }
}
