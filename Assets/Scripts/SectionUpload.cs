using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static XFadeConfig;

public class SectionUpload : AudioUpload
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override string GetLabel() {
        return "Section " + (index + 1);
    }

    public void ShowHideLoopLength(bool show) {
        gameObject.transform.Find($"Settings/LengthInputRow").gameObject.SetActive(show);
    }

    public Fadeable GetInfo() {
        Section section = new Section();
        section.file = GetFilePath();
        // Convert 1-index sections to 0-index
        if (GetSettingsInput("LengthInputRow/LengthInput").text != "") {
            section.loopLength = float.Parse(GetSettingsInput("LengthInputRow/LengthInput").text);
        }
        if (GetSettingsInput("FadeInOutRow/FadeInInput").text != "") {
            section.fadeInTime = float.Parse(GetSettingsInput("FadeInOutRow/FadeInInput").text);
        }
        if (GetSettingsInput("FadeInOutRow/FadeOutInput").text != "") {
            section.fadeOutTime = float.Parse(GetSettingsInput("FadeInOutRow/FadeOutInput").text);
        }
        return section;
    }

    public void SetValues(Fadeable info) {
        if (info.file != "") {
            base.SetFilePath(FilePathUtils.LocalPathToFullPath(info.file));
        }
        GetSettingsInput("LengthInputRow/LengthInput").text = info.loopLength.ToString();
        GetSettingsInput("FadeInOutRow/FadeInInput").text = info.fadeInTime.ToString();
        GetSettingsInput("FadeInOutRow/FadeOutInput").text = info.fadeOutTime.ToString();
    }

    private InputField GetSettingsInput(string path) {
        return gameObject.transform.Find($"Settings/{path}").gameObject.GetComponent<InputField>();
    }
}
