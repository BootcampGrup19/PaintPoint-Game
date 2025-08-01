using UnityEngine;

public class RemoveAllColliders : MonoBehaviour
{
    void Start()
    {
        Collider[] allColliders = FindObjectsOfType<Collider>();

        foreach (Collider col in allColliders)
        {
            Destroy(col);
        }

        Debug.Log($"Toplam {allColliders.Length} collider silindi.");
    }
}
