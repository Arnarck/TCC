using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
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
    public TMP_Text joinCodeText;
    public TMP_InputField inputJoinCode;
    public TMP_Text statusText;
    private string currentCode = "";

    public void Host()
    {
        statusText.text = "Creating room...";
        networkManager.StartRelayHost(2, null);
        StartCoroutine(WaitForCode());
    }

    System.Collections.IEnumerator WaitForCode()
    {
        while (string.IsNullOrEmpty(networkManager.relayJoinCode))
            yield return null;

        currentCode = networkManager.relayJoinCode;

        joinCodeText.text = currentCode;
        statusText.text = "Room created!";
    }

    public void CopyCode()
    {
        if (string.IsNullOrEmpty(currentCode)) return;

        GUIUtility.systemCopyBuffer = currentCode;
        statusText.text = "Code copied!";
    }

    public void Client()
    {
        string code = inputJoinCode.text;
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "Enter a code!";

            return;

        }

        statusText.text = "Logging in...";
        networkManager.relayJoinCode = code;
        networkManager.JoinRelayServer();
    }
}
