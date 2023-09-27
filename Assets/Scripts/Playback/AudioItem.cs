using System;

[System.Serializable]
public struct AudioItem {
    public AudioItem(string path, float length) {
        Path = path;
        Length = length;
    }

    public string Path { get; }
    public float Length { get; }
}
