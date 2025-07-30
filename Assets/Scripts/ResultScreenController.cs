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
        string winner = PlayerPrefs.GetString("WinnerTeam", "Takým A");
        string loser = PlayerPrefs.GetString("LoserTeam", "Takým B");

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
