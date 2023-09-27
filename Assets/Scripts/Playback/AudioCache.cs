using UnityEngine;
using System.Collections.Generic;

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
}
