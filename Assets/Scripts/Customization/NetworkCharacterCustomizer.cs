using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace Unity.BizimKodlar
{
    public class NetworkCharacterCustomizer : NetworkBehaviour
    {
        public Transform characterRoot; // Parçaların instantiate edileceği yer

        public NetworkVariable<FixedString4096Bytes> customizationData = new NetworkVariable<FixedString4096Bytes>(
            writePerm: NetworkVariableWritePermission.Owner);

        public void SaveCustomization(CharacterCustomizationData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("CharacterData", json);
            PlayerPrefs.Save();

            if (IsOwner)
            {
                customizationData.Value = json; // Otomatik sync olacak
            }
        }
        public override void OnNetworkSpawn()
        {
            Debug.Log($"[{OwnerClientId}] OnNetworkSpawn called. IsOwner: {IsOwner}, customizationData: {customizationData.Value}");
            customizationData.OnValueChanged += OnCustomizationChanged;

            // SADECE OWNER İSE VE LOCAL PLAYER PREFS VARSA DEĞERİ ATA
            if (IsOwner && PlayerPrefs.HasKey("CharacterData"))
            {
                string json = PlayerPrefs.GetString("CharacterData");
                customizationData.Value = json;
                Debug.Log($"[{OwnerClientId}] Loaded customization from PlayerPrefs: {json}");
            }

            // Tüm oyuncular için (host ve client) ilk değer zaten sync edildiyse onu uygula
            if (!string.IsNullOrEmpty(customizationData.Value.ToString()))
            {
                Debug.Log($"[{OwnerClientId}] Applying customization from synced value");
                CharacterCustomizationData data = JsonUtility.FromJson<CharacterCustomizationData>(customizationData.Value.ToString());
                ApplyCustomization(data);
            }
        }
        private void OnCustomizationChanged(FixedString4096Bytes oldValue, FixedString4096Bytes newValue)
        {
            if (!string.IsNullOrEmpty(newValue.ToString()))
            {
                CharacterCustomizationData data = JsonUtility.FromJson<CharacterCustomizationData>(newValue.ToString());
                ApplyCustomization(data);
            }
        }
        public void ApplyCustomization(CharacterCustomizationData data)
        {
            ClearExistingParts();

            if (!string.IsNullOrEmpty(data.faceName))
                LoadPart("Body/" + data.bodyName);

            if (!string.IsNullOrEmpty(data.faceName))
                LoadPart("Faces/" + data.faceName);

            if (!string.IsNullOrEmpty(data.hairName) && (data.hairName == "Hairstyle_Male_Single_006"))
            {
                LoadPart("Hairstyle Single/" + data.hairName);
            }
            else if (!string.IsNullOrEmpty(data.hairName))
            {
                LoadPart("Hairstyle/" + data.hairName);
            }

            if (!string.IsNullOrEmpty(data.hatName) && (data.hatName == "Hat_010"))
            {
                LoadPart("Hat/" + data.hatName);
            }
            else if (!string.IsNullOrEmpty(data.hairName))
            {
                LoadPart("Hat Single/" + data.hatName);
            }

            if (!string.IsNullOrEmpty(data.accessoriesName))
                LoadPart("Face Accessories/" + data.accessoriesName);

            if (!string.IsNullOrEmpty(data.glassesName))
                LoadPart("Glasses/" + data.glassesName);

            if (!string.IsNullOrEmpty(data.outerwearName) && (data.outerwearName == "Outfit_010"))
            {
                LoadPart("Outfit/" + data.outerwearName);
            }
            else if (!string.IsNullOrEmpty(data.outerwearName) && (data.outerwearName == "Mascot_002"))
            {
                LoadPart("Mascots/" + data.outerwearName);
            }
            else if (!string.IsNullOrEmpty(data.outerwearName))
            {
                LoadPart("Outwear/" + data.outerwearName);
            }

            if (!string.IsNullOrEmpty(data.pantsName) && (data.pantsName == "Shorts_003"))
            {
                LoadPart("Shorts/" + data.pantsName);
            }
            else if (!string.IsNullOrEmpty(data.pantsName))
            {
                LoadPart("Pants/" + data.pantsName);
            }

            if (!string.IsNullOrEmpty(data.shoesName) && (data.shoesName == "Socks_008"))
            {
                LoadPart("Socks/" + data.shoesName);
            }
            else if (!string.IsNullOrEmpty(data.shoesName))
            {
                LoadPart("Shoes/" + data.shoesName);
            }

            if (!string.IsNullOrEmpty(data.glovesName))
                LoadPart("Gloves/" + data.glovesName);

            if (!string.IsNullOrEmpty(data.costumeName))
                LoadPart("Costumes/" + data.costumeName);
        }

        private void LoadPart(string resourcePath)
        {
            GameObject partPrefab = Resources.Load<GameObject>(resourcePath);
            if (partPrefab != null)
                Instantiate(partPrefab, characterRoot);
            else
                Debug.LogWarning("Prefab not found: " + resourcePath);
        }

        private void ClearExistingParts()
        {
            foreach (Transform child in characterRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}