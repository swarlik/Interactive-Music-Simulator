using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;
using System;
using static XFadeConfig;
using static ModeDropdown;

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
    private bool fadeNext;
    private PlaybackMode currentMode;

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

        // If changing to a different segment and fadeNext is specifcied, perform a crossfade. 
        // Otherwise, just switch audio sources without a fade.
        if (current != null && current != next && fadeNext) {
            Debug.Log($"Performing xfade");
            XFade(nextClip, current.fadeOutTime, next.fadeInTime);
        } else {
            NextAudio().volume = 1.0f;
            NextAudio().clip = nextClip;
            NextAudio().Play();
            FlipAudio();
        }

        float lastFadeOutTime = current == null ? 0.0f : current.fadeOutTime;

        // Calculate the following segment.
        current = next;
        currentSegment = nextSegment;
        fadeNext = false;

        float playLength = config.hasReverb && current.loopLength > 0 ? current.loopLength : nextClip.length;
        int random = UnityEngine.Random.Range(0, config.sections.Length);
        int nextSection;

        switch (currentSegment) {
            case Segment.Intro:
                if (currentMode == PlaybackMode.Sequential) {
                    nextSection = 0;
                } else if (currentMode == PlaybackMode.Random) {
                    nextSection = random;
                } else {
                    nextSection = startSection;
                }
                next = config.sections[nextSection];
                nextSegment = Segment.Section;
                break;
            case Segment.Section:
                if (currentMode == PlaybackMode.Sequential) {
                    nextSection = (CurrentSectionIndex() + 1) % config.sections.Length;
                } else if (currentMode == PlaybackMode.Random) {
                    nextSection = random;
                } else {
                    nextSection = CurrentSectionIndex();
                }
                next = config.sections[nextSection];
                nextSegment = Segment.Section;
                break;
            case Segment.Transition:
                next = config.sections[((Transition) current).to];
                playLength = Math.Max(Math.Max(lastFadeOutTime, current.fadeInTime) + OFFSET, nextClip.length - current.fadeOutTime);
                nextSegment = Segment.Section;
                fadeNext = true;
                break;
            case Segment.Outro:
                next = null;
                nextSegment = Segment.None;
                break;
        }

        nextEventTime += playLength;
        Debug.Log($"Next has reverb: {config.hasReverb} loop length: {current.loopLength}. current audio time: {AudioSettings.dspTime} next event time: {nextEventTime}");
    }

    public void StartPlayback(XFadeConfig config, int startSection, PlaybackMode startMode) {
        this.config = config;
        Debug.Log($"Starting playback with {JsonUtility.ToJson(config)}");
        
        if (config.sections.Length <= startSection) {
            Debug.Log("Start section out of range!");
            return;
        }

        currentSegment = Segment.None;
        if (config.hasIntroOutro && config.intro != null && config.intro.file != "") {
            GoToSegment(config.intro, Segment.Intro);
        } else {
            GoToSegment(config.sections[startSection], Segment.Section);
        }

        currentMode = startMode;
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
        if (!isPlaying || section == CurrentSectionIndex()) {
            return;
        }
        if (config.sections.Length < section) {
            Debug.Log($"Invalid section {section}");
            return;
        }

        Transition transition = null;
        if (config.transitions != null) {
            transition = config.transitions.FirstOrDefault(t => t.from == CurrentSectionIndex() && t.to == section);
        }

        if (transition != null) {
            GoToSegment(transition, Segment.Transition);
        } else {
            Debug.Log($"No transition for {CurrentSectionIndex()} to {section}");
            GoToSegment(config.sections[section], Segment.Section);
        }
    }

    public void GoToOutro() {
        if (!isPlaying) {
            return;
        }

        if (!config.hasIntroOutro || config.outro == null) {
            Debug.Log("No outro");
            return;
        }

        GoToSegment(config.outro, Segment.Outro);
    }

    public void SetPlaybackMode(PlaybackMode mode) {
        currentMode = mode;
        int random = UnityEngine.Random.Range(0, config.sections.Length);
        int nextSection;

        if (currentSegment != Segment.Intro && currentSegment != Segment.Section) {
            return;
        }

        if (mode == PlaybackMode.Random) {
            nextSection = random;
        } else if (mode == PlaybackMode.Sequential) {
            nextSection = currentSegment == Segment.Intro ? 0 : (CurrentSectionIndex() + 1) % config.sections.Length;
        } else {
            nextSection = currentSegment == Segment.Intro ? 0 : CurrentSectionIndex();
        }

        next = config.sections[nextSection];
        
        Debug.Log($"Setting mode to {mode}. next section: {nextSection}");
    }

    public int CurrentSectionIndex() {
        if (current == null || currentSegment != Segment.Section) { 
            return -1;
        }

        return Array.IndexOf<Fadeable>(config.sections, current);
    }

    public int NextSectionIndex() {
        if (next == null || nextSegment != Segment.Section) { 
            return -1;
        }

        return Array.IndexOf<Fadeable>(config.sections, next);
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

    public bool InTransition() {
        return currentSegment == Segment.Transition;
    }

    public void SetVolume(float volume) {
        Debug.Log($"Volume slider value: {volume}; setting volume to {(Mathf.Log10(volume) * 20)}");
        mixer.SetFloat("MusicVolume", volume == 0.0f ? -100 : Mathf.Log10(volume) * 20);
    }

    private void GoToSegment(Fadeable next, Segment nextSegment) {
        this.next = next;
        this.nextSegment = nextSegment;
        fadeNext = true;
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
