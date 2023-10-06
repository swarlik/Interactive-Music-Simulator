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
    private static float OFFSET = 0.04f;
    public AudioMixer mixer;

    public enum Segment {
        Intro,
        Section,
        Transition,
        Outro,
        None
    }

    private AudioSource audio1;
    private AudioSource audio2;
    private bool audioSourceFlip = true;
    private bool isPlaying = false;

    private Coroutine fadeInCoroutine;
    private Coroutine fadeOutCoroutine;

    private Segment currentSegment;
    private Segment nextSegment;

    private Fadeable current;
    private Fadeable next;
    private int startSection;

    private int currentSectionIndex;
    private int nextSectionIndex;
    private Transition nextTransition;
    private double nextEventTime;

    private XFadeConfig config;

    void Awake() {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 2) {
            Debug.Log("Need 2 audio sources");
            return;
        }

        audio1 = audioSources[0];
        audio2 = audioSources[1];
        audio1.loop = false;
        audio2.loop = false;
    }

    void Update() {
        if (!isPlaying || AudioSettings.dspTime + OFFSET < nextEventTime) {
            return;
        }

        if (next == null) {
            isPlaying = false;
            currentSegment = Segment.None;
            return;
        }

    
        AudioClip nextClip = AudioCache.Instance().GetClip(FilePathUtils.LocalPathToFullPath(next.file));
        if (nextClip == null) {
            Debug.Log($"No audio loaded for file {next.file}");
            return;
        }

        Debug.Log($"About to play current: {currentSegment.ToString()} next: {nextSegment.ToString()}");

        // If changing to a different segment, perform a crossfade. Otherwise, just switch audio sources
        if (current != null && current != next) {
            Debug.Log($"Performing xfade");
            XFade(nextClip, current.fadeOutTime, next.fadeInTime);
        } else {
            NextAudio().volume = 1.0f;
            NextAudio().clip = nextClip;
            NextAudio().Play();
            FlipAudio();
        }


        // Calculate the following segment.
        current = next;
        currentSegment = nextSegment;

        float playLength = config.hasReverb && current.loopLength > 0 ? current.loopLength : nextClip.length;

        switch (currentSegment) {
            case Segment.Intro:
                next = config.sections[startSection];
                nextSegment = Segment.Section;
                break;
            case Segment.Transition:
                next = config.sections[((Transition) current).to];
                playLength = Math.Max(current.fadeInTime, nextClip.length - current.fadeOutTime);
                nextSegment = Segment.Section;
                break;
            case Segment.Outro:
                next = null;
                nextSegment = Segment.None;
                break;
        }

        nextEventTime += playLength;
        Debug.Log($"Next has reverb: {config.hasReverb} loop length: {current.loopLength}. current audio time: {AudioSettings.dspTime} next event time: {nextEventTime}");
    }

    public void StartPlayback(XFadeConfig config, int startSection) {
        this.config = config;
        Debug.Log($"Starting playback with {JsonUtility.ToJson(config)}");
        
        if (config.sections.Length <= startSection) {
            Debug.Log("Start section out of range!");
            return;
        }

        currentSegment = Segment.None;
        if (config.hasIntroOutro && config.intro != null && config.intro.file != "") {
            SetNext(config.intro, Segment.Intro);
        } else {
            SetNext(config.sections[startSection], Segment.Section);
        }

        this.startSection = startSection;
        isPlaying = true;

        SetVolume(config.musicVolume);
    }

    public void StopPlayback() {
        StopFades();
        if (CurrentAudio().isPlaying) {
            CurrentAudio().Stop();
        }
        if (NextAudio().isPlaying) {
            NextAudio().Stop();
        }
        current = null;
        currentSegment = Segment.None;
        next = null;
        nextSegment = Segment.None;
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

        nextSectionIndex = section;

        Transition transition = null;
        if (config.transitions != null) {
            transition = config.transitions.FirstOrDefault(t => t.from == currentSectionIndex && t.to == section);
        }

        if (transition != null) {
            SetNext(transition, Segment.Transition);
        } else {
            Debug.Log($"No transition for {currentSectionIndex} to {nextSectionIndex}");
            SetNext(config.sections[section], Segment.Section);
        }
    }

    public void GoToOutro() {
        if (!isPlaying) {
            return;
        }

        if (config.hasIntroOutro && config.outro == null) {
            Debug.Log("No outro");
            return;
        }

        SetNext(config.outro, Segment.Outro);
    }

    public bool IsFading() {
        return fadeInCoroutine != null || fadeOutCoroutine != null;
    }

    public Fadeable GetCurrent() {
        return current;
    }

    public Segment GetCurrentSegment() {
        return currentSegment;
    }

    public Fadeable GetNext() {
        return next;
    }

    public Segment GetNextSegment() {
        return nextSegment;
    }

    public bool IsPlaying() {
        return isPlaying;
    }

    public int GetCurrentSection() {
        return currentSectionIndex;
    }

    public int GetNextSection() {
        return nextSectionIndex;
    }

    public bool InTransition() {
        return currentSegment == Segment.Transition;
    }

    public void SetVolume(float volume) {
        Debug.Log($"Volume slider value: {volume}; setting volume to {(Mathf.Log10(volume) * 20)}");
        mixer.SetFloat("MusicVolume", volume == 0.0f ? -100 : Mathf.Log10(volume) * 20);
    }

    private void SetNext(Fadeable next, Segment nextSegment) {
        this.next = next;
        this.nextSegment = nextSegment;
        nextEventTime = AudioSettings.dspTime + OFFSET;
    }

    private void XFade(AudioClip nextClip, float fadeOutTime, float fadeInTime) {
        if (CurrentAudio().isPlaying) {
            fadeOutCoroutine = StartCoroutine(FadeOut(CurrentAudio(), fadeOutTime));
        }

        AudioSource nextAudio = NextAudio();
        nextAudio.clip = nextClip;
        fadeInCoroutine = StartCoroutine(FadeIn(nextAudio, fadeInTime));

        FlipAudio();
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
            fadeInCoroutine = null;
        }
        if (fadeOutCoroutine != null) {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }
    }
}
