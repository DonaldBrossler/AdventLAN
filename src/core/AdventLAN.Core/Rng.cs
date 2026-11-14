namespace AdventLAN.Core
{
    /// <summary>Kleiner deterministischer Zufallsgenerator (SplitMix64). Gleicher Seed ergibt gleiche Folge.</summary>
    public sealed class Rng
    {
        private ulong _state;

        public Rng(ulong seed)
        {
            _state = seed;
        }

        public ulong State => _state;

        public ulong NextU64()
        {
            unchecked
            {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        /// <summary>Zufallszahl in [0, 1).</summary>
        public float NextFloat() => (NextU64() >> 40) / 16777216f;

        /// <summary>Zufallszahl in [0, max).</summary>
        public int NextInt(int max) => (int)(NextU64() % (ulong)max);

        public float Range(float min, float max) => min + (max - min) * NextFloat();
    }
}
