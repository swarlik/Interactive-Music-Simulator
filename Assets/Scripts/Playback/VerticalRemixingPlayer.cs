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
    private Coroutine fadeInCoroutine;
    private Coroutine fadeOutCoroutine;

    private LayersPlayer layersPlayer;

    void Awake() {
        introOutroAudio = gameObject.AddComponent<AudioSource>();
        layersPlayer = gameObject.GetComponent<LayersPlayer>();
    }
    
    void Start() {
    }

    void Update() {
        if (!isPlaying || AudioSettings.dspTime + OFFSET < nextEventTime || 
            (currentSegment == Segment.Layers && nextSegment == Segment.Layers)) {
            return;
        }

        Debug.Log($"current {currentSegment} next {nextSegment}");

        // Handle current segment
        if (currentSegment == Segment.Intro && nextSegment != Segment.Intro) {
            StartCoroutine(FadeOut(introOutroAudio, config.intro.fadeOutTime));
        }

        if (currentSegment == Segment.Layers && nextSegment != Segment.Layers) {
            layersPlayer.FadeOutAllLayers();
        }

        // Handle next segment
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
            introOutroAudio.volume = 1.0f;
            nextEventTime += config.hasReverb && config.intro.loopLength > 0.0f ? config.intro.loopLength : intro.length;
            nextSegment = Segment.Layers;
            Debug.Log($"Playing Intro, next check at {nextEventTime}");
        }

        if (currentSegment == Segment.Layers) {
            layersPlayer.Start(nextEventTime);
            nextSegment = Segment.Layers;
        }

        if (currentSegment == Segment.Outro) {
            AudioClip outro = AudioCache.Instance().GetClip(FilePathUtils.LocalPathToFullPath(config.outro.file));
            if (outro == null) {
                Debug.Log($"No audio loaded for file {config.outro.file}");
                return;
            }
            introOutroAudio.clip = outro;
            StartCoroutine(FadeIn(introOutroAudio, config.outro.fadeInTime));
            nextSegment = Segment.None;
            nextEventTime += outro.length;
        }
    }

    public bool IsFading() {
        return fadeInCoroutine != null || fadeOutCoroutine != null;
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
        if (fadeInCoroutine != null) {
            StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = null;
        }
        if (fadeOutCoroutine != null) {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        layersPlayer.Stop();

        introOutroAudio.Stop();

        currentSegment = Segment.None;
        isPlaying = false;
    }

    private IEnumerator FadeIn(AudioSource audio, float fadeTime) {
        return Fade(audio, fadeTime, true);
    }

    private IEnumerator FadeOut(AudioSource audio, float fadeTime) {
        return Fade(audio, fadeTime, false);
    }

    private IEnumerator Fade(AudioSource audio, float fadeTime, bool fadeIn) {
        Debug.Log($"Starting fade in: {fadeIn} for {fadeTime} seconds");
        if (fadeIn) {
            audio.volume = 0;
            audio.Play();
        }

        while (fadeIn ? (audio.volume < 1.0f) : (audio.volume > 0.0f)) {
            float diff = fadeTime == 0.0f ? 1.0f : Time.deltaTime / fadeTime;
            audio.volume += fadeIn ? diff : (-1.0f * diff);
            yield return null;
        }

        if (!fadeIn) {
            audio.Stop();
        }
        
        if (fadeIn) {
            fadeInCoroutine = null;
        } else {
            fadeOutCoroutine = null;
        }
        Debug.Log($"Finished fade in: {fadeIn}");
    }
}