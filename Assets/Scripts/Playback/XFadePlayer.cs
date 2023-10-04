using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;
using System;
using static XFadeConfig;

public class XFadePlayer : MonoBehaviour
{
    public AudioMixer mixer;

    private AudioSource audio1;
    private AudioSource audio2;
    private bool audioSourceFlip = true;
    private bool isFading = false;
    private bool isPlaying = false;

    private Coroutine fadeInCoroutine;
    private Coroutine fadeOutCoroutine;

    private int currentSection;
    private int nextSection;

    private double transitionEndTime = -1.0;
    private float currentTransitionFadeOutTime;

    private float currentVolume = 1.0f;

    private XFadeConfig config;

    void Awake() {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 2) {
            Debug.Log("Need 2 audio sources");
            return;
        }

        Debug.Log("Found 2 audio sources");

        audio1 = audioSources[0];
        audio2 = audioSources[1];
        audio1.loop = true;
        audio2.loop = true;
    }

    void Update() {
        if (transitionEndTime == -1.0 || AudioSettings.dspTime < transitionEndTime) {
            return;
        }

        XFade(config.sections[nextSection], currentTransitionFadeOutTime, true);
        currentSection = nextSection;
        transitionEndTime = -1.0;
    }

    public void StartPlayback(XFadeConfig config, int startSection) {
        this.config = config;
        
        if (config.sections.Length <= startSection) {
            Debug.Log("No sections to play!");
            return;
        }

        // Play the first section
        AudioClip clip = AudioCache.Instance()
            .GetClip(FilePathUtils.LocalPathToFullPath(config.sections[startSection]));
        if (clip == null) {
            Debug.Log("Audio not loaded for first section");
            return;
        }

        SetVolume(config.musicVolume);
        CurrentAudio().clip = clip;
        CurrentAudio().volume = currentVolume;
        CurrentAudio().Play();
        currentSection = startSection;
        isPlaying = true;
    }

    public void StopPlayback() {
        StopFades();
        if (CurrentAudio().isPlaying) {
            CurrentAudio().Stop();
        }
        if (NextAudio().isPlaying) {
            NextAudio().Stop();
        }
        transitionEndTime = -1.0;
        isPlaying = false;
    }

    public void GoToSection(int section) {
        if (!isPlaying) {
            return;
        }
        if (config.sections.Length < section) {
            Debug.Log($"Invalid section {section}");
            return;
        }

        nextSection = section;

        Transition transition = null;
        if (config.transitions != null) {
            transition = config.transitions.FirstOrDefault(t => t.from == currentSection && t.to == nextSection);
        }

        // If there's no transition, fade directly into the next section
        if (transition == null) {
            Debug.Log($"No transition for {currentSection} to {nextSection}");
            XFade(config.sections[section], config.xfadeTime, true);
            currentSection = nextSection;
            return;
        }

        AudioClip transitionClip = AudioCache.Instance().GetClip(FilePathUtils.LocalPathToFullPath(transition.file));
        if (transitionClip == null) {
            Debug.Log($"No transition audio loaded: {transition.file}");
            return;
        }

        // If there is a transition, fade into the transition clip, and queue a fade into the next section
        XFade(transition.file, transition.fadeInTime, false);
        transitionEndTime = Math.Max(
            AudioSettings.dspTime + transition.fadeInTime, 
            AudioSettings.dspTime + transitionClip.length - transition.fadeOutTime);
        currentTransitionFadeOutTime = transition.fadeOutTime;
    }

    public bool IsFading() {
        return isFading;
    }

    public bool IsPlaying() {
        return isPlaying;
    }

    public int GetCurrentSection() {
        return currentSection;
    }

    public int GetNextSection() {
        return nextSection;
    }

    public bool InTransition() {
        return transitionEndTime != -1.0;
    }

    public void SetVolume(float volume) {
        Debug.Log($"Volume slider value: {volume}; setting volume to {(Mathf.Log10(volume) * 20)}");
        mixer.SetFloat("MusicVolume", volume == 0.0f ? -100 : Mathf.Log10(volume) * 20);
    }

    private void XFade(string nextFile, float xfadeTime, bool loopNext) {
        if (CurrentAudio().isPlaying) {
            fadeInCoroutine = StartCoroutine(FadeOut(CurrentAudio(), xfadeTime));
        }

        AudioClip clip = AudioCache.Instance().GetClip(FilePathUtils.LocalPathToFullPath(nextFile));
        if (clip == null) {
            Debug.Log($"No audio loaded for file {nextFile}");
            return;
        }

        AudioSource nextAudio = NextAudio();
        nextAudio.clip = clip;
        nextAudio.loop = loopNext;
        fadeOutCoroutine = StartCoroutine(FadeIn(nextAudio, xfadeTime));

        FlipAudio();
    }

    private IEnumerator FadeIn(AudioSource audio, float fadeTime) {
        return Fade(audio, fadeTime, true);
    }

    private IEnumerator FadeOut(AudioSource audio, float fadeTime) {
        return Fade(audio, fadeTime, false);
    }

    private IEnumerator Fade(AudioSource audio, float fadeTime, bool fadeIn) {
        isFading = true;

        if (fadeTime <= 0.0f) {
            Debug.Log("Invalid fade time");
            yield break;
        }

        if (fadeIn) {
            audio.volume = 0;
            audio.Play();
        }

        while (fadeIn ? (audio.volume < 1.0f) : (audio.volume > 0.0f)) {
            float diff = Time.deltaTime / fadeTime;
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
        isFading = false;
    }

    private AudioSource CurrentAudio() {
        return audioSourceFlip ? audio1 : audio2;
    }

    private AudioSource NextAudio() {
        return audioSourceFlip ? audio2 : audio1;
    }

    private void FlipAudio() {
        audioSourceFlip = !audioSourceFlip;
    }

    private void StopFades() {
        if (fadeInCoroutine != null) {
            StopCoroutine(fadeInCoroutine);
        }
        if (fadeOutCoroutine != null) {
            StopCoroutine(fadeOutCoroutine);
        }

        isFading = false;
    }
}
