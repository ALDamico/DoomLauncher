﻿using DoomLauncher.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomLauncher.DataSources
{
    public class SourcePort : ISourcePort
    {
        public int SourcePortID { get; set; }
        public string Name { get; set; }
        public string SupportedExtensions { get; set; }
        public string SettingsFiles { get; set; }
        public string Executable { get; set; }
        public SourcePortLaunchType LaunchType { get; set; }
        public string FileOption { get; set; }

        public string GetFullExecutablePath()
        {
            return Path.Combine(Directory.GetFullPath(), Executable);
        }

        public LauncherPath Directory
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            ISourcePort sourcePort = obj as ISourcePort;

            if (sourcePort != null)
            {
                return sourcePort.SourcePortID == SourcePortID;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return SourcePortID;
        }

        public static string[] GetSupportedExtensions(ISourcePort sourcePort)
        {
            string[] supportedExtensions = new string[] { };

            if (sourcePort != null)
                supportedExtensions = sourcePort.SupportedExtensions.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);

            return supportedExtensions;
        }
    }
}