﻿using DoomLauncher.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DoomLauncher
{
    class LaunchData
    {
        public LaunchData(IGameFile gameFile, IGameProfile gameProfile, IEnumerable<IGameFile> additionalGameFiles)
        {
            GameFile = gameFile;
            GameProfile = gameProfile;
            if (additionalGameFiles == null)
                AdditionalGameFiles = new List<IGameFile>();
            else
                AdditionalGameFiles = additionalGameFiles.ToList();
            Success = true;
        }

        public LaunchData(string errorTitle, string errorDescritpion)
        {
            Success = false;
            ErrorTitle = errorTitle;
            ErrorDescription = errorDescritpion;
        }

        public bool Success { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorDescription { get; set; }
        public IGameFile GameFile { get; set; }
        public IGameProfile GameProfile { get; set; }
        public List<IGameFile> AdditionalGameFiles { get; set; }
    }
}
