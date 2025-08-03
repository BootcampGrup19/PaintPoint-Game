using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class PaintWinChecker : MonoBehaviour
    {
        public Paintable paintable; // inspector'dan atanacak
        public float winThresholdPercent = 50f;
        bool hasChecked = false;

        public float totalPaintedPercent = 0f;

        public float [] valueArray = new float[4];

        public void CheckPaintWinCondition()
        {
            if (hasChecked) return; // bir kere kontrol et

            var colorRatios = PaintManager.instance.CalculatePaintedPercentageByColor(paintable);

            int counter = 0;
            foreach (var kvp in colorRatios)
            {
                totalPaintedPercent += kvp.Value;
                valueArray[counter++] = kvp.Value;
            }

            Debug.Log($"[PaintWinChecker] Toplam boyanma oran�: {totalPaintedPercent}%");

            hasChecked = true;
            var flowManager = FindObjectOfType<Unity.FPS.Game.GameFlowManager>();


            if (totalPaintedPercent >= winThresholdPercent)
            {
                // GameFlowManager'a EndGame(true) �a�r�s� g�nder
                if (flowManager != null)
                {
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
                flowManager.EndGame(false);
                Debug.Log("[PaintWinChecker] Kazanma �artlar� sa�lanamad�.");
            }
        }
    }
}
