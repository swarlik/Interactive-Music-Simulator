using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public enum PlayMode {
        Sequential,
        Random,
        Manual
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
    private Text nextBranchLabel;
    private Button restartButton;

    private bool initialized;
    private int lastClipIndex;
    private int nextClipIndex;
    private double nextEventTime;
    private bool isRunning;
    private bool audioSourceFlip;

    private bool queueIntro;
    private bool queueOutro;
    private bool playingIntro;
    private bool playingOutro;
    private bool queueImmediate;

    // Start is called before the first frame update
    void Start()
    {
        initialized = false;
        isRunning = false;
        audioSourceFlip = true;
        queueIntro = false;
        queueOutro = false;
        playingIntro = true;
        playingOutro = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning || !this.initialized) {
            return;
        }

        if (AudioSettings.dspTime + 1.0f < nextEventTime) {
            return;
        }

        AudioClip clipToPlay;
        float playLength;

        if (queueOutro && outro != null) {
            clipToPlay = outro;
            playLength = outro.length;
            isRunning = false;
            queueOutro = false;
            playingOutro = true;
            restartButton.interactable = true;
        } else if (queueIntro && intro != null) {
            clipToPlay = intro;
            queueIntro = false;
            playingIntro = true;
            playLength = introLength > 0 && hasReverb ? introLength : intro.length;
        } else {
            clipToPlay = branches[nextClipIndex];
            lastClipIndex = nextClipIndex;
            nextClipIndex = getNextIndex();
            if (hasReverb && lastClipIndex < branchLengths.Length && branchLengths[lastClipIndex] > 0) {
                playLength = branchLengths[lastClipIndex];
            } else {
                playLength = clipToPlay.length;
            }
            playingIntro = false;
            playingOutro = false;
        }

        if (immediate && queueImmediate) {
            AudioSource prev = audioSourceFlip ? audio2 : audio1;
            if (prev.isPlaying) {
                prev.SetScheduledEndTime(nextEventTime);
            }
            queueImmediate = false;
        }

        AudioSource audio = audioSourceFlip ? audio1 : audio2;
        audio.clip = clipToPlay;
        audio.PlayScheduled(nextEventTime);
        Debug.Log($"Scheduled branch {(queueIntro ? "intro" : (lastClipIndex + 1))} to start at time {nextEventTime}");

        nextEventTime += playLength;
        Debug.Log($"Next clip to play: {nextClipIndex + 1} at time {nextEventTime}");

        audioSourceFlip = !audioSourceFlip;
        updateText();
    }

    public void Initialize(
            AudioClip[] branches,
            float[] branchLengths,
            PlayMode playMode,
            bool hasReverb,
            bool hasIntroOutro,
            bool immediate,
            AudioClip intro,
            AudioClip outro,
            float introLength,
            Text nextBranchLabel,
            Button restartButton)
    {
        this.branches = branches;
        this.branchLengths = branchLengths;
        this.playMode = playMode;
        this.hasReverb = hasReverb;
        this.hasIntroOutro = hasIntroOutro;
        this.immediate = immediate;
        this.intro = intro;
        this.outro = outro;
        this.introLength = introLength;
        this.nextBranchLabel = nextBranchLabel;
        this.restartButton = restartButton;
        configureRestartButton(this.restartButton);
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
        queueIntro = hasIntroOutro && intro != null;
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
            queueImmediate = true;
            nextEventTime = AudioSettings.dspTime + 0.1f;
        }
        updateText();
    }

    public void setPlayMode(MusicManager.PlayMode newPlayMode) {
        if (!this.initialized) {
            Debug.Log("Music manager not initialized");
            return;
        }

        playMode = newPlayMode;
        nextClipIndex = getNextIndex();
        updateText();
    }

    public void goToOutro() {
        if (!this.initialized) {
            Debug.Log("Music manager not initialized");
            return;
        }

        queueOutro = true;
        if (immediate) {
            queueImmediate = true;
            nextEventTime = AudioSettings.dspTime + 0.1f;
        }
        updateText();
    }

    private void updateText() {
        string current;
        string next;
        if (playingIntro) {
            current = "Intro";
        } else if (playingOutro) {
            current = "Outro";
        } else {
            current = $"Branch {(lastClipIndex + 1)}";
        }

        if (queueIntro) {
            next = "Intro";
        } else if (queueOutro) {
            next = "Outro";
        } else {
            next = $"Branch {(nextClipIndex + 1)}";
        }

        nextBranchLabel.text = $"Now playing: {current}. Next: {next}";
    }

    private int getNextIndex() {
        if (playMode == PlayMode.Sequential) {
            return (lastClipIndex + 1) % branches.Length;
        }
        if (playMode == PlayMode.Random) {
            return Random.Range(0, branches.Length);
        } 
        return lastClipIndex == -1 ? 0 : lastClipIndex;
    }

    private void configureRestartButton(Button restartButton) {
        restartButton.interactable = false;
        restartButton.onClick.AddListener(delegate {
            if (!playingOutro) {
                return;
            }

            StartPlayback();
            restartButton.interactable = false;
        });
    }
}
