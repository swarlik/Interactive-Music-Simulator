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

    private AudioSource introOutroAudio;
    private VerticalRemixingConfig config;
    private bool isPlaying;
    private Segment currentSegment;
    private Segment nextSegment;
    private double nextEventTime;

    private int fadeCoroutineId = 0;
    private Dictionary<int, Coroutine> activeFades;

    private LayersPlayer layersPlayer;

    void Awake() {
        introOutroAudio = gameObject.AddComponent<AudioSource>();
        layersPlayer = gameObject.GetComponent<LayersPlayer>();
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
            layersPlayer.Start(nextEventTime);
            nextSegment = Segment.Layers;
        }

        if (currentSegment == Segment.Outro) {
            // Stop playing layers
            layersPlayer.FadeOutAllLayers();
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

    public Segment GetCurrentSegment() {
        return currentSegment;
    }

    public void Init(VerticalRemixingConfig config) {
        layersPlayer.Setup(config);
    }

    public void StartPlayback(VerticalRemixingConfig config, bool[] activeLayerList) {        
        for (int i = 0; i < activeLayerList.Length; i++) {
            layersPlayer.ToggleLayer(i, activeLayerList[i]);
        }

        nextEventTime = AudioSettings.dspTime + OFFSET;

        nextSegment = config.hasIntroOutro ? Segment.Intro : Segment.Layers;

        this.config = config;
        isPlaying = true;
    }

    public void ToggleLayer(int layerIndex, bool on) {
        layersPlayer.ToggleLayer(layerIndex, on);
    }

    public void GoToLayer(int targetLayer) {
        Debug.Log($"Going to layer {targetLayer}");
        if (!isPlaying) {
            return;
        }
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

        layersPlayer.Stop();

        introOutroAudio.Stop();

        currentSegment = Segment.None;
        isPlaying = false;
    }
}