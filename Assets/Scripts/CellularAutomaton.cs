using UnityEngine;
using UnityEngine.UI;

public class CellularAutomaton : MonoBehaviour
{
    public int width = 20;  // Размер по X
    public int height = 20; // Размер по Y
    public int depth = 20;  // Размер по Z (новая ось)
    private bool[,,] grid;  // 3D-сетка
    private bool[,,] nextGrid;

    public GameObject cellPrefab;
    private GameObject[,,] cellObjects;

    public RawImage mapImage;
    private Texture2D texture;

    void Start()
    {
        grid = new bool[width, height, depth];
        nextGrid = new bool[width, height, depth];
        InitializeGrid();
        cellObjects = new GameObject[width, height, depth];
        Update3DVisualization();
        texture = new Texture2D(width, depth); // 2D-карта будет показывать срез по Y
        texture.filterMode = FilterMode.Point;
        mapImage.texture = texture;
        Update2DVisualization();
    }

    void Update2DVisualization()
    {
        if (texture == null || mapImage == null) return;
        // Показываем срез по Y (например, средний уровень height/2)
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
            grid[x, y, z] = Random.value > 0.7f; // Более редкое заполнение для читаемости
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
        (grid, nextGrid) = (nextGrid, grid); // Обмен буферов
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
            timer = 0;
        }
    }
}
