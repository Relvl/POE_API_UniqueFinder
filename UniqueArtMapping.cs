using System.Reflection;
using ExileCore;
using Newtonsoft.Json;

namespace UniqueFinder;

public static class UniqueArtMapping
{
    private static Dictionary<string, List<string>> _mapping = new();
    private const string CustomUniqueArtMappingPath = "uniqueArtMapping.json";

    public static Dictionary<string, List<string>> Mapping(UniqueFinder plugin)
    {
        if (_mapping.Count != 0) return _mapping;

        var customFilePath = Path.Join(plugin.DirectoryFullName, CustomUniqueArtMappingPath);
        if (File.Exists(customFilePath))
        {
            DebugWindow.LogMsg($"UniqueFinder: Read {CustomUniqueArtMappingPath} from file system");
            ReadMapping(File.ReadAllText(customFilePath));
        }
        else
        {
            try
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"UniqueFinder.{CustomUniqueArtMappingPath}");
                if (stream is null)
                {
                    DebugWindow.LogError($"UniqueFinder: Assembly {CustomUniqueArtMappingPath} stream is null");
                    _mapping = new Dictionary<string, List<string>>();
                    return _mapping;
                }

                DebugWindow.LogMsg($"UniqueFinder: Read {CustomUniqueArtMappingPath} from assembly...");
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                ReadMapping(content);
                File.WriteAllText(customFilePath, content);
            }
            catch (Exception ex)
            {
                DebugWindow.LogError($"UniqueFinder: Unable to load embedded art mapping: {ex}");
                _mapping = new Dictionary<string, List<string>>();
                return _mapping;
            }
        }

        return _mapping;
    }

    private static void ReadMapping(string source)
    {
        try
        {
            _mapping = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(source) ?? new Dictionary<string, List<string>>();
            foreach (var (renderPath, names) in _mapping) names.RemoveAll(n => n.StartsWith("Replica") && !n.StartsWith("Replica Dragonfang"));
        }
        catch (Exception ex)
        {
            DebugWindow.LogError($"UniqueFinder: Unable to load art mapping: {ex}");
        }
    }
}