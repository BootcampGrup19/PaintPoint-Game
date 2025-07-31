using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

namespace Unity.BizimKodlar
{
    public class GameManager : MonoBehaviour
    {
        public TextMeshProUGUI timerText, countTxt; // UI'daki TMP Text referans�
        public float remainingTime = 90f; // 1:30 -> 90 saniye

        private bool timerRunning = false;

        [SerializeField]
        private GameObject countImgs;


        private void Start()
        {
            countTxt.text = "3";
            countImgs.GetComponent<RectTransform>().localScale = Vector3.zero;
            countImgs.GetComponent<CanvasGroup>().alpha = 0;
            StartCoroutine(CountCoroutine());
        }

        IEnumerator CountCoroutine()
        {

            countImgs.GetComponent<RectTransform>().DOScale(.27f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(1f, .5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(1f);

            countImgs.GetComponent<RectTransform>().DOScale(0f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(0f, .5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(.5f);

            countTxt.text = "2";
            countImgs.GetComponent<RectTransform>().DOScale(.27f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(1f, .5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(1f);

            countImgs.GetComponent<RectTransform>().DOScale(0f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(0f, .5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(.5f);

            countTxt.text = "1";
            countImgs.GetComponent<RectTransform>().DOScale(.27f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(1f, .5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(1f);

            countImgs.GetComponent<RectTransform>().DOScale(0f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(0f, .5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(.5f);

            countTxt.text = "START";
            countImgs.GetComponent<RectTransform>().DOScale(.27f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(1f, .5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(1f);

            countImgs.GetComponent<RectTransform>().DOScale(0f, .5f).SetEase(Ease.OutBack);
            countImgs.GetComponent<CanvasGroup>().DOFade(0f, .5f).SetEase(Ease.OutBack);

            timerRunning = true;
        }

        void Update()
        {
            if (!timerRunning)
                return;

            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                UpdateTimerDisplay(remainingTime);
            }
            else
            {
                remainingTime = 0;
                timerRunning = false;
                UpdateTimerDisplay(0);
                TimerFinished();
            }
        }

        void UpdateTimerDisplay(float timeToDisplay)
        {
            int minutes = Mathf.FloorToInt(timeToDisplay / 60);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }

        void TimerFinished()
        {
            Debug.Log("S�re bitti!");
            // Buraya s�re bitince ne olacak onu yaz
        }
    }
}
