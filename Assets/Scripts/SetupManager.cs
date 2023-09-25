using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
// using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using SimpleFileBrowser;

public class SetupManager : MonoBehaviour
{
    private static int MIN_BRANCHES = 1;
    private static int MAX_BRANCHES = 5;

    public static List<AudioClip> branchClips;
    public static List<float> branchLengths;
    public static bool hasIntroOutro;
    public static bool hasReverb;
    public static MusicManager.PlayMode playMode;
    public static bool immediate = false;
    public static AudioClip intro;
    public static AudioClip outro;
    public static float introLength = -1.0f;
    public static string videoFilePath;

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
        loopLengthObjects = GameObject.FindGameObjectsWithTag("Loop Length");
        branchObjects = new List<GameObject>();
        SetupManager.branchClips = new List<AudioClip>();
        SetupManager.branchLengths = new List<float>();

        for (int i = 0; i < MAX_BRANCHES; i++) {
            SetupManager.branchClips.Add(null);
            SetupManager.branchLengths.Add(-1.0f);
            configureBranch(i);
        }

        configureIntroOutro();

        onIntroOutroToggle(introOutroToggle.isOn);
        introOutroToggle.onValueChanged.AddListener(delegate {
            onIntroOutroToggle(introOutroToggle.isOn);
        });

        onReverbToggle(reverbToggle.isOn);
        reverbToggle.onValueChanged.AddListener(delegate {
            onReverbToggle(reverbToggle.isOn);
        });

        onModeChange(modeDropdown);
        modeDropdown.onValueChanged.AddListener(delegate {
            onModeChange(modeDropdown);
        });

        onStyleChange(styleDropdown);
        styleDropdown.onValueChanged.AddListener(delegate {
            onStyleChange(styleDropdown);
        });

        onBranchCountChange(branchCountInput);
        branchCountInput.onValueChanged.AddListener(delegate {
            onBranchCountChange(branchCountInput);
        });

        setupVideoButton(uploadVideoButton, videoFilePathLabel);

        startButton.interactable = false;
        startButton.onClick.AddListener(delegate {
            SetupManager.branchClips = SetupManager.branchClips.Where(clip => clip != null).ToList();
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
    }

    private void onReverbToggle(bool isOn) {
        SetupManager.hasReverb = reverbToggle.isOn;
        foreach (GameObject loopLength in loopLengthObjects) {
            loopLength.SetActive(isOn);
        }
    }

    private void onIntroOutroToggle(bool isOn) {
        SetupManager.hasIntroOutro = isOn;
        introObject.SetActive(isOn);
        outroObject.SetActive(isOn);
    }

    private void onModeChange(Dropdown modeDropdown) {
        SetupManager.playMode = 
            (MusicManager.PlayMode) System.Enum.Parse(typeof(MusicManager.PlayMode), modeDropdown.options[modeDropdown.value].text);
    }

    private void onStyleChange(Dropdown styleDropdown) {
        string value = styleDropdown.options[styleDropdown.value].text;
        SetupManager.immediate = value.Equals("Immediate");
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
        filePathLabel.text = "";

        GameObject lengthInputObject = GameObject.Find($"{prefix}/Loop Length 1/Length Input 1");
        InputField lengthInput = lengthInputObject.GetComponent<InputField>();
        lengthInput.onValueChanged.AddListener(delegate {
            string value = lengthInput.text;
            if (value == "") {
                return;
            }
            float length = float.Parse(value);
            Debug.Log($"setting length {index} to {length}");
            SetupManager.branchLengths[index] = length;
        });
        setupButton(uploadButton, filePathLabel, index);
    }

    private void setupButton(Button button, Text filePathLabel, int index) {
        button.onClick.AddListener(delegate {
            StartCoroutine(loadClip(delegate(AudioClip clip, string path) {
                SetupManager.branchClips[index] = clip;
                filePathLabel.text = path;
                startButton.interactable = true;
            }));
        });
    }

    private void setupVideoButton(Button uploadVideoButton, Text filePathLabel) {
        filePathLabel.text = "";
        uploadVideoButton.onClick.AddListener(delegate {
            FileBrowser.ShowLoadDialog((paths) => {
                if (paths.Length == 0) {
                    return;
                }
                string path = paths[0];
                Debug.Log($"Selected file {path}");
                SetupManager.videoFilePath = path;
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
            label.text = "";
            setupIntroOutroButton(button, value.Equals("Intro"), label);
        });

        InputField introLength = GameObject.Find("Intro/Intro Length/Length Input 1").GetComponent<InputField>();
        introLength.onValueChanged.AddListener(delegate {
            string value = introLength.text;
            if (value == "") {
                return;
            }
            float length = float.Parse(value);
            Debug.Log($"setting intro length to {length}");
            SetupManager.introLength = length;
        });
    }

    private void setupIntroOutroButton(Button button, bool isIntro, Text filePathLabel) {
        button.onClick.AddListener(delegate {
            StartCoroutine(loadClip(delegate(AudioClip clip, string path) {
                filePathLabel.text = path;
                if (isIntro) {
                    SetupManager.intro = clip;
                } else {
                    SetupManager.outro = clip;
                }
            }));
        });
    }

    private IEnumerator loadClip(Action<AudioClip, string> onLoad) {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, "./", null, "Load", "Select");
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

    private void showDialog() {
        FileBrowser.SetDefaultFilter(".wav");
        FileBrowser.ShowLoadDialog(
            (paths) => { Debug.Log( "Selected: " + paths[0] ); },
    		() => { Debug.Log( "Canceled" ); },
    		FileBrowser.PickMode.Files, false, "./", null, "Select Folder", "Select" );
    }
}
