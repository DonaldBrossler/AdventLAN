using System;
using System.Collections.Generic;

namespace AdventLAN.Core
{
    /// <summary>Werte der Standardwaffe. Startwerte siehe docs/features/F-001-kernmechanik.md.</summary>
    public sealed class WeaponConfig
    {
        public int Damage { get; set; } = 20;
        public float ShotsPerSecond { get; set; } = 8f;
        public float SpreadDegrees { get; set; } = 2f;
        public int MagazineSize { get; set; } = 30;
        public float ReloadSeconds { get; set; } = 2f;
        public float ProjectileSpeed { get; set; } = 60f;
        public float Range { get; set; } = 40f;
    }

    public sealed class GameConfig
    {
        public int TickRate { get; set; } = 30;
        public float PlayerRadius { get; set; } = 0.4f;
        public float MoveSpeed { get; set; } = 5f;
        public int MaxHealth { get; set; } = 100;
        public float RespawnSeconds { get; set; } = 2.5f;
        public float SpawnProtectionSeconds { get; set; } = 1.5f;
        public int ScoreLimit { get; set; } = 100;
        public float TimeLimitSeconds { get; set; } = 600f;
        public WeaponConfig Weapon { get; set; } = new WeaponConfig();

        public float Dt => 1f / TickRate;

        public int ToTicks(float seconds) =>
            (int)MathF.Round(seconds * TickRate, MidpointRounding.AwayFromZero);
    }

    public sealed class MapData
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public List<Rect> Obstacles { get; set; } = new List<Rect>();
        public List<Vec2> RedSpawns { get; set; } = new List<Vec2>();
        public List<Vec2> BlueSpawns { get; set; } = new List<Vec2>();

        public List<Vec2> GetSpawns(Team team) => team == Team.Red ? RedSpawns : BlueSpawns;

        /// <summary>Prüft die Karte. Gibt eine Liste von Problemen zurück, leer = in Ordnung.</summary>
        public List<string> Problems(float playerRadius)
        {
            var problems = new List<string>();
            if (Width <= 0f || Height <= 0f) problems.Add("Breite und Höhe müssen größer als 0 sein");
            if (RedSpawns.Count == 0) problems.Add("Team Rot hat keinen Spawnpunkt");
            if (BlueSpawns.Count == 0) problems.Add("Team Blau hat keinen Spawnpunkt");

            for (int i = 0; i < Obstacles.Count; i++)
            {
                Rect o = Obstacles[i];
                if (o.MinX >= o.MaxX || o.MinY >= o.MaxY)
                    problems.Add($"Hindernis {i} hat keine Fläche");
                if (o.MinX < 0f || o.MinY < 0f || o.MaxX > Width || o.MaxY > Height)
                    problems.Add($"Hindernis {i} liegt außerhalb der Karte");
            }

            CheckSpawns("Rot", RedSpawns, playerRadius, problems);
            CheckSpawns("Blau", BlueSpawns, playerRadius, problems);
            return problems;
        }

        private void CheckSpawns(string name, List<Vec2> spawns, float radius, List<string> problems)
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                Vec2 s = spawns[i];
                if (s.X < radius || s.Y < radius || s.X > Width - radius || s.Y > Height - radius)
                    problems.Add($"Spawn {name} {i} liegt außerhalb der Karte");
                foreach (Rect o in Obstacles)
                {
                    if (Geo.DistanceToRect(s, o) < radius)
                    {
                        problems.Add($"Spawn {name} {i} liegt in einem Hindernis");
                        break;
                    }
                }
            }
        }
    }
}
