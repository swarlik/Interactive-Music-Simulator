using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;
using System;
using static VerticalRemixingConfig;

public class LayersPlayer : MonoBehaviour {

    public AudioMixer mixer;
    // Pre-created audio channels (8 max)
    private AudioMixerGroup[] groups;
    private VerticalRemixingConfig config;
    private List<AudioLayer> layers;
    private int currentLayer;
    private bool isPlaying;

    private bool[] activeLayerList;
    private Coroutine[] activeFades;

    void Awake() {
        groups = mixer.FindMatchingGroups("Music/Layer");
        foreach (AudioMixerGroup group in groups) {
            Debug.Log(group);
        }
    }

    void Start() {
    }

    public void Setup(VerticalRemixingConfig config) {
        this.config = config;

        layers = new List<AudioLayer>();
        for (int i = 0; i < config.layers.Length; i++) {
            AudioLayer layer = gameObject.AddComponent<AudioLayer>();
            layer.Init(config.layers[i], config.hasReverb, groups[i]);
            layers.Add(layer);
            SetLayerVolume(i, 0.0f);
        }

        activeLayerList = new bool[config.layers.Length];
        Array.Fill(activeLayerList, false, 0, config.layers.Length);
        activeFades = new Coroutine[config.layers.Length];
    }

    public void Start(double startTime) {
        // Start all layers at once
        foreach (AudioLayer layer in layers) {
            layer.Play(startTime);
        }

        for (int i = 0; i < activeLayerList.Length; i++) {
            if (activeLayerList[i]) {
                StartFade(i, config.layers[i].fadeInTime, true);
            }
        }

        isPlaying = true;
    }

    public void ToggleLayer(int layerIndex, bool on) {
        activeLayerList[layerIndex] = on;

        if (isPlaying) {
            Fadeable layerInfo = config.layers[layerIndex];
            StartFade(layerIndex, on ? layerInfo.fadeInTime : layerInfo.fadeOutTime, on);
        }
    }

    public void FadeOutAllLayers() {
        for (int i = 0; i < config.layers.Length; i++) {
            if (activeLayerList[i]) {
                Fadeable layerInfo = config.layers[i];
                StartFade(i, layerInfo.fadeOutTime, false, true);
            }
        }
        isPlaying = false;
    }
    

    public void Stop() {
        for (int i = 0; i < activeFades.Length; i++) {
            if (activeFades[i] != null) {
                StopCoroutine(activeFades[i]);
            }
            activeFades[i] = null;
        }

        for (int i = 0; i < config.layers.Length; i++) {
            layers[i].Stop();
            SetLayerVolume(i, 0.0f);
        }

        isPlaying = false;
    }

    public bool IsPlaying() {
        return isPlaying;
    }

    private void SetLayerVolume(int layer, float volume) {
        mixer.SetFloat($"LayerVolume{layer + 1}", volume == 0.0f ? -100 : Mathf.Log10(volume) * 20);
    }

    private float GetLayerVolume(int layer) {
        float mixerVolume;
        mixer.GetFloat($"LayerVolume{layer + 1}", out mixerVolume);
        if (mixerVolume == -100) {
            return 0.0f;
        }
        return (float) Math.Pow(10, (mixerVolume / 20.0f));
    }

    private void StartFade(int layer, float fadeTime, bool fadeIn, bool stopAfterFadeOut = false) {
        // Stop fade coroutine if there's one in progress
        if (activeFades[layer] != null) {
            StopCoroutine(activeFades[layer]);
        }

        Coroutine fade = StartCoroutine(Fade(layer, fadeTime, fadeIn, () => {
            activeFades[layer] = null;
            if (!fadeIn && stopAfterFadeOut) {
                layers[layer].Stop();
            }
        }));


        activeFades[layer] = fade;
    }

    private IEnumerator Fade(int layer, float fadeTime, bool fadeIn, Action onComplete) {
        Debug.Log($"Starting fade {(fadeIn ? "in" : "out")} for {fadeTime} seconds on layer {layer}");

        float volume = GetLayerVolume(layer);
        while (fadeIn ? (volume < 1.0f) : (volume > 0.0f)) {
            float diff = fadeTime == 0.0f ? 1.0f : Time.deltaTime / fadeTime;
            volume += fadeIn ? diff : (-1.0f * diff);
            SetLayerVolume(layer, volume);
            yield return null;
        }
        SetLayerVolume(layer, fadeIn ? 1.0f : 0.0f);
        
        onComplete();
    }
}