using System;

namespace DoomLauncher.SaveGame.GZDoomHelperClasses
{
    public class GZDoomSaveMapInfo
    {
        public int FoundSecrets { get; set; }
        public int FoundItems { get; set; }
        public int KilledMonsters { get; set; }
        public int TotalSecrets { get; set; }
        public int TotalItems { get; set; }
        public int TotalMonsters { get; set; }
        public int GameTime { get; set; }
    }
}