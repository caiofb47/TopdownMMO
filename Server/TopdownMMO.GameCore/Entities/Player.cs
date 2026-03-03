namespace TopdownMMO.GameCore.Entities;

/// <summary>
/// Representa um jogador conectado ao mundo.
/// </summary>
public sealed class Player : Entity
{
    /// <summary>Nome exibido do jogador.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Indica se o jogador já entrou efetivamente no mundo.</summary>
    public bool IsInWorld { get; set; }
}
