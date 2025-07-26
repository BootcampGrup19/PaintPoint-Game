using UnityEngine;
using Unity.Netcode;

public class NetworkCharacterCustomizer : NetworkBehaviour
{
    public Transform characterRoot; // Parçaların instantiate edileceği yer

    private CharacterCustomizationData customizationData;

    public void SaveCustomization(CharacterCustomizationData data)
    {
        customizationData = data;

        if (IsOwner)
        {
            string json = JsonUtility.ToJson(data);
            SubmitCustomizationServerRpc(json);
        }
    }

    [ServerRpc]
    private void SubmitCustomizationServerRpc(string json)
    {
        BroadcastCustomizationClientRpc(json);
    }

    [ClientRpc]
    private void BroadcastCustomizationClientRpc(string json)
    {
        CharacterCustomizationData data = JsonUtility.FromJson<CharacterCustomizationData>(json);
        ApplyCustomization(data);
    }

    public void ApplyCustomization(CharacterCustomizationData data)
    {
        ClearExistingParts();

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
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            if (PlayerPrefs.HasKey("CharacterData"))
            {
                string json = PlayerPrefs.GetString("CharacterData");
                CharacterCustomizationData data = JsonUtility.FromJson<CharacterCustomizationData>(json);

                SaveCustomization(data); // Bu hem kendine uygular hem sunucuya yollar
            }
        }
    }
}