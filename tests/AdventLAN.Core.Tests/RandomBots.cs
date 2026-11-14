using System;

namespace AdventLAN.Core.Tests
{
    /// <summary>
    /// Einfache Zufallssteuerung für Simulationstests und die Lastmessung:
    /// läuft in wechselnde Richtungen, zielt auf den nächsten Gegner und schießt in Reichweite.
    /// </summary>
    public sealed class RandomBots
    {
        private readonly Rng _rng;
        private readonly Vec2[] _dir;
        private readonly int[] _until;
        private readonly PlayerInput[] _inputs;

        public RandomBots(ulong seed, int playerCount)
        {
            _rng = new Rng(seed);
            _dir = new Vec2[playerCount];
            _until = new int[playerCount];
            _inputs = new PlayerInput[playerCount];
        }

        public PlayerInput[] MakeInputs(Simulation sim)
        {
            for (int i = 0; i < _inputs.Length; i++)
            {
                Player p = sim.Players[i];
                if (sim.Tick >= _until[i])
                {
                    float angle = _rng.Range(0f, 2f * MathF.PI);
                    float speed = _rng.NextFloat() < 0.25f ? 0f : 1f;
                    _dir[i] = new Vec2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
                    _until[i] = sim.Tick + 20 + _rng.NextInt(40);
                }

                float bestDist2 = float.MaxValue;
                Vec2 target = p.Position;
                for (int j = 0; j < sim.Players.Count; j++)
                {
                    Player e = sim.Players[j];
                    if (!e.Alive || e.Team == p.Team) continue;
                    float d2 = (e.Position - p.Position).LengthSquared();
                    if (d2 < bestDist2)
                    {
                        bestDist2 = d2;
                        target = e.Position;
                    }
                }

                bool enemyInRange = bestDist2 < 30f * 30f;
                Vec2 delta = target - p.Position;
                _inputs[i] = new PlayerInput
                {
                    MoveX = _dir[i].X,
                    MoveY = _dir[i].Y,
                    AimAngle = enemyInRange ? MathF.Atan2(delta.Y, delta.X) : p.AimAngle,
                    Fire = enemyInRange,
                    Reload = !enemyInRange && p.Ammo < 15
                };
            }
            return _inputs;
        }
    }
}
