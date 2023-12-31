using System;

[System.Serializable]
public class AudioItem {
    public AudioItem(string path, float length) {
        Path = path;
        Length = length;
    }

    public string Path { get; set; }
    public float Length { get; set; }
}
