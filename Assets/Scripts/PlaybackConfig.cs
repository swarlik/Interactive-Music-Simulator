using static MusicManager;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaybackConfig {
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

    public void SetActiveBranches(int count) {
        numBranches = count;
    }

    public void SetBranchClip(AudioClip clip, int index, string path) {
        if (index >= branchClips.Count) {
            Debug.Log("index out of bounds");
            return;
        }

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
        List<string> activePaths = new List<string>();
        for (int i = 0; i < numBranches; i++) {
            if (branchFiles[i] != null) {
                activePaths.Add(branchFiles[i]);
            }
        }

        return activePaths;
    }
}