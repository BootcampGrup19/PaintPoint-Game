using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;

    private bool isPanelOn;

    public void ChangePanelVisibility()
    {
        if (isPanelOn)
        {
            isPanelOn = false;

            StartCoroutine(ClosePanel());
           
        }
        else
        {
            isPanelOn = true;
            panel.gameObject.SetActive(true);

            panel.GetComponent<RectTransform>().DOScale(1.3f, .5f).SetEase(Ease.OutBack);
            panel.GetComponent<CanvasGroup>().DOFade(1f, .5f).SetEase(Ease.OutBack);
        }
    }

    IEnumerator ClosePanel()
    {
        panel.GetComponent<RectTransform>().DOScale(0, .5f).SetEase(Ease.OutBack);
        panel.GetComponent<CanvasGroup>().DOFade(0f, .5f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(.5f);
        panel.gameObject.SetActive(false);

    }

    public void QuitGame()
  {
        Application.Quit();
  }
}
