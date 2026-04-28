using System.IO;
using UnityEditor;
using UnityEngine;

public static class GeneratePrefabThumbnails
{
    private const string OutputFolder = "Assets/Prefab/Furniture/Thumbnails";

    [MenuItem("Room Builder/Generate Thumbnails For Selected Prefabs")]
    public static void GenerateForSelectedPrefabs()
    {
        EnsureOutputFolderExists();

        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("Select one or more prefab assets in the Project window first.");
            return;
        }

        int generatedCount = 0;

        foreach (Object selectedObject in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".prefab"))
            {
                Debug.LogWarning($"Skipped non-prefab asset: {selectedObject.name}");
                continue;
            }

            Texture2D preview = GetPreviewTexture(selectedObject);

            if (preview == null)
            {
                Debug.LogWarning($"Could not generate preview for: {selectedObject.name}");
                continue;
            }

            string safeName = MakeSafeFileName(selectedObject.name);
            string outputPath = $"{OutputFolder}/{safeName}_thumbnail.png";

            SaveTextureAsPng(preview, outputPath);
            ConfigureAsSprite(outputPath);

            generatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Generated {generatedCount} prefab thumbnails in: {OutputFolder}");
    }

    private static Texture2D GetPreviewTexture(Object asset)
    {
        Texture2D preview = null;

        // Unity previews can take a few frames to generate, so we try several times.
        for (int i = 0; i < 30; i++)
        {
            preview = AssetPreview.GetAssetPreview(asset);

            if (preview != null)
                break;

            // Force the editor to update preview generation.
            System.Threading.Thread.Sleep(50);
        }

        if (preview == null)
            preview = AssetPreview.GetMiniThumbnail(asset);

        return preview;
    }

    private static void SaveTextureAsPng(Texture2D source, string outputPath)
    {
        Texture2D readableTexture = MakeReadableCopy(source);
        byte[] pngData = readableTexture.EncodeToPNG();

        File.WriteAllBytes(outputPath, pngData);

        Object.DestroyImmediate(readableTexture);
    }

    private static Texture2D MakeReadableCopy(Texture2D source)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB
        );

        Graphics.Blit(source, renderTexture);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
        readableTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return readableTexture;
    }

    private static void ConfigureAsSprite(string assetPath)
    {
        AssetDatabase.ImportAsset(assetPath);

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;

        importer.SaveAndReimport();
    }

    private static void EnsureOutputFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_RoomBuilder"))
            AssetDatabase.CreateFolder("Assets", "_RoomBuilder");

        if (!AssetDatabase.IsValidFolder("Assets/_RoomBuilder/Generated"))
            AssetDatabase.CreateFolder("Assets/_RoomBuilder", "Generated");

        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/_RoomBuilder/Generated", "Thumbnails");
    }

    private static string MakeSafeFileName(string fileName)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalidChar, '_');

        return fileName;
    }
}