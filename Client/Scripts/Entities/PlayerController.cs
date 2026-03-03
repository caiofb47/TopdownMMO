using Godot;

#nullable enable

namespace TopdownMMO.Client.Entities;

/// <summary>
/// Controlador do jogador local.
/// Captura input (WASD / setas) e envia MoveRequest ao servidor.
/// NÃO move diretamente — aguarda confirmação (PlayerMoved) do servidor.
/// </summary>
public partial class PlayerController : CharacterBody2D
{
    /// <summary>Tamanho da tile em pixels.</summary>
    public const int TileSize = 32;

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
    private ColorRect? _sprite;

    // ═══════════════════════════════════════════════
    // Lifecycle
    // ═══════════════════════════════════════════════

    public override void _Ready()
    {
        // Cria visual simples: retângulo colorido + nome
        _sprite = new ColorRect
        {
            Size = new Vector2(TileSize - 2, TileSize - 2),
            Position = new Vector2(1, 1),
            Color = new Color(0.2f, 0.6f, 1.0f) // azul para o jogador local
        };
        AddChild(_sprite);

        _nameLabel = new Label
        {
            Text = PlayerName,
            Position = new Vector2(-10, -20),
        };
        _nameLabel.AddThemeFontSizeOverride("font_size", 10);
        AddChild(_nameLabel);
    }

    public override void _Process(double delta)
    {
        _moveCooldown -= delta;
        if (_moveCooldown > 0) return;

        int dx = 0, dy = 0;

        if (Input.IsActionPressed("move_up"))    dy = -1;
        else if (Input.IsActionPressed("move_down"))  dy = 1;
        else if (Input.IsActionPressed("move_left"))  dx = -1;
        else if (Input.IsActionPressed("move_right")) dx = 1;

        if (dx == 0 && dy == 0) return;

        // Envia pedido de movimento ao servidor
        SendMoveRequest(dx, dy);
        _moveCooldown = MoveCooldownTime;
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
    public void SetGridPosition(int x, int y)
    {
        Position = new Vector2(x * TileSize, y * TileSize);
    }
}
