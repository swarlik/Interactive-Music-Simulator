using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static XFadeConfig;

public class IntroUpload : SectionUpload
{
    protected override string GetLabel() {
        return "Intro";
    }

    public Fadeable GetInfo() {
        Fadeable section = base.GetInfo();
        // Calculate length based on fade out time
        AudioClip introClip = AudioCache.Instance().GetClip(section.file);
        if (introClip == null) {
            Debug.Log("No intro clip");
            return section;
        }
        section.loopLength = introClip.length - section.fadeOutTime;
        Debug.Log($"Setting intro length to {section.loopLength}");
        return section;
    }
}
