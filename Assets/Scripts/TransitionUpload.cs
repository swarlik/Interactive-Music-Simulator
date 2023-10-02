using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static XFadeConfig;

public class TransitionUpload : AudioUpload
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override string GetLabelType() {
        return "Transition";
    }

    public Transition GetInfo() {
        Transition transition = new Transition();
        transition.file = GetFilePath();
        transition.from = 1;
        transition.to = 2;
        return transition;
    }
}
