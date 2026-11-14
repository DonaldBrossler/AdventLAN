using System;

namespace AdventLAN.Core.Tests
{
    public static class WeaponTests
    {
        [Test]
        public static void Schuss_trifft_Gegner_und_macht_Schaden()
        {
            var sim = TestWorld.Duel();
            TestWorld.FireOnce(sim, 25);
            Assert.Equal(80, sim.Players[1].Health);
            Assert.Equal(29, sim.Players[0].Ammo);
            Assert.Equal(0, sim.Projectiles.Count);
        }

        [Test]
        public static void Projektil_hat_Flugzeit()
        {
            var sim = TestWorld.Duel();
            TestWorld.FireOnce(sim, 5);
            Assert.Equal(100, sim.Players[1].Health);
            Assert.Equal(1, sim.Projectiles.Count);
        }

        [Test]
        public static void Feuerrate_acht_Schuss_pro_Sekunde()
        {
            var sim = TestWorld.Duel();
            TestWorld.Run(sim, 30, TestWorld.Fire());
            Assert.Equal(22, sim.Players[0].Ammo);
        }

        [Test]
        public static void Leeres_Magazin_laedt_automatisch_nach()
        {
            var sim = TestWorld.Duel();
            TestWorld.Run(sim, 117, TestWorld.Fire());
            Assert.Equal(0, sim.Players[0].Ammo);
            Assert.True(sim.Players[0].IsReloading, "sollte nachladen");

            TestWorld.Run(sim, 59, TestWorld.Fire());
            Assert.Equal(0, sim.Players[0].Ammo);

            TestWorld.Run(sim, 1, TestWorld.Fire());
            Assert.Equal(30, sim.Players[0].Ammo);
            Assert.False(sim.Players[0].IsReloading, "Nachladen sollte fertig sein");
        }

        [Test]
        public static void Nachladen_per_Taste_dauert_60_Ticks()
        {
            var sim = TestWorld.Duel();
            sim.Step(TestWorld.Inputs(TestWorld.Fire()));
            Assert.Equal(29, sim.Players[0].Ammo);

            sim.Step(TestWorld.Inputs(new PlayerInput { Reload = true }));
            TestWorld.Run(sim, 59);
            Assert.Equal(29, sim.Players[0].Ammo);

            TestWorld.Run(sim, 1);
            Assert.Equal(30, sim.Players[0].Ammo);
        }

        [Test]
        public static void Feuern_waehrend_Nachladen_wird_ignoriert()
        {
            var sim = TestWorld.Duel();
            sim.Step(TestWorld.Inputs(TestWorld.Fire()));
            sim.Step(TestWorld.Inputs(new PlayerInput { Reload = true }));
            TestWorld.Run(sim, 59, TestWorld.Fire());
            Assert.Equal(29, sim.Players[0].Ammo);
        }

        [Test]
        public static void Laufen_waehrend_Nachladen_ist_erlaubt()
        {
            var sim = TestWorld.Duel();
            sim.Step(TestWorld.Inputs(TestWorld.Fire()));
            sim.Step(TestWorld.Inputs(new PlayerInput { Reload = true }));
            float x = sim.Players[0].Position.X;
            TestWorld.Run(sim, 30, new PlayerInput { MoveX = 1f });
            Assert.Near(x + 5f, sim.Players[0].Position.X, 0.01f);
            Assert.True(sim.Players[0].IsReloading, "sollte noch nachladen");
        }

        [Test]
        public static void Streuung_bleibt_im_Rahmen()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.Weapon.SpreadDegrees = 2f;
            cfg.Weapon.MagazineSize = 1000;
            var sim = TestWorld.Duel(cfg);

            int lastId = -1;
            int shots = 0;
            float maxAbs = 0f;
            for (int i = 0; i < 400; i++)
            {
                sim.Step(TestWorld.Inputs(TestWorld.Fire()));
                for (int k = 0; k < sim.Projectiles.Count; k++)
                {
                    Projectile pr = sim.Projectiles[k];
                    if (pr.Id <= lastId) continue;
                    lastId = Math.Max(lastId, pr.Id);
                    float deg = MathF.Atan2(pr.Velocity.Y, pr.Velocity.X) * 180f / MathF.PI;
                    maxAbs = MathF.Max(maxAbs, MathF.Abs(deg));
                    shots++;
                }
            }
            Assert.True(shots >= 50, "zu wenige Schüsse: " + shots);
            Assert.True(maxAbs <= 2.001f, "Streuung zu groß: " + maxAbs);
            Assert.True(maxAbs >= 1f, "Streuung scheint nicht zu wirken: " + maxAbs);
        }

        [Test]
        public static void Projektil_stoppt_an_der_Wand()
        {
            var sim = TestWorld.Duel(map: TestWorld.OpenArena(new Rect(20f, 0f, 22f, 20f)));
            TestWorld.FireOnce(sim, 40);
            Assert.Equal(100, sim.Players[1].Health);
            Assert.Equal(0, sim.Projectiles.Count);
        }

        [Test]
        public static void Duenne_Wand_wird_nicht_durchflogen()
        {
            var sim = TestWorld.Duel(map: TestWorld.OpenArena(new Rect(20f, 0f, 20.05f, 20f)));
            TestWorld.FireOnce(sim, 40);
            Assert.Equal(100, sim.Players[1].Health);
        }

        [Test]
        public static void Treffer_am_Rand_des_Spielerkreises()
        {
            var sim = TestWorld.Duel();
            sim.Players[1].Position = new Vec2(35f, 10.3f);
            TestWorld.FireOnce(sim, 25);
            Assert.Equal(80, sim.Players[1].Health);
        }

        [Test]
        public static void Schuss_knapp_vorbei()
        {
            var sim = TestWorld.Duel();
            sim.Players[1].Position = new Vec2(35f, 10.5f);
            TestWorld.FireOnce(sim, 25);
            Assert.Equal(100, sim.Players[1].Health);
        }

        [Test]
        public static void Reichweite_endet_nach_40_Metern()
        {
            GameConfig cfg = TestWorld.Config();
            cfg.Weapon.Range = 20f;
            var sim = TestWorld.Duel(cfg);
            TestWorld.FireOnce(sim, 40);
            Assert.Equal(100, sim.Players[1].Health);
            Assert.Equal(0, sim.Projectiles.Count);
        }

        [Test]
        public static void Friendly_Fire_ist_aus()
        {
            var sim = new Simulation(TestWorld.Config(), TestWorld.OpenArena(), 1UL);
            sim.AddPlayer(Team.Red);
            sim.AddPlayer(Team.Red);
            sim.Players[1].Position = new Vec2(35f, 10f);
            TestWorld.FireOnce(sim, 25);
            Assert.Equal(100, sim.Players[1].Health);
        }
    }
}
