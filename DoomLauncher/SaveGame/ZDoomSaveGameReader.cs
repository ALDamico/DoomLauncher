using System;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DoomLauncher.Interfaces;
using DoomLauncher.SaveGame.GZDoomHelperClasses;

namespace DoomLauncher.SaveGame
{
    public class ZDoomSaveGameReader : ISaveGameReader
    {
        private readonly string m_file;

        public ZDoomSaveGameReader(string file)
        {
            m_file = file;
        }

        public string GetName()
        {
            string name = GetNameFromJson(m_file);

            if (name == null)
                name = GetNameFromBinary(m_file);

            if (name == null)
                return Path.GetFileName(m_file);

            return name;
        }

        private static string GetNameFromBinary(string file)
        {
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(file));
            long startPos = Util.ReadAfter(ms, Encoding.UTF8.GetBytes("tEXtTitle"));

            if (startPos != -1)
            {
                ms.Position = ++startPos;
                long endPos = Util.ReadAfter(ms, new byte[] {0, 0, 0});

                if (endPos != -1)
                {
                    int totalLength = (int) (endPos - startPos - 7);
                    if (totalLength > 0)
                    {
                        if (totalLength > 24)
                            totalLength = 24;
                        ms.Position = startPos;
                        byte[] strData = new byte[totalLength];
                        ms.Read(strData, 0, totalLength);
                        return Encoding.UTF8.GetString(strData);
                    }
                }
            }

            return null;
        }

        private static string GetNameFromJson(string file)
        {
            try
            {
                using (ZipArchive za = ZipFile.OpenRead(file))
                {
                    var entry = za.Entries.FirstOrDefault(x => x.Name.Equals("info.json"));

                    if (entry != null)
                    {
                        using (var stream = new StreamReader(entry.Open()))
                        {
                            JObject obj = JsonConvert.DeserializeObject(stream.ReadToEnd()) as JObject;
                            var saveName = obj.Values().FirstOrDefault(x => x.Path == "Title");
                            if (saveName != null)
                                return saveName.ToString();
                        }
                    }
                }
            }
            catch
            {
                //continue to return the filename if we fail for any reason
            }

            return null;
        }

        public ISaveGameFile GetInfoFromFile(IIWadData iWadData, ISourcePortData sourcePort)
        {
            using (ZipArchive za = ZipFile.OpenRead(m_file))
            {
                ISaveGameFile saveGame = new SaveGameFile();
                var entries = za.Entries;
                GZDoomGlobals globals = null;
                int totalTimeTics = -1;
                
                foreach (var entry in entries)
                {
                    if (entry.Name.Equals("info.json"))
                    {
                        var jsonInfo = ReadInfoJsonFile(entry);
                        if (jsonInfo != null)
                        {
                            saveGame.MapTitle = jsonInfo.MapTitle;
                        }
                    }

                    if (entry.Name.Equals("savepic.png"))
                    {
                        var pic = ReadSavepic(entry);
                        saveGame.Picture = pic;
                    }

                    if (entry.Name.Equals("globals.json"))
                    {
                        globals = GetInfoFromGlobalsFile(entry);
                        if (globals.LevelTime != null)
                        {
                            saveGame.MapTime = globals.LevelTime.Value;
                        }

                        saveGame.SkillLevel = globals.SkillLevel;
                    }

                    if (Regex.Match(entry.Name, "\\.map\\.json").Success)
                    {
                        var saveMapInfo = ReadInfoFromMapFile(entry, globals);
                        saveGame.FoundSecrets = saveMapInfo.FoundSecrets;
                        saveGame.FoundItems = saveMapInfo.FoundItems;
                        saveGame.KilledMonsters = saveMapInfo.KilledMonsters;
                        saveGame.TotalItems = saveMapInfo.TotalItems;
                        saveGame.TotalMonsters = saveMapInfo.TotalMonsters;
                        saveGame.TotalSecrets = saveMapInfo.TotalSecrets;
                        totalTimeTics = saveMapInfo.GameTime;
                    }
                }
                if (globals?.TicRate > 0 && totalTimeTics >= 0)
                {
                    var totalTimeSeconds = totalTimeTics / globals.TicRate;
                    saveGame.GameTime = TimeSpan.FromSeconds(totalTimeSeconds);
                }
                saveGame.Timestamp = File.GetLastWriteTime(m_file);

                return saveGame;
            }
        }

        private GZDoomInfo ReadInfoJsonFile(ZipArchiveEntry entry)
        {
            using (var stream = new StreamReader(entry.Open()))
            {
                var gzDoomInfo = new GZDoomInfo();
                ISaveGameFile saveGame = new SaveGameFile();
                JObject obj = JsonConvert.DeserializeObject(stream.ReadToEnd()) as JObject;
                if (obj == null)
                {
                    return null;
                }

                var mapTitle = obj.Values().FirstOrDefault(x => x.Path.Equals("Current Map"));
                if (mapTitle != null)
                {
                    gzDoomInfo.MapTitle = mapTitle.ToString();
                }

                return gzDoomInfo;
            }
        }

        private Image ReadSavepic(ZipArchiveEntry entry)
        {
            var stream = entry.Open();
            return Image.FromStream(stream);
        }

        private GZDoomGlobals GetInfoFromGlobalsFile(ZipArchiveEntry entry)
        {
            using (var stream = new StreamReader(entry.Open()))
            {
                GZDoomGlobals globals = new GZDoomGlobals();
                JObject obj = JsonConvert.DeserializeObject(stream.ReadToEnd()) as JObject;
                if (obj == null)
                {
                    return null;
                }

                var serverVars = obj.Values().FirstOrDefault(x => x.Path.Equals("servercvars")) as JObject;
                if (serverVars == null)
                {
                    return null;
                }

                var skillLevel = serverVars["skill"];
                if (skillLevel != null)
                {
                    globals.SkillLevel = skillLevel.ToObject<int>();
                }
                else
                {
                    globals.SkillLevel = 0;
                }

                var levelTimeJToken = obj["ticrate"];
                if (levelTimeJToken != null)
                {
                    var levelTime = levelTimeJToken.ToObject<int>();
                    var ticRateJToken = obj["ticrate"];
                    if (ticRateJToken != null)
                    {
                        var ticRate = ticRateJToken.ToObject<int>();
                        if (ticRate > 0)
                        {
                            var computedSeconds = levelTime / ticRate;
                            var timeTaken = TimeSpan.FromSeconds(computedSeconds);
                            globals.LevelTime = timeTaken;
                        }
                    }
                }

                return globals;
            }
        }

        private GZDoomSaveMapInfo ReadInfoFromMapFile(ZipArchiveEntry entry, GZDoomGlobals globals)
        {
            using (var stream = new StreamReader(entry.Open()))
            {
                GZDoomSaveMapInfo saveMapInfo = new GZDoomSaveMapInfo();
                var jObj = JsonConvert.DeserializeObject(stream.ReadToEnd()) as JObject;
                if (jObj == null)
                {
                    return null;
                }

                var foundSecretsJToken = jObj["found_secrets"];
                if (foundSecretsJToken != null)
                {
                    var foundSecrets = foundSecretsJToken.ToObject<int>();
                    saveMapInfo.FoundSecrets = foundSecrets;
                }

                var foundItemsJToken = jObj["found_items"];
                if (foundItemsJToken != null)
                {
                    var foundItems = foundItemsJToken.ToObject<int>();
                    saveMapInfo.FoundItems = foundItems;
                }

                var killedMonstersJToken = jObj["killed_monsters"];
                if (killedMonstersJToken != null)
                {
                    var killedMonsters = killedMonstersJToken.ToObject<int>();
                    saveMapInfo.KilledMonsters = killedMonsters;
                }

                var totalSecretsJToken = jObj["total_secrets"];
                if (totalSecretsJToken != null)
                {
                    var totalSecrets = totalSecretsJToken.ToObject<int>();
                    saveMapInfo.TotalSecrets = totalSecrets;
                }

                var totalItemsJToken = jObj["total_items"];
                if (totalItemsJToken != null)
                {
                    var totalItems = totalItemsJToken.ToObject<int>();
                    saveMapInfo.TotalItems = totalItems;
                }

                var totalMonstersJToken = jObj["total_monsters"];
                if (totalMonstersJToken != null)
                {
                    var totalMonsters = totalMonstersJToken.ToObject<int>();
                    saveMapInfo.TotalMonsters = totalMonsters;
                }

                var totalTimeJToken = jObj["totaltime"];
                if (totalTimeJToken != null)
                {
                    var totalTimeTics = totalTimeJToken.ToObject<int>();
                    saveMapInfo.GameTime = totalTimeTics;
                   
                }

                return saveMapInfo;
            }
        }
    }
}