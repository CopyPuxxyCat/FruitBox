using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonMenuSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject settingsPanel;

    private void Start()
    {
        settingsPanel.transform.localScale = Vector2.zero;
    }

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
        settingsPanel.transform.LeanScale(Vector2.one, 0.5f);
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.transform.LeanScale(Vector2.zero, 1f).setEaseInBack().setOnComplete(OnCloseSettingComplete);
        //DelayAction(1f);
    }

    void OnCloseSettingComplete()
    {
        settingsPanel.SetActive(false);
    }
    IEnumerator DelayAction(float time)
    {
        yield return new WaitForSeconds(time);
    }
}
