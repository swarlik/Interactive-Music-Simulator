using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class MusicManager : MonoBehaviour
{
    public enum PlayMode {
        Sequential,
        Random,
        Manual
    }

    public enum Section {
        Intro,
        Branch,
        Outro,
        None
    }

    public AudioSource audio1;
    public AudioSource audio2;

    private AudioClip[] branches;
    private float[] branchLengths;
    private PlayMode playMode;
    private bool hasReverb;
    private bool hasIntroOutro;
    private bool immediate;
    private AudioClip intro;
    private AudioClip outro;
    private float introLength;
    private Action<Section, Section, int, int> onPlaylistChange;

    private bool initialized;
    private int lastClipIndex;
    private int nextClipIndex;
    private double nextEventTime;
    private bool isRunning;
    private bool audioSourceFlip;

    private Section currentSection;
    private Section nextSection;

    private bool queueImmediate;

    // Start is called before the first frame update
    void Start()
    {
        initialized = false;
        isRunning = false;
        audioSourceFlip = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning || !this.initialized) {
            return;
        }

        // Don't switch clips until audio is within 1s of ending.
        if (AudioSettings.dspTime + 1.0f < nextEventTime) {
            return;
        }  

        // About to start playback of the next section.
        currentSection = nextSection;
        AudioClip clipToPlay = null;
        float playLength = -1.0f;

        switch (currentSection) {
            case Section.Intro:
                clipToPlay = intro;
                playLength = introLength > 0 && hasReverb ? introLength : intro.length;
                nextSection = Section.Branch;
                break;
            case Section.Branch:
                clipToPlay = branches[nextClipIndex];
                lastClipIndex = nextClipIndex;
                nextClipIndex = getNextIndex();
                if (hasReverb && lastClipIndex < branchLengths.Length && branchLengths[lastClipIndex] > 0) {
                    playLength = branchLengths[lastClipIndex];
                } else {
                    playLength = clipToPlay.length;
                }
                nextSection = Section.Branch;
                break;
            case Section.Outro:
                clipToPlay = outro;
                playLength = outro.length;
                nextSection = Section.None;
                break;
            case Section.None:
                isRunning = false;
                break;
        }

        if (queueImmediate) {
            AudioSource prev = audioSourceFlip ? audio2 : audio1;
            if (prev.isPlaying) {
                prev.SetScheduledEndTime(nextEventTime);
            }
            queueImmediate = false;
        }

        dispatchUpdate();
        if (clipToPlay == null) {
            // No clip selected, we are ending
            return;
        }

        // Schedule the next clip
        AudioSource audio = audioSourceFlip ? audio1 : audio2;
        audio.clip = clipToPlay;
        audio.PlayScheduled(nextEventTime);

        // Set the next event time and next audio source
        nextEventTime += playLength;
        audioSourceFlip = !audioSourceFlip;
    }

    public void Initialize(PlaybackConfig config, Action<Section, Section, int, int> onPlaylistChange) {
        this.branches = config.GetBranchClips().ToArray();
        this.branchLengths = config.GetBranchLengths().ToArray();
        this.playMode = config.playMode;
        this.hasReverb = config.hasReverb;
        this.hasIntroOutro = config.hasIntroOutro;
        this.immediate = config.immediate;
        this.intro = config.intro;
        this.outro = config.outro;
        this.introLength = config.introLength;
        this.onPlaylistChange = onPlaylistChange;
        initialized = true;
    }

    public void StartPlayback() {
        if (!this.initialized) {
            Debug.Log("Music manager not initialized");
            return;
        }
        Debug.Log("Current dsp time: " + AudioSettings.dspTime);
        nextEventTime = AudioSettings.dspTime + 0.2f;
        lastClipIndex = -1;
        nextClipIndex = getNextIndex();
        currentSection = Section.None;
        nextSection = hasIntroOutro && intro ? Section.Intro : Section.Branch;
        isRunning = true;
    }

    public void selectBranch(int index) {
        if (!this.initialized) {
            Debug.Log("Music manager not initialized");
            return;
        }

        if (playMode != PlayMode.Manual) {
            Debug.Log("can't select branch when not in manual mode");
            return;
        }

        if (nextClipIndex >= branches.Length) {
            Debug.Log("index out of range: " + index);
        }

        nextClipIndex = index;
        if (immediate) {
            changeImmediately();
        }
        dispatchUpdate();
    }

    public void setPlayMode(MusicManager.PlayMode newPlayMode) {
        if (!this.initialized) {
            Debug.Log("Music manager not initialized");
            return;
        }

        playMode = newPlayMode;
        nextClipIndex = getNextIndex();
        dispatchUpdate();
    }

    public void goToOutro() {
        if (!this.initialized) {
            Debug.Log("Music manager not initialized");
            return;
        }

        if (!hasIntroOutro || outro == null) {
            Debug.Log("Intro/outro disabled or no outro provided");
            return;
        }

        nextSection = Section.Outro;
        
        if (immediate) {
            changeImmediately();
        }

        dispatchUpdate();
    }

    public void endPlayback() {
        nextSection = Section.None;
        changeImmediately();
    }

    private void dispatchUpdate() {
        onPlaylistChange(currentSection, nextSection, lastClipIndex, nextClipIndex);
    }

    private int getNextIndex() {
        if (playMode == PlayMode.Sequential) {
            return (lastClipIndex + 1) % branches.Length;
        }
        if (playMode == PlayMode.Random) {
            return UnityEngine.Random.Range(0, branches.Length);
        } 
        return lastClipIndex == -1 ? 0 : lastClipIndex;
    }

    // Trigger the next section near immediately (0.1s delay)
    private void changeImmediately() {
        queueImmediate = true;
        nextEventTime = AudioSettings.dspTime + 0.1f;
    }
}
