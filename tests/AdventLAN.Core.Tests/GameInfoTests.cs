namespace AdventLAN.Core.Tests
{
    public static class GameInfoTests
    {
        [Test]
        public static void Name_IstAdventLAN()
        {
            Assert.Equal("AdventLAN", GameInfo.Name);
        }
    }
}
