using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Vivox;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.ComponentModel;
using Unity.Services.Lobbies;
using System.Linq;

public class TextChatManager : MonoBehaviour
{
    public static TextChatManager Instance { get; private set; }

    [Header("UI References")]
    public TMP_InputField sendInput;
    public TMP_Dropdown chanelName;
    private string tempChanelName;
    private string lobbyId;
    [SerializeField] private ScrollRect scrollRect;
    private InputAction sendMessageAction;
    [SerializeField] private Transform chatContentParent; // ScrollView > Content objesi
    [SerializeField] private GameObject textMessagePrefab; // Prefab referansı
    private const int MaxMessageCount = 50;
    private readonly Queue<GameObject> messageQueue = new Queue<GameObject>();
    private CurrentLobby _currentLobby;
    private Player LocalPlayer => _currentLobby.currentLobby?.Players?.Find(p => p.Id == AuthenticationService.Instance.PlayerId);

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        _currentLobby = CurrentLobby.Instance;

        await VivoxService.Instance.InitializeAsync();

        LoginOptions options = new LoginOptions();
        options.DisplayName = _currentLobby.currentLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId)?.Data["playerName"].Value ?? "Unknown";
        await VivoxService.Instance.LoginAsync(options);

        sendMessageAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/enter");
        sendMessageAction.performed += ctx =>
        {
            if (sendInput.isFocused)
            {
                Debug.Log("ENTER tiklandi, mesaj gönderiliyor...");
                SendMessageAsync();
            }
        };
        sendMessageAction.Enable();

        SubscribeVivoxChannel();
        chanelName.onValueChanged.AddListener(OnChannelDropdownChanged);
    }
    void Update()
    {
        if (sendInput.isFocused && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("Enter basildi, mesaj gönderiliyor...");
            SendMessageAsync();
        }
    }
    async void SubscribeVivoxChannel()
    {
        lobbyId = _currentLobby.currentLobby.Id;
        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogError("Vivox: LobbyId boş!");
            return;
        }

        if (chanelName.options[chanelName.value].text == "Team")
        {
            var localPlayer = LocalPlayer;

            if (localPlayer != null && localPlayer.Data != null && localPlayer.Data.TryGetValue("team", out var teamdata))
            {
                if (teamdata.Value == "none")
                {
                    tempChanelName = "All";
                }
                else
                {
                    tempChanelName = teamdata.Value;
                }
            }
        }
        else
        {
            tempChanelName = chanelName.options[chanelName.value].text;
        }

        // Join kanal
        await VivoxService.Instance.JoinGroupChannelAsync(tempChanelName, ChatCapability.TextOnly);

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

        await VivoxService.Instance.SendChannelTextMessageAsync(tempChanelName, sendInput.text);
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

        DisplayMessage(senderName, messageText, timeReceived, channelName, fromSelf);
    }
    private void OnParticipantAdded(VivoxParticipant participant)
    {
        DisplayMessage("System",
        $"[Info] <color=green>{participant.DisplayName}</color> joined the channel.\n",
        DateTime.Now,
        participant.ChannelName,
        false);
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        DisplayMessage("System",
        $"[Info] <color=green>{participant.DisplayName}</color> left the channel.\n",
        DateTime.Now,
        participant.ChannelName,
        false);
    }
    public async void LeaveEchoChannelAsync()
    {
        string channelToLeave = tempChanelName;
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    }
    public async void QuitChatChannelAsync()
    {
        string channelToLeave = tempChanelName;
        sendMessageAction.Dispose();
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
        await VivoxService.Instance.LogoutAsync();
    }
    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); // Layout'un güncellenmesini garanti et
        scrollRect.verticalNormalizedPosition = 0f;
    }
    private void DisplayMessage(string sender, string messageText, DateTime time, string chanelName, bool fromSelf)
    { 
        GameObject newMessageGO = Instantiate(textMessagePrefab, chatContentParent);
        TextMeshProUGUI messageTextUI = newMessageGO.GetComponent<TextMeshProUGUI>();

        // Renk seçimi
        string color = fromSelf ? "green" : "blue";

        if (chanelName == "All")
        {
            messageTextUI.text = String.Format($"[{time:HH:mm}] [All] <color={color}>{sender}</color>: {messageText}");
        }
        else
        {
            messageTextUI.text = String.Format($"[{time:HH:mm}] <color={color}>{sender}</color>: {messageText}");
        }

        var rectTransform = newMessageGO.GetComponent<RectTransform>();
        rectTransform.SetParent(chatContentParent.transform);

        messageQueue.Enqueue(newMessageGO);

        // Fazla mesajları sil
        if (messageQueue.Count > MaxMessageCount)
        {
            GameObject oldest = messageQueue.Dequeue();
            Destroy(oldest);
        }

        ScrollToBottom();
    }
    private async void OnChannelDropdownChanged(int index)
    {
        Debug.Log("Dropdown değişti. Kanal değiştiriliyor...");

        var localPlayer = LocalPlayer;
        if (localPlayer == null) return;

        // Önce mevcut kanaldan çık
        try
        {
            if (localPlayer != null && localPlayer.Data != null && localPlayer.Data.TryGetValue("team", out var teamData))
            {
                if (teamData.Value != "none")
                {
                    Debug.Log(teamData.Value);
                    LeaveEchoChannelAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Kanal çikişinda hata oluştu: {ex.Message}");
        }

        // Yeni kanal ismini güncelle
        if (chanelName.options[index].text == "Team" && localPlayer.Data.TryGetValue("team", out var teamValue))
        {

            if (teamValue.Value == "none")
            {
                tempChanelName = "All";
            }
            else
            {
                tempChanelName = teamValue.Value;
            }
        }
        else
        {
            tempChanelName = chanelName.options[index].text;
        }

        if (VivoxService.Instance.ActiveChannels.Any(c => c.Key == tempChanelName))
        {
            Debug.Log($"Zaten {tempChanelName} kanalina bağlisin.");
            return; // Aynı kanalsa tekrar bağlanma
        }

        // Yeni kanala bağlan
        await VivoxService.Instance.JoinGroupChannelAsync(tempChanelName, ChatCapability.TextOnly);

        Debug.Log($"Yeni kanal: {tempChanelName}");

        // Chat ekranına bilgi mesajı ekle
        DisplayMessage("System", $"[Info] Changed to channel: {tempChanelName}", DateTime.Now, tempChanelName, false);
    }
}
