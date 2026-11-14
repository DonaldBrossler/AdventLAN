using System;

namespace AdventLAN.Core.Tests
{
    public static class SimulationTests
    {
        private static Simulation NewMatch(int perTeam, ulong seed, GameConfig cfg, MapData map)
        {
            var sim = new Simulation(cfg, map, seed);
            for (int i = 0; i < perTeam; i++)
            {
                sim.AddPlayer(Team.Red);
                sim.AddPlayer(Team.Blue);
            }
            return sim;
        }

        private static void CheckInvariants(Simulation sim, GameConfig cfg, MapData map)
        {
            float r = cfg.PlayerRadius;
            for (int i = 0; i < sim.Players.Count; i++)
            {
                Player p = sim.Players[i];
                string who = $"Spieler {i} bei Tick {sim.Tick}: ";
                Assert.True(p.Health >= 0 && p.Health <= cfg.MaxHealth, who + "Leben außerhalb 0..Max");
                Assert.True(p.Ammo >= 0 && p.Ammo <= cfg.Weapon.MagazineSize, who + "Magazin außerhalb 0..Max");
                Assert.True(p.Alive == (p.Health > 0), who + "Alive passt nicht zum Leben");
                if (!p.Alive) continue;

                Assert.True(p.Position.X >= r - 0.001f && p.Position.X <= map.Width - r + 0.001f
                         && p.Position.Y >= r - 0.001f && p.Position.Y <= map.Height - r + 0.001f,
                    who + "außerhalb der Karte");
                for (int o = 0; o < map.Obstacles.Count; o++)
                    Assert.True(Geo.DistanceToRect(p.Position, map.Obstacles[o]) >= r - 0.01f,
                        who + "steckt in Hindernis " + o);
            }
        }

        [Test]
        public static void Zufallsspiel_2x16_haelt_alle_Regeln_ein()
        {
            GameConfig cfg = TestWorld.LoadGameConfig();
            cfg.ScoreLimit = 1000000; // das Match soll die volle Zeit laufen
            MapData map = TestWorld.LoadTestArena();
            Simulation sim = NewMatch(16, 42UL, cfg, map);
            var bots = new RandomBots(7UL, sim.Players.Count);

            int killEvents = 0;
            while (!sim.Ended)
            {
                sim.Step(bots.MakeInputs(sim));
                for (int i = 0; i < sim.Events.Count; i++)
                    if (sim.Events[i].Type == SimEventType.Kill) killEvents++;
                CheckInvariants(sim, cfg, map);
            }

            Assert.Equal(cfg.ToTicks(cfg.TimeLimitSeconds), sim.Tick);
            int kills = 0;
            int deaths = 0;
            for (int i = 0; i < sim.Players.Count; i++)
            {
                kills += sim.Players[i].Kills;
                deaths += sim.Players[i].Deaths;
            }
            Assert.Equal(killEvents, kills, "Kill-Ereignisse und Kills");
            Assert.Equal(kills, deaths, "Kills und Tode");
            Assert.Equal(kills, sim.ScoreRed + sim.ScoreBlue, "Kills und Punkte");
            Assert.True(kills >= 10, "Die Zufallsspieler haben kaum getroffen: " + kills);
        }

        [Test]
        public static void Gleicher_Seed_gleicher_Verlauf()
        {
            ulong a = RunHash(7UL, 3000);
            ulong b = RunHash(7UL, 3000);
            Assert.Equal(a, b);
        }

        [Test]
        public static void Anderer_Seed_anderer_Verlauf()
        {
            Assert.True(RunHash(7UL, 3000) != RunHash(8UL, 3000), "Seeds sollten sich unterscheiden");
        }

        private static ulong RunHash(ulong seed, int ticks)
        {
            GameConfig cfg = TestWorld.LoadGameConfig();
            MapData map = TestWorld.LoadTestArena();
            Simulation sim = NewMatch(16, seed, cfg, map);
            var bots = new RandomBots(seed + 1000UL, sim.Players.Count);
            for (int i = 0; i < ticks; i++) sim.Step(bots.MakeInputs(sim));
            return sim.ComputeHash();
        }

        [Test]
        public static void Viele_Spieler_auf_wenigen_Spawnpunkten_funktionieren()
        {
            GameConfig cfg = TestWorld.LoadGameConfig();
            MapData map = TestWorld.LoadTestArena();
            Simulation sim = NewMatch(32, 3UL, cfg, map);
            var bots = new RandomBots(5UL, sim.Players.Count);
            for (int i = 0; i < 1500; i++)
            {
                sim.Step(bots.MakeInputs(sim));
                CheckInvariants(sim, cfg, map);
            }
            Assert.Equal(64, sim.Players.Count);
        }
    }
}
