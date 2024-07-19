using UnityEngine;
using UnityEditor;
using System.IO;

public class ColorTextureGenerator : EditorWindow
{
    private string hexColor = "#FF0000"; // Color rojo por defecto
    private string textureName = "NewTexture";
    private string texturePath = "Assets/";

    [MenuItem("Tools/Color Texture Generator")]
    public static void ShowWindow()
    {
        GetWindow<ColorTextureGenerator>("Color Texture Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Color Texture Generator", EditorStyles.boldLabel);

        hexColor = EditorGUILayout.TextField("Hex Color:", hexColor);
        textureName = EditorGUILayout.TextField("Texture Name:", textureName);
        texturePath = EditorGUILayout.TextField("Save Path:", texturePath);

        if (GUILayout.Button("Generate Texture"))
        {
            GenerateTexture();
        }
    }

    private void GenerateTexture()
    {
        if (!ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            EditorUtility.DisplayDialog("Invalid Color", "The provided hex color code is invalid.", "OK");
            return;
        }

        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        byte[] pngData = texture.EncodeToPNG();
        string filePath = Path.Combine(texturePath, textureName + ".png");

        File.WriteAllBytes(filePath, pngData);
        AssetDatabase.Refresh();

        //EditorUtility.DisplayDialog("Success", "Texture created successfully!", "OK");
    }
}
