using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;
using Utp;
using Unity.Services.Relay;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;

public class Connect : MonoBehaviour
{

    public CardNetworkManager networkManager;

    [Header("UI")]
     public GameObject panelLobbyMenu;

    public GameObject hostButton;
    public GameObject clientButton;
    public GameObject joinCodeButton;
    public GameObject joinCodeTextGameObject;
    public GameObject disconnectButton;
    public TMP_Text joinCodeText;
    public TMP_InputField inputJoinCode;
    public TMP_Text statusText;
    private string currentCode = "";
private Coroutine statusCoroutine;

    public static Connect Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // opcional, se o menu persistir entre cenas
    }


    public void HideJoinMatchElements()
    {
        hostButton.SetActive(false);
        clientButton.SetActive(false);
        disconnectButton.SetActive(false);
    }

    public void ShowJoinMatchElements()
    {
        hostButton.SetActive(true);
        clientButton.SetActive(true);
        disconnectButton.SetActive(true);
    }

    public void Host()
    {
        statusText.text = "";
        joinCodeText.text = "";
        SetStatus("Creating room...");
        networkManager.StartRelayHost(3, null);
        StartCoroutine(WaitForCode());
    }

    System.Collections.IEnumerator WaitForCode()
    {
        while (string.IsNullOrEmpty(networkManager.relayJoinCode))
            yield return null;

        currentCode = networkManager.relayJoinCode;
        statusText.text = "";
        joinCodeText.text = currentCode;
        SetStatus("Room created!");
        HideJoinMatchElements();
    }
private void SetStatus(string message, float duration = 3f)
{
    statusText.text = message;

    if (statusCoroutine != null)
        StopCoroutine(statusCoroutine);

    statusCoroutine = StartCoroutine(ClearStatusAfterTime(duration));
}

private System.Collections.IEnumerator ClearStatusAfterTime(float seconds)
{
    yield return new WaitForSeconds(seconds);
    statusText.text = "";
}

    public void CopyCode()
    {
        statusText.text = "";
        if (string.IsNullOrEmpty(currentCode)) return;

        GUIUtility.systemCopyBuffer = currentCode;
        SetStatus("Code copied!");
    }

    public void Client()
    {
        statusText.text = "";
        joinCodeText.text = "";
        string code = inputJoinCode.text;
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Enter a code!");

            return;

        }

        SetStatus("Logging in...");
        networkManager.relayJoinCode = code;
        networkManager.JoinRelayServer();
        HideJoinMatchElements();
    }

    public void Disconnect()
    {
        statusText.text = "";
        joinCodeText.text = "";
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            // é host
            networkManager.StopHost();
            SetStatus("Host stopped");
        }
        else if (NetworkClient.isConnected)
        {
            // é client
            networkManager.StopClient();
            SetStatus("Client disconnected");
        }
        networkManager.relayJoinCode = "";
        ShowJoinMatchElements();
    }

    public void HideConnectPanel()
{
    if (panelLobbyMenu != null)
        panelLobbyMenu.SetActive(false);
}
}
