using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

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
            string path = EditorUtility.OpenFilePanel("Choose audio for branch " + (index + 1), "", "wav");
            if (string.IsNullOrWhiteSpace(path)) {
                return;
            }
            Debug.Log(path);
            byte[] fileData = File.ReadAllBytes(path);
            Debug.Log(fileData.Length);
            StartCoroutine(loadClip(path, delegate(AudioClip clip) {
                SetupManager.branchClips[index] = clip;
                filePathLabel.text = path;
                startButton.interactable = true;
            }));
        });
    }

    private void setupVideoButton(Button uploadVideoButton, Text filePathLabel) {
        filePathLabel.text = "";
        uploadVideoButton.onClick.AddListener(delegate {
            string path = EditorUtility.OpenFilePanel("Choose video", "", "mp4,mov");
            Debug.Log(path);
            SetupManager.videoFilePath = path;
            filePathLabel.text = path;
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
            string path = EditorUtility.OpenFilePanel("Choose audio", "", "wav");
            if (path == null) {
                return;
            }
            Debug.Log(path);
            byte[] fileData = File.ReadAllBytes(path);
            StartCoroutine(loadClip(path, delegate(AudioClip clip) {
                filePathLabel.text = path;
                if (isIntro) {
                    SetupManager.intro = clip;
                } else {
                    SetupManager.outro = clip;
                }
            }));
        });
    }

    private IEnumerator loadClip(string path, Action<AudioClip> onLoad) {
        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV)) {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError) {
                Debug.Log(req.error);
            } else {
                Debug.Log("Loaded audio: " + path);
                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                onLoad(clip);
            }
        }
    }
}
