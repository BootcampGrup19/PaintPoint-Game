using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultScreenController : MonoBehaviour
{
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI loserText;

    void Start()
    {
        string winner = PlayerPrefs.GetString("WinnerTeam", "Tak�m A");
        string loser = PlayerPrefs.GetString("LoserTeam", "Tak�m B");

        winnerText.text = "Kazanan: " + winner;
        loserText.text = "Kaybeden: " + loser;
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene("LobbyBrowserScene"); 
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
    }
}
