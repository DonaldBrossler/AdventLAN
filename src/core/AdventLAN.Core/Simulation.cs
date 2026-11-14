using System;
using System.Collections.Generic;

namespace AdventLAN.Core
{
    /// <summary>
    /// Match-Simulation ohne Grafik und Netzwerk. Ein Aufruf von <see cref="Step"/> rechnet genau einen Tick.
    /// Gleiche Konfiguration, Karte, Seed und Eingaben ergeben denselben Verlauf.
    /// </summary>
    public sealed class Simulation
    {
        private const float SpawnClearance = 1.0f;

        private readonly GameConfig _cfg;
        private readonly MapData _map;
        private readonly Rng _rng;
        private readonly List<Player> _players = new List<Player>();
        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private readonly List<SimEvent> _events = new List<SimEvent>();
        private readonly float _dt;
        private readonly int _cooldownTicks;
        private readonly int _reloadTicks;
        private readonly int _respawnTicks;
        private readonly int _protectionTicks;
        private readonly int _timeLimitTicks;
        private int _nextProjectileId;

        public Simulation(GameConfig config, MapData map, ulong seed)
        {
            if (map.RedSpawns.Count == 0 || map.BlueSpawns.Count == 0)
                throw new ArgumentException("Jedes Team braucht mindestens einen Spawnpunkt", nameof(map));

            _cfg = config;
            _map = map;
            _rng = new Rng(seed);
            _dt = config.Dt;
            _cooldownTicks = Math.Max(1, config.ToTicks(1f / config.Weapon.ShotsPerSecond));
            _reloadTicks = Math.Max(1, config.ToTicks(config.Weapon.ReloadSeconds));
            _respawnTicks = Math.Max(1, config.ToTicks(config.RespawnSeconds));
            _protectionTicks = Math.Max(0, config.ToTicks(config.SpawnProtectionSeconds));
            _timeLimitTicks = config.ToTicks(config.TimeLimitSeconds);
        }

        public GameConfig Config => _cfg;
        public MapData Map => _map;
        public int Tick { get; private set; }
        public int ScoreRed { get; private set; }
        public int ScoreBlue { get; private set; }
        public bool Ended { get; private set; }

        /// <summary>Siegerteam nach Matchende, null bei Unentschieden oder laufendem Match.</summary>
        public Team? Winner { get; private set; }

        public IReadOnlyList<Player> Players => _players;
        public IReadOnlyList<Projectile> Projectiles => _projectiles;

        /// <summary>Ereignisse des letzten Ticks.</summary>
        public IReadOnlyList<SimEvent> Events => _events;

        public int GetScore(Team team) => team == Team.Red ? ScoreRed : ScoreBlue;

        /// <summary>Fügt einen Spieler hinzu. Die Id entspricht dem Index in der Eingabeliste von <see cref="Step"/>.</summary>
        public int AddPlayer(Team team)
        {
            var player = new Player(_players.Count, team);
            _players.Add(player);
            SpawnPlayer(player);
            player.ProtectionTicksLeft = _protectionTicks;
            return player.Id;
        }

        /// <summary>Rechnet einen Tick. Fehlende Einträge in <paramref name="inputs"/> zählen als „keine Eingabe“.</summary>
        public void Step(IReadOnlyList<PlayerInput> inputs)
        {
            _events.Clear();
            if (Ended) return;

            Tick++;
            for (int i = 0; i < _players.Count; i++)
                UpdatePlayer(_players[i], i < inputs.Count ? inputs[i] : default);

            UpdateProjectiles();

            for (int i = 0; i < _players.Count; i++)
            {
                Player p = _players[i];
                if (p.Alive && p.ProtectionTicksLeft > 0) p.ProtectionTicksLeft--;
            }

            CheckEnd();
        }

        // ---------------------------------------------------------------- Spieler

        private void UpdatePlayer(Player p, PlayerInput inp)
        {
            if (!p.Alive)
            {
                p.RespawnTicksLeft--;
                if (p.RespawnTicksLeft <= 0) Respawn(p);
                return;
            }

            if (p.CooldownTicksLeft > 0) p.CooldownTicksLeft--;
            p.AimAngle = inp.AimAngle;
            Move(p, inp);

            WeaponConfig w = _cfg.Weapon;
            if (p.ReloadTicksLeft > 0)
            {
                p.ReloadTicksLeft--;
                if (p.ReloadTicksLeft == 0) p.Ammo = w.MagazineSize;
            }
            else if (inp.Reload && p.Ammo < w.MagazineSize)
            {
                p.ReloadTicksLeft = _reloadTicks;
            }
            else if (inp.Fire && p.CooldownTicksLeft == 0 && p.Ammo > 0)
            {
                Shoot(p);
            }
        }

        private void Move(Player p, PlayerInput inp)
        {
            float mx = inp.MoveX;
            float my = inp.MoveY;
            float len2 = mx * mx + my * my;
            if (len2 > 1f)
            {
                float inv = 1f / MathF.Sqrt(len2);
                mx *= inv;
                my *= inv;
            }

            float step = _cfg.MoveSpeed * _dt;
            Vec2 pos = new Vec2(p.Position.X + mx * step, p.Position.Y + my * step);
            float r = _cfg.PlayerRadius;

            for (int pass = 0; pass < 3; pass++)
            {
                bool moved = false;
                for (int i = 0; i < _map.Obstacles.Count; i++)
                    if (Geo.ResolveCircleRect(ref pos, r, _map.Obstacles[i])) moved = true;
                if (!moved) break;
            }

            pos.X = Geo.Clamp(pos.X, r, _map.Width - r);
            pos.Y = Geo.Clamp(pos.Y, r, _map.Height - r);
            p.Position = pos;
        }

        private void Shoot(Player p)
        {
            WeaponConfig w = _cfg.Weapon;
            float angle = p.AimAngle;
            if (w.SpreadDegrees > 0f)
            {
                float spread = w.SpreadDegrees * (MathF.PI / 180f);
                angle += _rng.Range(-spread, spread);
            }

            var velocity = new Vec2(MathF.Cos(angle) * w.ProjectileSpeed, MathF.Sin(angle) * w.ProjectileSpeed);
            _projectiles.Add(new Projectile(_nextProjectileId++, p.Id, p.Team, p.Position, velocity));

            p.Ammo--;
            p.CooldownTicksLeft = _cooldownTicks;
            p.ProtectionTicksLeft = 0; // Schießen beendet den Spawnschutz
            if (p.Ammo == 0) p.ReloadTicksLeft = _reloadTicks; // leeres Magazin lädt automatisch nach
        }

        private void Respawn(Player p)
        {
            SpawnPlayer(p);
            p.ProtectionTicksLeft = _protectionTicks;
            _events.Add(new SimEvent(SimEventType.Respawn, p.Id, -1, 0));
        }

        private void SpawnPlayer(Player p)
        {
            p.Position = PickSpawn(p.Team, p.Id);
            p.Health = _cfg.MaxHealth;
            p.Alive = true;
            p.Ammo = _cfg.Weapon.MagazineSize;
            p.CooldownTicksLeft = 0;
            p.ReloadTicksLeft = 0;
            p.RespawnTicksLeft = 0;
        }

        private Vec2 PickSpawn(Team team, int exceptId)
        {
            List<Vec2> spawns = _map.GetSpawns(team);
            int free = 0;
            for (int s = 0; s < spawns.Count; s++)
                if (IsSpawnFree(spawns[s], exceptId)) free++;

            if (free == 0) return spawns[_rng.NextInt(spawns.Count)];

            int k = _rng.NextInt(free);
            for (int s = 0; s < spawns.Count; s++)
            {
                if (!IsSpawnFree(spawns[s], exceptId)) continue;
                if (k == 0) return spawns[s];
                k--;
            }
            return spawns[0];
        }

        private bool IsSpawnFree(Vec2 spawn, int exceptId)
        {
            float limit = SpawnClearance * SpawnClearance;
            for (int i = 0; i < _players.Count; i++)
            {
                Player o = _players[i];
                if (o.Id == exceptId || !o.Alive) continue;
                if ((o.Position - spawn).LengthSquared() < limit) return false;
            }
            return true;
        }

        // ------------------------------------------------------------- Projektile

        private void UpdateProjectiles()
        {
            WeaponConfig w = _cfg.Weapon;
            float r = _cfg.PlayerRadius;

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                Projectile pr = _projectiles[i];
                float stepLen = w.ProjectileSpeed * _dt;
                float remaining = w.Range - pr.Traveled;
                float len = Math.Min(stepLen, remaining);

                Vec2 from = pr.Position;
                Vec2 to = from + pr.Velocity * (len / w.ProjectileSpeed);

                // Frühester Treffer auf der Strecke: Hindernis oder gegnerischer Spieler.
                float bestT = 2f;
                int hitPlayer = -1;
                for (int o = 0; o < _map.Obstacles.Count; o++)
                {
                    if (Geo.SegmentRect(from, to, _map.Obstacles[o], out float t) && t < bestT)
                    {
                        bestT = t;
                        hitPlayer = -1;
                    }
                }

                for (int j = 0; j < _players.Count; j++)
                {
                    Player target = _players[j];
                    if (!target.Alive || target.Team == pr.Team || target.IsProtected) continue;
                    if (Geo.SegmentCircle(from, to, target.Position, r, out float t) && t < bestT)
                    {
                        bestT = t;
                        hitPlayer = j;
                    }
                }

                if (bestT <= 1f)
                {
                    if (hitPlayer >= 0) ApplyHit(pr, _players[hitPlayer]);
                    _projectiles.RemoveAt(i);
                    continue;
                }

                pr.Position = to;
                pr.Traveled += len;
                bool outside = to.X < 0f || to.Y < 0f || to.X > _map.Width || to.Y > _map.Height;
                if (outside || pr.Traveled >= w.Range - 1e-4f) _projectiles.RemoveAt(i);
            }
        }

        private void ApplyHit(Projectile pr, Player victim)
        {
            int damage = _cfg.Weapon.Damage;
            victim.Health = Math.Max(0, victim.Health - damage);
            _events.Add(new SimEvent(SimEventType.Hit, victim.Id, pr.OwnerId, damage));
            if (victim.Health > 0) return;

            victim.Alive = false;
            victim.RespawnTicksLeft = _respawnTicks;
            victim.ProtectionTicksLeft = 0;
            victim.ReloadTicksLeft = 0;
            victim.Deaths++;

            Player shooter = _players[pr.OwnerId];
            shooter.Kills++;
            if (shooter.Team == Team.Red) ScoreRed++;
            else ScoreBlue++;
            _events.Add(new SimEvent(SimEventType.Kill, victim.Id, shooter.Id, 0));
        }

        // ------------------------------------------------------------------ Match

        private void CheckEnd()
        {
            bool scoreReached = ScoreRed >= _cfg.ScoreLimit || ScoreBlue >= _cfg.ScoreLimit;
            bool timeUp = Tick >= _timeLimitTicks;
            if (!scoreReached && !timeUp) return;

            Ended = true;
            if (ScoreRed > ScoreBlue) Winner = Team.Red;
            else if (ScoreBlue > ScoreRed) Winner = Team.Blue;
            else Winner = null;

            int value = Winner.HasValue ? (int)Winner.Value : -1;
            _events.Add(new SimEvent(SimEventType.MatchEnded, -1, -1, value));
        }

        // ------------------------------------------------------------------- Hash

        /// <summary>Prüfsumme des gesamten Zustands. Für Determinismus-Tests und Replays.</summary>
        public ulong ComputeHash()
        {
            ulong h = 14695981039346656037UL;
            h = Mix(h, Tick);
            h = Mix(h, ScoreRed);
            h = Mix(h, ScoreBlue);
            h = Mix(h, Ended ? 1 : 0);
            h = Mix(h, (long)_rng.State);

            for (int i = 0; i < _players.Count; i++)
            {
                Player p = _players[i];
                h = Mix(h, BitConverter.SingleToInt32Bits(p.Position.X));
                h = Mix(h, BitConverter.SingleToInt32Bits(p.Position.Y));
                h = Mix(h, BitConverter.SingleToInt32Bits(p.AimAngle));
                h = Mix(h, p.Health);
                h = Mix(h, p.Alive ? 1 : 0);
                h = Mix(h, p.Ammo);
                h = Mix(h, p.CooldownTicksLeft);
                h = Mix(h, p.ReloadTicksLeft);
                h = Mix(h, p.ProtectionTicksLeft);
                h = Mix(h, p.RespawnTicksLeft);
                h = Mix(h, p.Kills);
                h = Mix(h, p.Deaths);
            }

            for (int i = 0; i < _projectiles.Count; i++)
            {
                Projectile pr = _projectiles[i];
                h = Mix(h, pr.Id);
                h = Mix(h, BitConverter.SingleToInt32Bits(pr.Position.X));
                h = Mix(h, BitConverter.SingleToInt32Bits(pr.Position.Y));
                h = Mix(h, BitConverter.SingleToInt32Bits(pr.Traveled));
            }
            return h;
        }

        private static ulong Mix(ulong h, long value)
        {
            unchecked
            {
                h ^= (ulong)value;
                h *= 1099511628211UL;
            }
            return h;
        }
    }
}
