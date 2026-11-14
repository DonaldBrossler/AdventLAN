using System;
using System.IO;
using AdventLAN.Content;

namespace AdventLAN.Core.Tests
{
    /// <summary>Hilfen für Tests: kleine Karten, Standardwerte, Projektwurzel.</summary>
    public static class TestWorld
    {
        /// <summary>Standardwerte ohne Streuung und ohne Spawnschutz, damit Treffer vorhersagbar sind.</summary>
        public static GameConfig Config()
        {
            var c = new GameConfig();
            c.SpawnProtectionSeconds = 0f;
            c.Weapon.SpreadDegrees = 0f;
            return c;
        }

        /// <summary>40 × 20 m, Rot spawnt bei (5, 10), Blau bei (35, 10).</summary>
        public static MapData OpenArena(params Rect[] obstacles)
        {
            var m = new MapData { Width = 40f, Height = 20f };
            m.Obstacles.AddRange(obstacles);
            m.RedSpawns.Add(new Vec2(5f, 10f));
            m.BlueSpawns.Add(new Vec2(35f, 10f));
            return m;
        }

        /// <summary>Ein Spieler pro Team: Id 0 = Rot (Schütze), Id 1 = Blau (Ziel).</summary>
        public static Simulation Duel(GameConfig? config = null, MapData? map = null, ulong seed = 1UL)
        {
            var sim = new Simulation(config ?? Config(), map ?? OpenArena(), seed);
            sim.AddPlayer(Team.Red);
            sim.AddPlayer(Team.Blue);
            return sim;
        }

        public static PlayerInput[] Inputs(params PlayerInput[] inputs) => inputs;

        public static PlayerInput Fire() => new PlayerInput { Fire = true };

        public static void Run(Simulation sim, int ticks, params PlayerInput[] inputs)
        {
            for (int i = 0; i < ticks; i++) sim.Step(inputs);
        }

        /// <summary>Ein Schuss im ersten Tick, danach keine Eingabe.</summary>
        public static void FireOnce(Simulation sim, int totalTicks)
        {
            sim.Step(Inputs(Fire()));
            Run(sim, totalTicks - 1);
        }

        public static bool HasEvent(Simulation sim, SimEventType type)
        {
            for (int i = 0; i < sim.Events.Count; i++)
                if (sim.Events[i].Type == type) return true;
            return false;
        }

        public static string Root()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "CLAUDE.md"))) return dir.FullName;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Projektwurzel (CLAUDE.md) nicht gefunden");
        }

        public static GameConfig LoadGameConfig() =>
            ContentLoader.LoadGameConfig(Path.Combine(Root(), "content", "game.json"));

        public static MapData LoadTestArena() =>
            ContentLoader.LoadMap(Path.Combine(Root(), "content", "maps", "test_arena.json"));
    }
}
