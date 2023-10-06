using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeDropdown : MonoBehaviour
{
    public enum PlaybackMode {
        Random,
        Sequential,
        Manual
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public PlaybackMode GetPlaybackMode() {
        Dropdown dropdown = GetComponent<Dropdown>();
        string currentOption = dropdown.options[dropdown.value].text;
        return (PlaybackMode) System.Enum.Parse(typeof(PlaybackMode), currentOption);
    }

    public void SetPlaybackMode(PlaybackMode mode) {
        Dropdown dropdown = GetComponent<Dropdown>();
        int index = dropdown.options.FindIndex((option) => option.text.Equals(mode.ToString()));
        dropdown.value = index;
    }
}
