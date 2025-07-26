using UnityEngine;

public class CharacterPreviewLoader : MonoBehaviour
{
    public Transform previewRoot; // RenderTexture için PreviewCamera’nın baktığı pivot
    private GameObject currentModel;

    public void LoadPreview(CharacterCustomizationData data)
    {
        if (currentModel)
            Destroy(currentModel);

        // Karakter preview objesini oluştur
        currentModel = new GameObject("PreviewCharacter");
        currentModel.transform.SetParent(previewRoot, false);

        // Layer ayarı
        SetLayerRecursively(currentModel.transform, LayerMask.NameToLayer("CharacterPreview"));

        // Karakter özelleştirme bileşeni ekle
        var customizer = currentModel.AddComponent<NetworkCharacterCustomizer>();
        customizer.characterRoot = currentModel.transform;
        customizer.ApplyCustomization(data);
    }

    private void SetLayerRecursively(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, layer);
        }
    }
}
