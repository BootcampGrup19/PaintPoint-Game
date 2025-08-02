using UnityEngine;
using UnityEditor;

public class ColliderCleaner : MonoBehaviour
{
    [MenuItem("Tools/Clean All Colliders in Scene")]
    static void CleanColliders()
    {
        Collider[] colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Collider c in colliders)
        {
            Undo.DestroyObjectImmediate(c);
            count++;
        }

        Debug.Log($"Temizlenen Collider sayısı: {count}");
    }
}
