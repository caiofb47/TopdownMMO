using System.Net.WebSockets;

namespace TopdownMMO.WorldServer.Network;

/// <summary>
/// Representa uma conexão individual de um cliente.
/// Armazena o WebSocket e o mapeamento para o jogador no mundo.
/// </summary>
public sealed class ClientSession
{
    /// <summary>ID interno da sessão (gerado na conexão).</summary>
    public string SessionId { get; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>ID do jogador associado (atribuído após ConnectRequest).</summary>
    public string? PlayerId { get; set; }

    /// <summary>WebSocket subjacente.</summary>
    public WebSocket Socket { get; }

    public ClientSession(WebSocket socket)
    {
        Socket = socket;
    }
}
