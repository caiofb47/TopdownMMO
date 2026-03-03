using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace TopdownMMO.WorldServer.Network;

/// <summary>
/// Servidor WebSocket simples baseado em HttpListener.
/// Aceita conexões e delega para <see cref="ClientSession"/>.
/// </summary>
public sealed class WebSocketServer
{
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();

    /// <summary>Sessões ativas — chave = PlayerId atribuído após ConnectRequest.</summary>
    public ConcurrentDictionary<string, ClientSession> Sessions { get; } = new();

    /// <summary>Evento disparado quando uma nova sessão é estabelecida.</summary>
    public event Action<ClientSession>? OnSessionConnected;

    /// <summary>Evento disparado quando uma sessão é encerrada.</summary>
    public event Action<ClientSession>? OnSessionDisconnected;

    /// <summary>Evento disparado quando uma mensagem é recebida.</summary>
    public event Action<ClientSession, string>? OnMessageReceived;

    public WebSocketServer(string uri = "http://localhost:7777/ws/")
    {
        _listener.Prefixes.Add(uri);
    }

    /// <summary>Inicia o servidor e começa a aceitar conexões.</summary>
    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"[WebSocketServer] Escutando em {string.Join(", ", _listener.Prefixes)}");

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();

                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                    continue;
                }

                _ = HandleConnectionAsync(context);
            }
            catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException)
            {
                break; // servidor parando
            }
        }
    }

    private async Task HandleConnectionAsync(HttpListenerContext httpContext)
    {
        var wsContext = await httpContext.AcceptWebSocketAsync(null);
        var session = new ClientSession(wsContext.WebSocket);

        Console.WriteLine($"[WebSocketServer] Nova conexão: {session.SessionId}");
        Sessions[session.SessionId] = session;
        OnSessionConnected?.Invoke(session);

        try
        {
            await ReceiveLoopAsync(session);
        }
        catch (WebSocketException)
        {
            // Conexão perdida
        }
        finally
        {
            Sessions.TryRemove(session.SessionId, out _);
            OnSessionDisconnected?.Invoke(session);
            Console.WriteLine($"[WebSocketServer] Desconectado: {session.SessionId}");
        }
    }

    private async Task ReceiveLoopAsync(ClientSession session)
    {
        var buffer = new byte[4096];
        var sb = new StringBuilder();

        while (session.Socket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
        {
            var result = await session.Socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), _cts.Token);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await session.Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
                break;
            }

            sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

            if (result.EndOfMessage)
            {
                var json = sb.ToString();
                sb.Clear();
                OnMessageReceived?.Invoke(session, json);
            }
        }
    }

    /// <summary>Envia mensagem para uma sessão específica.</summary>
    public async Task SendAsync(ClientSession session, string json)
    {
        if (session.Socket.State != WebSocketState.Open) return;

        var bytes = Encoding.UTF8.GetBytes(json);
        await session.Socket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: CancellationToken.None);
    }

    /// <summary>Broadcast para todas as sessões ativas.</summary>
    public async Task BroadcastAsync(string json)
    {
        var tasks = Sessions.Values
            .Where(s => s.Socket.State == WebSocketState.Open)
            .Select(s => SendAsync(s, json));
        await Task.WhenAll(tasks);
    }

    /// <summary>Encerra o servidor.</summary>
    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
    }
}
