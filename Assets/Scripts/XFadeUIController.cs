using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Linq;
using static MusicManager;

public class XFadeUIController : MonoBehaviour
{
    public XFadePlayer player;
    public VideoPlayerController videoPlayerController;

    public Dropdown sectionsDropdown;
    public InputField xfadeTimeInput;
    public Text nextSectionText;

    public Button restartButton;
    public Button stopButton;

    public Slider musicSlider;
    public Slider videoSlider;

    private XFadeConfig currentConfig;

    // Start is called before the first frame update
    void Start()
    {
        currentConfig = XFadeSetupManager.CURRENT_CONFIG;

        setupSectionsDropdown(sectionsDropdown);

        restartButton.onClick.AddListener(() => {
            player.StartPlayback(currentConfig, sectionsDropdown.value);
        });

        stopButton.onClick.AddListener(() => {
            player.StopPlayback();
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

        player.StartPlayback(currentConfig, 0);
    }

    // Update is called once per frame
    void Update()
    {
        sectionsDropdown.interactable = !player.IsFading() && !player.InTransition();
        restartButton.interactable = !player.IsPlaying();
        stopButton.interactable = player.IsPlaying();

        string text = "";
        if (!player.IsPlaying()) {
            text = "Stopped";
        } else if (player.InTransition()) {
            text = $"Transitioning from {(player.GetCurrentSection() + 1)} to {(player.GetNextSection() + 1)}";
        } else if (player.IsFading()) {
            text = $"Crossfading into Section {(player.GetCurrentSection() + 1)}";
        } else {
            text = $"Playing Section {(player.GetCurrentSection() + 1)}";
        }
        nextSectionText.text = text;
    }

    private void setupSectionsDropdown(Dropdown sectionsDropdown) {
        sectionsDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < currentConfig.sections.Length; i++) {
            options.Add($"Section {i + 1}");
        }
        sectionsDropdown.AddOptions(options);
        sectionsDropdown.onValueChanged.AddListener(delegate {
            int value = sectionsDropdown.value;
            Debug.Log($"Switching to section {value + 1}");
            player.GoToSection(value);
        });
    }
}
