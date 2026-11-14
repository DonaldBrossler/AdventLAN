using System;
using System.Diagnostics;
using System.IO;
using AdventLAN.Content;
using AdventLAN.Core;
using AdventLAN.Core.Tests;

namespace SimBench
{
    /// <summary>
    /// Lastmessung: spielt ein volles Match mit Zufallsspielern und misst die Rechenzeit pro Tick.
    /// Aufruf: dotnet run --project tools/SimBench -c Release [Spieler-pro-Team ...]
    /// Das ist eine Messung, kein Test. Die Zahlen hängen vom Rechner ab.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            string root = FindRoot();
            GameConfig cfg = ContentLoader.LoadGameConfig(Path.Combine(root, "content", "game.json"));
            MapData map = ContentLoader.LoadMap(Path.Combine(root, "content", "maps", "test_arena.json"));
            cfg.ScoreLimit = 1000000; // volle Spielzeit

            int[] sizes = args.Length > 0 ? Array.ConvertAll(args, int.Parse) : new[] { 16, 32 };
            foreach (int perTeam in sizes) Measure(perTeam, cfg, map);
            return 0;
        }

        private static void Measure(int perTeam, GameConfig cfg, MapData map)
        {
            var sim = new Simulation(cfg, map, 1UL);
            for (int i = 0; i < perTeam; i++)
            {
                sim.AddPlayer(Team.Red);
                sim.AddPlayer(Team.Blue);
            }
            var bots = new RandomBots(2UL, sim.Players.Count);

            // Aufwärmen, damit der JIT-Compiler nicht in die Messung fällt.
            for (int i = 0; i < 300; i++) sim.Step(bots.MakeInputs(sim));

            double total = 0;
            double max = 0;
            int ticks = 0;
            int maxProjectiles = 0;
            while (!sim.Ended)
            {
                var inputs = bots.MakeInputs(sim);
                long t0 = Stopwatch.GetTimestamp();
                sim.Step(inputs);
                double ms = (Stopwatch.GetTimestamp() - t0) * 1000.0 / Stopwatch.Frequency;
                total += ms;
                if (ms > max) max = ms;
                if (sim.Projectiles.Count > maxProjectiles) maxProjectiles = sim.Projectiles.Count;
                ticks++;
            }

            Console.WriteLine(
                $"2 x {perTeam,2} Spieler: {ticks} Ticks, Ø {total / ticks:F3} ms/Tick, max {max:F3} ms, " +
                $"max. {maxProjectiles} Projektile gleichzeitig, Kills {sim.ScoreRed + sim.ScoreBlue}");
        }

        private static string FindRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "CLAUDE.md"))) return dir.FullName;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Projektwurzel (CLAUDE.md) nicht gefunden");
        }
    }
}
