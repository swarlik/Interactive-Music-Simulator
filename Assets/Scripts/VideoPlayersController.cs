using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayersController : MonoBehaviour
{
    public VideoPlayer primaryVideoPlayer;
    public VideoPlayer gameEndVideoPlayer;
    private List<VideoPlayer>  videoPlayers = new List<VideoPlayer>();
    private float crossfadeTime;
    private string gameEndPositiveUrl;
    private string gameEndNegativeUrl;
    private GameConfig.VideoDelay[] videoDelays;
    private bool transitionInProgress = false;
    private Queue<int> queuedVideoChangeRequests = new Queue<int>();
    private Queue<bool> queuedEndGameChangeRequests = new Queue<bool>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!transitionInProgress && queuedVideoChangeRequests.Count > 0) {
            Debug.Log("No transition; initiating queued request.");
            SwitchToVideoAtIndex(queuedVideoChangeRequests.Dequeue());
        }

        if (!transitionInProgress && queuedEndGameChangeRequests.Count > 0) {
            Debug.Log("No transition; initiating game end request.");
            PlayGameEndVideo(queuedEndGameChangeRequests.Dequeue());
        }
    }

    public void InitializeVideoSurfaces(
        string[] videoUrls, 
        string gameEndPositiveUrl, 
        string gameEndNegativeUrl, 
        float crossfadeTime,
        GameConfig.VideoDelay[] videoDelays) 
    {
        if (videoUrls.Length == 0) {
            Debug.Log("No video urls provided.");
            return;
        }

        primaryVideoPlayer.url = videoUrls[0];
        primaryVideoPlayer.Play();
        videoPlayers.Add(primaryVideoPlayer);

        for (int i = 1; i < videoUrls.Length; i ++) {
            VideoPlayer newPlayer = Instantiate(primaryVideoPlayer, primaryVideoPlayer.transform.parent);
            newPlayer.url = videoUrls[i];
            videoPlayers.Add(newPlayer);
        }

        this.crossfadeTime = crossfadeTime;
        this.gameEndPositiveUrl = gameEndPositiveUrl;
        this.gameEndNegativeUrl = gameEndNegativeUrl;
        this.videoDelays = videoDelays;
    }

    public void SwitchToVideoAtIndex(int index) {
        if (index < 0 || index >= videoPlayers.Count) {
            Debug.LogWarning("Video index out of bounds");
            return;
        }

        if (transitionInProgress) {
            Debug.Log("Transition in progress; queueing request for index: " + index);
            queuedVideoChangeRequests.Enqueue(index);
            return;
        }

        Debug.Log("Switching to video index: " + index);

        int currentIndex = getCurrentlyPlayingVideoIndex();
        if (currentIndex == index) {
            Debug.Log("Already playing video at index: " + index);
            return;
        }

        float delayTimeSeconds = getDelayTimeForTransition(currentIndex, index);

        VideoPlayer newVideoPlayer = videoPlayers[index];
        VideoPlayer currentVideoPlayer = getCurrentlyPlayingVideoPlayer();

        if (currentVideoPlayer != null) {
            StartCoroutine(fadeOutAndStopVideo(currentVideoPlayer, crossfadeTime, delayTimeSeconds));
        }
        
        StartCoroutine(playAndFadeInVideo(newVideoPlayer, crossfadeTime, delayTimeSeconds));
    }

    public void PlayGameEndVideo(bool positiveEnd) {
        if (transitionInProgress) {
            Debug.Log("Transition in progress; queueing request for game end video with value: " + positiveEnd);
            queuedEndGameChangeRequests.Enqueue(positiveEnd);
            return;
        }

        VideoPlayer currentVideoPlayer = getCurrentlyPlayingVideoPlayer();
        
        if (currentVideoPlayer == gameEndVideoPlayer) {
            Debug.Log("Game end already playing; ignoring request.");
            return;
        }

        Debug.Log("Switching to game end player with value: " + positiveEnd);

        if (currentVideoPlayer != null) {
            StartCoroutine(fadeOutAndStopVideo(currentVideoPlayer, crossfadeTime, 0.0f));
        }

        gameEndVideoPlayer.url = positiveEnd ? gameEndPositiveUrl : gameEndNegativeUrl;
        gameEndVideoPlayer.targetCameraAlpha = 1.0f;
        gameEndVideoPlayer.Play();
    }

    private VideoPlayer getCurrentlyPlayingVideoPlayer() {
        int currentIndex = getCurrentlyPlayingVideoIndex();
        if (currentIndex != -1) {
            return videoPlayers[currentIndex];
        }

        if (gameEndVideoPlayer.isPlaying) {
            return gameEndVideoPlayer;
        }

        return null;
    }

    private int getCurrentlyPlayingVideoIndex() {
        for (int i = 0; i < videoPlayers.Count; i++) {
            if (videoPlayers[i].isPlaying) {
                return i;
            }
        }

        return -1;
    }

    private IEnumerator fadeOutAndStopVideo(VideoPlayer player, float fadeOutTime, float delayTimeSeconds) {
        transitionInProgress = true;

        Debug.Log("Delay time for transition: " + delayTimeSeconds);
        yield return new WaitForSeconds(delayTimeSeconds);
        Debug.Log("Done waiting.");
        
        float alpha = 1.0f;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / fadeOutTime)
        {
            alpha = Mathf.Lerp(alpha, 0, t);
            player.targetCameraAlpha = alpha;
            yield return null;
        }

        player.Stop();
        transitionInProgress = false;
    }

    private IEnumerator playAndFadeInVideo(VideoPlayer player, float fadeInTime, float delayTimeSeconds) {
        player.targetCameraAlpha = 0.0f;

        yield return new WaitForSeconds(delayTimeSeconds);
        
        player.Play();

        float alpha = 0.0f;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / fadeInTime)
        {
            alpha = Mathf.Lerp(alpha, 1, t);
            player.targetCameraAlpha = alpha;
            yield return null;
        }
    }

    private float getDelayTimeForTransition(int fromIndex, int toIndex) {
        if (videoDelays == null) {
            return 0.0f;
        }
        
        foreach (GameConfig.VideoDelay delay in videoDelays) {
            if (delay.toIndex == toIndex && delay.fromIndex == fromIndex) {
                return delay.delayTimeSeconds;
            }
        }

        return 0.0f;
    }
}
