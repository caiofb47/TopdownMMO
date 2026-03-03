using Godot;

#nullable enable

namespace TopdownMMO.Client.Entities;

/// <summary>
/// Controlador do jogador local.
/// Captura input (WASD / setas) e envia MoveRequest ao servidor.
/// NÃO move diretamente — aguarda confirmação (PlayerMoved) do servidor.
/// Usa spritesheet do Ninja Adventure (4 colunas × 7 linhas de 16×16).
/// Linhas: 0=down, 1=left, 2=right, 3=up (walk frames nas colunas 0-3).
/// </summary>
public partial class PlayerController : CharacterBody2D
{
    /// <summary>Tamanho da tile em pixels.</summary>
    public const int TileSize = 32;

    // Tamanho de cada frame no spritesheet
    private const int FrameWidth = 16;
    private const int FrameHeight = 16;

    // Número de frames de walk por direção (colunas 0-3)
    private const int WalkFrames = 4;

    /// <summary>ID do jogador no servidor.</summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>Nome do jogador.</summary>
    public string PlayerName { get; set; } = string.Empty;

    // Referência ao NetworkClient (injetada pelo GameManager)
    private Network.NetworkClient? _network;

    // Cooldown simples para não enviar requests a cada frame
    private double _moveCooldown;
    private const double MoveCooldownTime = 0.15; // segundos entre movimentos

    // ── Visual ──
    private Label? _nameLabel;
    private Sprite2D? _sprite;
    private Texture2D? _spriteSheet;

    // ── Animação ──
    private int _direction; // 0=down, 1=left, 2=right, 3=up
    private int _animFrame; // frame atual da walk (0-3)
    private double _animTimer;
    private const double AnimFrameTime = 0.15; // tempo por frame de animação
    private bool _isMoving;

    // ═══════════════════════════════════════════════
    // Lifecycle
    // ═══════════════════════════════════════════════

    public override void _Ready()
    {
        // Carrega spritesheet do ninja azul
        _spriteSheet = GD.Load<Texture2D>("res://Assets/NinjaAdventure/Characters/ninja_blue/sprite.png");

        _sprite = new Sprite2D
        {
            Texture = _spriteSheet,
            RegionEnabled = true,
            RegionRect = new Rect2(0, 0, FrameWidth, FrameHeight),
            // Escala 2x para preencher tile de 32×32
            Scale = new Vector2(2.0f, 2.0f),
            // Centraliza no tile
            Position = new Vector2(TileSize / 2.0f, TileSize / 2.0f),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest, // pixel art nítido
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
        // Animação de walk
        if (_isMoving)
        {
            _animTimer += delta;
            if (_animTimer >= AnimFrameTime)
            {
                _animTimer -= AnimFrameTime;
                _animFrame = (_animFrame + 1) % WalkFrames;
                UpdateSpriteRegion();
            }
        }

        _moveCooldown -= delta;
        if (_moveCooldown > 0) return;

        int dx = 0, dy = 0;
        _isMoving = false;

        if (Input.IsActionPressed("move_up"))         { dy = -1; _direction = 3; _isMoving = true; }
        else if (Input.IsActionPressed("move_down"))   { dy = 1;  _direction = 0; _isMoving = true; }
        else if (Input.IsActionPressed("move_left"))   { dx = -1; _direction = 1; _isMoving = true; }
        else if (Input.IsActionPressed("move_right"))  { dx = 1;  _direction = 2; _isMoving = true; }

        if (!_isMoving)
        {
            // Parado — reseta para frame 0 (idle)
            if (_animFrame != 0)
            {
                _animFrame = 0;
                UpdateSpriteRegion();
            }
            return;
        }

        // Envia pedido de movimento ao servidor
        SendMoveRequest(dx, dy);
        _moveCooldown = MoveCooldownTime;
    }

    // ═══════════════════════════════════════════════
    // Sprite
    // ═══════════════════════════════════════════════

    private void UpdateSpriteRegion()
    {
        if (_sprite is null) return;
        _sprite.RegionRect = new Rect2(
            _animFrame * FrameWidth,
            _direction * FrameHeight,
            FrameWidth,
            FrameHeight);
    }

    // ═══════════════════════════════════════════════
    // Rede
    // ═══════════════════════════════════════════════

    /// <summary>Injeta a referência do NetworkClient.</summary>
    public void Initialize(Network.NetworkClient network)
    {
        _network = network;
    }

    private void SendMoveRequest(int dx, int dy)
    {
        if (_network is null || !_network.IsConnected()) return;

        var msg = Network.NetworkMessage.Create(
            Network.MessageType.MoveRequest,
            new Network.MoveRequest { Dx = dx, Dy = dy });

        _network.Send(msg);
    }

    /// <summary>
    /// Chamado quando o servidor confirma a posição do jogador.
    /// Atualiza posição visual imediatamente (sem interpolação no MVP).
    /// </summary>
    public void SetGridPosition(int x, int y, int dx = 0, int dy = 0)
    {
        Position = new Vector2(x * TileSize, y * TileSize);

        // Atualiza direção se recebeu delta de movimento
        if (dy < 0) _direction = 3;      // up
        else if (dy > 0) _direction = 0; // down
        else if (dx < 0) _direction = 1; // left
        else if (dx > 0) _direction = 2; // right

        UpdateSpriteRegion();
    }
}
