using System;

[System.Serializable]
public class XFadeConfig {
    public static int MAX_SECTIONS = 5;
    public static int MAX_TRANSITIONS = 10;

    [System.Serializable]
    public class Fadeable {
        public string file;
        public float fadeInTime;
        public float fadeOutTime;
        public float loopLength;
    }

    [System.Serializable]
    public class Section : Fadeable {
    
    }

    [System.Serializable]
    public class Transition : Fadeable {
        public int from;
        public int to;
    }

    public float xfadeTime = 2.0f;
    public Fadeable[] sections = {};
    public Transition[] transitions = {};
    public string videoFilePath;
    public float musicVolume = 1.0f;
    public float videoVolume = 0.0f;
    public bool hasReverb = false;

    public Fadeable intro;
    public Fadeable outro;
    public bool hasIntroOutro = false;
}