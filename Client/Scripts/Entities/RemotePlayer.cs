using Godot;

#nullable enable

namespace TopdownMMO.Client.Entities;

/// <summary>
/// Representa outro jogador no mundo (controlado pelo servidor).
/// Apenas reflete a posição recebida via rede.
/// Usa spritesheet do Ninja Adventure (samurai_green) para visual distinto.
/// </summary>
public partial class RemotePlayer : Node2D
{
    public const int TileSize = 32;

    private const int FrameWidth = 16;
    private const int FrameHeight = 16;
    private const int WalkFrames = 4;

    /// <summary>ID do jogador remoto.</summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>Nome do jogador remoto.</summary>
    public string PlayerName { get; set; } = string.Empty;

    private Label? _nameLabel;
    private Sprite2D? _sprite;
    private Texture2D? _spriteSheet;

    // Animação
    private int _direction; // 0=down, 1=left, 2=right, 3=up
    private int _animFrame;
    private double _animTimer;
    private const double AnimFrameTime = 0.15;
    private bool _isMoving;
    private double _moveTimeout;

    // Sprites disponíveis para jogadores remotos (cicla por índice)
    private static readonly string[] RemoteSprites = new[]
    {
        "res://Assets/NinjaAdventure/Characters/samurai_green/samurai_green.png",
        "res://Assets/NinjaAdventure/Characters/samurai_blue/sprite.png",
        "res://Assets/NinjaAdventure/Characters/pig/pig.png",
    };

    private static int _spriteIndex;

    public override void _Ready()
    {
        // Cada jogador remoto recebe um sprite diferente (round-robin)
        var spritePath = RemoteSprites[_spriteIndex % RemoteSprites.Length];
        _spriteIndex++;

        _spriteSheet = GD.Load<Texture2D>(spritePath);

        _sprite = new Sprite2D
        {
            Texture = _spriteSheet,
            RegionEnabled = true,
            RegionRect = new Rect2(0, 0, FrameWidth, FrameHeight),
            Scale = new Vector2(2.0f, 2.0f),
            Position = new Vector2(TileSize / 2.0f, TileSize / 2.0f),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        };
        AddChild(_sprite);

        _nameLabel = new Label
        {
            Text = PlayerName,
            Position = new Vector2(-10, -20),
        };
        _nameLabel.AddThemeFontSizeOverride("font_size", 10);
        AddChild(_nameLabel);

        UpdateSpriteRegion();
    }

    public override void _Process(double delta)
    {
        if (_isMoving)
        {
            _animTimer += delta;
            if (_animTimer >= AnimFrameTime)
            {
                _animTimer -= AnimFrameTime;
                _animFrame = (_animFrame + 1) % WalkFrames;
                UpdateSpriteRegion();
            }

            // Para a animação se nenhum SetGridPosition recente
            _moveTimeout -= delta;
            if (_moveTimeout <= 0)
            {
                _isMoving = false;
                _animFrame = 0;
                UpdateSpriteRegion();
            }
        }
    }

    private void UpdateSpriteRegion()
    {
        if (_sprite is null) return;
        _sprite.RegionRect = new Rect2(
            _animFrame * FrameWidth,
            _direction * FrameHeight,
            FrameWidth,
            FrameHeight);
    }

    /// <summary>Atualiza posição no grid com direção inferida do delta.</summary>
    public void SetGridPosition(int x, int y, int dx = 0, int dy = 0)
    {
        // Infere direção do delta, se não fornecido calcula da posição anterior
        if (dx == 0 && dy == 0)
        {
            int prevX = (int)(Position.X / TileSize);
            int prevY = (int)(Position.Y / TileSize);
            dx = x - prevX;
            dy = y - prevY;
        }

        if (dy < 0) _direction = 3;      // up
        else if (dy > 0) _direction = 0; // down
        else if (dx < 0) _direction = 1; // left
        else if (dx > 0) _direction = 2; // right

        Position = new Vector2(x * TileSize, y * TileSize);

        if (dx != 0 || dy != 0)
        {
            _isMoving = true;
            _moveTimeout = 0.3; // para de animar após 0.3s sem movimentação
        }

        UpdateSpriteRegion();
    }
}
