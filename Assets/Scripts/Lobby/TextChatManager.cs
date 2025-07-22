using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Vivox;
using Unity.Services.Authentication;

public class TextChatManager : MonoBehaviour
{
    [Header("UI References")]
    public InputField sendInput;
    public Button sendButton;
    public Text chatLog;

    private string lobbyId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await VivoxService.Instance.InitializeAsync();

        LoginOptions options = new LoginOptions();
        options.DisplayName = AuthenticationService.Instance.PlayerId;
        await VivoxService.Instance.LoginAsync(options);

        SubscribeVivoxChannel();
    }

    async void SubscribeVivoxChannel()
    {
        var current = CurrentLobby.Instance;
        lobbyId = current.currentLobby.Id;
        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogError("Vivox: LobbyId boş!");
            return;
        }

        // Text channel
        await VivoxService.Instance.JoinEchoChannelAsync("Chat", ChatCapability.TextOnly);

        chatLog.text += $"Chat channel: {"chat"}\n";

        // Join kanal
        await VivoxService.Instance.JoinGroupChannelAsync("chat", ChatCapability.TextOnly);

        // event handler
        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
    }
    public async void SendMessageAsync()
    {
        if (string.IsNullOrEmpty(sendInput.text))
        {
            return;
        }

        await VivoxService.Instance.SendChannelTextMessageAsync("chat", sendInput.text);
        sendInput.text = string.Empty;
    }
    void OnChannelMessageReceived(VivoxMessage message)
    {
        var channelName = message.ChannelName;
        var senderName = message.SenderDisplayName;
        var senderId = message.SenderPlayerId;
        var messageText = message.MessageText;
        var timeReceived = message.ReceivedTime;
        var language = message.Language;
        var fromSelf = message.FromSelf;
        var messageId = message.MessageId;

        chatLog.text += $"{senderName}: {messageText}\n";
    }
    private void OnParticipantAdded(VivoxParticipant participant)
    {
        chatLog.text += $"[Info] {participant.DisplayName} joined the channel.\n";
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        chatLog.text += $"[Info] {participant.DisplayName} left the channel.\n";
    }
    public async void LeaveEchoChannelAsync()
    {
        string channelToLeave = "chat";
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
        await VivoxService.Instance.LogoutAsync();
    }
}
