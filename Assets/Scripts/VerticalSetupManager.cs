using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class VerticalSetupManager : MonoBehaviour
{
    private static string SETTINGS_FILE_NAME = "settings-vertical.json";
    private static bool loadedFromFile = false;

    public static VerticalRemixingConfig CURRENT_CONFIG;

    public Button addLayerButton;
    // public Button addIntroButton;
    // public Button addOutroButton;
    public Button saveButton;
    public Button loadButton;
    public Text statusText;
    public Button startButton;
    public Toggle reverbToggle;
    public Button backToMainButton;
    // public Toggle introOutroToggle;
    public LayeringModeDropdown modeDropdown;
    public ScrollRect scrollContainer;

    public GameObject container;
    public GameObject layerPrefab;
    // public GameObject introPrefab;
    // public GameObject outroPrefab;
    public VideoUpload videoUpload;

    private List<LayerUpload> layers;
    private IntroUpload intro;
    private OutroUpload outro;

    // Start is called before the first frame update
    void Start()
    {
        if (CURRENT_CONFIG == null) {
            CURRENT_CONFIG = new VerticalRemixingConfig();
        }

        addLayerButton.onClick.AddListener(() => {
            CreateUpload<LayerUpload>(layerPrefab, layers);
            scrollContainer.verticalNormalizedPosition = 0.0f;
        });

        // addIntroButton.onClick.AddListener(() => {
        //     CreateIntro();
        //     scrollContainer.verticalNormalizedPosition = 1.0f;
        // });

        // addOutroButton.onClick.AddListener(() => {
        //     CreateOutro();
        //     scrollContainer.verticalNormalizedPosition = 0.0f;
        // });

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
            SceneManager.LoadScene("PlayVertical");
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

    private void SetupFromConfig(VerticalRemixingConfig config) {
        layers = new List<LayerUpload>();
        
        foreach (Fadeable layerInfo in config.layers) {
            LayerUpload layer = CreateUpload<LayerUpload>(layerPrefab, layers);
            layer.SetValues(layerInfo);
        }

        // if (config.intro != null && config.intro.file != "") {
        //     CreateIntro();
        //     intro.SetValues(config.intro);
        // }

        // if (config.outro != null && config.outro.file != "") {
        //     CreateOutro();
        //     outro.SetValues(config.outro);
        // }

        if (config.layers.Length == 0) {
            // Add a default layer if there are no layers
            CreateUpload<LayerUpload>(layerPrefab, layers);
        }

        if (config.videoFilePath != null && config.videoFilePath != "") {
            videoUpload.SetFilePath(FilePathUtils.LocalPathToFullPath(config.videoFilePath));
        }

        reverbToggle.isOn = config.hasReverb;
        // introOutroToggle.isOn = config.hasIntroOutro;
        modeDropdown.SetLayeringMode(config.layeringMode);

        AllLoopableSections().ForEach((section) => {
            section.ShowHideLoopLength(reverbToggle.isOn);
        });
    }

    // Update is called once per frame
    void Update()
    {
        addLayerButton.interactable = layers.Count < VerticalRemixingConfig.MAX_LAYERS;
        startButton.interactable = layers.Exists(layer => layer.HasLoadedFile());
        // addIntroButton.interactable = intro == null;
        // addOutroButton.interactable = outro == null;

        // addIntroButton.gameObject.SetActive(introOutroToggle.isOn);
        // addOutroButton.gameObject.SetActive(introOutroToggle.isOn);
        // if (intro != null) {
        //     intro.gameObject.SetActive(introOutroToggle.isOn);
        // }
        // if (outro != null) {
        //     outro.gameObject.SetActive(introOutroToggle.isOn);
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
        if (upload is SectionUpload) {
            (upload as SectionUpload).ShowHideLoopLength(reverbToggle.isOn);
        }
        if (upload is LayerUpload) {
            (upload as LayerUpload).SetOnMove((moveUp) => {
                int currentIndex = list.IndexOf(upload);
                Debug.Log($"moving {moveUp} on {currentIndex}");
                if ((moveUp && currentIndex == 0) || (!moveUp && currentIndex == list.Count() - 1)) {
                    return;
                }

                int nextIndex = moveUp ? currentIndex - 1 : currentIndex + 1;
                list[currentIndex] = list[nextIndex];
                list[nextIndex] = upload;
                ResetIndices(list);
            });
        }
        ResetIndices(list);
        return upload;
    }

    // private void CreateIntro() {
    //     GameObject introObject = Instantiate(introPrefab, container.transform);
    //     // Insert at the top of the container
    //     introObject.transform.SetSiblingIndex(0);
    //     intro = introObject.GetComponent<IntroUpload>();
    //     intro.Init(0, () => {
    //         intro = null;
    //     });
    // }

    // private void CreateOutro() {
    //     GameObject outroObject = Instantiate(outroPrefab, container.transform);
    //     // Insert at the top of the container
    //     outro = outroObject.GetComponent<OutroUpload>();
    //     outro.Init(0, () => {
    //         outro = null;
    //     });
    // }

    private void ResetIndices<T>(List<T> list) where T : AudioUpload {
        for (int i = 0; i < list.Count; i++) {
            list[i].SetIndex(i);
            if (list[i] is LayerUpload) {
                (list[i] as LayerUpload).SetMoveButtons(i > 0, i < list.Count - 1);
                list[i].gameObject.transform.SetSiblingIndex(i);
            }
        }
    }

    private List<SectionUpload> AllLoopableSections() {
        List<SectionUpload> uploads = new List<SectionUpload>();
        uploads.AddRange(layers);
        return uploads;
    }

    private void SaveSettings() {
        CURRENT_CONFIG = new VerticalRemixingConfig();
        Fadeable[] layersInfo = layers.Select(layer => {
                Fadeable info = layer.GetInfo();
                info.file = FilePathUtils.FullPathToLocalPath(info.file);
                return info;
            }
        ).ToArray();

        CURRENT_CONFIG.layers = layersInfo;
        CURRENT_CONFIG.videoFilePath = FilePathUtils.FullPathToLocalPath(videoUpload.GetFilePath());
        CURRENT_CONFIG.hasReverb = reverbToggle.isOn;
        // CURRENT_CONFIG.hasIntroOutro = introOutroToggle.isOn;

        // if (intro != null) {
        //     Fadeable introInfo = intro.GetInfo();
        //     introInfo.file = FilePathUtils.FullPathToLocalPath(introInfo.file);
        //     CURRENT_CONFIG.intro = introInfo;
        // }

        // if (outro != null) {
        //     Fadeable outroInfo = outro.GetInfo();
        //     outroInfo.file = FilePathUtils.FullPathToLocalPath(outroInfo.file);
        //     CURRENT_CONFIG.outro = outroInfo;   
        // }

        CURRENT_CONFIG.layeringMode = modeDropdown.GetLayeringMode();
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
        string configJson;
        try {
            configJson = File.ReadAllText($"./{SETTINGS_FILE_NAME}");
        } catch (FileNotFoundException e) {
            ErrorToast.Instance().ShowError(
                $"Error loading settings file. Is there a file named {SETTINGS_FILE_NAME} in this directory?"
            );
            Debug.Log(e);
            return;
        }
        Debug.Log($"Read file: {configJson}");
        VerticalRemixingConfig config = JsonUtility.FromJson<VerticalRemixingConfig>(configJson); 
        Debug.Log(config);

        int filesToLoad = config.layers.Length; // add 2 for intro & outro
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

        foreach (Fadeable layer in config.layers) {
            if (layer.file != "") {
                StartCoroutine(AudioCache.Instance().LoadClip(
                    FilePathUtils.LocalPathToFullPath(layer.file),
                    callback,
                    (error) => {
                       ErrorToast.Instance().ShowError(error);
                    }
                ));
            } else {
                callback();
            }
        }

        // if (config.intro != null && config.intro.file != "") {
        //     StartCoroutine(AudioCache.Instance().LoadClip(
        //         FilePathUtils.LocalPathToFullPath(config.intro.file),
        //         callback,
        //         (error) => {
        //             ErrorToast.Instance().ShowError(error);
        //         }
        //     ));
        // } else {
        //     callback();
        // }

        // if (config.outro != null && config.outro.file != "") {
        //     StartCoroutine(AudioCache.Instance().LoadClip(
        //         FilePathUtils.LocalPathToFullPath(config.outro.file),
        //         callback,
        //         (error) => {
        //             ErrorToast.Instance().ShowError(error);
        //         }
        //     ));
        // } else {
        //     callback();
        // }
    }
}
