using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonMenuSceneManager : MonoBehaviour
{
    public void StartButton()
    {
        SceneManager.LoadScene("Level_1");
    }    
}
