using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LayerToggle : MonoBehaviour
{
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(int index, bool isOn, Action<int, bool> onLayerToggle) {
        this.index = index;
        gameObject.GetComponent<Toggle>().onValueChanged.AddListener((isOn) => {
            onLayerToggle(this.index, isOn);
        });
        gameObject.transform.Find("Label").GetComponent<Text>().text = $"Layer {(index + 1)}";
        GetComponent<Toggle>().isOn = isOn;
    }

    public int GetIndex() {
        return index;
    }
}
