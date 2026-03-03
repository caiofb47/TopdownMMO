using Godot;
using System;
using System.Text;

#nullable enable

namespace TopdownMMO.Client.Network;

/// <summary>
/// Cliente WebSocket para comunicação com o WorldServer.
/// Gerencia conexão, envio e recebimento de mensagens JSON.
/// Deve ser adicionado como Autoload ou filho de um nó persistente.
/// </summary>
public partial class NetworkClient : Node
{
    // ── Configuração ──
    [Export] public string ServerUrl { get; set; } = "ws://localhost:7777/ws/";

    // ── WebSocket ──
    private WebSocketPeer _socket = new();
    private bool _wasConnected;

    // ── Sinais Godot (eventos) ──
    [Signal] public delegate void ConnectedToServerEventHandler();
    [Signal] public delegate void DisconnectedFromServerEventHandler();
    [Signal] public delegate void MessageReceivedEventHandler(string json);

    // ═══════════════════════════════════════════════
    // Lifecycle
    // ═══════════════════════════════════════════════

    public override void _Ready()
    {
        GD.Print("[NetworkClient] Pronto. URL: ", ServerUrl);
    }

    public override void _Process(double delta)
    {
        _socket.Poll();

        var state = _socket.GetReadyState();

        switch (state)
        {
            case WebSocketPeer.State.Open:
                if (!_wasConnected)
                {
                    _wasConnected = true;
                    GD.Print("[NetworkClient] Conectado ao servidor!");
                    EmitSignal(SignalName.ConnectedToServer);
                }
                // Processa mensagens pendentes
                while (_socket.GetAvailablePacketCount() > 0)
                {
                    var packet = _socket.GetPacket();
                    var json = Encoding.UTF8.GetString(packet);
                    EmitSignal(SignalName.MessageReceived, json);
                }
                break;

            case WebSocketPeer.State.Closed:
                if (_wasConnected)
                {
                    _wasConnected = false;
                    GD.Print("[NetworkClient] Desconectado do servidor.");
                    EmitSignal(SignalName.DisconnectedFromServer);
                }
                break;

            case WebSocketPeer.State.Closing:
                // Aguarda fechar
                break;

            case WebSocketPeer.State.Connecting:
                // Aguarda conectar
                break;
        }
    }

    // ═══════════════════════════════════════════════
    // API pública
    // ═══════════════════════════════════════════════

    /// <summary>Inicia a conexão com o servidor.</summary>
    public Error Connect()
    {
        GD.Print("[NetworkClient] Conectando a ", ServerUrl, "...");
        var err = _socket.ConnectToUrl(ServerUrl);
        if (err != Error.Ok)
            GD.PrintErr("[NetworkClient] Erro ao conectar: ", err);
        return err;
    }

    /// <summary>Envia uma mensagem serializada para o servidor.</summary>
    public void Send(NetworkMessage message)
    {
        if (_socket.GetReadyState() != WebSocketPeer.State.Open)
        {
            GD.PrintErr("[NetworkClient] Tentativa de enviar sem conexão.");
            return;
        }

        var json = message.Serialize();
        var bytes = Encoding.UTF8.GetBytes(json);
        _socket.PutPacket(bytes);
    }

    /// <summary>Fecha a conexão.</summary>
    public void Disconnect()
    {
        _socket.Close();
    }

    public bool IsConnected()
    {
        return _socket.GetReadyState() == WebSocketPeer.State.Open;
    }
}
