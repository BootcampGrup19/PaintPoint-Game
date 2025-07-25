using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class PrefabThumbnailGenerator
{
    [MenuItem("Tools/Generate Character Thumbnails")]
    public static void GenerateThumbnails()
    {
        string prefabRoot = "Assets/ithappy/Creative_Characters_FREE/Prefabs";
        string outputFolder = "Assets/Resources/CharacterIcons";

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabRoot });

        foreach (string guid in prefabGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
                continue;

            Texture2D preview = null;

            // ❗️En fazla 50 frame bekle (önizlemenin hazır olmasını)
            for (int i = 0; i < 50; i++)
            {
                preview = AssetPreview.GetAssetPreview(prefab);

                if (preview != null && !AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()))
                    break;

                System.Threading.Thread.Sleep(100); // 100ms bekle
            }

            if (preview == null)
            {
                Debug.LogWarning($"❌ Preview not ready for {prefab.name}, skipping...");
                continue;
            }

            // ✅ Yeni: Şeffaf arka planı beyazla birleştir
            Texture2D whiteBgTexture = new Texture2D(preview.width, preview.height, TextureFormat.RGBA32, false);

            for (int y = 0; y < preview.height; y++)
            {
                for (int x = 0; x < preview.width; x++)
                {
                    Color pixel = preview.GetPixel(x, y);
                    Color bg = Color.white;
                    Color final = Color.Lerp(bg, pixel, pixel.a);
                    whiteBgTexture.SetPixel(x, y, final);
                }
            }
            whiteBgTexture.Apply();

            string safeName = prefab.name.Replace(" ", "_");

            byte[] pngData = whiteBgTexture.EncodeToPNG();
            string filePath = Path.Combine(outputFolder, safeName + ".png");

            File.WriteAllBytes(filePath, pngData);
            Debug.Log($"✅ Saved white-background thumbnail for {prefab.name} to {filePath}");
        }

        AssetDatabase.Refresh();
        Debug.Log("🎉 Tüm önizlemeler tamamlandı.");
    }
}
