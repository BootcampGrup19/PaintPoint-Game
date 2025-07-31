using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unity.BizimKodlar
{
    public class GetLobbies : MonoBehaviour
    {
        public GameObject lobbyRowPrefab;
        public GameObject rowContainer;
        public Button lobbiesButton;
        public Button refreshButton;
        public GameObject playerNamePanel;
        public GameObject lobbyBrowserPanel;
        public TMP_InputField playerNameInput;
        public Button playerNameButton;
        private readonly HashSet<string> _existingLobbyIds = new HashSet<string>();

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        async void OnSceneLoaded(Scene s, LoadSceneMode mode)
        {
            if (s.name == "LobbyBrowserScene")
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                // Yeni UI referanslarını bul:
                playerNamePanel = GameObject.Find("PlayerNamePanel");
                lobbyBrowserPanel = GameObject.Find("LobbyBrowserScenePanel");
                //playerNameInput = GameObject.Find("PlayerNameInputField").GetComponent<TMP_InputField>();
                //playerNameButton = GameObject.Find("PlayerNameButton").GetComponent<Button>();

                HandlePlayerNameLogic();
                StartCoroutine(WaitForInputField());
            }
        }
        IEnumerator WaitForInputField()
        {
            GameObject inputObj = null;

            while (inputObj == null)
            {
                inputObj = GameObject.Find("LobbyBrowserScenePanel");
                yield return null; // her frame yeniden dener
            }

            rowContainer = GameObject.Find("PlayerContent");
            lobbiesButton = GameObject.Find("Lobbies").GetComponent<Button>();
            refreshButton = GameObject.Find("RefreshButton").GetComponent<Button>();

            lobbiesButton.onClick.RemoveAllListeners();
            refreshButton.onClick.RemoveAllListeners();
            lobbiesButton.onClick.AddListener(GetLobbiesTest);
            refreshButton.onClick.AddListener(GetLobbiesTest);
        }
        private void HandlePlayerNameLogic()
        {
            // Eğer daha önce isim atanmışsa PlayerNamePanel'i gösterme
            if (PlayerPrefs.GetInt("isCustomNameSet", 0) == 1)
            {
                Debug.Log("Player has set a custom name previously: " + AuthenticationService.Instance.PlayerName);

                playerNamePanel.SetActive(false);
                lobbyBrowserPanel.SetActive(true);
                GetLobbiesTest();
            }
            else
            {
                Debug.Log("Prompting for custom player name.");
                playerNamePanel.SetActive(true);
                lobbyBrowserPanel.SetActive(false);
            }
        }
        public async void SetPlayerNameAndShowLobby()
        {
            string inputName = playerNameInput.text.Trim();

            if (string.IsNullOrEmpty(inputName))
            {
                Debug.LogWarning("Player name input is empty!");
                return;
            }

            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(inputName);
                Debug.Log("Player name set to: " + inputName);

                PlayerPrefs.SetInt("isCustomNameSet", 1);
                PlayerPrefs.Save();

                playerNamePanel.SetActive(false);
                lobbyBrowserPanel.SetActive(true);

                GetLobbiesTest(); // Lobi listesini getir
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError("Failed to set player name: " + ex.Message);
            }
        }
        public async void GetLobbiesTest()
        {
            ClearContainer();
            _existingLobbyIds.Clear();

            try
            {
                QueryLobbiesOptions options = new();
                Debug.LogWarning("QueryLobbiesTest");
                options.Count = 25;

                options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: "open"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

                options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

                QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
                Debug.LogWarning("Get Lobbies Done! Count::" + lobbies.Results.Count);

                foreach (Lobby bulunanLobby in lobbies.Results)
                {
                    if (_existingLobbyIds.Contains(bulunanLobby.Id)) continue;

                    Debug.Log("Lobby Name: " + bulunanLobby.Name + "\n" +
                    "Time for Created Lobby: " + bulunanLobby.Created + "\n" +
                    "Lobby Code: " + bulunanLobby.LobbyCode);

                    _existingLobbyIds.Add(bulunanLobby.Id);
                    CreateLobbyRow(bulunanLobby);
                }
            }
            catch (LobbyServiceException e)
            {

                Debug.Log(e);
            }
        }
        private void CreateLobbyRow(Lobby lobby)
        {
            GameObject row = Instantiate(lobbyRowPrefab, rowContainer.transform);
            row.name = lobby.Name;

            row.transform.Find("LobbyNameText").GetComponent<TextMeshProUGUI>().text = lobby.Name;
            row.transform.Find("OwnerText").GetComponent<TextMeshProUGUI>().text = lobby.Players.Find(p => p.Id == lobby.HostId)?.Data["playerName"].Value ?? "Unknown";
            row.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = lobby.Players.Count + "/" + lobby.MaxPlayers;

            var rectTransform = row.GetComponent<RectTransform>();
            rectTransform.SetParent(rowContainer.transform);

            Button joinButton = row.transform.Find("JoinButton").GetComponent<Button>();
            joinButton.onClick.AddListener(() => Lobby_OnClick(lobby));
        }
        public void Lobby_OnClick(Lobby lobby)
        {
            try
            {
                Debug.Log("Clicked Lobby " + lobby.Name);

                // JoinLobbyWithLobbyId çağrılmadan önce kontrol:
                if (CurrentLobby.Instance == null)
                {
                    Debug.LogError("CurrentLobby bulunamadi. LobbyManager objesi eksik olabilir.");
                }

                GetComponent<JoinLobby>().JoinLobbyWithLobbyId(lobby.Id);
            }
            catch (LobbyServiceException ex) when
                (ex.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                GetLobbiesTest();
            }
        }
        private void ClearContainer()
        {
            if (rowContainer != null)
            {
                for (int i = rowContainer.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(rowContainer.transform.GetChild(i).gameObject);
                }
            }
        }
    }
}
