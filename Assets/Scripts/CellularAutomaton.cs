using UnityEngine;
using UnityEngine.UI;
using TMPro; // Для TextMeshProUGUI
using System.Collections.Generic;

public class CellularAutomaton : MonoBehaviour
{
    public int width = 20;
    public int height = 20;
    public int depth = 20;
    private bool[,,] grid;
    private bool[,,] nextGrid;

    public GameObject cellPrefab;
    private GameObject[,,] cellObjects;

    public RawImage mapImage;
    private Texture2D texture;

    // Для графика энтропии
    public LineRenderer entropyGraph;
    private List<float> entropyValues = new List<float>();
    public int maxGraphPoints = 50; // Максимальное количество точек на графике
    public float graphWidth = 10f;  // Ширина графика по X
    public float graphHeight = 2f;  // Высота графика по Y

    // Для UI
    public TextMeshProUGUI entropyText;

    void Start()
    {
        grid = new bool[width, height, depth];
        nextGrid = new bool[width, height, depth];
        InitializeGrid();
        cellObjects = new GameObject[width, height, depth];
        Update3DVisualization();
        texture = new Texture2D(width, depth);
        texture.filterMode = FilterMode.Point;
        mapImage.texture = texture;
        Update2DVisualization();

        // Настройка LineRenderer
        entropyGraph.positionCount = 0;
        entropyGraph.startWidth = 0.1f;
        entropyGraph.endWidth = 0.1f;
    }

    void Update2DVisualization()
    {
        if (texture == null || mapImage == null) return;
        int ySlice = height / 2;
        for (int x = 0; x < width; x++)
        for (int z = 0; z < depth; z++)
            texture.SetPixel(x, z, grid[x, ySlice, z] ? Color.white : Color.black);
        texture.Apply();
    }

    void Update3DVisualization()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        for (int z = 0; z < depth; z++)
        {
            if (grid[x, y, z] && cellObjects[x, y, z] == null)
            {
                cellObjects[x, y, z] = Instantiate(cellPrefab, new Vector3(x, y, z), Quaternion.identity);
            }
            else if (!grid[x, y, z] && cellObjects[x, y, z] != null)
            {
                Destroy(cellObjects[x, y, z]);
                cellObjects[x, y, z] = null;
            }
        }
    }

    void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        for (int z = 0; z < depth; z++)
            grid[x, y, z] = Random.value > 0.7f;
    }

    void UpdateGrid()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        for (int z = 0; z < depth; z++)
        {
            int neighbors = CountNeighbors(x, y, z);
            bool isAlive = grid[x, y, z];
            nextGrid[x, y, z] = isAlive ? (neighbors >= 5 && neighbors <= 7) : (neighbors == 6);
        }
        (grid, nextGrid) = (nextGrid, grid);
    }

    int CountNeighbors(int x, int y, int z)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        for (int dz = -1; dz <= 1; dz++)
        {
            if (dx == 0 && dy == 0 && dz == 0) continue;
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && nz >= 0 && nz < depth && grid[nx, ny, nz])
                count++;
        }
        return count;
    }

    float CalculateEntropy()
    {
        int totalCells = width * height * depth;
        int aliveCells = 0;

        // Подсчёт живых клеток
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        for (int z = 0; z < depth; z++)
            if (grid[x, y, z]) aliveCells++;

        float p1 = (float)aliveCells / totalCells; // Доля живых клеток
        float p0 = 1f - p1;                        // Доля мёртвых клеток

        // Вычисление энтропии
        float entropy = 0f;
        if (p1 > 0) entropy -= p1 * Mathf.Log(p1, 2);
        if (p0 > 0) entropy -= p0 * Mathf.Log(p0, 2);

        return entropy;
    }

    void UpdateEntropyGraph(float entropy)
    {
        entropyValues.Add(entropy);
        if (entropyValues.Count > maxGraphPoints)
            entropyValues.RemoveAt(0);

        entropyGraph.positionCount = entropyValues.Count;
        for (int i = 0; i < entropyValues.Count; i++)
        {
            float x = (float)i / maxGraphPoints * graphWidth; // Позиция по X (время)
            float y = entropyValues[i] * graphHeight;         // Позиция по Y (энтропия)
            entropyGraph.SetPosition(i, new Vector3(x, y, 0));
        }
        entropyGraph.colorGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.red, 1) },
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
        };
    }

    void UpdateEntropyText(float entropy)
    {
        if (entropyText != null)
            entropyText.text = $"Entropy: {entropy:F3}";
    }

    public float updateInterval = 0.5f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateGrid();
            Update3DVisualization();
            Update2DVisualization();

            // Расчёт и визуализация энтропии
            float entropy = CalculateEntropy();
            UpdateEntropyGraph(entropy);
            UpdateEntropyText(entropy);

            timer = 0;
        }
    }
}
