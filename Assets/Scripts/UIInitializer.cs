using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Linq;
using static MusicManager;

public class UIInitializer : MonoBehaviour
{
    public MusicManager musicManager;
    public VideoPlayerController videoPlayerController;

    public Dropdown branchModeDropdown;
    public Dropdown branchesDropdown;
    public Text nextBranchText;

    public Button restartButton;
    public Button stopButton;
    public Button outroButton;
    public Slider musicSlider;
    public Slider videoSlider;

    public PlaybackConfig currentConfig;

    // Start is called before the first frame update
    void Start()
    {
        currentConfig = SetupManager.config;
        Debug.Log("*** Starting playback. Intro/Outro? " + currentConfig.hasIntroOutro + "; Reverb? " + currentConfig.hasReverb + "; # Clips: " + currentConfig.GetBranchClips().Count + "; Play Mode: " + currentConfig.playMode);
        setupBranchesDropdown(branchesDropdown);
        setupBranchModeDropdown(branchModeDropdown);

        branchModeDropdown.onValueChanged.AddListener(delegate {
            onBranchModeChange(branchModeDropdown);
        });

        outroButton.interactable = currentConfig.hasIntroOutro && currentConfig.outro != null;
        outroButton.onClick.AddListener(() => {
            musicManager.goToOutro();
        });

        restartButton.onClick.AddListener(() => {
            musicManager.StartPlayback();
        });

        stopButton.onClick.AddListener(() => {
            musicManager.endPlayback();
        });

        musicSlider.value = currentConfig.musicVolume;
        musicSlider.onValueChanged.AddListener((float value) => {
            musicManager.setVolume(value);
            currentConfig.musicVolume = value;
        });

        videoSlider.value = currentConfig.videoVolume;
        videoSlider.onValueChanged.AddListener((float value) => {
            videoPlayerController.setVolume(value);
            currentConfig.videoVolume = value;
        });


        musicManager.Initialize(
            currentConfig,
            (Section currentSection, Section nextSection, int lastClipIndex, int nextClipIndex) => {
                onPlaylistChange(currentSection, nextSection, lastClipIndex, nextClipIndex);
                Debug.Log("callback");
            });

        branchesDropdown.interactable = currentConfig.playMode == MusicManager.PlayMode.Manual;
        if (currentConfig.videoFilePath != null) {
            videoPlayerController.startVideo(currentConfig.videoFilePath, currentConfig.videoVolume);
        }

        musicManager.StartPlayback();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void setupBranchesDropdown(Dropdown branchesDropdown) {
        branchesDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < currentConfig.GetBranchClips().Count; i++) {
            options.Add($"Branch {i+1}");
        }
        branchesDropdown.AddOptions(options);
        branchesDropdown.onValueChanged.AddListener(delegate {
            int value = branchesDropdown.value;
            Debug.Log($"Switching to {value + 1}");
            musicManager.selectBranch(value);
        });
    }

    private void setupBranchModeDropdown(Dropdown branchModeDropdown) {
        Dropdown.OptionData option = branchModeDropdown.options.First(option => option.text.Equals(currentConfig.playMode.ToString()));
        int index = branchModeDropdown.options.IndexOf(option);
        branchModeDropdown.value = index;
        Debug.Log("Selected mode: " + option.text);
    }

    private void onBranchModeChange(Dropdown branchModeDropdown) {
        string value = branchModeDropdown.options[branchModeDropdown.value].text;
        Debug.Log($"Branch mode changed {value}");
        onPlayModeChange((MusicManager.PlayMode) System.Enum.Parse(typeof(MusicManager.PlayMode), value));
    }

    private void onPlayModeChange(MusicManager.PlayMode playMode) {
        branchesDropdown.interactable = playMode == MusicManager.PlayMode.Manual;
        musicManager.setPlayMode(playMode);
    }

    private void onPlaylistChange(Section currentSection, Section nextSection, int lastClipIndex, int nextClipIndex) {
        // Update text label
        string current = currentSection.ToString();
        string next = nextSection.ToString();

        if (currentSection == Section.Branch) {
            current = $"Branch {(lastClipIndex + 1)}";
        }
        if (nextSection == Section.Branch) {
            next = $"Branch {(nextClipIndex + 1)}";
        }

        nextBranchText.text = $"Now playing: {current}. Next: {next}";

        // Update buttons
        restartButton.interactable = currentSection == Section.None;
        stopButton.interactable = currentSection != Section.None;
        outroButton.interactable = currentConfig.hasIntroOutro && currentConfig.outro != null && currentSection != Section.Outro;
    }
}
