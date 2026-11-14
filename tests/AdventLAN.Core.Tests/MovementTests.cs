namespace AdventLAN.Core.Tests
{
    public static class MovementTests
    {
        [Test]
        public static void Gerade_5_Meter_pro_Sekunde()
        {
            var sim = TestWorld.Duel();
            TestWorld.Run(sim, 30, new PlayerInput { MoveX = 1f });
            Assert.Near(10f, sim.Players[0].Position.X, 0.01f);
            Assert.Near(10f, sim.Players[0].Position.Y, 0.001f);
        }

        [Test]
        public static void Diagonal_ist_nicht_schneller()
        {
            var sim = TestWorld.Duel();
            Vec2 start = sim.Players[0].Position;
            TestWorld.Run(sim, 30, new PlayerInput { MoveX = 1f, MoveY = 1f });
            Vec2 end = sim.Players[0].Position;
            Assert.Near(5f, (end - start).Length(), 0.01f);
        }

        [Test]
        public static void Eingabe_groesser_als_eins_wird_begrenzt()
        {
            var sim = TestWorld.Duel();
            TestWorld.Run(sim, 30, new PlayerInput { MoveX = 5f });
            Assert.Near(10f, sim.Players[0].Position.X, 0.01f);
        }

        [Test]
        public static void Rueckwaerts_und_seitwaerts_gleich_schnell()
        {
            var sim = TestWorld.Duel();
            sim.Players[0].Position = new Vec2(20f, 10f);
            sim.Players[0].AimAngle = 0f; // Blick nach +X

            TestWorld.Run(sim, 30, new PlayerInput { MoveX = -1f, AimAngle = 0f });
            Assert.Near(15f, sim.Players[0].Position.X, 0.01f, "rückwärts");

            TestWorld.Run(sim, 30, new PlayerInput { MoveY = 1f, AimAngle = 0f });
            Assert.Near(15f, sim.Players[0].Position.Y, 0.01f, "seitwärts");
        }

        [Test]
        public static void Wand_blockiert()
        {
            var sim = TestWorld.Duel(map: TestWorld.OpenArena(new Rect(8f, 0f, 9f, 20f)));
            TestWorld.Run(sim, 90, new PlayerInput { MoveX = 1f });
            Assert.True(sim.Players[0].Position.X <= 7.6f + 0.001f, "Spieler steckt in der Wand");
            Assert.True(sim.Players[0].Position.X >= 7.5f, "Spieler sollte an der Wand stehen");
        }

        [Test]
        public static void Gleitet_an_der_Wand_entlang()
        {
            var sim = TestWorld.Duel(map: TestWorld.OpenArena(new Rect(8f, 0f, 9f, 20f)));
            TestWorld.Run(sim, 30, new PlayerInput { MoveX = 1f, MoveY = 1f });
            Assert.True(sim.Players[0].Position.X <= 7.6f + 0.001f, "Spieler steckt in der Wand");
            Assert.Near(10f + 3.5355f, sim.Players[0].Position.Y, 0.05f);
        }

        [Test]
        public static void Kartenrand_begrenzt()
        {
            var sim = TestWorld.Duel();
            TestWorld.Run(sim, 120, new PlayerInput { MoveX = -1f, MoveY = 1f });
            Assert.Near(0.4f, sim.Players[0].Position.X, 0.001f);
            Assert.Near(19.6f, sim.Players[0].Position.Y, 0.001f);
        }

        [Test]
        public static void Zielwinkel_wird_sofort_uebernommen()
        {
            var sim = TestWorld.Duel();
            sim.Step(TestWorld.Inputs(new PlayerInput { AimAngle = 1.25f }));
            Assert.Near(1.25f, sim.Players[0].AimAngle, 0.0001f);
        }

        [Test]
        public static void Spieler_blockieren_sich_nicht()
        {
            var sim = TestWorld.Duel();
            sim.Players[1].Position = new Vec2(sim.Players[0].Position.X + 1f, sim.Players[0].Position.Y);
            TestWorld.Run(sim, 30, new PlayerInput { MoveX = 1f });
            Assert.Near(10f, sim.Players[0].Position.X, 0.01f);
        }
    }
}
