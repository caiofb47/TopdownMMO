using Godot;

#nullable enable

namespace TopdownMMO.Client.Entities;

/// <summary>
/// Representa outro jogador no mundo (controlado pelo servidor).
/// Apenas reflete a posição recebida via rede.
/// </summary>
public partial class RemotePlayer : Node2D
{
    public const int TileSize = 32;

    /// <summary>ID do jogador remoto.</summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>Nome do jogador remoto.</summary>
    public string PlayerName { get; set; } = string.Empty;

    private Label? _nameLabel;
    private ColorRect? _sprite;

    public override void _Ready()
    {
        // Visual: retângulo vermelho + nome
        _sprite = new ColorRect
        {
            Size = new Vector2(TileSize - 2, TileSize - 2),
            Position = new Vector2(1, 1),
            Color = new Color(1.0f, 0.3f, 0.3f) // vermelho para jogadores remotos
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

    /// <summary>Atualiza posição no grid.</summary>
    public void SetGridPosition(int x, int y)
    {
        Position = new Vector2(x * TileSize, y * TileSize);
    }
}
