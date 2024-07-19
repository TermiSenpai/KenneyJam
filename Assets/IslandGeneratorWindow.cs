using UnityEditor;
using UnityEngine;

public class IslandGeneratorWindow : EditorWindow
{
    public Terrain terrain;
    public float scale = 20f;
    public float islandRadius = 50f;
    public float heightMultiplier = 10f;
    public float mountainFactor = 1f;
    public float erosionAmount = 0.1f;
    public float minNoiseOffset = 0f;
    public float maxNoiseOffset = 1f;

    private float noiseOffset;

    [MenuItem("Tools/Island Generator")]
    public static void ShowWindow()
    {
        GetWindow<IslandGeneratorWindow>("Island Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Island Generator", EditorStyles.boldLabel);

        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        scale = EditorGUILayout.FloatField("Scale", scale);
        islandRadius = EditorGUILayout.FloatField("Island Radius", islandRadius);
        heightMultiplier = EditorGUILayout.FloatField("Height Multiplier", heightMultiplier);
        mountainFactor = EditorGUILayout.FloatField("Mountain Factor", mountainFactor);
        erosionAmount = EditorGUILayout.FloatField("Erosion Amount", erosionAmount);
        minNoiseOffset = EditorGUILayout.FloatField("Min Noise Offset", minNoiseOffset);
        maxNoiseOffset = EditorGUILayout.FloatField("Max Noise Offset", maxNoiseOffset);

        if (GUILayout.Button("Generate Island"))
        {
            GenerateIsland();
        }
    }

    void GenerateIsland()
    {
        if (terrain == null)
        {
            Debug.LogError("No Terrain assigned!");
            return;
        }

        // Generate a random noise offset
        noiseOffset = Random.Range(minNoiseOffset, maxNoiseOffset);

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        float[,] heights = new float[width, height];

        // Generate Perlin noise
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = ((float)x / width + noiseOffset) * scale;
                float yCoord = ((float)y / height + noiseOffset) * scale;

                // Generate noise value
                float noise = Mathf.PerlinNoise(xCoord, yCoord);

                // Calculate distance from center
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));

                // Apply distance to create island shape
                float islandFactor = Mathf.Clamp01((islandRadius - dist) / islandRadius);

                // Create mountain effect
                float mountain = Mathf.Pow(islandFactor, mountainFactor);

                // Set heightmap values with erosion effect
                float baseHeight = noise * islandFactor * heightMultiplier;
                heights[x, y] = Mathf.Clamp01(baseHeight - erosionAmount) * mountain;
            }
        }

        // Apply heights to terrain
        terrainData.SetHeights(0, 0, heights);
    }
}
