using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SimpleFileBrowser;

public abstract class AudioUpload : MonoBehaviour
{
    protected int index;
    private Action onClear;
    private bool hasLoadedFile;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected abstract string GetLabel();

    public void Init(int index, Action onClear) {
        this.index = index;
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
        SetFilePath("");
    }

    public void SetIndex(int index) {
        this.index = index;
        Text sectionLabel = gameObject.transform.Find("UploadRow/SectionLabel").gameObject.GetComponent<Text>();
        sectionLabel.text = $"{(GetLabel())}:";
    }

    public string GetFilePath() {
        return GetFilePathComponent().text;
    }

    public void SetFilePath(string path) {
        hasLoadedFile = path != "";
        GetFilePathComponent().text = path;
    }

    public bool HasLoadedFile() {
        return hasLoadedFile;
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