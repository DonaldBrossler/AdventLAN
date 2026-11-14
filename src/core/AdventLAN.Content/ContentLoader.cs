using System.IO;
using System.Text.Json;
using AdventLAN.Core;

namespace AdventLAN.Content
{
    /// <summary>Lädt Spielwerte und Karten aus den JSON-Dateien in content/.</summary>
    public static class ContentLoader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static GameConfig LoadGameConfig(string path) => Load<GameConfig>(path);

        public static MapData LoadMap(string path) => Load<MapData>(path);

        private static T Load<T>(string path) where T : class
        {
            string json = File.ReadAllText(path);
            T? value = JsonSerializer.Deserialize<T>(json, Options);
            if (value == null) throw new InvalidDataException(path + ": Datei ist leer");
            return value;
        }
    }
}
