using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomLauncher.Interfaces
{
    public interface ISaveGameFile
    {
        string MapName { get; set; }
        string MapTitle { get; set; }
        string SaveName { get; set; }
        Image Picture { get; set; }
        int PlayerHealth { get; set; }
        int PlayerArmor { get; set; }
        DateTime? Timestamp { get; set; }
        TimeSpan MapTime { get; set; }
        TimeSpan? GameTime { get; set; }
        IIWadData IWadData { get; set; }
        ISourcePortData SourcePort { get; set; }
        int SkillLevel { get; set; }
        int ArmorType { get; set; }
        int GameEpisode { get; set; }
        int GameMap { get; set; }
        int FoundItems { get; set; }
        int FoundSecrets { get; set; }
        int KilledMonsters { get; set; }
        int TotalItems { get; set; }
        int TotalSecrets { get; set; }
        int TotalMonsters { get; set; }
        string FullName { get; set; }
    }
}
