using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using static XFadeConfig;

public class XFadeSetupManager : MonoBehaviour
{
    private static string SETTINGS_FILE_NAME = "settings-xfade.json";
    private static bool loadedFromFile = false;

    public static XFadeConfig CURRENT_CONFIG;

    public Button addSectionButton;
    public Button addTransitionButton;
    public InputField xfadeTimeInput;
    public Button saveButton;
    public Button loadButton;
    public Text statusText;
    public Button startButton;
    public Toggle reverbToggle;

    public GameObject container;
    public GameObject sectionPrefab;
    public GameObject transitionPrefab;
    public VideoUpload videoUpload;

    private List<SectionUpload> sections;
    private List<TransitionUpload> transitions;

    // Start is called before the first frame update
    void Start()
    {
        if (CURRENT_CONFIG == null) {
            CURRENT_CONFIG = new XFadeConfig();
        }

        addSectionButton.onClick.AddListener(() => {
            CreateUpload<SectionUpload>(sectionPrefab, sections);
        });

        addTransitionButton.onClick.AddListener(() => {
            CreateUpload<TransitionUpload>(transitionPrefab, transitions);
        });

        saveButton.onClick.AddListener(() => {
            SaveSettings();
            WriteConfigToFile();
            statusText.GetComponent<FadeText>().SetText("settings saved!");
        });
        
        loadButton.onClick.AddListener(() => {
            LoadFromConfig();
        });

        startButton.onClick.AddListener(() => {
            SaveSettings();
            SceneManager.LoadScene("PlayXFade");
        });

        statusText.GetComponent<FadeText>().SetText(loadedFromFile ? "settings loaded!" : "");
        loadedFromFile = false;

        SetupFromConfig(CURRENT_CONFIG);
    }

    private void SetupFromConfig(XFadeConfig config) {
        sections = new List<SectionUpload>();
        transitions = new List<TransitionUpload>();
        
        foreach (Section sectionInfo in config.sections) {
            SectionUpload section = CreateUpload<SectionUpload>(sectionPrefab, sections);
            section.SetValues(sectionInfo);
        }

        foreach (Transition transitionInfo in config.transitions) {
            TransitionUpload transition = CreateUpload<TransitionUpload>(transitionPrefab, transitions);
            transition.SetValues(transitionInfo);
            // Set other values
        }

        if (config.xfadeTime > 0.0f) {
            xfadeTimeInput.text = config.xfadeTime.ToString();
        }

        if (config.sections.Length == 0 && config.transitions.Length == 0) {
            // Add a default sections if there are no sections or transitions
            CreateUpload<SectionUpload>(sectionPrefab, sections);
        }

        if (config.videoFilePath != null && config.videoFilePath != "") {
            videoUpload.SetFilePath(FilePathUtils.LocalPathToFullPath(config.videoFilePath));
        }

        reverbToggle.isOn = config.hasReverb;
    }

    // Update is called once per frame
    void Update()
    {
        addSectionButton.interactable = sections.Count < XFadeConfig.MAX_SECTIONS;
        addTransitionButton.interactable = transitions.Count < XFadeConfig.MAX_TRANSITIONS;
        startButton.interactable = sections.Exists(section => section.HasLoadedFile());
    }

    private T CreateUpload<T>(GameObject prefab, List<T> list) where T : AudioUpload {
        GameObject uploadObject = Instantiate(prefab, container.transform);
        T upload = uploadObject.GetComponent<T>();
        upload.Init(list.Count, 
            (string path) => { 
                Debug.Log(path); 
            }, 
            () => { 
                list.Remove(upload);
                ResetIndices(list);
            });
        list.Add(upload);
        return upload;
    }

    private void ResetIndices<T>(List<T> list) where T : AudioUpload {
        for (int i = 0; i < list.Count; i++) {
            list[i].SetIndex(i);
        }
    }

    private void SaveSettings() {
        Section[] sectionsInfo = sections.Select(section => {
                Section info = section.GetInfo();
                info.file = FilePathUtils.FullPathToLocalPath(info.file);
                return info;
            }
        ).ToArray();
        Transition[] transitionsInfo = transitions.Select(transition => {
                Transition info = transition.GetInfo();
                info.file = FilePathUtils.FullPathToLocalPath(info.file);
                return info;
            }
        ).ToArray();

        string value = xfadeTimeInput.text;
        if (value != "") {
            float time = float.Parse(value);
            CURRENT_CONFIG.xfadeTime = time;
        }

        CURRENT_CONFIG.sections = sectionsInfo;
        CURRENT_CONFIG.transitions = transitionsInfo;
        CURRENT_CONFIG.videoFilePath = FilePathUtils.FullPathToLocalPath(videoUpload.GetFilePath());
        CURRENT_CONFIG.hasReverb = reverbToggle.isOn;
    }

    private void WriteConfigToFile() {
        string jsonString = JsonUtility.ToJson(CURRENT_CONFIG);
        Debug.Log(jsonString);

        using (StreamWriter writer = new StreamWriter(SETTINGS_FILE_NAME))
        {
            writer.Write(jsonString);
        }
    }

    private void LoadFromConfig() {
        string configJson = File.ReadAllText($"./{SETTINGS_FILE_NAME}");
        Debug.Log($"Read file: {configJson}");
        XFadeConfig config = JsonUtility.FromJson<XFadeConfig>(configJson); 
        Debug.Log(config);

        int filesToLoad = config.sections.Length + config.transitions.Length;
        int filesLoaded = 0;
        Action callback = () => {
            filesLoaded++;
            if (filesLoaded == filesToLoad) {
                // Set the config and reload the scene once all files have loaded
                CURRENT_CONFIG = config;
                loadedFromFile = true;
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        };

        foreach (Section section in config.sections) {
            StartCoroutine(AudioCache.Instance().LoadClip(
                FilePathUtils.LocalPathToFullPath(section.file),
                callback,
                () => {
                    Debug.Log("error loading file");
                }
            ));
        }

        
        foreach (Transition transition in config.transitions) {
            StartCoroutine(AudioCache.Instance().LoadClip(
                FilePathUtils.LocalPathToFullPath(transition.file),
                callback,
                () => {
                    Debug.Log("error loading file");
                }
            ));
        }
    }
}
