namespace AdventLAN.Core.Tests
{
    public static class RulesTests
    {
        /// <summary>Lässt Spieler 0 feuern, bis ein Kill passiert. Gibt den Tick des Kills zurück oder -1.</summary>
        private static int RunUntilKill(Simulation sim, int maxTicks)
        {
            for (int i = 0; i < maxTicks; i++)
            {
                sim.Step(TestWorld.Inputs(TestWorld.Fire()));
                if (TestWorld.HasEvent(sim, SimEventType.Kill)) return sim.Tick;
            }
            return -1;
        }

        [Test]
        public static void Fuenf_Treffer_toeten_und_zaehlen_fuer_den_Schuetzen()
        {
            var sim = TestWorld.Duel();
            int killTick = RunUntilKill(sim, 100);
            Assert.True(killTick > 0, "kein Kill");
            Assert.False(sim.Players[1].Alive, "Ziel lebt noch");
            Assert.Equal(0, sim.Players[1].Health);
            Assert.Equal(1, sim.Players[0].Kills);
            Assert.Equal(1, sim.Players[1].Deaths);
            Assert.Equal(1, sim.ScoreRed);
            Assert.Equal(0, sim.ScoreBlue);
        }

        [Test]
        public static void Respawn_nach_75_Ticks_mit_vollem_Leben_und_Magazin()
        {
            var sim = TestWorld.Duel();
            RunUntilKill(sim, 100);
            TestWorld.Run(sim, 74);
            Assert.False(sim.Players[1].Alive, "zu früh wieder lebendig");

            TestWorld.Run(sim, 1);
            Assert.True(sim.Players[1].Alive, "sollte jetzt respawnen");
            Assert.True(TestWorld.HasEvent(sim, SimEventType.Respawn), "Respawn-Ereignis fehlt");
            Assert.Equal(100, sim.Players[1].Health);
            Assert.Equal(30, sim.Players[1].Ammo);
        }

        [Test]
        public static void Respawn_an_Spawnpunkt_des_eigenen_Teams()
        {
            var sim = TestWorld.Duel();
            RunUntilKill(sim, 100);
            TestWorld.Run(sim, 75);
            Assert.Near(35f, sim.Players[1].Position.X, 0.001f);
            Assert.Near(10f, sim.Players[1].Position.Y, 0.001f);
        }

        [Test]
        public static void Spawnschutz_verhindert_Schaden()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.SpawnProtectionSeconds = 1.5f;
            var sim = TestWorld.Duel(cfg);
            TestWorld.FireOnce(sim, 25);
            Assert.Equal(100, sim.Players[1].Health);
        }

        [Test]
        public static void Spawnschutz_dauert_45_Ticks()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.SpawnProtectionSeconds = 1.5f;
            var sim = TestWorld.Duel(cfg);
            TestWorld.Run(sim, 44);
            Assert.True(sim.Players[1].IsProtected, "sollte noch geschützt sein");
            TestWorld.Run(sim, 1);
            Assert.False(sim.Players[1].IsProtected, "Schutz sollte vorbei sein");
        }

        [Test]
        public static void Schiessen_beendet_den_Spawnschutz()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.SpawnProtectionSeconds = 1.5f;
            var sim = TestWorld.Duel(cfg);
            Assert.True(sim.Players[0].IsProtected, "sollte zu Beginn geschützt sein");
            sim.Step(TestWorld.Inputs(TestWorld.Fire()));
            Assert.False(sim.Players[0].IsProtected, "Schutz sollte enden");
        }

        [Test]
        public static void Respawn_gibt_wieder_Spawnschutz()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.SpawnProtectionSeconds = 1.5f;
            var sim = TestWorld.Duel(cfg);
            TestWorld.Run(sim, 45);
            RunUntilKill(sim, 100);
            TestWorld.Run(sim, 75);
            Assert.True(sim.Players[1].Alive, "sollte respawnt sein");
            Assert.True(sim.Players[1].IsProtected, "sollte nach Respawn geschützt sein");
        }

        [Test]
        public static void Matchende_bei_Punktelimit()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.ScoreLimit = 2;
            var sim = TestWorld.Duel(cfg);
            TestWorld.Run(sim, 600, TestWorld.Fire());
            Assert.True(sim.Ended, "Match sollte beendet sein");
            Assert.Equal(2, sim.ScoreRed);
            Assert.Equal(Team.Red, sim.Winner!.Value);
        }

        [Test]
        public static void Matchende_bei_Zeitlimit_ist_unentschieden_ohne_Punkte()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.TimeLimitSeconds = 2f;
            var sim = TestWorld.Duel(cfg);
            TestWorld.Run(sim, 59);
            Assert.False(sim.Ended, "zu früh beendet");
            TestWorld.Run(sim, 1);
            Assert.True(sim.Ended, "sollte nach 60 Ticks enden");
            Assert.False(sim.Winner.HasValue, "sollte unentschieden sein");
            Assert.True(TestWorld.HasEvent(sim, SimEventType.MatchEnded), "Ereignis fehlt");
        }

        [Test]
        public static void Zeitlimit_gewinnt_das_Team_mit_mehr_Punkten()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.TimeLimitSeconds = 4f;
            var sim = TestWorld.Duel(cfg);
            TestWorld.Run(sim, 120, TestWorld.Fire());
            Assert.True(sim.Ended, "sollte beendet sein");
            Assert.Equal(1, sim.ScoreRed);
            Assert.Equal(Team.Red, sim.Winner!.Value);
        }

        [Test]
        public static void Nach_Matchende_aendert_sich_nichts_mehr()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.TimeLimitSeconds = 1f;
            var sim = TestWorld.Duel(cfg);
            TestWorld.Run(sim, 30);
            Assert.True(sim.Ended, "sollte beendet sein");
            ulong hash = sim.ComputeHash();
            TestWorld.Run(sim, 20, TestWorld.Fire());
            Assert.Equal(hash, sim.ComputeHash());
        }
    }
}
