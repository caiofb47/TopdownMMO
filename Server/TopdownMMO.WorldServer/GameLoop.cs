using System.Diagnostics;
using TopdownMMO.GameCore.World;
using TopdownMMO.Protocol;
using TopdownMMO.Protocol.Messages;
using TopdownMMO.WorldServer.Network;

namespace TopdownMMO.WorldServer;

/// <summary>
/// Game loop fixo que roda a 20 ticks por segundo.
/// A cada tick, envia um WorldSnapshot para todos os clientes.
/// </summary>
public sealed class GameLoop
{
    private const int TicksPerSecond = 20;
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(1000.0 / TicksPerSecond);

    private readonly GameWorld _world;
    private readonly WebSocketServer _server;
    private readonly CancellationTokenSource _cts = new();

    private long _tickCount;

    public GameLoop(GameWorld world, WebSocketServer server)
    {
        _world = world;
        _server = server;
    }

    /// <summary>Inicia o game loop em background.</summary>
    public Task StartAsync() => Task.Run(RunAsync);

    private async Task RunAsync()
    {
        Console.WriteLine($"[GameLoop] Iniciado — {TicksPerSecond} ticks/s");
        var sw = Stopwatch.StartNew();

        while (!_cts.IsCancellationRequested)
        {
            var tickStart = sw.Elapsed;

            // ── Lógica do tick ──
            _tickCount++;

            // Envia snapshot a cada 1 segundo (a cada 20 ticks)
            if (_tickCount % TicksPerSecond == 0)
            {
                var snapshot = _world.CreateSnapshot();
                if (snapshot.Players.Count > 0)
                {
                    var msg = NetworkMessage.Create(MessageType.WorldSnapshot, snapshot);
                    await _server.BroadcastAsync(msg.Serialize());
                }
            }

            // ── Controle de tempo ──
            var elapsed = sw.Elapsed - tickStart;
            var sleepTime = TickInterval - elapsed;
            if (sleepTime > TimeSpan.Zero)
                await Task.Delay(sleepTime);
        }
    }

    /// <summary>Para o game loop.</summary>
    public void Stop() => _cts.Cancel();
}
