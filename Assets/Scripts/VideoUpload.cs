using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SimpleFileBrowser;

public class VideoUpload : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Button uploadButton = gameObject.transform.Find("UploadRow/Upload").gameObject.GetComponent<Button>();
        uploadButton.onClick.AddListener(() => {
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
                SetFilePath(path);
            }, () => {
                Debug.Log("Canceled");
            }, FileBrowser.PickMode.Files, true, null, null, "Select", "Select");
        });

        SetFilePath("");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFilePath(string path) {
        GetFilePathComponent().text = path;
    }
    
    public string GetFilePath() {
        return GetFilePathComponent().text;
    }

    private Text GetFilePathComponent() {
        return gameObject.transform.Find("FilePath").gameObject.GetComponent<Text>();
    }
}
