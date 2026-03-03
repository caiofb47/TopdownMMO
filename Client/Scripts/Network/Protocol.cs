using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace TopdownMMO.Client.Network;

/// <summary>
/// Tipos de mensagem trocadas entre cliente e servidor.
/// Espelha a enum do servidor (TopdownMMO.Protocol).
/// </summary>
public enum MessageType
{
    // ── Client → Server ──
    ConnectRequest     = 1,
    EnterWorld         = 2,
    MoveRequest        = 3,

    // ── Server → Client ──
    ConnectResponse    = 100,
    EnterWorldResponse = 101,
    PlayerMoved        = 102,
    PlayerDisconnected = 103,
    WorldSnapshot      = 110,
}

/// <summary>
/// Envelope genérico de mensagem — mesmo formato que o servidor usa.
/// </summary>
public sealed class NetworkMessage
{
    [JsonPropertyName("type")]
    public MessageType Type { get; set; }

    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static NetworkMessage Create<T>(MessageType type, T payload)
    {
        var element = JsonSerializer.SerializeToElement(payload, Options);
        return new NetworkMessage { Type = type, Payload = element };
    }

    public static NetworkMessage Create(MessageType type)
        => new() { Type = type };

    public T? GetPayload<T>() =>
        Payload.HasValue
            ? JsonSerializer.Deserialize<T>(Payload.Value.GetRawText(), Options)
            : default;

    public string Serialize() => JsonSerializer.Serialize(this, Options);

    public static NetworkMessage? Deserialize(string json)
        => JsonSerializer.Deserialize<NetworkMessage>(json, Options);
}

// ═══════════════════════════════════════════════
// Payload DTOs (espelham o servidor)
// ═══════════════════════════════════════════════

public sealed class ConnectRequest
{
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;
}

public sealed class ConnectResponse
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

public sealed class EnterWorldResponse
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

public sealed class MoveRequest
{
    [JsonPropertyName("dx")]
    public int Dx { get; set; }

    [JsonPropertyName("dy")]
    public int Dy { get; set; }
}

public sealed class PlayerMovedMessage
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

public sealed class PlayerDisconnectedMessage
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;
}

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

public sealed class WorldSnapshot
{
    [JsonPropertyName("players")]
    public List<SnapshotPlayer> Players { get; set; } = new();
}
