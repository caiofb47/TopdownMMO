namespace TopdownMMO.GameCore.World;

/// <summary>
/// Grid 2D simples que mantém informações de walkability (colisão).
/// Cada tile é representado por um bool: true = passável, false = bloqueado.
/// </summary>
public sealed class TileMap
{
    public int Width { get; }
    public int Height { get; }

    // true = walkable
    private readonly bool[,] _tiles;

    public TileMap(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new bool[width, height];

        // Inicializa tudo como passável
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y] = true;
    }

    /// <summary>Define se uma tile é passável ou não.</summary>
    public void SetWalkable(int x, int y, bool walkable)
    {
        if (InBounds(x, y))
            _tiles[x, y] = walkable;
    }

    /// <summary>Verifica se a posição está dentro dos limites e é passável.</summary>
    public bool IsWalkable(int x, int y)
        => InBounds(x, y) && _tiles[x, y];

    /// <summary>Verifica se a coordenada está dentro do mapa.</summary>
    public bool InBounds(int x, int y)
        => x >= 0 && x < Width && y >= 0 && y < Height;
}
