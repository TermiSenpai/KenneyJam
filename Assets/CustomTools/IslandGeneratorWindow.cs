using UnityEditor;
using UnityEngine;

public class IslandGeneratorWindow : EditorWindow
{
    public Terrain terrain;
    public float scale = 7f;
    public float islandRadius = 450f;
    public float heightMultiplier = 1f;
    public float mountainFactor = 1f;
    public float erosionAmount = 0.1f;
    public float minNoiseOffset = -100f;
    public float maxNoiseOffset = 100f;

    // Parámetro para la capa base
    public TerrainLayer baseLayer;

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

        // Campo para la capa base
        baseLayer = (TerrainLayer)EditorGUILayout.ObjectField("Base Layer", baseLayer, typeof(TerrainLayer), true);

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

        // Generar un desplazamiento de ruido aleatorio
        noiseOffset = Random.Range(minNoiseOffset, maxNoiseOffset);

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        float[,] heights = new float[width, height];

        // Generar ruido Perlin
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = ((float)x / width + noiseOffset) * scale;
                float yCoord = ((float)y / height + noiseOffset) * scale;

                // Generar valor de ruido
                float noise = Mathf.PerlinNoise(xCoord, yCoord);

                // Calcular distancia desde el centro
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));

                // Aplicar distancia para crear la forma de la isla
                float islandFactor = Mathf.Clamp01((islandRadius - dist) / islandRadius);

                // Crear efecto de montaña
                float mountain = Mathf.Pow(islandFactor, mountainFactor);

                // Establecer valores del heightmap con efecto de erosión
                float baseHeight = noise * islandFactor * heightMultiplier;
                heights[x, y] = Mathf.Clamp01(baseHeight - erosionAmount) * mountain;
            }
        }

        // Aplicar alturas al terreno
        terrainData.SetHeights(0, 0, heights);

        // Aplicar capa base
        ApplyBaseLayer();
    }

    void ApplyBaseLayer()
    {
        if (baseLayer == null)
        {
            Debug.LogError("Base Layer not assigned!");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        // Crear y asignar la capa base
        TerrainLayer[] terrainLayers = new TerrainLayer[] { baseLayer };
        terrainData.terrainLayers = terrainLayers;

        float[,,] splatmapData = new float[width, height, 1];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                splatmapData[x, y, 0] = 1f; // Aplicar la capa base completa
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}
