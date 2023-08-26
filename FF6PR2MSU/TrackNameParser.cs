using System.Reflection;

namespace FF6PR2MSU;

public class TrackNameParser
{
    private const string LOOKUP_TABLES_FILE_NAME = "LookupTables.ini";

    // Haven't found a more graceful way to check which keys have different languages' vocals
    private static readonly string[] FF6_VOCAL_TRACK_KEYS = { "32b", "33", "34c" };

    private readonly Dictionary<string, string> wav2msuTable;

    /// <param name="gameCode">Game's code as </param>
    /// <param name="languageCode">Code of the language chosen by the user for the vocal tracks (applicable for FFVI only)</param>
    public TrackNameParser(string gameCode, string? languageCode = null)
    {
        StreamReader? iniStream = null;
        languageCode ??= string.Empty;

        try
        {
            if (!File.Exists(LOOKUP_TABLES_FILE_NAME))
            {
                throw new Exception($"{LOOKUP_TABLES_FILE_NAME} could not be found.");
            }

            iniStream = new StreamReader(LOOKUP_TABLES_FILE_NAME);

            if (iniStream == null)
            {
                throw new Exception("The INI file could not be read.");
            }

            var parser = new IniParser.FileIniDataParser();
            var section = parser?.ReadData(iniStream)?.Sections.GetSectionData(gameCode);

            if (section == null)
            {
                throw new Exception("This game is not supported or the INI file is invalid.");
            }

            // Copying data in a Dictionary just because it's simpler to use
            this.wav2msuTable = new Dictionary<string, string>();

            foreach (var data in section.Keys)
            {
                if (gameCode == Program.GAME_CODE_FF6 && FF6_VOCAL_TRACK_KEYS.Any(c => data.KeyName.StartsWith(c) && c + languageCode != data.KeyName))
                {
                    continue;
                }

                var value = data.Value;

                // APPARENTLY... the parser can't understand comment characters when used at the end of lines... So I have to handle them myself.
                // What's the point of using an external library if it won't even properly parse the data...? Might as well redo it myself. TBD.
                if (value.Contains(';'))
                {
                    value = value.Remove(value.IndexOf(';')).Trim();
                }

                wav2msuTable.Add(data.KeyName, value);
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            iniStream?.Close();
        }
    }

    public string LookupName(string gameTrackCode)
    {
        if (!wav2msuTable.TryGetValue(gameTrackCode, out string? pcmNumber))
        {
            return null;
        }

        return pcmNumber;
    }
}