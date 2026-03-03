using Godot;

namespace TopdownMMO.Client.World;

/// <summary>
/// Renderiza o grid do mundo como referência visual.
/// Desenha linhas no grid 20×20 com tiles de 32px.
/// </summary>
public partial class WorldGrid : Node2D
{
    public const int TileSize = 32;
    public int MapWidth { get; set; } = 20;
    public int MapHeight { get; set; } = 20;

    public override void _Draw()
    {
        var gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        var wallColor = new Color(0.5f, 0.3f, 0.1f, 0.8f);

        // Desenha paredes (bordas)
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                bool isWall = x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1;
                if (isWall)
                {
                    DrawRect(new Rect2(x * TileSize, y * TileSize, TileSize, TileSize), wallColor);
                }
            }
        }

        // Desenha linhas do grid
        for (int x = 0; x <= MapWidth; x++)
        {
            DrawLine(
                new Vector2(x * TileSize, 0),
                new Vector2(x * TileSize, MapHeight * TileSize),
                gridColor);
        }
        for (int y = 0; y <= MapHeight; y++)
        {
            DrawLine(
                new Vector2(0, y * TileSize),
                new Vector2(MapWidth * TileSize, y * TileSize),
                gridColor);
        }
    }
}
