using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;
using System;
using static VerticalRemixingConfig;

public class VerticalRemixingPlayer : MonoBehaviour 
{
    public AudioMixer mixer;
    // Pre-created audio channels (8 max)
    private AudioMixerGroup[] groups;
    private VerticalRemixingConfig config;
    private List<AudioLayer> layers;
    private int currentLayer;
    private LayeringMode currentMode;
    private int activeFades = 0;

    void Awake() {
        groups = mixer.FindMatchingGroups("Music/");
        foreach (AudioMixerGroup group in groups) {
            Debug.Log(group);
        }
    }
    
    void Start() {
        RunTest();
    }

    void Update() {
        if (!IsFading() && Application.isEditor) {
            HandleLayerKeyPress();
        }
    }

    public bool IsFading() {
        return activeFades > 0;
    }

    public void StartPlayback(VerticalRemixingConfig config, int startLayer, LayeringMode startMode) {
        layers = new List<AudioLayer>();
        for (int i = 0; i < config.layers.Length; i++) {
            AudioLayer layer = gameObject.AddComponent<AudioLayer>();
            layer.Init(config.layers[i], config.hasReverb, groups[i]);
            layers.Add(layer);
            SetLayerVolume(i, startMode == LayeringMode.ADDITIVE 
                ? i <= startLayer ? 1.0f : 0.0f 
                : i == startLayer ? 1.0f : 0.0f);
        }

        // Start all layers at the exact same time
        double startTime = AudioSettings.dspTime + 1.0f;
        foreach (AudioLayer layer in layers) {
            layer.Play(startTime);
        }

        currentLayer = startLayer;
        currentMode = startMode;
        this.config = config;
    }

    public void GoToLayer(int targetLayer) {
        Debug.Log($"Going to layer {targetLayer}");
        if (currentLayer == targetLayer || targetLayer >= layers.Count()) {
            return;
        }

        if (currentMode == LayeringMode.ADDITIVE) {
            bool fadeIn = currentLayer < targetLayer;
            int start = fadeIn ? currentLayer + 1 : currentLayer;
            int end = fadeIn ? targetLayer : targetLayer + 1;
            Debug.Log($"iterating from {start} to {end}");
            for (int i = start; fadeIn ? i <= end : i >= end; i = fadeIn ? i + 1 : i - 1) {
                StartFade(i, fadeIn ? config.layers[i].fadeInTime : config.layers[i].fadeOutTime, fadeIn);
            }
        } else {
            for (int i = 0; i < layers.Count(); i++) {
                if (i == targetLayer) {
                    StartFade(i, config.layers[i].fadeInTime, true);
                } else if (IsLayerOn(i)) {
                    StartFade(i, config.layers[i].fadeOutTime, false);
                }
            }
        }

        currentLayer = targetLayer;
    }

    private void SetLayerVolume(int layer, float volume) {
        mixer.SetFloat($"LayerVolume{layer + 1}", volume == 0.0f ? -100 : Mathf.Log10(volume) * 20);
    }

    private bool IsLayerOn(int layer) {
        float volume;
        mixer.GetFloat($"LayerVolume{layer + 1}", out volume);
        return volume != -100;
    }

    private void StartFadeIn(int layer, float fadeTime) {
        StartFade(layer, fadeTime, true);
    }

    private void StartFadeOut(int layer, float fadeTime, Action onComplete) {
        StartFade(layer, fadeTime, false);
    }

    private void StartFade(int layer, float fadeTime, bool fadeIn) {
        StartCoroutine(Fade(layer, fadeTime, fadeIn, () => {
            activeFades--;
        }));

        activeFades++;
    }

    private IEnumerator Fade(int layer, float fadeTime, bool fadeIn, Action onComplete) {
        Debug.Log($"Starting fade {(fadeIn ? "in" : "out")} for {fadeTime} seconds on layer {layer}");

        float volume = fadeIn ? 0.0f : 1.0f;
        while (fadeIn ? (volume < 1.0f) : (volume > 0.0f)) {
            float diff = fadeTime == 0.0f ? 1.0f : Time.deltaTime / fadeTime;
            volume += fadeIn ? diff : (-1.0f * diff);
            SetLayerVolume(layer, volume);
            yield return null;
        }
        SetLayerVolume(layer, fadeIn ? 1.0f : 0.0f);
        

        onComplete();
    }

    /// TEST CODE

    private static string[] TEST_FILES = {
        "Builds/Latest/_TestAudio/Layering/L4 Bass Omni A.wav",
        "Builds/Latest/_TestAudio/Layering/L2 Choir Pad.wav",
        "Builds/Latest/_TestAudio/Layering/L3 Kora Outside.wav", 
        "Builds/Latest/_TestAudio/Layering/L6 Shaker.wav",
        "Builds/Latest/_TestAudio/Layering/L1 Sanchit Vox.wav", 
    };

    private static float[] LENGTHS = {
        25.263f,
        114.0f, 
        12.632f,
        12.632f,
        114.0f
    };

    private VerticalRemixingConfig createSampleConfig() {
        VerticalRemixingConfig config = new VerticalRemixingConfig();
        config.hasReverb = true;
        config.layeringMode = LayeringMode.ADDITIVE;
        List<Fadeable> layers = new List<Fadeable>();
        for (int i = 0; i < TEST_FILES.Length; i++) {
            Fadeable layer = new Fadeable();
            layer.file = TEST_FILES[i];
            layer.loopLength = LENGTHS[i];
            layer.fadeInTime = 4.0f;
            layer.fadeOutTime = 4.0f;
            layers.Add(layer);
        }
        config.layers = layers.ToArray();
        return config;
    }

    private void RunTest() {
        VerticalRemixingConfig config = createSampleConfig();
        int loadCounter = 0;
        Action callback = () => {
            loadCounter++;
            if (loadCounter == TEST_FILES.Length) {
                StartPlayback(config, 1, config.layeringMode);
            }
        };

        foreach (String file in TEST_FILES) {
            StartCoroutine(
                AudioCache.Instance().LoadClip(FilePathUtils.LocalPathToFullPath(file), callback, (error) => {
                    Debug.Log(error);
                }));
        }
    }

    private void HandleLayerKeyPress() {
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            GoToLayer(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            GoToLayer(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            GoToLayer(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            GoToLayer(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            Debug.Log("four");
            GoToLayer(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            GoToLayer(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            GoToLayer(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7)) {
            GoToLayer(7);
        }
    }
}