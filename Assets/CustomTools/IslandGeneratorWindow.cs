using UnityEditor;
using UnityEngine;

// Main editor window for the Island Generator tool
public class IslandGeneratorWindow : EditorWindow
{
    private IslandGeneratorSettings settings = new IslandGeneratorSettings();
    private IslandGenerator islandGenerator;

    [MenuItem("Tools/Island Generator")]
    public static void ShowWindow()
    {
        GetWindow<IslandGeneratorWindow>("Island Generator");
    }

    private void OnEnable()
    {
        islandGenerator = new IslandGenerator();
    }

    private void OnGUI()
    {
        GUILayout.Label("Island Generator", EditorStyles.boldLabel);

        settings.terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", settings.terrain, typeof(Terrain), true);
        settings.scale = EditorGUILayout.FloatField("Scale", settings.scale);
        settings.islandRadius = EditorGUILayout.FloatField("Island Radius", settings.islandRadius);
        settings.heightMultiplier = EditorGUILayout.FloatField("Height Multiplier", settings.heightMultiplier);
        settings.mountainFactor = EditorGUILayout.FloatField("Mountain Factor", settings.mountainFactor);
        settings.erosionAmount = EditorGUILayout.FloatField("Erosion Amount", settings.erosionAmount);
        settings.minNoiseOffset = EditorGUILayout.FloatField("Min Noise Offset", settings.minNoiseOffset);
        settings.maxNoiseOffset = EditorGUILayout.FloatField("Max Noise Offset", settings.maxNoiseOffset);
        settings.baseLayer = (TerrainLayer)EditorGUILayout.ObjectField("Base Layer", settings.baseLayer, typeof(TerrainLayer), true);

        settings.noiseType = (NoiseType)EditorGUILayout.EnumPopup("Noise Type", settings.noiseType);
        settings.terrainType = (TerrainType)EditorGUILayout.EnumPopup("Terrain Type", settings.terrainType);

        if (GUILayout.Button("Generate Terrain"))
        {
            islandGenerator.Generate(settings);
        }
    }
}

// Enumeration for different types of noise
public enum NoiseType
{
    Perlin,
    Simplex
}

// Enumeration for different types of terrain
public enum TerrainType
{
    Island,
    Plains,
    Mountains,
    Plateau
}

// Holds settings for the island generation
[System.Serializable]
public class IslandGeneratorSettings
{
    public Terrain terrain;
    public float scale = 7f;
    public float islandRadius = 450f;
    public float heightMultiplier = 1f;
    public float mountainFactor = 1f;
    public float erosionAmount = 0.1f;
    public float minNoiseOffset = -100f;
    public float maxNoiseOffset = 100f;
    public TerrainLayer baseLayer;
    public NoiseType noiseType = NoiseType.Perlin;
    public TerrainType terrainType = TerrainType.Island;
}

// Generates the island terrain
public class IslandGenerator
{
    private float noiseOffset;

    public void Generate(IslandGeneratorSettings settings)
    {
        if (settings.terrain == null)
        {
            Debug.LogError("No Terrain assigned!");
            return;
        }

        noiseOffset = Random.Range(settings.minNoiseOffset, settings.maxNoiseOffset);

        TerrainData terrainData = settings.terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        float[,] heights = GenerateHeights(settings, width, height);

        terrainData.SetHeights(0, 0, heights);
        ApplyBaseLayer(settings);
    }

    private float[,] GenerateHeights(IslandGeneratorSettings settings, int width, int height)
    {
        float[,] heights = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = ((float)x / width + noiseOffset) * settings.scale;
                float yCoord = ((float)y / height + noiseOffset) * settings.scale;
                float noise = GenerateNoise(settings.noiseType, xCoord, yCoord);

                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));
                float islandFactor = Mathf.Clamp01((settings.islandRadius - dist) / settings.islandRadius);
                float mountain = Mathf.Pow(islandFactor, settings.mountainFactor);
                float baseHeight = noise * islandFactor * settings.heightMultiplier;

                heights[x, y] = Mathf.Clamp01(ApplyTerrainType(settings, baseHeight, islandFactor, mountain));
            }
        }

        return heights;
    }

    private float GenerateNoise(NoiseType noiseType, float xCoord, float yCoord)
    {
        switch (noiseType)
        {
            case NoiseType.Perlin:
                return Mathf.PerlinNoise(xCoord, yCoord);
            case NoiseType.Simplex:
                return SimplexNoise.Generate(xCoord, yCoord);
            default:
                return Mathf.PerlinNoise(xCoord, yCoord);
        }
    }

    private float ApplyTerrainType(IslandGeneratorSettings settings, float baseHeight, float islandFactor, float mountain)
    {
        switch (settings.terrainType)
        {
            case TerrainType.Island:
                return baseHeight - settings.erosionAmount * mountain;
            case TerrainType.Plains:
                return baseHeight * 0.5f;
            case TerrainType.Mountains:
                return baseHeight * 1.5f * mountain;
            case TerrainType.Plateau:
                return baseHeight * mountain + 0.1f;
            default:
                return baseHeight - settings.erosionAmount * mountain;
        }
    }

    private void ApplyBaseLayer(IslandGeneratorSettings settings)
    {
        if (settings.baseLayer == null)
        {
            Debug.LogError("Base Layer not assigned!");
            return;
        }

        TerrainData terrainData = settings.terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        TerrainLayer[] terrainLayers = new TerrainLayer[] { settings.baseLayer };
        terrainData.terrainLayers = terrainLayers;

        float[,,] splatmapData = new float[width, height, 1];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                splatmapData[x, y, 0] = 1f;
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}

// Simplex noise generation class
public static class SimplexNoise
{
    // Add your implementation of the Simplex noise algorithm here
    public static float Generate(float x, float y)
    {
        // This is a placeholder, replace with actual Simplex noise generation code
        return Mathf.PerlinNoise(x, y);
    }
}
