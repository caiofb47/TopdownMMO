using System.Text.Json;
using System.Text.Json.Serialization;

namespace TopdownMMO.Protocol;

/// <summary>
/// Envelope genérico que encapsula toda mensagem trocada via WebSocket.
/// O campo <see cref="Type"/> determina como interpretar <see cref="Payload"/>.
/// </summary>
public sealed class NetworkMessage
{
    [JsonPropertyName("type")]
    public MessageType Type { get; set; }

    /// <summary>
    /// Conteúdo serializado da mensagem (JSON interno).
    /// Pode ser null quando o tipo de mensagem não requer dados extras.
    /// </summary>
    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    // ────────────────────────────────────────────
    // Helpers de serialização
    // ────────────────────────────────────────────

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>Cria um <see cref="NetworkMessage"/> com payload tipado.</summary>
    public static NetworkMessage Create<T>(MessageType type, T payload)
    {
        var element = JsonSerializer.SerializeToElement(payload, _options);
        return new NetworkMessage { Type = type, Payload = element };
    }

    /// <summary>Cria um <see cref="NetworkMessage"/> sem payload.</summary>
    public static NetworkMessage Create(MessageType type)
        => new() { Type = type };

    /// <summary>Deserializa o payload para o tipo informado.</summary>
    public T? GetPayload<T>() =>
        Payload.HasValue
            ? JsonSerializer.Deserialize<T>(Payload.Value.GetRawText(), _options)
            : default;

    /// <summary>Serializa a mensagem completa para JSON string.</summary>
    public string Serialize() => JsonSerializer.Serialize(this, _options);

    /// <summary>Deserializa uma JSON string em <see cref="NetworkMessage"/>.</summary>
    public static NetworkMessage? Deserialize(string json)
        => JsonSerializer.Deserialize<NetworkMessage>(json, _options);
}
