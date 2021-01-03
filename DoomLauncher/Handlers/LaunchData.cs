using DoomLauncher.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DoomLauncher
{
    public class LaunchData
    {
        public LaunchData(IGameFile gameFile, IGameProfile gameProfile, IEnumerable<IGameFile> additionalGameFiles, ISaveGameFile saveGame = null)
        {
            GameFile = gameFile;
            GameProfile = gameProfile;
            if (additionalGameFiles == null)
                AdditionalGameFiles = new List<IGameFile>();
            else
                AdditionalGameFiles = additionalGameFiles.ToList();
            Success = true;
            AddSaveGame(saveGame);
        }

        public LaunchData(string errorTitle, string errorDescritpion)
        {
            Success = false;
            ErrorTitle = errorTitle;
            ErrorDescription = errorDescritpion;
        }

        public void AddSaveGame(ISaveGameFile saveGame)
        {
            if (saveGame != null)
            {
                SaveGame = saveGame;
                // Adding the -loadgame option explicitly in the Extra Parameters textbox overrides the checkbox value
                if (!Regex.Match(GameProfile.SettingsExtraParams, "-loadgame").Success)
                {
                    GameProfile.SettingsExtraParams = new StringBuilder(GameProfile.SettingsExtraParams)
                        .Append(" -loadgame ")
                        .Append(SaveGame.FullName)
                        .ToString();
                }
            }
        }

        public bool Success { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorDescription { get; set; }
        public IGameFile GameFile { get; set; }
        public IGameProfile GameProfile { get; set; }
        public List<IGameFile> AdditionalGameFiles { get; set; }
        public ISaveGameFile SaveGame { get; private set; }
    }
}
