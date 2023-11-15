using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LayerUpload : SectionUpload
{    
    protected override string GetLabel() {
        return "Layer " + (index + 1);;
    }

    public void SetOnMove(Action<bool> onMove) {
        Debug.Log("Setting on move");
        Button moveUpButton = gameObject.transform.Find("UploadRow/MoveUp").gameObject.GetComponent<Button>();
        moveUpButton.onClick.AddListener(() => {
            onMove(true);
        });

        Button moveDownButton = gameObject.transform.Find("UploadRow/MoveDown").gameObject.GetComponent<Button>();
        moveDownButton.onClick.AddListener(() => {
            onMove(false);
        });
    }

    public void SetMoveButtons(bool canMoveUp, bool canMoveDown) {
        Debug.Log($"Setting move {canMoveUp} {canMoveDown}");
        Button moveUpButton = gameObject.transform.Find("UploadRow/MoveUp").gameObject.GetComponent<Button>();
        Button moveDownButton = gameObject.transform.Find("UploadRow/MoveDown").gameObject.GetComponent<Button>();
        moveUpButton.interactable = canMoveUp;
        moveDownButton.interactable = canMoveDown;
    }
}
