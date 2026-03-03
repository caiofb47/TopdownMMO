using Godot;

#nullable enable

namespace TopdownMMO.Client.UI;

/// <summary>
/// HUD simples exibindo informações de conexão e posição.
/// </summary>
public partial class GameHUD : CanvasLayer
{
    private Label? _statusLabel;
    private Label? _positionLabel;

    public override void _Ready()
    {
        // Status de conexão (canto superior esquerdo)
        _statusLabel = new Label
        {
            Text = "Desconectado",
            Position = new Vector2(10, 10),
        };
        _statusLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0));
        AddChild(_statusLabel);

        // Posição do jogador (abaixo do status)
        _positionLabel = new Label
        {
            Text = "",
            Position = new Vector2(10, 30),
        };
        _positionLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        AddChild(_positionLabel);
    }

    public void SetStatus(string status) => _statusLabel!.Text = status;
    public void SetPosition(int x, int y) => _positionLabel!.Text = $"Pos: ({x}, {y})";
}
