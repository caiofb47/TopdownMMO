using Godot;

#nullable enable

namespace TopdownMMO.Client.World;

/// <summary>
/// Renderiza o grid do mundo usando tilesets do Ninja Adventure.
/// Tiles de 16×16 escalados 2× = 32×32 no mundo.
/// </summary>
public partial class WorldGrid : Node2D
{
    public const int TileSize = 32;
    public const int SrcTile = 16; // tamanho original no tileset
    public int MapWidth { get; set; } = 20;
    public int MapHeight { get; set; } = 20;

    private Texture2D? _floorTileset;
    private Texture2D? _wallTileset;

    // Região do tile de grama no tileset_floor.png (tile 0,0 = grama)
    private readonly Rect2 _grassRegion = new(0, 0, SrcTile, SrcTile);

    // Região do tile de parede no tileset_wall_simple.png (tile de pedra)
    private readonly Rect2 _wallRegion = new(0, 0, SrcTile, SrcTile);

    public override void _Ready()
    {
        _floorTileset = GD.Load<Texture2D>("res://Assets/NinjaAdventure/Tilesets/tileset_floor.png");
        _wallTileset = GD.Load<Texture2D>("res://Assets/NinjaAdventure/Tilesets/tileset_wall_simple.png");

        GD.Print($"[WorldGrid] Floor tileset: {_floorTileset != null}, Wall tileset: {_wallTileset != null}");
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_floorTileset is null || _wallTileset is null) return;

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                var destRect = new Rect2(x * TileSize, y * TileSize, TileSize, TileSize);
                bool isWall = x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1;

                if (isWall)
                {
                    DrawTextureRectRegion(_wallTileset, destRect, _wallRegion);
                }
                else
                {
                    DrawTextureRectRegion(_floorTileset, destRect, _grassRegion);
                }
            }
        }

        // Linhas do grid semi-transparentes
        var gridColor = new Color(0.0f, 0.0f, 0.0f, 0.15f);
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
