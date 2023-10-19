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
    public Button addIntroButton;
    public Button addOutroButton;
    public InputField xfadeTimeInput;
    public Button saveButton;
    public Button loadButton;
    public Text statusText;
    public Button startButton;
    public Toggle reverbToggle;
    public Button backToMainButton;
    public Toggle introOutroToggle;
    public ModeDropdown modeDropdown;
    public ScrollRect scrollContainer;

    public GameObject container;
    public GameObject sectionPrefab;
    public GameObject transitionPrefab;
    public GameObject introPrefab;
    public GameObject outroPrefab;
    public VideoUpload videoUpload;

    private List<SectionUpload> sections;
    private List<TransitionUpload> transitions;
    private IntroUpload intro;
    private OutroUpload outro;

    // Start is called before the first frame update
    void Start()
    {
        if (CURRENT_CONFIG == null) {
            CURRENT_CONFIG = new XFadeConfig();
        }

        addSectionButton.onClick.AddListener(() => {
            CreateUpload<SectionUpload>(sectionPrefab, sections);
            scrollContainer.verticalNormalizedPosition = 0.0f;
        });

        addTransitionButton.onClick.AddListener(() => {
            CreateUpload<TransitionUpload>(transitionPrefab, transitions);
            scrollContainer.verticalNormalizedPosition = 0.0f;
        });

        addIntroButton.onClick.AddListener(() => {
            CreateIntro();
            scrollContainer.verticalNormalizedPosition = 1.0f;
        });

        addOutroButton.onClick.AddListener(() => {
            CreateOutro();
            scrollContainer.verticalNormalizedPosition = 0.0f;
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

        backToMainButton.onClick.AddListener(() => {
            SaveSettings();
            SceneManager.LoadScene("Launch");
        });

        reverbToggle.onValueChanged.AddListener((isOn) => {
            AllLoopableSections().ForEach((section) => {
                section.ShowHideLoopLength(isOn);
            });
        });

        statusText.GetComponent<FadeText>().SetText(loadedFromFile ? "settings loaded!" : "");
        loadedFromFile = false;

        SetupFromConfig(CURRENT_CONFIG);
    }

    private void SetupFromConfig(XFadeConfig config) {
        sections = new List<SectionUpload>();
        transitions = new List<TransitionUpload>();
        
        foreach (Fadeable sectionInfo in config.sections) {
            SectionUpload section = CreateUpload<SectionUpload>(sectionPrefab, sections);
            section.SetValues(sectionInfo);
        }

        foreach (Transition transitionInfo in config.transitions) {
            TransitionUpload transition = CreateUpload<TransitionUpload>(transitionPrefab, transitions);
            transition.SetValues(transitionInfo);
        }

        if (config.intro != null && config.intro.file != "") {
            CreateIntro();
            intro.SetValues(config.intro);
        }

        if (config.outro != null && config.outro.file != "") {
            CreateOutro();
            outro.SetValues(config.outro);
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
        introOutroToggle.isOn = config.hasIntroOutro;
        modeDropdown.SetPlaybackMode(config.playbackMode);

        AllLoopableSections().ForEach((section) => {
            section.ShowHideLoopLength(reverbToggle.isOn);
        });
    }

    // Update is called once per frame
    void Update()
    {
        addSectionButton.interactable = sections.Count < XFadeConfig.MAX_SECTIONS;
        addTransitionButton.interactable = transitions.Count < XFadeConfig.MAX_TRANSITIONS;
        startButton.interactable = sections.Exists(section => section.HasLoadedFile());
        addIntroButton.interactable = intro == null;
        addOutroButton.interactable = outro == null;

        addIntroButton.gameObject.SetActive(introOutroToggle.isOn);
        addOutroButton.gameObject.SetActive(introOutroToggle.isOn);
        if (intro != null) {
            intro.gameObject.SetActive(introOutroToggle.isOn);
        }
        if (outro != null) {
            outro.gameObject.SetActive(introOutroToggle.isOn);
        }
    }

    private T CreateUpload<T>(GameObject prefab, List<T> list) where T : AudioUpload {
        GameObject uploadObject = Instantiate(prefab, container.transform);
        T upload = uploadObject.GetComponent<T>();
        upload.Init(list.Count, 
            () => { 
                list.Remove(upload);
                ResetIndices(list);
            });
        list.Add(upload);
        return upload;
    }

    private void CreateIntro() {
        GameObject introObject = Instantiate(introPrefab, container.transform);
        // Insert at the top of the container
        introObject.transform.SetSiblingIndex(0);
        intro = introObject.GetComponent<IntroUpload>();
        intro.Init(0, () => {
            intro = null;
        });
    }

    private void CreateOutro() {
        GameObject outroObject = Instantiate(outroPrefab, container.transform);
        // Insert at the top of the container
        outro = outroObject.GetComponent<OutroUpload>();
        outro.Init(0, () => {
            outro = null;
        });
    }

    private void ResetIndices<T>(List<T> list) where T : AudioUpload {
        for (int i = 0; i < list.Count; i++) {
            list[i].SetIndex(i);
        }
    }

    private List<SectionUpload> AllLoopableSections() {
        List<SectionUpload> uploads = new List<SectionUpload>();
        uploads.AddRange(sections);
        if (intro != null) {
            uploads.Add(intro);
        }
        return uploads;
    }

    private void SaveSettings() {
        CURRENT_CONFIG = new XFadeConfig();
        Fadeable[] sectionsInfo = sections.Select(section => {
                Fadeable info = section.GetInfo();
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
        CURRENT_CONFIG.hasIntroOutro = introOutroToggle.isOn;

        if (intro != null) {
            Fadeable introInfo = intro.GetInfo();
            introInfo.file = FilePathUtils.FullPathToLocalPath(introInfo.file);
            CURRENT_CONFIG.intro = introInfo;
        }

        if (outro != null) {
            Fadeable outroInfo = outro.GetInfo();
            outroInfo.file = FilePathUtils.FullPathToLocalPath(outroInfo.file);
            CURRENT_CONFIG.outro = outroInfo;   
        }

        CURRENT_CONFIG.playbackMode = modeDropdown.GetPlaybackMode();
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

        int filesToLoad = config.sections.Length + config.transitions.Length + 2; // intro & outro
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

        foreach (Fadeable section in config.sections) {
            if (section.file != "") {
                StartCoroutine(AudioCache.Instance().LoadClip(
                    FilePathUtils.LocalPathToFullPath(section.file),
                    callback,
                    () => {
                        Debug.Log($"error loading file {section.file}");
                    }
                ));
            } else {
                callback();
            }
        }

        
        foreach (Transition transition in config.transitions) {
            if (transition.file != "") {
                StartCoroutine(AudioCache.Instance().LoadClip(
                    FilePathUtils.LocalPathToFullPath(transition.file),
                    callback,
                    () => {
                        Debug.Log($"error loading file {transition.file}");
                    }
                ));
            } else {
                callback();
            }
        }

        if (config.intro != null && config.intro.file != "") {
            StartCoroutine(AudioCache.Instance().LoadClip(
                FilePathUtils.LocalPathToFullPath(config.intro.file),
                callback,
                () => {
                    Debug.Log($"error loading file {config.intro.file}");
                }
            ));
        } else {
            callback();
        }

        if (config.outro != null && config.outro.file != "") {
            StartCoroutine(AudioCache.Instance().LoadClip(
                FilePathUtils.LocalPathToFullPath(config.outro.file),
                callback,
                () => {
                    Debug.Log($"error loading file {config.outro.file}");
                }
            ));
        } else {
            callback();
        }
    }
}
