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
    private static float OFFSET = 0.04f;

    public AudioMixer mixer;

    public enum Segment {
        Intro,
        Layers,
        Outro,
        None
    }

    // Pre-created audio channels (8 max)
    private AudioMixerGroup[] groups;
    private AudioSource introOutroAudio;
    private VerticalRemixingConfig config;
    private List<AudioLayer> layers;
    private int currentLayer;
    private LayeringMode currentMode;
    private bool isPlaying;
    private Segment currentSegment;
    private Segment nextSegment;
    private double nextEventTime;

    private int fadeCoroutineId = 0;
    private Dictionary<int, Coroutine> activeFades;

    void Awake() {
        groups = mixer.FindMatchingGroups("Music/");
        foreach (AudioMixerGroup group in groups) {
            Debug.Log(group);
        }
        introOutroAudio = gameObject.AddComponent<AudioSource>();
    }
    
    void Start() {
        activeFades = new Dictionary<int, Coroutine>();
    }

    void Update() {
        if (!isPlaying || AudioSettings.dspTime + OFFSET < nextEventTime || 
            (currentSegment == Segment.Layers && nextSegment == Segment.Layers)) {
            return;
        }

        currentSegment = nextSegment;

        if (currentSegment == Segment.None) {
            isPlaying = false;
            return;
        }

        Debug.Log($"current time is {nextEventTime}");

        if (currentSegment == Segment.Intro) {
            AudioClip intro = AudioCache.Instance().GetClip(FilePathUtils.LocalPathToFullPath(config.intro.file));
            if (intro == null) {
                Debug.Log($"No audio loaded for file {config.intro.file}");
                return;
            }
            introOutroAudio.clip = intro;
            introOutroAudio.PlayScheduled(nextEventTime);
            nextEventTime += config.hasReverb && config.intro.loopLength > 0.0f ? config.intro.loopLength : intro.length;
            nextSegment = Segment.Layers;
            Debug.Log($"Playing Intro, next check at {nextEventTime}");
        }

        if (currentSegment == Segment.Layers) {
            foreach (AudioLayer layer in layers) {
                layer.Play(nextEventTime);
            }
            nextSegment = Segment.Layers;
        }

        if (currentSegment == Segment.Outro) {
            // Stop playing layers
            foreach (AudioLayer layer in layers) {
                layer.Stop();
            }
            AudioClip outro = AudioCache.Instance().GetClip(FilePathUtils.LocalPathToFullPath(config.outro.file));
            if (outro == null) {
                Debug.Log($"No audio loaded for file {config.outro.file}");
                return;
            }
            introOutroAudio.clip = outro;
            introOutroAudio.PlayScheduled(nextEventTime);
            nextSegment = Segment.None;
            nextEventTime += config.hasReverb && config.outro.loopLength > 0.0f ? config.outro.loopLength : outro.length;
        }
    }

    public bool IsFading() {
        return activeFades.Count() > 0;
    }

    public bool IsPlaying() {
        return isPlaying;
    }

    public int GetCurrentLayer() {
        return currentLayer;
    }

    public Segment GetCurrentSegment() {
        return currentSegment;
    }

    public void StartPlayback(VerticalRemixingConfig config, int startLayer, LayeringMode startMode) {
        layers = new List<AudioLayer>();
        for (int i = 0; i < config.layers.Length; i++) {
            AudioLayer layer = gameObject.AddComponent<AudioLayer>();
            layer.Init(config.layers[i], config.hasReverb, groups[i]);
            layers.Add(layer);
            SetLayerVolume(i, startMode == LayeringMode.Additive 
                ? i <= startLayer ? 1.0f : 0.0f 
                : i == startLayer ? 1.0f : 0.0f);
        }

        // Start all layers at the exact same time
        nextEventTime = AudioSettings.dspTime + OFFSET;

        nextSegment = config.hasIntroOutro ? Segment.Intro : Segment.Layers;

        currentLayer = startLayer;
        currentMode = startMode;
        this.config = config;
        isPlaying = true;
    }

    public void GoToLayer(int targetLayer) {
        Debug.Log($"Going to layer {targetLayer}");
        if (!isPlaying || currentLayer == targetLayer || targetLayer >= layers.Count()) {
            return;
        }

        if (currentMode == LayeringMode.Additive) {
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

    public void SetVolume(float volume) {
        mixer.SetFloat("MusicVolume", volume == 0.0f ? -100 : Mathf.Log10(volume) * 20);
    }

    public void GoToOutro() {
        nextSegment = Segment.Outro;
        nextEventTime = AudioSettings.dspTime + OFFSET;
    }

    public void StopPlayback() {
        foreach (Coroutine coroutine in activeFades.Values) {
            StopCoroutine(coroutine);
        }
        activeFades.Clear();

        foreach (AudioLayer layer in layers) {
            layer.Stop();
        }

        introOutroAudio.Stop();

        currentSegment = Segment.None;
        isPlaying = false;
    }

    private void FadeOutAllLayers() {
        for (int i = 0; i < layers.Count(); i++) {
            if (IsLayerOn(i)) {
                StartFade(i, config.layers[i].fadeOutTime, false);
            }
        }
    }

    private void SetLayerVolume(int layer, float volume) {
        mixer.SetFloat($"LayerVolume{layer + 1}", volume == 0.0f ? -100 : Mathf.Log10(volume) * 20);
    }

    private bool IsLayerOn(int layer) {
        float volume;
        mixer.GetFloat($"LayerVolume{layer + 1}", out volume);
        return volume != -100;
    }

    private void StartFade(int layer, float fadeTime, bool fadeIn) {
        int currentId = fadeCoroutineId++;
        Coroutine fade = StartCoroutine(Fade(layer, fadeTime, fadeIn, () => {
            activeFades.Remove(currentId);
        }));

        activeFades.Add(currentId, fade);
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