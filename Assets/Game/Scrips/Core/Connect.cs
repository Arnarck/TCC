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
    public GameObject hostButton;
    public GameObject clientButton;
    public GameObject joinCodeButton;
    public GameObject joinCodeTextGameObject;
    public GameObject disconnectButton;
    public TMP_Text joinCodeText;
    public TMP_InputField inputJoinCode;
    public TMP_Text statusText;
    private string currentCode = "";

    public void HideJoinMatchElements()
    {
        hostButton.SetActive(false);
        clientButton.SetActive(false);
        joinCodeButton.SetActive(false);
        joinCodeTextGameObject.SetActive(false);
        disconnectButton.SetActive(false);
    }

    public void ShowJoinMatchElements()
    {
        hostButton.SetActive(true);
        clientButton.SetActive(true);
        joinCodeButton.SetActive(true);
        joinCodeTextGameObject.SetActive(true);
        disconnectButton.SetActive(true);
    }

    public void Host()
    {
        statusText.text = "";
        joinCodeText.text = "";
        statusText.text = "Creating room...";
        networkManager.StartRelayHost(2, null);
        StartCoroutine(WaitForCode());
    }

    System.Collections.IEnumerator WaitForCode()
    {
        while (string.IsNullOrEmpty(networkManager.relayJoinCode))
            yield return null;

        currentCode = networkManager.relayJoinCode;
        statusText.text = "";
        joinCodeText.text = currentCode;
        statusText.text = "Room created!";
        HideJoinMatchElements();
    }

    public void CopyCode()
    {
        statusText.text = "";
        if (string.IsNullOrEmpty(currentCode)) return;

        GUIUtility.systemCopyBuffer = currentCode;
        statusText.text = "Code copied!";
    }

    public void Client()
    {
        statusText.text = "";
        joinCodeText.text = "";
        string code = inputJoinCode.text;
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "Enter a code!";

            return;

        }

        statusText.text = "Logging in...";
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
            statusText.text = "Host stopped";
        }
        else if (NetworkClient.isConnected)
        {
            // é client
            networkManager.StopClient();
            statusText.text = "Client disconnected";
        }
        networkManager.relayJoinCode = "";
        ShowJoinMatchElements();
    }
}
