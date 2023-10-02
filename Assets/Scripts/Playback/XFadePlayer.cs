using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class MusicPlayer : MonoBehaviour
{
    private XFadeConfig config;
    
    private AudioSource audio1;
    private AudioSource audio2;
    private bool audioSourceFlip;

    private int nextSection;

    private double nextEventTime;
    private AudioItem nextAudio;
    private bool nextHasReverb;
    private bool queueImmediate;
    private bool endAfterNext;

    // Start is called before the first frame update
    void Start()
    {
        audioSourceFlip = true;
        
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length != 2) {
            Debug.Log("Didn't find 2 audio sources");
        } else {
            audio1 = sources[0];
            audio2 = sources[1];
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Don't switch clips until audio is within 1s of ending.
        if (AudioSettings.dspTime + 1.0f < nextEventTime) {
            return;
        }

        if (queueImmediate) {
            AudioSource prev = audioSourceFlip ? audio2 : audio1;
            if (prev.isPlaying) {
                prev.SetScheduledEndTime(nextEventTime);
            }
            queueImmediate = false;
        }

        if (nextAudio == null) {
            return;
        }

        AudioClip nextClip = AudioCache.Instance().GetClip(nextAudio.Path);
        if (nextClip == null) {
            Debug.Log($"Missing audio file in cache: {nextAudio.Path}");
            return;
        }

        float playLength = nextHasReverb ? nextAudio.Length : nextClip.length;

        // Schedule the next clip
        AudioSource audio = audioSourceFlip ? audio1 : audio2;
        audio.clip = nextClip;
        audio.PlayScheduled(nextEventTime);

        // Set the next event time and next audio source
        nextEventTime += playLength;
        audioSourceFlip = !audioSourceFlip;

        if (endAfterNext) {
            nextAudio = null;
        }
    }

    public void Init(XFadeConfig config) {
        this.config = config;
    }

    public void StartPlayback(int section) {
        if (config == null) {
            Debug.Log("not initialized");
            return;
        }

        string path = config.sections[section];
        AudioClip clip = AudioCache.Instance().GetClip(path);
        audio1.clip = clip;
        audio1.loop = true;
        audio1.Play();
    }

    public void GoToSection(int section) {
        if (config == null) {
            Debug.Log("Not initialized");
            return;
        }


    }

    // private AudioClip GetClip(int section) {

    // }

    public void StartPlayback(AudioItem item, bool hasReverb) {
        nextAudio = item;
        nextHasReverb = hasReverb;
        nextEventTime = AudioSettings.dspTime + 0.2f;
    }

    public void SetNext(AudioItem item, bool hasReverb, bool immediate, bool endAfter) {
        nextAudio = item;
        nextHasReverb = hasReverb;
        endAfterNext = endAfter;

        if (immediate) {
            changeImmediately();
        }
    }

    public void StopPlayback() {
        nextAudio = null;
        changeImmediately();
    }

    // Trigger the next section near immediately (0.1s delay)
    private void changeImmediately() {
        queueImmediate = true;
        nextEventTime = AudioSettings.dspTime + 0.1f;
    }
}
