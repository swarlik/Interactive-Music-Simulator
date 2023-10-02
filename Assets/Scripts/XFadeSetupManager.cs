using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static XFadeConfig;

public class XFadeSetupManager : MonoBehaviour
{
    public static XFadeConfig CURRENT_CONFIG;

    public Button addSectionButton;
    public Button addTransitionButton;
    public InputField xfadeTimeInput;

    public GameObject container;
    public GameObject sectionPrefab;
    public GameObject transitionPrefab;

    private List<SectionUpload> sections;
    private List<TransitionUpload> transitions;

    // Start is called before the first frame update
    void Start()
    {
        if (CURRENT_CONFIG == null) {
            CURRENT_CONFIG = new XFadeConfig();
        }

        sections = new List<SectionUpload>();
        transitions = new List<TransitionUpload>();

        addSectionButton.onClick.AddListener(() => {
            CreateUpload<SectionUpload>(sectionPrefab, sections);
        });

        addTransitionButton.onClick.AddListener(() => {
            CreateUpload<TransitionUpload>(transitionPrefab, transitions);
        });
    }

    // Update is called once per frame
    void Update()
    {
        addSectionButton.interactable = sections.Count < XFadeConfig.MAX_SECTIONS;
        addTransitionButton.interactable = transitions.Count < XFadeConfig.MAX_TRANSITIONS;
    }

    private void CreateUpload<T>(GameObject prefab, List<T> list) where T : AudioUpload {
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
    }

    private void ResetIndices<T>(List<T> list) where T : AudioUpload {
        for (int i = 0; i < list.Count; i++) {
            list[i].SetIndex(i);
        }
    }

    private void SaveToConfig() {
        string[] sectionPaths = sections
            .Select(section => FilePathUtils.FullPathToLocalPath(section.GetFilePath()))
            .Where(path => path != null && path != "")
            .ToArray();
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

        CURRENT_CONFIG.sections = sectionPaths;
        CURRENT_CONFIG.transitions = transitionsInfo;
        Debug.Log(JsonUtility.ToJson(CURRENT_CONFIG));
    }
}
