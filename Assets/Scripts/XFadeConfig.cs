using System;

[System.Serializable]
public class XFadeConfig {
    public static int MAX_SECTIONS = 5;
    public static int MAX_TRANSITIONS = 10;

    [System.Serializable]
    public struct Transition {
        public string file;
        public int from;
        public int to;
        public float fadeInTime;
        public float fadeOutTime;
    }

    public float xfadeTime;
    public string[] sections = {};
    public Transition[] transitions = {};
}