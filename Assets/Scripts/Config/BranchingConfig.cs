using System;

[System.Serializable]
public class BranchingConfig : BaseConfig {
    public static int MAX_BRANCHES = 5;
    public static int DEFAULT_BRANCHES = 2;

    public int numBranches = DEFAULT_BRANCHES;
    public bool hasIntroOutro = false;
    public bool hasReverb = false;
    public string playMode = MusicManager.PlayMode.Random.ToString();
    public bool immediate = false;

    public AudioItem intro;
    public string outroFile;
    public string videoFilePath;
    public AudioItem[] branches = new AudioItem[MAX_BRANCHES];
}
