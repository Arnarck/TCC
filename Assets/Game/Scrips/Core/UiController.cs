using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.InputSystem;
using Mirror;


public class UiController : MonoBehaviour
{
    public GameObject pausePanel, optionsPanel, creditsPanel, confirmMenuPanel, confirmResetPanel, audioPanel, videoPanel, menuPanel, panelLobbyMenu;
    private Stack<GameObject> panelStack = new Stack<GameObject>();

    public Slider masterVolumeSlider, musicVolumeSlider, vfxVolumeSlider;

    void Start()
    {
        /* if (GameController.controller != null)
         {
             GameController.controller.uiController = this;
         } */
        pausePanel.SetActive(false);
        LoadSliderValues();
    }

    void Update()
    {
        // Close current panel when pressing Escape
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (panelLobbyMenu.activeSelf == true) return;
            if (panelStack.Count == 0)
            {
                OpenPanel(pausePanel);
            }
            else
            {
                CloseCurrentPanel();
                return;
            }
        }
    }

    public void ResetButton()
    {
        // GameController.controller.Resetar();
        Debug.Log("Resetado");
    }

    void OpenPanel(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(true);
        panelStack.Push(panel);
    }

    void CloseCurrentPanel()
    {
        if (panelStack.Count > 0)
        {
            GameObject currentPanel = panelStack.Pop();
            currentPanel.SetActive(false);
        }

        if (panelStack.Count == 0)
        {
            Time.timeScale = 1.0f;
        }
        else
        {
            Time.timeScale = 0.0f;
        }
    }

    public void PauseButton()
    {
        if (panelStack.Count > 0)
            CloseCurrentPanel();
        else
            OpenPanel(pausePanel);
    }

    public void OptionsPanel()
    {
        if (panelStack.Count > 0 && panelStack.Peek() == optionsPanel)
        {
            CloseCurrentPanel();
        }
        else if (panelStack.Count > 0 && panelStack.Peek() == pausePanel)
        {
            OpenPanel(optionsPanel);
        }
    }

    public void CreditsPanel()
    {
        if (panelStack.Count > 0 && panelStack.Peek() == creditsPanel)
        {
            CloseCurrentPanel();
        }
        else if (panelStack.Count > 0 && panelStack.Peek() == pausePanel)
        {
            OpenPanel(optionsPanel);
        }
    }

    public void OpenAudioPanel()
    {
        if (panelStack.Count > 1 && panelStack.Peek() == audioPanel)
        {
            CloseCurrentPanel();
        }
        else if (panelStack.Count > 1 && panelStack.Peek() == optionsPanel)
        {
            OpenPanel(audioPanel);
        }
    }

    public void OpenVideoPanel()
    {
        if (panelStack.Count > 1 && panelStack.Peek() == videoPanel)
        {
            CloseCurrentPanel();
        }
        else if (panelStack.Count > 1 && panelStack.Peek() == optionsPanel)
        {
            OpenPanel(videoPanel);
        }
    }

    public void ConfirmMenuButton()
    {
        if (panelStack.Count > 0 && panelStack.Peek() == confirmMenuPanel)
        {
            CloseCurrentPanel();
        }
        else if (panelStack.Count > 0 && panelStack.Peek() == pausePanel)
        {
            OpenPanel(confirmMenuPanel);
        }
    }

    public void ConfirmResetButton()
    {
        if (panelStack.Count > 0 && panelStack.Peek() == confirmResetPanel)
        {
            CloseCurrentPanel();
        }
        else if (panelStack.Count > 0 && panelStack.Peek() == pausePanel)
        {
            OpenPanel(confirmResetPanel);
        }
    }

    public void CloseAllPanels()
    {
        while (panelStack.Count > 0)
        {
            GameObject panel = panelStack.Pop();
            panel.SetActive(false);
        }
        Time.timeScale = 1.0f;
    }

    public void ResetScene(int sceneIndex)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneIndex);
    }

    public void ChangeScene(string levelToGo)
    {
        // AudioController.audioController.ChangeMusic(levelToGo);
        SceneManager.LoadScene(levelToGo);
        Time.timeScale = 1.0f;
    }

    public void OnClick_GoToPreviousMenu()
    {
        if (NetworkClient.isConnected)
        {
            // Player connected
            pausePanel.SetActive(true);
        }
        else
        {
            // Main menu
            menuPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadSliderValues()
    {
        /* if (PlayerPrefs.HasKey("MasterVolume") && masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");

        if (PlayerPrefs.HasKey("MusicVolume") && musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");

        if (PlayerPrefs.HasKey("VFXVolume") && vfxVolumeSlider != null)
            vfxVolumeSlider.value = PlayerPrefs.GetFloat("VFXVolume"); */
    }

    public void ChangeAllVolume()
    {
        // AudioController.audioController?.ChangeAllVolume(masterVolumeSlider.value);
    }

    public void ChangeMusicVolume()
    {
        // AudioController.audioController?.ChangeMusicVolume(musicVolumeSlider.value);
    }

    public void ChangeVFXVolume()
    {
        // AudioController.audioController?.ChangeVFXVolume(vfxVolumeSlider.value);
    }
}