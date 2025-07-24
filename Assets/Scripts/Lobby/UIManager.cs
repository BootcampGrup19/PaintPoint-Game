using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject lobbyRoomPanel;
    public GameObject characterCustomizationPanel;
    public Camera mainCamera;
    public Camera previewCamera;

    // Main Camera eski pozisyon/rotasyonu
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private int originalCullingMask;

    public void OnChangeCharacterClicked()
    {
        // Panelleri değiştir
        lobbyRoomPanel.SetActive(false);
        characterCustomizationPanel.SetActive(true);

        // Kamera ayarlarını sakla
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;
        originalCullingMask = mainCamera.cullingMask;

        // Yeni kamera pozisyonu ve rotasyonu
        mainCamera.transform.position = new Vector3(0.5f, 0.3f, 0.5f);
        mainCamera.transform.rotation = Quaternion.Euler(0f, 17f, 0f);

        // Culling Mask'i Everything yap
        mainCamera.cullingMask = ~0;

        // PreviewCamera devre dışı bırakılabilir veya gerekirse başka bir şey yapılabilir
        if (previewCamera != null)
        {
            previewCamera.gameObject.SetActive(false);
        }
    }
    public void OnBackToLobbyClicked()
    {
        // Panelleri yönet
        characterCustomizationPanel.SetActive(false);
        lobbyRoomPanel.SetActive(true);

        // Kamera eski konuma ve rotasyona dönsün
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;

        // Eski culling mask'e dön
        mainCamera.cullingMask = originalCullingMask;

        // Preview Camera'ı tekrar aç (isteğe bağlı)
        if (previewCamera != null)
        {
            previewCamera.gameObject.SetActive(true);
        }
    }
}
