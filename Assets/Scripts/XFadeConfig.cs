using System;

[System.Serializable]
public class XFadeConfig {
    public static int MAX_SECTIONS = 5;
    public static int MAX_TRANSITIONS = 10;

    [System.Serializable]
    public class Transition {
        public string file;
        public int from;
        public int to;
        public float fadeInTime;
        public float fadeOutTime;
    }

    public float xfadeTime = 2.0f;
    public string[] sections = {};
    public Transition[] transitions = {};
    public string videoFilePath;
    public float musicVolume = 1.0f;
    public float videoVolume = 0.0f;
}