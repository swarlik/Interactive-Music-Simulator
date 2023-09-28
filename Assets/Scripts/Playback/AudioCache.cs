using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class AudioCache {
    private static AudioCache instance = new AudioCache();
    
    public static AudioCache Instance() {
        return instance;
    }

    private readonly Dictionary<string, AudioClip> cache;

    public AudioCache() {
        this.cache = new Dictionary<string, AudioClip>();
    }

    public AudioClip GetClip(string path) {
        return cache[path];
    }

    // Load audio from the filepath into the cache
    public IEnumerator LoadClip(string path, Action onSuccess, Action onError) {
        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV)) {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError) {
                Debug.Log(req.error);
                onError();
            } else {
                Debug.Log("Loaded audio: " + path);
                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                cache[path] = clip;
                onSuccess();
            }
        }
    }
}
