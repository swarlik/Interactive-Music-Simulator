using UnityEngine;
using UnityEngine.UI;
using System;
using SimpleFileBrowser;

public class IntroOutro : SettingElement {
    private GameObject introObject;
    private GameObject outroObject;

    public IntroOutro(GameObject introObject, GameObject outroObject) {
        this.introObject = introObject;
        this.outroObject = outroObject;
    }

    public override void Setup(BranchingConfig config, Action notifyConfigChange) {
        Array.ForEach(new[] {"Intro", "Outro"}, delegate(string value) {
            Button button = GameObject.Find($"{value}/Upload {value}").GetComponent<Button>();
            setupIntroOutroButton(button, value.Equals("Intro"), config, notifyConfigChange);
        });

        InputField introLength = GameObject.Find("Intro/Intro Length/Length Input 1").GetComponent<InputField>();
        introLength.onValueChanged.AddListener(delegate {
            string value = introLength.text;
            if (value == "") {
                return;
            }
            float length = float.Parse(value);
            Debug.Log($"setting intro length to {length}");
            config.intro.Length = length;
            notifyConfigChange();
        });
    }

    public override void OnConfigChange(BranchingConfig config) {
        introObject.SetActive(config.hasIntroOutro);
        outroObject.SetActive(config.hasIntroOutro);

        InputField introLength = GameObject.Find("Intro/Intro Length/Length Input 1").GetComponent<InputField>();
        if (config.intro.Length > 0) {
            introLength.text = config.intro.Length.ToString();
        }
    }

    private void setupIntroOutroButton(Button button, bool isIntro, BranchingConfig config, Action notifyConfigChange) {
        button.onClick.AddListener(() => {
            FileBrowser.ShowLoadDialog((paths) => {
                if (paths.Length == 0) {
                    return;
                }
                string path = paths[0];
                Debug.Log($"Selected file {path}");

                // StartCoroutine(AudioCache.Instance().LoadClip(
                //     path, 
                //     () => {
                //         if (isIntro) {
                //             config.intro.Path = path;
                //         } else {
                //             config.outroFile = path;
                //         }
                //         notifyConfigChange();
                //     }, 
                //     () => { 
                //         Debug.Log("Error");
                //     }
                // ));
            }, () => {
                Debug.Log("Canceled");
            }, FileBrowser.PickMode.Files, true, null, null, "Select", "Select");
        });
    }
}