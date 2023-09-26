using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using SimpleFileBrowser;

public class SetupManager : MonoBehaviour
{
    private static int MIN_BRANCHES = 1;
    private static int MAX_BRANCHES = 5;

    public static PlaybackConfig config = null;

    public Toggle introOutroToggle;
    public Toggle reverbToggle;
    public Dropdown modeDropdown;
    public Dropdown styleDropdown;
    public InputField branchCountInput;
    public Button startButton;
    public Button uploadVideoButton;
    public Text videoFilePathLabel;

    private GameObject[] loopLengthObjects;
    private List<GameObject> branchObjects;
    private GameObject introObject;
    private GameObject outroObject;

    // Start is called before the first frame update
    void Start()
    {
        if (config == null) {
            config = new PlaybackConfig(MAX_BRANCHES);
        }

        loopLengthObjects = GameObject.FindGameObjectsWithTag("Loop Length");
        branchObjects = new List<GameObject>();

        for (int i = 0; i < MAX_BRANCHES; i++) {
            configureBranch(i);
        }

        configureIntroOutro();

        introOutroToggle.isOn = config.hasIntroOutro;
        onIntroOutroToggle(introOutroToggle.isOn);
        introOutroToggle.onValueChanged.AddListener(delegate {
            onIntroOutroToggle(introOutroToggle.isOn);
        });

        reverbToggle.isOn = config.hasReverb;
        onReverbToggle(reverbToggle.isOn);
        reverbToggle.onValueChanged.AddListener(delegate {
            onReverbToggle(reverbToggle.isOn);
        });


        int index = modeDropdown.options.FindIndex((option) => option.text.Equals(config.playMode.ToString()));
        modeDropdown.value = index;
        onModeChange(modeDropdown);
        modeDropdown.onValueChanged.AddListener(delegate {
            onModeChange(modeDropdown);
        });

        styleDropdown.value = config.immediate ? 1 : 0;
        onStyleChange(styleDropdown);
        styleDropdown.onValueChanged.AddListener(delegate {
            onStyleChange(styleDropdown);
        });

        branchCountInput.text = config.numBranches.ToString();
        onBranchCountChange(branchCountInput);
        branchCountInput.onValueChanged.AddListener(delegate {
            onBranchCountChange(branchCountInput);
        });

        setupVideoButton(uploadVideoButton, videoFilePathLabel);

        startButton.interactable = config.GetBranchClips().Count > 0;
        startButton.onClick.AddListener(delegate {
            SceneManager.LoadScene("Main Scene");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void showBranches(int count) {
        for (int i = 0; i < MAX_BRANCHES; i++) {
            branchObjects[i].SetActive(i < count);
        }
        config.SetActiveBranches(count);
    }

    private void onReverbToggle(bool isOn) {
        config.hasReverb = isOn;
        foreach (GameObject loopLength in loopLengthObjects) {
            loopLength.SetActive(isOn);
        }
    }

    private void onIntroOutroToggle(bool isOn) {
        config.hasIntroOutro = isOn;
        introObject.SetActive(isOn);
        outroObject.SetActive(isOn);
    }

    private void onModeChange(Dropdown modeDropdown) {
        MusicManager.PlayMode playMode =
            (MusicManager.PlayMode) System.Enum.Parse(typeof(MusicManager.PlayMode), modeDropdown.options[modeDropdown.value].text);
        config.playMode = playMode;
    }

    private void onStyleChange(Dropdown styleDropdown) {
        string value = styleDropdown.options[styleDropdown.value].text;
        config.immediate = value.Equals("Immediate");
    }

    private void onBranchCountChange(InputField branchCountInput) {
        string value = branchCountInput.text;
        if (value == "") {
            return;
        }
        int count = int.Parse(value);
        if (count < MIN_BRANCHES || count > MAX_BRANCHES) {
            return;
        }
        showBranches(count);
    }

    private void configureBranch(int index) {
        string prefix = $"Branches/Branch Row {index + 1}";
        GameObject branchObject = GameObject.Find($"{prefix}");
        branchObjects.Add(branchObject);

        GameObject labelObject = GameObject.Find($"{prefix}/Branch 1");
        Text label = labelObject.GetComponent<Text>();
        label.text = $"Branch {(index + 1)}:";

        GameObject uploadButtonObject = GameObject.Find($"{prefix}/Upload 1");
        Button uploadButton = uploadButtonObject.GetComponent<Button>();

        GameObject filePathObject = GameObject.Find($"{prefix}/File Path 1");
        Text filePathLabel = filePathObject.GetComponent<Text>();
        filePathLabel.text = index < config.GetBranchPaths().Count ? config.GetBranchPaths()[index] : "";

        GameObject lengthInputObject = GameObject.Find($"{prefix}/Loop Length 1/Length Input 1");
        InputField lengthInput = lengthInputObject.GetComponent<InputField>();
        if (index < config.GetBranchLengths().Count) {
            lengthInput.text = config.GetBranchLengths()[index].ToString();
        }
        lengthInput.onValueChanged.AddListener(delegate {
            string value = lengthInput.text;
            if (value == "") {
                return;
            }
            float length = float.Parse(value);
            Debug.Log($"setting length {index} to {length}");
            config.SetBranchLength(length, index);
        });
        setupButton(uploadButton, filePathLabel, index);
    }

    private void setupButton(Button button, Text filePathLabel, int index) {
        button.onClick.AddListener(delegate {
            StartCoroutine(loadClip(delegate(AudioClip clip, string path) {
                config.SetBranchClip(clip, index, path);
                filePathLabel.text = path;
                startButton.interactable = true;
            }));
        });
    }

    private void setupVideoButton(Button uploadVideoButton, Text filePathLabel) {
        filePathLabel.text = config.videoFilePath;
        uploadVideoButton.onClick.AddListener(delegate {
            FileBrowser.ShowLoadDialog((paths) => {
                if (paths.Length == 0) {
                    return;
                }
                string path = paths[0];
                Debug.Log($"Selected file {path}");
                config.videoFilePath = path;
                filePathLabel.text = path;
            }, () => {
                Debug.Log("Canceled");
            }, FileBrowser.PickMode.Files, true, null, null, "Select", "Select");
        });
    }

    private void configureIntroOutro() {
        introObject = GameObject.Find("Intro");
        outroObject = GameObject.Find("Outro");

        Array.ForEach(new[] {"Intro", "Outro"}, delegate(string value) {
            Button button = GameObject.Find($"{value}/Upload {value}").GetComponent<Button>();
            Text label = GameObject.Find($"{value}/File Path").GetComponent<Text>();
            label.text = value.Equals("Intro") ? config.introFile : config.outroFile;
            setupIntroOutroButton(button, value.Equals("Intro"), label);
        });

        InputField introLength = GameObject.Find("Intro/Intro Length/Length Input 1").GetComponent<InputField>();
        if (config.introLength > 0) {
            introLength.text = config.introLength.ToString();
        }
        introLength.onValueChanged.AddListener(delegate {
            string value = introLength.text;
            if (value == "") {
                return;
            }
            float length = float.Parse(value);
            Debug.Log($"setting intro length to {length}");
            config.introLength = length;
        });
    }

    private void setupIntroOutroButton(Button button, bool isIntro, Text filePathLabel) {
        button.onClick.AddListener(delegate {
            StartCoroutine(loadClip(delegate(AudioClip clip, string path) {
                filePathLabel.text = path;
                if (isIntro) {
                    config.intro = clip;
                    config.introFile = path;
                } else {
                    config.outro = clip;
                    config.outroFile = path;
                }
            }));
        });
    }

    private IEnumerator loadClip(Action<AudioClip, string> onLoad) {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load", "Select");
        if (!FileBrowser.Success || FileBrowser.Result.Length == 0) {
            yield break;
        }

        Debug.Log($"Selected file: {FileBrowser.Result[0]}");

        string path = FileBrowser.Result[0];
        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV)) {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError) {
                Debug.Log(req.error);
            } else {
                Debug.Log("Loaded audio: " + path);
                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                onLoad(clip, path);
            }
        }
    }
}
