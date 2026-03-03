using System.Collections.Concurrent;
using TopdownMMO.GameCore.Entities;
using TopdownMMO.Protocol.Messages;

namespace TopdownMMO.GameCore.World;

/// <summary>
/// Representa o mundo do jogo — mantém mapa, jogadores e a fila de comandos pendentes.
/// </summary>
public sealed class GameWorld
{
    /// <summary>Mapa de tiles (colisão).</summary>
    public TileMap Map { get; }

    /// <summary>Jogadores conectados — chave = PlayerId.</summary>
    public ConcurrentDictionary<string, Player> Players { get; } = new();

    /// <summary>Posição de spawn padrão.</summary>
    public (int X, int Y) SpawnPoint { get; set; } = (5, 5);

    public GameWorld(int mapWidth = 20, int mapHeight = 20)
    {
        Map = new TileMap(mapWidth, mapHeight);

        // Exemplo: cria algumas paredes nas bordas
        for (int x = 0; x < mapWidth; x++)
        {
            Map.SetWalkable(x, 0, false);               // parede norte
            Map.SetWalkable(x, mapHeight - 1, false);    // parede sul
        }
        for (int y = 0; y < mapHeight; y++)
        {
            Map.SetWalkable(0, y, false);                // parede oeste
            Map.SetWalkable(mapWidth - 1, y, false);     // parede leste
        }
    }

    // ────────────────────────────────────────────
    // Operações de jogador
    // ────────────────────────────────────────────

    /// <summary>Adiciona um novo jogador no spawn e retorna a entidade.</summary>
    public Player AddPlayer(string name)
    {
        var player = new Player
        {
            Name = name,
            X = SpawnPoint.X,
            Y = SpawnPoint.Y,
            IsInWorld = false
        };
        Players[player.Id] = player;
        return player;
    }

    /// <summary>Remove um jogador do mundo.</summary>
    public bool RemovePlayer(string playerId)
        => Players.TryRemove(playerId, out _);

    /// <summary>Marca o jogador como presente no mundo.</summary>
    public void EnterWorld(string playerId)
    {
        if (Players.TryGetValue(playerId, out var p))
            p.IsInWorld = true;
    }

    /// <summary>Tenta mover o jogador. Retorna true se o movimento foi válido.</summary>
    public bool MovePlayer(string playerId, int dx, int dy)
    {
        if (!Players.TryGetValue(playerId, out var player))
            return false;
        if (!player.IsInWorld)
            return false;

        return MovementSystem.TryMove(player, dx, dy, Map);
    }

    /// <summary>Gera um snapshot completo do mundo.</summary>
    public WorldSnapshot CreateSnapshot()
    {
        var snapshot = new WorldSnapshot();
        foreach (var kvp in Players)
        {
            var p = kvp.Value;
            if (!p.IsInWorld) continue;

            snapshot.Players.Add(new SnapshotPlayer
            {
                PlayerId = p.Id,
                Name = p.Name,
                X = p.X,
                Y = p.Y
            });
        }
        return snapshot;
    }
}
