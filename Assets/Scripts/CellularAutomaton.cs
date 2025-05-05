using UnityEngine;
using UnityEngine.UI; // Добавлено для RawImage

public class CellularAutomaton : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    private bool[,] grid;
    private bool[,] nextGrid;

    public GameObject cellPrefab;
    private GameObject[,] cellObjects;

    public RawImage mapImage;
    private Texture2D texture;

    void Start()
    {
        grid = new bool[width, height];
        nextGrid = new bool[width, height];
        InitializeGrid();
        cellObjects = new GameObject[width, height];
        Update3DVisualization();
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        mapImage.texture = texture;
        Update2DVisualization();
    }

    void Update2DVisualization()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            texture.SetPixel(x, y, grid[x, y] ? Color.white : Color.black);
        texture.Apply();
    }

    void Update3DVisualization()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] && cellObjects[x, y] == null)
                {
                    cellObjects[x, y] = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                }
                else if (!grid[x, y] && cellObjects[x, y] != null)
                {
                    Destroy(cellObjects[x, y]);
                    cellObjects[x, y] = null;
                }
            }
        }
    }

    void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            grid[x, y] = Random.value > 0.5f; // Случайная инициализация
    }

    void UpdateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbors = CountNeighbors(x, y);
                bool isAlive = grid[x, y];
                nextGrid[x, y] = isAlive ? (neighbors == 2 || neighbors == 3) : (neighbors == 3);
            }
        }
        (grid, nextGrid) = (nextGrid, grid); // Обмен буферов
    }

    int CountNeighbors(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int nx = x + dx, ny = y + dy;
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && grid[nx, ny])
                count++;
        }
        return count;
    }

    public float updateInterval = 0.5f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateGrid();
            Update3DVisualization(); // Обновляем визуализацию
            Update2DVisualization(); // Обновляем 2D-карту
            timer = 0;
        }
    }
}
