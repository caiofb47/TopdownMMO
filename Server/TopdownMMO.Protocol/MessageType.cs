namespace TopdownMMO.Protocol;

/// <summary>
/// Tipos de mensagem trocadas entre cliente e servidor.
/// </summary>
public enum MessageType
{
    // ── Client → Server ──
    ConnectRequest  = 1,
    EnterWorld      = 2,
    MoveRequest     = 3,

    // ── Server → Client ──
    ConnectResponse = 100,
    EnterWorldResponse = 101,
    PlayerMoved     = 102,
    PlayerDisconnected = 103,
    WorldSnapshot   = 110,
}
