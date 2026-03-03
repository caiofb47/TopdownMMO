using TopdownMMO.GameCore.World;
using TopdownMMO.Protocol;
using TopdownMMO.Protocol.Messages;
using TopdownMMO.WorldServer.Network;

namespace TopdownMMO.WorldServer;

/// <summary>
/// Processa mensagens recebidas dos clientes e atua sobre o <see cref="GameWorld"/>.
/// </summary>
public sealed class MessageHandler
{
    private readonly GameWorld _world;
    private readonly WebSocketServer _server;

    public MessageHandler(GameWorld world, WebSocketServer server)
    {
        _world = world;
        _server = server;
    }

    /// <summary>Trata uma mensagem JSON recebida de um cliente.</summary>
    public async Task HandleAsync(ClientSession session, string json)
    {
        var msg = NetworkMessage.Deserialize(json);
        if (msg is null) return;

        switch (msg.Type)
        {
            case MessageType.ConnectRequest:
                await HandleConnect(session, msg);
                break;

            case MessageType.EnterWorld:
                await HandleEnterWorld(session);
                break;

            case MessageType.MoveRequest:
                await HandleMove(session, msg);
                break;

            default:
                Console.WriteLine($"[MessageHandler] Tipo desconhecido: {msg.Type}");
                break;
        }
    }

    // ────────────────────────────────────────────
    // Handlers individuais
    // ────────────────────────────────────────────

    private async Task HandleConnect(ClientSession session, NetworkMessage msg)
    {
        var req = msg.GetPayload<ConnectRequest>();
        var playerName = req?.PlayerName ?? "Unknown";

        var player = _world.AddPlayer(playerName);
        session.PlayerId = player.Id;

        // Registra a sessão pelo PlayerId também (para broadcasts direcionados)
        _server.Sessions[player.Id] = session;

        var response = NetworkMessage.Create(MessageType.ConnectResponse, new ConnectResponse
        {
            PlayerId = player.Id,
            Success = true
        });

        Console.WriteLine($"[MessageHandler] Jogador conectado: {playerName} (id={player.Id})");
        await _server.SendAsync(session, response.Serialize());
    }

    private async Task HandleEnterWorld(ClientSession session)
    {
        if (session.PlayerId is null) return;

        _world.EnterWorld(session.PlayerId);

        if (_world.Players.TryGetValue(session.PlayerId, out var player))
        {
            var response = NetworkMessage.Create(MessageType.EnterWorldResponse, new EnterWorldResponse
            {
                PlayerId = player.Id,
                X = player.X,
                Y = player.Y
            });

            Console.WriteLine($"[MessageHandler] {player.Name} entrou no mundo em ({player.X},{player.Y})");
            await _server.SendAsync(session, response.Serialize());

            // Envia snapshot atual para o novo jogador
            var snapshot = NetworkMessage.Create(MessageType.WorldSnapshot, _world.CreateSnapshot());
            await _server.SendAsync(session, snapshot.Serialize());
        }
    }

    private async Task HandleMove(ClientSession session, NetworkMessage msg)
    {
        if (session.PlayerId is null) return;

        var req = msg.GetPayload<MoveRequest>();
        if (req is null) return;

        bool moved = _world.MovePlayer(session.PlayerId, req.Dx, req.Dy);

        if (moved && _world.Players.TryGetValue(session.PlayerId, out var player))
        {
            var broadcast = NetworkMessage.Create(MessageType.PlayerMoved, new PlayerMovedMessage
            {
                PlayerId = player.Id,
                X = player.X,
                Y = player.Y
            });

            // Broadcast para todos os conectados
            await _server.BroadcastAsync(broadcast.Serialize());
        }
    }
}
