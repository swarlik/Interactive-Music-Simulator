using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SimpleFileBrowser;

public abstract class AudioUpload : MonoBehaviour
{
    private int index;
    private Action<string> onLoad;
    private Action onClear;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected abstract string GetLabelType();

    public void Init(int index, Action<string> onLoad, Action onClear) {
        this.index = index;
        this.onLoad = onLoad;
        this.onClear = onClear;

        Button uploadButton = gameObject.transform.Find("UploadRow/Upload").gameObject.GetComponent<Button>();
        uploadButton.onClick.AddListener(() => {
            LoadAudioFile();
        });

        Button removeButton = gameObject.transform.Find("UploadRow/Remove").gameObject.GetComponent<Button>();
        removeButton.onClick.AddListener(() => {
            this.onClear();
            Destroy(gameObject);
        });

        SetIndex(index);
    }

    public void SetIndex(int index) {
        this.index = index;
        Text sectionLabel = gameObject.transform.Find("UploadRow/SectionLabel").gameObject.GetComponent<Text>();
        sectionLabel.text = $"{GetLabelType()} {(index + 1)}:";
    }

    public string GetFilePath() {
        return GetFilePathComponent().text;
    }

    private void SetFilePath(string path) {
        GetFilePathComponent().text = path;
    }

    private Text GetFilePathComponent() {
        return gameObject.transform.Find("FilePath").gameObject.GetComponent<Text>();
    }

    private void LoadAudioFile() {
        FileBrowser.ShowLoadDialog((paths) => {
            if (paths.Length == 0) {
                return;
            }
            string path = paths[0];
            Debug.Log($"Selected file {path}");
            if (!FilePathUtils.IsPathValid(path)) {
                Debug.Log("Invalid file!");
                return;
            }
            StartCoroutine(AudioCache.Instance().LoadClip(path, 
                () => {
                    SetFilePath(path);
                },
                () => {
                    Debug.Log("Audio Error");
                }));
        }, () => {
            Debug.Log("Canceled");
        }, FileBrowser.PickMode.Files, true, null, null, "Select", "Select");
    }
}