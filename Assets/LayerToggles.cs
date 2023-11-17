using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LayerToggles : MonoBehaviour
{
    public GameObject togglePrefab;
    private int numLayers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(int numLayers, Action<int, bool> onLayerToggle) {
        for (int i = 0; i < numLayers; i++) {
            GameObject toggleObject = Instantiate(togglePrefab, gameObject.transform);
            toggleObject.GetComponent<LayerToggle>().Setup(i, i == 0, onLayerToggle);
        }
        this.numLayers = numLayers;
    }

    public bool[] GetActiveLayers() {
        bool[] activeLayersList = new bool[numLayers];
        foreach (Transform layerTransform in transform) {
            GameObject layerObj = layerTransform.gameObject;
            activeLayersList[layerObj.GetComponent<LayerToggle>().GetIndex()]
                = layerObj.GetComponent<Toggle>().isOn;
        }
        return activeLayersList;
    }
}
