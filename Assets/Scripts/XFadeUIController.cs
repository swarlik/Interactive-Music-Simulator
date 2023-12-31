using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Linq;
using static XFadePlayer;
using static XFadeConfig;
using static ModeDropdown;

public class XFadeUIController : MonoBehaviour
{
    public XFadePlayer player;
    public VideoPlayerController videoPlayerController;

    public ModeDropdown modeDropdown;
    public Dropdown sectionsDropdown;
    public InputField xfadeTimeInput;
    public Text nextSectionText;

    public Button restartButton;
    public Button stopButton;
    public Button outroButton;

    public Slider musicSlider;
    public Slider videoSlider;

    private XFadeConfig currentConfig;

    // Start is called before the first frame update
    void Start()
    {
        currentConfig = XFadeSetupManager.CURRENT_CONFIG;

        setupSectionsDropdown(sectionsDropdown);

        modeDropdown.SetPlaybackMode(currentConfig.playbackMode);
        modeDropdown.GetComponent<Dropdown>().onValueChanged.AddListener((dropdown) => {
            player.SetPlaybackMode(modeDropdown.GetPlaybackMode());
        });

        restartButton.onClick.AddListener(() => {
            player.StartPlayback(currentConfig, sectionsDropdown.value, modeDropdown.GetPlaybackMode());
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

        player.StartPlayback(currentConfig, 0, modeDropdown.GetPlaybackMode());
    }

    // Update is called once per frame
    void Update()
    {
        bool changeInProgress = player.IsFading() || player.InTransition();
        sectionsDropdown.interactable = 
            modeDropdown.GetPlaybackMode() == PlaybackMode.Manual &&
            !changeInProgress && 
            player.GetCurrentSegment() != Segment.Outro;
        modeDropdown.GetComponent<Dropdown>().interactable = !changeInProgress;

        restartButton.interactable = !player.IsPlaying();
        stopButton.interactable = player.IsPlaying();

        outroButton.interactable = 
            currentConfig.hasIntroOutro &&
            currentConfig.intro != null && 
            player.IsPlaying() && 
            player.GetCurrentSegment() != Segment.Outro && 
            !changeInProgress;

        nextSectionText.text = GetPlayingText();
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

    private string GetPlayingText() {
        if (!player.IsPlaying()) {
            return "Stopped";
        }

        Segment currentSegment = player.GetCurrentSegment();
        if (currentSegment == Segment.None) {
            return "";
        }

        if (currentSegment == Segment.Intro || currentSegment == Segment.Outro) {
            return $"Playing {currentSegment.ToString()}";
        }

        if (currentSegment == Segment.Transition) {
            Transition t = (Transition) player.GetCurrent();
            return $"Transitioning from {(t.from + 1)} to {(t.to + 1)}";
        }

        int currentSection = player.CurrentSectionIndex() + 1;
        int nextSection = player.NextSectionIndex() + 1;
        return player.IsFading() ? 
            $"Fading into Section {currentSection}" : 
            $"Playing Section {currentSection}. Next Section {nextSection}.";
    }
}
