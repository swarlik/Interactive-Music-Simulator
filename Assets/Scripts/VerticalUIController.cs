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

public class VerticalUIController : MonoBehaviour
{
    public VerticalRemixingPlayer player;
    public VideoPlayerController videoPlayerController;

    // public ModeDropdown modeDropdown;
    public Dropdown layersDropdown;
    // public InputField xfadeTimeInput;
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
        // currentConfig = XFadeSetupManager.CURRENT_CONFIG;
        currentConfig = VerticalSetupManager.CURRENT_CONFIG;

        setupLayersDropdown();

        // modeDropdown.SetPlaybackMode(currentConfig.playbackMode);
        // modeDropdown.GetComponent<Dropdown>().onValueChanged.AddListener((dropdown) => {
        //     player.SetPlaybackMode(modeDropdown.GetPlaybackMode());
        // });

        restartButton.onClick.AddListener(() => {
            player.StartPlayback(currentConfig, layersDropdown.value, currentConfig.layeringMode);
        });

        stopButton.onClick.AddListener(() => {
            player.StopPlayback();
        });

        // outroButton.onClick.AddListener(() => {
        //     player.GoToOutro();
        // });

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

        player.StartPlayback(currentConfig, 0, currentConfig.layeringMode);
    }

    // Update is called once per frame
    void Update()
    {
        layersDropdown.interactable = !player.IsFading();
        restartButton.interactable = !player.IsPlaying();
        stopButton.interactable = player.IsPlaying();
        nextSectionText.text = GetPlayingText();
    }

    private void setupLayersDropdown() {
        layersDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < currentConfig.layers.Length; i++) {
            options.Add($"Layer {i + 1}");
        }
        layersDropdown.AddOptions(options);
        layersDropdown.onValueChanged.AddListener(delegate {
            int value = layersDropdown.value;
            Debug.Log($"Switching to section {value + 1}");
            player.GoToLayer(value);
        });
    }

    private string GetPlayingText() {
        if (!player.IsPlaying()) {
            return "Stopped";
        }

        int layer = player.GetCurrentLayer();
        return player.IsFading() ? $"Changing to Layer {layer + 1}" : $"Playing Layer {layer + 1}";
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
