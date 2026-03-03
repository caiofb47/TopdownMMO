using Godot;
using System.Collections.Generic;
using TopdownMMO.Client.Entities;
using TopdownMMO.Client.Network;
using TopdownMMO.Client.UI;
using TopdownMMO.Client.World;

#nullable enable

namespace TopdownMMO.Client;

/// <summary>
/// Gerenciador principal do jogo.
/// Coordena conexão, criação de jogadores e processamento de mensagens.
/// Este script é o root da cena principal.
/// </summary>
public partial class GameManager : Node2D
{
    // ── Configuração ──
    [Export] public string PlayerName { get; set; } = "Player";

    // ── Referências ──
    private NetworkClient _network = null!;
    private GameHUD _hud = null!;
    private WorldGrid _worldGrid = null!;
    private Node2D _entitiesContainer = null!;

    // ── Estado ──
    private string _localPlayerId = string.Empty;
    private PlayerController? _localPlayer;
    private readonly Dictionary<string, RemotePlayer> _remotePlayers = new();

    // ═══════════════════════════════════════════════
    // Lifecycle
    // ═══════════════════════════════════════════════

    public override void _Ready()
    {
        GD.Print("[GameManager] Inicializando...");

        // Gera nome aleatório simples
        PlayerName = $"Player_{GD.Randi() % 1000:D3}";

        // Cria componentes
        _worldGrid = new WorldGrid();
        AddChild(_worldGrid);

        _entitiesContainer = new Node2D { Name = "Entities" };
        AddChild(_entitiesContainer);

        _hud = new GameHUD();
        AddChild(_hud);

        _network = new NetworkClient();
        AddChild(_network);

        // Conecta sinais
        _network.ConnectedToServer += OnConnected;
        _network.DisconnectedFromServer += OnDisconnected;
        _network.MessageReceived += OnMessageReceived;

        // Inicia conexão
        _hud.SetStatus("Conectando...");
        _network.Connect();
    }

    // ═══════════════════════════════════════════════
    // Eventos de rede
    // ═══════════════════════════════════════════════

    private void OnConnected()
    {
        GD.Print("[GameManager] Conectado! Enviando ConnectRequest...");
        _hud.SetStatus("Conectado — entrando...");

        var msg = NetworkMessage.Create(
            MessageType.ConnectRequest,
            new ConnectRequest { PlayerName = PlayerName });
        _network.Send(msg);
    }

    private void OnDisconnected()
    {
        GD.Print("[GameManager] Desconectado do servidor.");
        _hud.SetStatus("Desconectado");
    }

    private void OnMessageReceived(string json)
    {
        var msg = NetworkMessage.Deserialize(json);
        if (msg is null) return;

        switch (msg.Type)
        {
            case MessageType.ConnectResponse:
                HandleConnectResponse(msg);
                break;
            case MessageType.EnterWorldResponse:
                HandleEnterWorldResponse(msg);
                break;
            case MessageType.PlayerMoved:
                HandlePlayerMoved(msg);
                break;
            case MessageType.PlayerDisconnected:
                HandlePlayerDisconnected(msg);
                break;
            case MessageType.WorldSnapshot:
                HandleWorldSnapshot(msg);
                break;
        }
    }

    // ═══════════════════════════════════════════════
    // Handlers de mensagem
    // ═══════════════════════════════════════════════

    private void HandleConnectResponse(NetworkMessage msg)
    {
        var res = msg.GetPayload<ConnectResponse>();
        if (res is null || !res.Success) return;

        _localPlayerId = res.PlayerId;
        GD.Print($"[GameManager] ID recebido: {_localPlayerId}. Enviando EnterWorld...");

        // Pede para entrar no mundo
        _network.Send(NetworkMessage.Create(MessageType.EnterWorld));
    }

    private void HandleEnterWorldResponse(NetworkMessage msg)
    {
        var res = msg.GetPayload<EnterWorldResponse>();
        if (res is null) return;

        GD.Print($"[GameManager] Entrou no mundo em ({res.X}, {res.Y})");
        _hud.SetStatus($"No mundo — {PlayerName}");

        // Cria o jogador local
        _localPlayer = new PlayerController
        {
            PlayerId = res.PlayerId,
            PlayerName = PlayerName
        };
        _localPlayer.Initialize(_network);
        _localPlayer.SetGridPosition(res.X, res.Y);
        _entitiesContainer.AddChild(_localPlayer);

        _hud.SetPosition(res.X, res.Y);
    }

    private void HandlePlayerMoved(NetworkMessage msg)
    {
        var data = msg.GetPayload<PlayerMovedMessage>();
        if (data is null) return;

        if (data.PlayerId == _localPlayerId)
        {
            // Atualiza jogador local
            _localPlayer?.SetGridPosition(data.X, data.Y);
            _hud.SetPosition(data.X, data.Y);
        }
        else
        {
            // Atualiza jogador remoto (cria se necessário)
            if (_remotePlayers.TryGetValue(data.PlayerId, out var remote))
            {
                remote.SetGridPosition(data.X, data.Y);
            }
            else
            {
                CreateRemotePlayer(data.PlayerId, "Remote", data.X, data.Y);
            }
        }
    }

    private void HandlePlayerDisconnected(NetworkMessage msg)
    {
        var data = msg.GetPayload<PlayerDisconnectedMessage>();
        if (data is null) return;

        if (_remotePlayers.TryGetValue(data.PlayerId, out var remote))
        {
            remote.QueueFree();
            _remotePlayers.Remove(data.PlayerId);
            GD.Print($"[GameManager] Jogador remoto removido: {data.PlayerId}");
        }
    }

    private void HandleWorldSnapshot(NetworkMessage msg)
    {
        var snapshot = msg.GetPayload<WorldSnapshot>();
        if (snapshot is null) return;

        // Conjunto de IDs presentes no snapshot
        var presentIds = new HashSet<string>();

        foreach (var p in snapshot.Players)
        {
            presentIds.Add(p.PlayerId);

            if (p.PlayerId == _localPlayerId)
            {
                // Atualiza local
                _localPlayer?.SetGridPosition(p.X, p.Y);
                _hud.SetPosition(p.X, p.Y);
                continue;
            }

            if (_remotePlayers.TryGetValue(p.PlayerId, out var remote))
            {
                remote.SetGridPosition(p.X, p.Y);
            }
            else
            {
                CreateRemotePlayer(p.PlayerId, p.Name, p.X, p.Y);
            }
        }

        // Remove jogadores que não estão mais no snapshot
        var toRemove = new List<string>();
        foreach (var kvp in _remotePlayers)
        {
            if (!presentIds.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }
        foreach (var id in toRemove)
        {
            _remotePlayers[id].QueueFree();
            _remotePlayers.Remove(id);
        }
    }

    // ═══════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════

    private void CreateRemotePlayer(string id, string name, int x, int y)
    {
        var remote = new RemotePlayer
        {
            PlayerId = id,
            PlayerName = name
        };
        remote.SetGridPosition(x, y);
        _entitiesContainer.AddChild(remote);
        _remotePlayers[id] = remote;
        GD.Print($"[GameManager] Jogador remoto criado: {name} ({id})");
    }
}
