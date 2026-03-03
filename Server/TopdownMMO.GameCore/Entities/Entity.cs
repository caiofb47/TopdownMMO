namespace TopdownMMO.GameCore.Entities;

/// <summary>
/// Entidade base do mundo — qualquer objeto posicionável no grid.
/// </summary>
public abstract class Entity
{
    /// <summary>Identificador único da entidade.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Posição X no grid (em tiles).</summary>
    public int X { get; set; }

    /// <summary>Posição Y no grid (em tiles).</summary>
    public int Y { get; set; }
}
