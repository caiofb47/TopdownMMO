using TopdownMMO.GameCore.World;
using TopdownMMO.Protocol;
using TopdownMMO.Protocol.Messages;
using TopdownMMO.WorldServer;
using TopdownMMO.WorldServer.Network;

// ═══════════════════════════════════════════════
// TopdownMMO — World Server
// ═══════════════════════════════════════════════

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║   TopdownMMO — World Server v0.1     ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine();

// 1. Cria o mundo (mapa 20×20 com paredes nas bordas)
var world = new GameWorld(mapWidth: 20, mapHeight: 20);
Console.WriteLine("[Init] Mundo criado: 20x20 tiles");

// 2. Inicia o servidor WebSocket
var server = new WebSocketServer("http://localhost:7777/ws/");

// 3. Configura o handler de mensagens
var handler = new MessageHandler(world, server);

server.OnMessageReceived += (session, json) =>
{
    _ = handler.HandleAsync(session, json);
};

server.OnSessionDisconnected += (session) =>
{
    if (session.PlayerId is not null)
    {
        world.RemovePlayer(session.PlayerId);

        // Notifica todos que o jogador saiu
        var msg = NetworkMessage.Create(MessageType.PlayerDisconnected,
            new PlayerDisconnectedMessage { PlayerId = session.PlayerId });
        _ = server.BroadcastAsync(msg.Serialize());

        Console.WriteLine($"[Server] Jogador removido: {session.PlayerId}");
    }
};

// 4. Inicia game loop (20 ticks/s)
var gameLoop = new GameLoop(world, server);
_ = gameLoop.StartAsync();

// 5. Roda o server no main thread
Console.WriteLine("[Init] Pressione Ctrl+C para encerrar.\n");
await server.StartAsync();
