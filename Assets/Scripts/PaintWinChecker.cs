using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class PaintWinChecker : MonoBehaviour
    {
        public Paintable paintable; // inspector'dan atanacak
        public float winThresholdPercent = 50f;
        ResultManager resultManager;

        bool hasChecked = false;

        private void Awake()
        {
            resultManager = FindFirstObjectByType<ResultManager>();
        }


        public void CheckPaintWinCondition()
        {
            if (hasChecked) return; // bir kere kontrol et

            var colorRatios = PaintManager.instance.CalculatePaintedPercentageByColor(paintable);

            float totalPaintedPercent = 0f;
            foreach (var kvp in colorRatios)
            {
                totalPaintedPercent += kvp.Value;
            }

            Debug.Log($"[PaintWinChecker] Toplam boyanma oran�: {totalPaintedPercent}%");

            hasChecked = true;
            var flowManager = FindObjectOfType<Unity.FPS.Game.GameFlowManager>();


            if (totalPaintedPercent >= winThresholdPercent)
            {
                // GameFlowManager'a EndGame(true) �a�r�s� g�nder
                if (flowManager != null)
                {
                    resultManager.SetResults(totalPaintedPercent, colorRatios["Red"], colorRatios["Blue"], colorRatios["Green"], colorRatios["Yellow"]);
                    flowManager.EndGame(true);

                    Debug.Log("[PaintWinChecker] Oyunu kazand�n!");
                }
                else
                {
                    Debug.LogWarning("GameFlowManager bulunamad�!");
                }
            }
            else
            {
                resultManager.SetResults(totalPaintedPercent, colorRatios["Red"], colorRatios["Blue"], colorRatios["Green"], colorRatios["Yellow"]);
                flowManager.EndGame(false);
                Debug.Log("[PaintWinChecker] Kazanma �artlar� sa�lanamad�.");
            }
        }
    }
}
