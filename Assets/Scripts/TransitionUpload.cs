using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static XFadeConfig;

public class TransitionUpload : AudioUpload
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
        return "Transition " + (index + 1);
    }

    public Transition GetInfo() {
        Transition transition = new Transition();
        transition.file = GetFilePath();
        // Convert 1-index sections to 0-index
        transition.from = int.Parse(GetSettingsInput("FromToRow/FromInput").text) - 1;
        transition.to = int.Parse(GetSettingsInput("FromToRow/ToInput").text) - 1;
        transition.fadeInTime = float.Parse(GetSettingsInput("FadeInOutRow/FadeInInput").text);
        transition.fadeOutTime = float.Parse(GetSettingsInput("FadeInOutRow/FadeOutInput").text);
        return transition;
    }

    public void SetValues(Transition info) {
        if (info.file != "") {
            base.SetFilePath(FilePathUtils.LocalPathToFullPath(info.file));
        }
        GetSettingsInput("FromToRow/FromInput").text = (info.from + 1).ToString();
        GetSettingsInput("FromToRow/ToInput").text = (info.to + 1).ToString();
        GetSettingsInput("FadeInOutRow/FadeInInput").text = info.fadeInTime.ToString();
        GetSettingsInput("FadeInOutRow/FadeOutInput").text = info.fadeOutTime.ToString();
    }

    private InputField GetSettingsInput(string path) {
        return gameObject.transform.Find($"Settings/{path}").gameObject.GetComponent<InputField>();
    }
}
