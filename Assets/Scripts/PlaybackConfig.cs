using static MusicManager;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

[System.Serializable]
public class PlaybackConfig {
    public class SerializedSettings {
        public PlaybackConfig config;
        public string[] branchFiles;
        public float[] branchLengths;
    }

    private static int DEFAULT_BRANCHES = 2;

    private readonly List<AudioClip> branchClips;
    private readonly List<float> branchLengths;
    private readonly List<string> branchFiles;
    
    public int numBranches = DEFAULT_BRANCHES;
    public bool hasIntroOutro = false;
    public bool hasReverb = false;
    public MusicManager.PlayMode playMode = MusicManager.PlayMode.Random;
    public bool immediate = false;
    public AudioClip intro;
    public AudioClip outro;
    public string introFile;
    public string outroFile;
    public float introLength = -1.0f;
    public string videoFilePath;

    public PlaybackConfig(int maxBranches) {
        branchClips = Enumerable.Repeat<AudioClip>(null, maxBranches).ToList();
        branchFiles = Enumerable.Repeat<string>(null, maxBranches).ToList();
        branchLengths = Enumerable.Repeat(-1.0f, maxBranches).ToList();
    }

    public PlaybackConfig(int maxBranches, PlaybackConfig baseConfig)
        : this(maxBranches) 
    {
        Debug.Log($"config constructor. baseConfig branches: {baseConfig.numBranches}");
        this.numBranches = baseConfig.numBranches;
        this.hasIntroOutro = baseConfig.hasIntroOutro;
        this.hasReverb = baseConfig.hasReverb;
        this.immediate = baseConfig.immediate;
        this.introFile = baseConfig.introFile != "" ? LocalPathToFullPath(baseConfig.introFile) : null;
        this.outroFile = baseConfig.outroFile != "" ? LocalPathToFullPath(baseConfig.outroFile) : null;
        this.introLength = baseConfig.introLength;
        this.videoFilePath = baseConfig.videoFilePath != "" ? LocalPathToFullPath(baseConfig.videoFilePath) : baseConfig.videoFilePath;
        // Copy all settings
    }

    public void SetActiveBranches(int count) {
        numBranches = count;
    }

    public void SetBranchClip(AudioClip clip, int index, string path) {
        if (index >= branchClips.Count) {
            Debug.Log("index out of bounds");
            return;
        }
        Debug.Log($"Setting {path} at index {index}");
        branchClips[index] = clip;
        branchFiles[index] = path;
    }

    public void SetBranchLength(float length, int index) {
        if (index >= branchLengths.Count) {
            Debug.Log("index out of bounds");
            return;
        }

        branchLengths[index] = length;
    }

    // TODO: probably make a struct with AudioClip, length, and file path

    // Limits to active branch count & filters out null values
    public List<AudioClip> GetBranchClips() {
        List<AudioClip> activeBranches = new List<AudioClip>();
        for (int i = 0; i < numBranches; i++) {
            if (branchClips[i] != null) {
                activeBranches.Add(branchClips[i]);
            }
        }

        return activeBranches;
    }

    public List<float> GetBranchLengths() {
        List<float> activeLengths = new List<float>();
        for (int i = 0; i < numBranches; i++) {
            if (branchLengths[i] > 0) {
                activeLengths.Add(branchLengths[i]);
            }
        }

        return activeLengths;
    }

    public List<string> GetBranchPaths() {
        Debug.Log($"Getting branch paths. Num branches {numBranches}");
        List<string> activePaths = new List<string>();
        for (int i = 0; i < numBranches; i++) {
            Debug.Log(branchFiles[i]);
            if (branchFiles[i] != null) {
                activePaths.Add(branchFiles[i]);
            }
        }

        return activePaths;
    }

    public static SerializedSettings Serialize(PlaybackConfig config) {
        SerializedSettings settings = new SerializedSettings();
        settings.config = config;
        // Copy object to avoid affecting current settings
        SerializedSettings settingsCopy = JsonUtility.FromJson<SerializedSettings>(JsonUtility.ToJson(settings));
        if (settingsCopy.config.introFile != null || settingsCopy.config.introFile != "") {
            settingsCopy.config.introFile = FullPathToLocalPath(settingsCopy.config.introFile);
        }
        if (settingsCopy.config.outroFile != null || settingsCopy.config.outroFile != "") {
            settingsCopy.config.outroFile = FullPathToLocalPath(settingsCopy.config.outroFile);
        }
        if (settingsCopy.config.videoFilePath != null || settingsCopy.config.videoFilePath != "") {
            settingsCopy.config.videoFilePath = FullPathToLocalPath(settingsCopy.config.videoFilePath);
        }
        settingsCopy.branchFiles = config.GetBranchPaths().Select(path => {
            return FullPathToLocalPath(path);
        }).Where(path => path != null).ToArray();
        settingsCopy.branchLengths = config.GetBranchLengths().ToArray();
        return settingsCopy;
    }
    
    public static bool IsPathValid(string path) {
        return Application.isEditor || path.IndexOf(Path.GetFullPath("./")) == 0;
    }

    private static string FullPathToLocalPath(string path) {
        string currentDirectory = Path.GetFullPath("./");
        int index = path.IndexOf(currentDirectory);
        if (index != 0) {
            return null;
        }

        return path.Substring(currentDirectory.Length);        
    }

    private static string LocalPathToFullPath(string path) {
        return Path.GetFullPath("./") + path;
    }
}
