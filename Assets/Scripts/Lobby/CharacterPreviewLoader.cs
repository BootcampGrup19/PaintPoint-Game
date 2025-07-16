using UnityEngine;

public class CharacterPreviewLoader : MonoBehaviour
{
    public Transform previewRoot;          // PreviewCamera’nın baktığı boş pivot
    public string resourcePath = "Characters/";
    GameObject currentModel;

    public void LoadPreview(string modelName)
    {
        if (currentModel) Destroy(currentModel);
        var prefab = Resources.Load<GameObject>(resourcePath + modelName);
        currentModel = Instantiate(prefab, previewRoot);
        currentModel.layer = LayerMask.NameToLayer("CharacterPreview"); // Özel layer
    }
}
