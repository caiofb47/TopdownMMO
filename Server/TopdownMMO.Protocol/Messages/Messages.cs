using System.Text.Json.Serialization;

namespace TopdownMMO.Protocol.Messages;

// ═══════════════════════════════════════════════
// Client → Server
// ═══════════════════════════════════════════════

/// <summary>Pedido de conexão inicial do cliente.</summary>
public sealed class ConnectRequest
{
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;
}

/// <summary>Cliente pede para entrar no mundo.</summary>
public sealed class EnterWorldRequest
{
    // Sem campos extras no MVP — bastará o tipo da mensagem.
}

/// <summary>Pedido de movimento em uma direção.</summary>
public sealed class MoveRequest
{
    /// <summary>Deslocamento X no grid (-1, 0 ou +1).</summary>
    [JsonPropertyName("dx")]
    public int Dx { get; set; }

    /// <summary>Deslocamento Y no grid (-1, 0 ou +1).</summary>
    [JsonPropertyName("dy")]
    public int Dy { get; set; }
}

// ═══════════════════════════════════════════════
// Server → Client
// ═══════════════════════════════════════════════

/// <summary>Resposta à conexão — informa o ID atribuído ao jogador.</summary>
public sealed class ConnectResponse
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

/// <summary>Resposta ao pedido de entrar no mundo — posição inicial.</summary>
public sealed class EnterWorldResponse
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

/// <summary>Notifica que um jogador se moveu.</summary>
public sealed class PlayerMovedMessage
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

/// <summary>Notifica que um jogador desconectou.</summary>
public sealed class PlayerDisconnectedMessage
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;
}

/// <summary>Dados de um jogador dentro do snapshot.</summary>
public sealed class SnapshotPlayer
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

/// <summary>Snapshot completo do mundo enviado periodicamente.</summary>
public sealed class WorldSnapshot
{
    [JsonPropertyName("players")]
    public List<SnapshotPlayer> Players { get; set; } = [];
}
