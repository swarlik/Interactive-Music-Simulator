using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startVideo(string path, float initialVolume) {
        videoPlayer.url = "file://" + path;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetDirectAudioVolume(0, initialVolume);
        videoPlayer.Play();
    }

    public void setVolume(float volume) {
        videoPlayer.SetDirectAudioVolume(0, volume);
    }
}
