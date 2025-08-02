using UnityEngine;

public class RemoveAllColliders : MonoBehaviour
{
    void Start()
    {
        Collider[] allColliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);

        foreach (Collider col in allColliders)
        {
            Destroy(col);
        }

        Debug.Log($"Toplam {allColliders.Length} collider silindi.");
    }
}
