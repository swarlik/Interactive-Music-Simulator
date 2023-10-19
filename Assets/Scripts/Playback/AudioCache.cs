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
        return cache.ContainsKey(path) ? cache[path] : null;
    }

    // Load audio from the filepath into the cache
    public IEnumerator LoadClip(string path, Action onSuccess, Action<string> onError) {
        // Break early if the clip has already been loaded
        if (cache.ContainsKey(path)) {
            Debug.Log($"File {path} already present in cache");
            onSuccess();
            yield break;
        }
        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(new Uri(path), AudioType.WAV)) {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.responseCode != 200) {
                onError($"Error loading audio file: {path}. \nError message: {req.error}");
            } else {
                Debug.Log("Loaded audio: " + path);
                AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
                cache[path] = clip;
                onSuccess();
            }
        }
    }
}
