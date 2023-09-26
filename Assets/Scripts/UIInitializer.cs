using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Linq;

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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("*** Starting playback. Intro/Outro? " + SetupManager.hasIntroOutro + "; Reverb? " + SetupManager.hasReverb + "; # Clips: " + SetupManager.branchClips.Count + "; Play Mode: " + SetupManager.playMode);
        setupBranchesDropdown(branchesDropdown);
        setupBranchModeDropdown(branchModeDropdown);

        branchModeDropdown.onValueChanged.AddListener(delegate {
            onBranchModeChange(branchModeDropdown);
        });

        outroButton.interactable = SetupManager.hasIntroOutro;
        outroButton.onClick.AddListener(delegate {
            musicManager.goToOutro();
        });

        stopButton.onClick.AddListener(() => {
            musicManager.endPlayback();
        });

        musicManager.Initialize(
            SetupManager.branchClips.ToArray(), 
            SetupManager.branchLengths.ToArray(), 
            SetupManager.playMode, 
            SetupManager.hasReverb, 
            SetupManager.hasIntroOutro, 
            SetupManager.immediate,
            SetupManager.intro, 
            SetupManager.outro,
            SetupManager.introLength,
            nextBranchText,
            restartButton,
            stopButton);

        branchesDropdown.interactable = SetupManager.playMode == MusicManager.PlayMode.Manual;
        if (SetupManager.videoFilePath != null) {
            videoPlayerController.startVideo(SetupManager.videoFilePath);
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
        for (int i = 0; i < SetupManager.branchClips.Count; i++) {
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
        Dropdown.OptionData option = branchModeDropdown.options.First(option => option.text.Equals(SetupManager.playMode.ToString()));
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
}
