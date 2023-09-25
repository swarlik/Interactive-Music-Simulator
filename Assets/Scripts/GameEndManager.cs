using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndManager : MonoBehaviour
{
    public Button restartButton;

    // Start is called before the first frame update
    void Start()
    {
        restartButton.interactable = false;
    }

    public void SetGameEnd(bool isInGameEnd) {
        restartButton.interactable = isInGameEnd;
    }
}
