using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioLayer : MonoBehaviour
{
    private static double OFFSET = 1.0f;

    private Fadeable section;
    private bool hasReverb;
    private AudioSource audio1;
    private AudioSource audio2;
    private AudioSource currentAudio;
    private AudioMixerGroup output;
    private AudioClip clip;

    private double nextEventTime;
    private bool isPlaying;

    void Awake() {
        audio1 = gameObject.AddComponent<AudioSource>();
        audio2 = gameObject.AddComponent<AudioSource>();
    }

    void Update() {
        if (!isPlaying || !hasReverb || AudioSettings.dspTime + OFFSET < nextEventTime) {
            return;
        }

        Debug.Log("Starting to play");
        FlipAudio();
        currentAudio.clip = clip;
        currentAudio.PlayScheduled(nextEventTime);

        nextEventTime += section.loopLength;
    }

    public void Init(Fadeable section, bool hasReverb, AudioMixerGroup output) {
        this.section = section;
        this.hasReverb = hasReverb;
        audio1.outputAudioMixerGroup = output;
        audio2.outputAudioMixerGroup = output;
    }

    public void Play(double playTime) {
        Debug.Log("Starting to play!");
        if (section == null) {
            Debug.Log("Play called before Init!");
            return;
        }

        AudioClip clip = AudioCache.Instance().GetClip(FilePathUtils.LocalPathToFullPath(section.file));
        if (clip == null) {
            Debug.Log($"No audio loaded for {section.file}");
            return;
        }

        this.clip = clip;

        if (hasReverb && section.loopLength > 0) {
            nextEventTime = playTime;
            Debug.Log($"playing next event at {nextEventTime}");
        } else {
            currentAudio = audio1;
            Debug.Log("Starting to play!");
            currentAudio.clip = clip;
            currentAudio.loop = true;
            currentAudio.PlayScheduled(playTime);
        }

        isPlaying = true;
    }

    public void Stop() {
        currentAudio.Stop();
        isPlaying = false;
    }

    private void FlipAudio() {
        if (currentAudio == audio1) {
            currentAudio = audio2;
        } else {
            currentAudio = audio1;
        }
    }
}