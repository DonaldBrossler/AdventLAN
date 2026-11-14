namespace AdventLAN.Core
{
    public enum Team
    {
        Red = 0,
        Blue = 1
    }

    /// <summary>Eingabe eines Spielers für genau einen Tick.</summary>
    public struct PlayerInput
    {
        public float MoveX;
        public float MoveY;
        public float AimAngle;
        public bool Fire;
        public bool Reload;
    }

    public sealed class Player
    {
        public int Id { get; }
        public Team Team { get; }
        public Vec2 Position { get; internal set; }
        public float AimAngle { get; internal set; }
        public int Health { get; internal set; }
        public bool Alive { get; internal set; }
        public int Ammo { get; internal set; }
        public int CooldownTicksLeft { get; internal set; }
        public int ReloadTicksLeft { get; internal set; }
        public int ProtectionTicksLeft { get; internal set; }
        public int RespawnTicksLeft { get; internal set; }
        public int Kills { get; internal set; }
        public int Deaths { get; internal set; }

        public bool IsReloading => ReloadTicksLeft > 0;
        public bool IsProtected => Alive && ProtectionTicksLeft > 0;

        internal Player(int id, Team team)
        {
            Id = id;
            Team = team;
        }
    }

    public sealed class Projectile
    {
        public int Id { get; }
        public int OwnerId { get; }
        public Team Team { get; }
        public Vec2 Position { get; internal set; }
        public Vec2 Velocity { get; }
        public float Traveled { get; internal set; }

        internal Projectile(int id, int ownerId, Team team, Vec2 position, Vec2 velocity)
        {
            Id = id;
            OwnerId = ownerId;
            Team = team;
            Position = position;
            Velocity = velocity;
        }
    }

    public enum SimEventType
    {
        Hit,
        Kill,
        Respawn,
        MatchEnded
    }

    /// <summary>
    /// Ereignis eines Ticks. Hit: Subject = Getroffener, Other = Schütze, Value = Schaden.
    /// Kill: Subject = Getöteter, Other = Schütze. Respawn: Subject = Spieler.
    /// MatchEnded: Value = Siegerteam (0 Rot, 1 Blau) oder -1 bei Unentschieden.
    /// </summary>
    public readonly struct SimEvent
    {
        public SimEventType Type { get; }
        public int Subject { get; }
        public int Other { get; }
        public int Value { get; }

        public SimEvent(SimEventType type, int subject, int other, int value)
        {
            Type = type;
            Subject = subject;
            Other = other;
            Value = value;
        }
    }
}
