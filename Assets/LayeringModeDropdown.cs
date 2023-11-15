using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static VerticalRemixingConfig;

public class LayeringModeDropdown : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public LayeringMode GetLayeringMode() {
        Dropdown dropdown = GetComponent<Dropdown>();
        string currentOption = dropdown.options[dropdown.value].text;
        return (LayeringMode) System.Enum.Parse(typeof(LayeringMode), currentOption);
    }

    public void SetLayeringMode(LayeringMode mode) {
        Dropdown dropdown = GetComponent<Dropdown>();
        int index = dropdown.options.FindIndex((option) => option.text.Equals(mode.ToString()));
        dropdown.value = index;
    }
}
