using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonMenuSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject settingsPanel;


    public void StartGame()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
    }    
}
