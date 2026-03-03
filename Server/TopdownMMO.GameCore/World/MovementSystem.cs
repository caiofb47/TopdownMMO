using TopdownMMO.GameCore.Entities;

namespace TopdownMMO.GameCore.World;

/// <summary>
/// Sistema de movimentação — valida e aplica deslocamentos no grid.
/// </summary>
public static class MovementSystem
{
    /// <summary>
    /// Tenta mover um jogador em (dx, dy).
    /// Retorna true se o movimento foi aplicado, false se foi bloqueado.
    /// </summary>
    public static bool TryMove(Player player, int dx, int dy, TileMap map)
    {
        // Sanitiza: só aceita -1, 0 ou +1 por eixo (sem diagonal por enquanto)
        dx = Math.Clamp(dx, -1, 1);
        dy = Math.Clamp(dy, -1, 1);

        // MVP: sem movimento diagonal
        if (dx != 0 && dy != 0)
            return false;

        // Sem movimento nulo
        if (dx == 0 && dy == 0)
            return false;

        int newX = player.X + dx;
        int newY = player.Y + dy;

        if (!map.IsWalkable(newX, newY))
            return false;

        player.X = newX;
        player.Y = newY;
        return true;
    }
}
