using System;

[System.Serializable]
public class VerticalRemixingConfig {
    public static int MAX_LAYERS = 8;

    public Fadeable[] layers = {};
    public string videoFilePath;
    public float musicVolume = 1.0f;
    public float videoVolume = 0.0f;
    public bool hasReverb = false;
    
    public bool hasIntroOutro = false;
    public Fadeable intro;
    public Fadeable outro;

    public LayeringMode layeringMode;

    public enum LayeringMode {
        Additive,
        Independent
    }

    public bool HasIntro() {
        return hasIntroOutro && intro != null && intro.file != null && intro.file != "";
    }

    public bool HasOutro() {
        return hasIntroOutro && outro != null && outro.file != null && outro.file != "";
    }
}