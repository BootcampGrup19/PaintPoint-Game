using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace Unity.BizimKodlar
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject lobbyRoomPanel;
        public GameObject characterCustomizationPanel;

        [Header("Cameras")]
        public Camera mainCamera;
        public Camera previewCamera;

        [Header("Option UI")]
        public GameObject optionButtonPrefab;
        public Transform optionGrid;

        // Main Camera eski pozisyon/rotasyonu
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        private int originalCullingMask;
        private PreviewCameraRotator characterPreview;

        // UI'de gösterilecek asset görselleri
        [SerializeField] private Transform characterRoot;
        public List<CustomizationCategory> customizationCategories;
        private Dictionary<string, GameObject> equippedPrefabs = new Dictionary<string, GameObject>();

        CharacterCustomizationData data = new CharacterCustomizationData();
        //public GameObject myCharacter;

        [System.Serializable]
        public class CustomizationCategory
        {
            public string categoryName;
            public List<GameObject> availablePrefabs;
            public Button tabButton;
        }

        void Start()
        {
            characterPreview = GameObject.Find("RotatePivot").GetComponent<PreviewCameraRotator>();

            foreach (var category in customizationCategories)
            {
                if (category.tabButton != null)
                {
                    string capturedCategory = category.categoryName;
                    List<GameObject> capturedPrefabs = category.availablePrefabs;

                    category.tabButton.onClick.AddListener(() =>
                    {
                        PopulateOptions(capturedPrefabs, capturedCategory);
                    });
                }
            }

            data.bodyName = "Body_010";
        }
        public void OnChangeCharacterClicked()
        {
            // Panelleri değiştir
            characterPreview.inCustomization = false;
            lobbyRoomPanel.SetActive(false);
            characterCustomizationPanel.SetActive(true);

            // Kamera ayarlarını sakla
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
            originalCullingMask = mainCamera.cullingMask;

            // Yeni kamera pozisyonu ve rotasyonu
            mainCamera.transform.position = new Vector3(0.5f, 1.05f, -2.5f);
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
            characterPreview.inCustomization = true;
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
            // Karakter verisini burada kullanabilirsin
            Debug.Log("Karakter verisi kaydedildi:");
            Debug.Log(JsonUtility.ToJson(data, true));

            //myCharacter.GetComponent<NetworkCharacterCustomizer>().SaveCustomization(data);
            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (player != null)
            {
                player.GetComponent<NetworkCharacterCustomizer>().SaveCustomization(data);
            }

            // Eğer PlayerPrefs ile kaydetmek istiyorsan:
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("CharacterData", json);
            PlayerPrefs.Save();
        }
        void PopulateOptions(List<GameObject> prefabs, string category)
        {
            foreach (Transform child in optionGrid) Destroy(child.gameObject);
            StartCoroutine(GenerateRuntimePreviews(prefabs, category));
        }
        IEnumerator GenerateRuntimePreviews(List<GameObject> prefabs, string category)
        {
            // 🔘 NONE Butonu oluştur
            GameObject noneBtnObj = Instantiate(optionButtonPrefab, optionGrid);
            RawImage noneImg = noneBtnObj.GetComponentInChildren<RawImage>();
            Button noneBtn = noneBtnObj.GetComponentInChildren<Button>();

            // Arzu edersen kendin bir None görseli ekleyebilirsin (Resources altına koy)
            Texture2D noneTex = Resources.Load<Texture2D>("CharacterIcons/None");
            if (noneTex != null)
                noneImg.texture = noneTex;
            else
                noneImg.color = Color.gray; // Yedek plan

            noneBtn.onClick.AddListener(() => ApplyCustomization(null, category));

            foreach (var prefab in prefabs)
            {
                Texture2D tex = RuntimePreviewGenerator.GenerateModelPreview(
                    prefab.transform, 128, 128, true, true);
                RuntimePreviewGenerator.BackgroundColor = Color.clear;

                var btn = Instantiate(optionButtonPrefab, optionGrid);
                var img = btn.GetComponentInChildren<RawImage>();
                img.texture = tex;
                btn.GetComponentInChildren<Button>().onClick.AddListener(() => ApplyCustomization(prefab, category));

                yield return null;
            }
        }
        public void ApplyCustomization(GameObject selectedPrefab, string category)
        {
            // Eğer bu kategoriye ait daha önce yerleştirilmiş prefab varsa, onu yok et
            if (equippedPrefabs.ContainsKey(category) && equippedPrefabs[category] != null)
            {
                Destroy(equippedPrefabs[category]);
                equippedPrefabs[category] = null;
            }

            // Yeni prefab varsa instantiate et
            if (selectedPrefab != null)
            {
                GameObject newInstance = Instantiate(selectedPrefab, characterRoot);
                newInstance.transform.localPosition = Vector3.zero;
                newInstance.transform.localRotation = Quaternion.identity;
                newInstance.transform.localScale = Vector3.one;
                newInstance.gameObject.layer = 6;

                foreach (Transform child in newInstance.transform)
                {
                    child.gameObject.layer = 6;

                    if (category == "Costumes")
                    {
                        foreach (Transform child2 in child)
                        {
                            child2.gameObject.layer = 6;
                        }
                    }
                }

                equippedPrefabs[category] = newInstance;

                // Prefab ismini veriye yaz
                switch (category.ToLower())
                {
                    case "faces": data.faceName = selectedPrefab.name; break;
                    case "hair": data.hairName = selectedPrefab.name; break;
                    case "hat": data.hatName = selectedPrefab.name; break;
                    case "accessories": data.accessoriesName = selectedPrefab.name; break;
                    case "glasses": data.glassesName = selectedPrefab.name; break;
                    case "outerwear": data.outerwearName = selectedPrefab.name; break;
                    case "pants": data.pantsName = selectedPrefab.name; break;
                    case "shoes": data.shoesName = selectedPrefab.name; break;
                    case "gloves": data.glovesName = selectedPrefab.name; break;
                    case "costumes": data.costumeName = selectedPrefab.name; break;
                }
            }
            else
            {
                // Eğer "None" seçildiyse veriyi temizle
                switch (category.ToLower())
                {
                    case "faces": data.faceName = ""; break;
                    case "hair": data.hairName = ""; break;
                    case "hat": data.hatName = ""; break;
                    case "accessories": data.accessoriesName = ""; break;
                    case "glasses": data.glassesName = ""; break;
                    case "outerwear": data.outerwearName = ""; break;
                    case "pants": data.pantsName = ""; break;
                    case "shoes": data.shoesName = ""; break;
                    case "gloves": data.glovesName = ""; break;
                    case "costumes": data.costumeName = ""; break;
                }
            }
        }
        public void RandomizeCharacter()
        {
            foreach (var category in customizationCategories)
            {
                if (category.availablePrefabs.Count == 0) continue;

                int randomIndex = UnityEngine.Random.Range(0, category.availablePrefabs.Count + 1); // +1 for "none" possibility
                GameObject randomPrefab = randomIndex < category.availablePrefabs.Count
                    ? category.availablePrefabs[randomIndex]
                    : null;

                ApplyCustomization(randomPrefab, category.categoryName);
            }
        }
    }
}
