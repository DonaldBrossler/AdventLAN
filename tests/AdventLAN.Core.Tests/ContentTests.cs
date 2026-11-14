using System.Linq;

namespace AdventLAN.Core.Tests
{
    public static class ContentTests
    {
        [Test]
        public static void Spielwerte_entsprechen_der_Spec()
        {
            GameConfig c = TestWorld.LoadGameConfig();
            Assert.Equal(30, c.TickRate);
            Assert.Near(0.4f, c.PlayerRadius, 0.0001f);
            Assert.Near(5f, c.MoveSpeed, 0.0001f);
            Assert.Equal(100, c.MaxHealth);
            Assert.Equal(75, c.ToTicks(c.RespawnSeconds));
            Assert.Equal(45, c.ToTicks(c.SpawnProtectionSeconds));
            Assert.Equal(100, c.ScoreLimit);
            Assert.Equal(18000, c.ToTicks(c.TimeLimitSeconds));
            Assert.Equal(20, c.Weapon.Damage);
            Assert.Equal(30, c.Weapon.MagazineSize);
            Assert.Equal(60, c.ToTicks(c.Weapon.ReloadSeconds));
            Assert.Equal(4, c.ToTicks(1f / c.Weapon.ShotsPerSecond));
            Assert.Near(60f, c.Weapon.ProjectileSpeed, 0.0001f);
            Assert.Near(40f, c.Weapon.Range, 0.0001f);
            Assert.Near(2f, c.Weapon.SpreadDegrees, 0.0001f);
        }

        [Test]
        public static void Testkarte_ist_gueltig()
        {
            GameConfig c = TestWorld.LoadGameConfig();
            MapData map = TestWorld.LoadTestArena();
            var problems = map.Problems(c.PlayerRadius);
            Assert.True(problems.Count == 0, string.Join("; ", problems));
            Assert.Equal(60f, map.Width);
            Assert.Equal(40f, map.Height);
            Assert.True(map.Obstacles.Count >= 12, "zu wenige Hindernisse");
            Assert.True(map.RedSpawns.Count >= 8 && map.BlueSpawns.Count >= 8, "zu wenige Spawnpunkte");
        }

        [Test]
        public static void Kartenpruefung_findet_Fehler()
        {
            var map = new MapData { Width = 10f, Height = 10f };
            map.Obstacles.Add(new Rect(4f, 4f, 6f, 6f));
            map.RedSpawns.Add(new Vec2(5f, 5f));   // im Hindernis
            map.BlueSpawns.Add(new Vec2(0.1f, 5f)); // zu nah am Rand
            var problems = map.Problems(0.4f);
            Assert.True(problems.Any(p => p.Contains("Hindernis")), "Spawn im Hindernis nicht erkannt");
            Assert.True(problems.Any(p => p.Contains("außerhalb")), "Spawn am Rand nicht erkannt");
        }
    }
}
