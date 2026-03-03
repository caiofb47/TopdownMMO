# TopdownMMO — Esqueleto MVP

MMORPG top-down inspirado em Tibia.  
**Servidor**: .NET 9 (WebSocket) · **Cliente**: Godot 4 (C#)

---

## Estrutura do Projeto

```
GameDev/
├── Server/                          ← Solution .NET 9
│   ├── TopdownMMO.sln
│   ├── TopdownMMO.Protocol/         ← Mensagens e serialização
│   │   ├── MessageType.cs
│   │   ├── NetworkMessage.cs
│   │   └── Messages/Messages.cs
│   ├── TopdownMMO.GameCore/         ← Lógica do jogo
│   │   ├── Entities/
│   │   │   ├── Entity.cs
│   │   │   └── Player.cs
│   │   └── World/
│   │       ├── TileMap.cs
│   │       ├── MovementSystem.cs
│   │       └── GameWorld.cs
│   └── TopdownMMO.WorldServer/      ← Servidor executável
│       ├── Program.cs
│       ├── MessageHandler.cs
│       ├── GameLoop.cs
│       └── Network/
│           ├── WebSocketServer.cs
│           └── ClientSession.cs
│
└── Client/                          ← Projeto Godot 4 (C#)
    ├── project.godot
    ├── TopdownMMO.Client.csproj
    ├── Scenes/
    │   └── Main.tscn
    └── Scripts/
        ├── GameManager.cs
        ├── Network/
        │   ├── Protocol.cs
        │   └── NetworkClient.cs
        ├── Entities/
        │   ├── PlayerController.cs
        │   └── RemotePlayer.cs
        ├── World/
        │   └── WorldGrid.cs
        └── UI/
            └── GameHUD.cs
```

---

## Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| .NET SDK | 9.0 |
| Godot Engine | 4.3+ (versão .NET/Mono) |

---

## Como rodar

### 1. Servidor

```bash
cd Server
dotnet run --project TopdownMMO.WorldServer
```

O servidor inicia em `ws://localhost:7777/ws/` com game loop a 20 ticks/s.

> **Windows**: Se pedir permissão de firewall, aceite para `localhost`.

### 2. Cliente (Godot)

1. Abra o Godot 4 (.NET version)
2. Importe o projeto em `Client/`
3. O Godot vai gerar o `.godot/` e compilar os scripts C#
4. Pressione **F5** (ou Play) — a cena `Main.tscn` conecta automaticamente
5. Use **WASD** ou **setas** para mover

Para testar multiplayer, abra múltiplas instâncias do Godot (ou exporte e rode vários clientes).

---

## Fluxo de Comunicação

```
Cliente                          Servidor
  │                                │
  ├── ConnectRequest ──────────►   │  (nome do jogador)
  │                                │
  │   ◄────────── ConnectResponse  │  (playerId, success)
  │                                │
  ├── EnterWorld ──────────────►   │
  │                                │
  │   ◄──────── EnterWorldResponse │  (playerId, x, y)
  │   ◄──────── WorldSnapshot      │  (todos os jogadores)
  │                                │
  ├── MoveRequest ─────────────►   │  (dx, dy)
  │                                │
  │   ◄──────── PlayerMoved        │  (playerId, x, y) ← broadcast
  │                                │
  │   ◄──────── WorldSnapshot      │  (periódico, 1x/s)
```

---

## Exemplos de Mensagens JSON

### ConnectRequest (Client → Server)
```json
{
  "type": "connectRequest",
  "payload": {
    "playerName": "Player_042"
  }
}
```

### ConnectResponse (Server → Client)
```json
{
  "type": "connectResponse",
  "payload": {
    "playerId": "a1b2c3d4",
    "success": true
  }
}
```

### MoveRequest (Client → Server)
```json
{
  "type": "moveRequest",
  "payload": {
    "dx": 1,
    "dy": 0
  }
}
```

### PlayerMoved (Server → Client — broadcast)
```json
{
  "type": "playerMoved",
  "payload": {
    "playerId": "a1b2c3d4",
    "x": 6,
    "y": 5
  }
}
```

### WorldSnapshot (Server → Client — periódico)
```json
{
  "type": "worldSnapshot",
  "payload": {
    "players": [
      { "playerId": "a1b2c3d4", "name": "Player_042", "x": 6, "y": 5 },
      { "playerId": "e5f6g7h8", "name": "Player_099", "x": 3, "y": 7 }
    ]
  }
}
```

---

## Regras de Arquitetura

- **Servidor é autoridade total** — cliente nunca define posição final
- **Movimento em grid** — tiles de 32×32 px, sem diagonal no MVP
- **Sem interpolação** — posição salta direto (MVP)
- **Validação server-side** — colisão verificada antes de aplicar movimento
- **Separação clara**: Protocol (mensagens) → GameCore (lógica) → WorldServer (rede + loop)

---

## Próximos Passos (pós-MVP)

- [ ] Interpolação de movimento no cliente
- [ ] Sistema de chat
- [ ] NPCs e mobs com IA básica
- [ ] Inventário e itens
- [ ] Tilemap real (importar de editor de mapas)
- [ ] Autenticação de jogadores
- [ ] Persistência (banco de dados)
- [ ] Múltiplos mapas / áreas
