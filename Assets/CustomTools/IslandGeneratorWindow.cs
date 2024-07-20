using UnityEditor;
using UnityEngine;

// Main editor window for the Island Generator tool
public class IslandGeneratorWindow : EditorWindow
{
    private IslandGeneratorSettings settings = new IslandGeneratorSettings();
    private IslandGenerator islandGenerator;

    private bool showGeneralSettings = true;
    private bool showNoiseSettings = true;
    private bool showTerrainSettings = true;

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

        showGeneralSettings = EditorGUILayout.Foldout(showGeneralSettings, "General Settings");
        if (showGeneralSettings)
        {
            settings.terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", settings.terrain, typeof(Terrain), true);
            settings.scale = EditorGUILayout.FloatField("Scale", settings.scale);
            settings.islandRadius = EditorGUILayout.FloatField("Island Radius", settings.islandRadius);
            settings.heightMultiplier = EditorGUILayout.FloatField("Height Multiplier", settings.heightMultiplier);
        }

        showNoiseSettings = EditorGUILayout.Foldout(showNoiseSettings, "Noise Settings");
        if (showNoiseSettings)
        {
            settings.mountainFactor = EditorGUILayout.FloatField("Mountain Factor", settings.mountainFactor);
            settings.erosionAmount = EditorGUILayout.FloatField("Erosion Amount", settings.erosionAmount);
            settings.minNoiseOffset = EditorGUILayout.FloatField("Min Noise Offset", settings.minNoiseOffset);
            settings.maxNoiseOffset = EditorGUILayout.FloatField("Max Noise Offset", settings.maxNoiseOffset);
            settings.noiseType = (NoiseType)EditorGUILayout.EnumPopup("Noise Type", settings.noiseType);
        }

        showTerrainSettings = EditorGUILayout.Foldout(showTerrainSettings, "Terrain Settings");
        if (showTerrainSettings)
        {
            settings.baseLayer = (TerrainLayer)EditorGUILayout.ObjectField("Base Layer", settings.baseLayer, typeof(TerrainLayer), true);
            settings.terrainType = (TerrainType)EditorGUILayout.EnumPopup("Terrain Type", settings.terrainType);
        }

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
    private static int[] perm = new int[512];
    private static int[] permOriginal = new int[]
    {
        151, 160, 137, 91, 90, 15,
        131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30,
        69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234,
        75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
        33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111,
        229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40,
        244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132,
        187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
        164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
        5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227,
        47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248,
        152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
        112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144,
        12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49,
        192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
        50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29,
        24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

    static SimplexNoise()
    {
        for (int i = 0; i < 512; i++)
        {
            perm[i] = permOriginal[i % 256];
        }
    }

    public static float Generate(float x, float y)
    {
        float s = (x + y) * 0.5f * (Mathf.Sqrt(3.0f) - 1.0f);
        int i = Mathf.FloorToInt(x + s);
        int j = Mathf.FloorToInt(y + s);
        float t = (i + j) * (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = x - X0;
        float y0 = y - Y0;

        int i1, j1;
        if (x0 > y0)
        {
            i1 = 1;
            j1 = 0;
        }
        else
        {
            i1 = 0;
            j1 = 1;
        }

        float x1 = x0 - i1 + (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;
        float y1 = y0 - j1 + (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;
        float x2 = x0 - 1.0f + 2.0f * (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;
        float y2 = y0 - 1.0f + 2.0f * (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;

        int ii = i & 255;
        int jj = j & 255;
        float gi0 = Grad(perm[ii + perm[jj]], x0, y0);
        float gi1 = Grad(perm[ii + i1 + perm[jj + j1]], x1, y1);
        float gi2 = Grad(perm[ii + 1 + perm[jj + 1]], x2, y2);

        float n0 = 0.5f - x0 * x0 - y0 * y0;
        float n1 = 0.5f - x1 * x1 - y1 * y1;
        float n2 = 0.5f - x2 * x2 - y2 * y2;

        if (n0 < 0) n0 = 0.0f; else n0 *= n0 * n0 * n0 * gi0;
        if (n1 < 0) n1 = 0.0f; else n1 *= n1 * n1 * n1 * gi1;
        if (n2 < 0) n2 = 0.0f; else n2 *= n2 * n2 * n2 * gi2;

        return 70.0f * (n0 + n1 + n2);
    }

    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }
}
