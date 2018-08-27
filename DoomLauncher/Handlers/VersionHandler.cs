﻿using DoomLauncher.DataSources;
using DoomLauncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoomLauncher
{
    class VersionHandler
    {
        private readonly IDataSourceAdapter m_adapter;
        private readonly AppConfiguration m_appConfig;

        public event EventHandler UpdateProgress;
        public event EventHandler UpdateComplete;
        public int ProgressPercent { get; private set; }

        public VersionHandler(DataAccess access, IDataSourceAdapter adapter, AppConfiguration appConfig)
        {
            DataAccess = access;
            m_adapter = adapter;
            m_appConfig = appConfig;
        }

        public bool UpdateRequired()
        {
            AppVersion version = GetVersion();

            if (version == AppVersion.Unknown || version < AppVersion.Version_2_6_0)
            {
                return true;
            }

            return false;
        }

        private void ExecuteUpdate(Action update, AppVersion version)
        {
            if (GetVersion() < version || version == AppVersion.Unknown)
            {
                update();

                if (version != AppVersion.Unknown)
                    WriteVersion(version);
            }
        }

        public void HandleVersionUpdate()
        {
            if (UpdateRequired())
            {
                ExecuteUpdate(Pre_0_9_2, AppVersion.Unknown);
                ExecuteUpdate(Pre_1_0_0, AppVersion.Version_1_0_0);
                ExecuteUpdate(Pre_1_1_0, AppVersion.Version_1_1_0);
                ExecuteUpdate(Pre_1_2_0, AppVersion.Version_1_2_0);
                ExecuteUpdate(Pre_2_1_0, AppVersion.Version_2_1_0);
                ExecuteUpdate(Pre_2_2_0, AppVersion.Version_2_2_0);
                ExecuteUpdate(Pre_2_2_1, AppVersion.Version_2_2_1);
                ExecuteUpdate(Pre_2_3_0, AppVersion.Version_2_3_0);
                ExecuteUpdate(Pre_2_4_0, AppVersion.Version_2_4_0);
                ExecuteUpdate(Pre_2_4_1, AppVersion.Version_2_4_1);
                ExecuteUpdate(Pre_2_6_0, AppVersion.Version_2_6_0);
            }
        }

        private AppVersion GetVersion()
        {
            IConfigurationData config = m_adapter.GetConfiguration().Where(x => x.Name == "Version").FirstOrDefault();

            if (config != null)
                return (AppVersion)Convert.ToInt32(config.Value);

            return AppVersion.Unknown;
        }

        private void WriteVersion(AppVersion version)
        {
            IConfigurationData config = m_adapter.GetConfiguration().Where(x => x.Name == "Version").FirstOrDefault();

            if (config != null)
            {
                config.Value = Convert.ToInt32(version).ToString();
                m_adapter.UpdateConfiguration(config);
            }
            else
            {
                ConfigurationData newConfig = new ConfigurationData();
                newConfig.Name = "Version";
                newConfig.UserCanModify = false;
                newConfig.Value = Convert.ToInt32(version).ToString();
                m_adapter.InsertConfiguration(newConfig);
            }
        }

        public void Pre_1_0_0()
        {
            DataTable dt = DataAccess.ExecuteSelect("pragma table_info(GameFiles);").Tables[0];

            if (dt.Select("name = 'SettingsMap'").Count() == 0)
            {
                string query = @"alter table GameFiles add column 'SettingsMap' TEXT;
                alter table GameFiles add column 'SettingsSkill' TEXT;
                alter table GameFiles add column 'SettingsExtraParams' TEXT;
                alter table GameFiles add column 'SettingsFiles' TEXT;";

                DataAccess.ExecuteNonQuery(query);
            }

            dt = DataAccess.ExecuteSelect("select * from Configuration where Name = 'ColumnConfig'").Tables[0];

            if (dt.Rows.Count == 0)
            {
                string query = @"insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('SplitTopBottom', '475', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('SplitLeftRight', '680', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('AppWidth', '1024', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('AppHeight', '768', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('AppX', '0', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('AppY', '0', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('WindowState', 'Normal', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('ColumnConfig', '', '', 0);";
                DataAccess.ExecuteNonQuery(query);
            }
        }

        public void Pre_0_9_2()
        {
            DataTable dt = DataAccess.ExecuteSelect("select name from sqlite_master where type='table' and name='Configuration';").Tables[0];

            if (dt.Rows.Count == 0)
            {
                string query = @"CREATE TABLE 'Configuration' (
	                'ConfigID'	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	                'Name'	TEXT NOT NULL,
	                'Value'	TEXT NOT NULL,
	                'AvailableValues'	TEXT NOT NULL,
	                'UserCanModify'	INTEGER);";

                DataAccess.ExecuteNonQuery(query);

                query = string.Format(@"insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('IdGamesUrl', 'http://www.doomworld.com/idgames/', '', 1);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('ApiPage', 'api/api.php', '', 1);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('MirrorUrl', 'ftp://mancubus.net/pub/idgames/', 'Germany;ftp://ftp.fu-berlin.de/pc/games/idgames/;Idaho;ftp://mirrors.syringanetworks.net/idgames/;Greece;ftp://ftp.ntua.gr/pub/vendors/idgames/;Texas;ftp://mancubus.net/pub/idgames/;New York;http://youfailit.net/pub/idgames/;Florida;http://www.gamers.org/pub/idgames/;', 1);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('ScreenshotCaptureDirectories', '', '', 1);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('DateParseFormats', 'dd/M/yy;dd/MM/yyyy;dd MMMM yyyy;', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('CleanTemp', 'true', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('GameFileDirectory', '{0}', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('ScreenshotDirectory', '{1}', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('TempDirectory', '{2}', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('GameWadDirectory', '{3}', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('DemoDirectory', '{4}', '', 0);
                insert into Configuration (Name, Value, AvailableValues, UserCanModify) values('SaveGameDirectory', '{5}', '', 0);",
                ConfigurationManager.AppSettings["GameFileDirectory"], ConfigurationManager.AppSettings["ScreenshotDirectory"], ConfigurationManager.AppSettings["TempDirectory"],
                ConfigurationManager.AppSettings["GameWadDirectory"], ConfigurationManager.AppSettings["DemoDirectory"], ConfigurationManager.AppSettings["GameFileDirectory"] + "SaveGames\\");

                DataAccess.ExecuteNonQuery(query);

                DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + ConfigurationManager.AppSettings["GameFileDirectory"]  + "SaveGames\\");
                if (!di.Exists)
                    di.Create();
            }

            dt = DataAccess.ExecuteSelect("pragma table_info(Files);").Tables[0];
            
            if (dt.Select("name = 'OriginalFileName'").Count() == 0)
            {
                string query = @"alter table Files add column 'OriginalFileName' TEXT;
                alter table Files add column 'OriginalFilePath' TEXT;";

                DataAccess.ExecuteNonQuery(query);
            }
        }

        public void Pre_1_1_0()
        {
            DataTable dt = DataAccess.ExecuteSelect("select name from sqlite_master where type='table' and name='Tags';").Tables[0];
            //tag table update
            if (dt.Rows.Count == 0)
            {
                string query = @"CREATE TABLE 'Tags' (
	            'TagID'	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	            'Name'	TEXT NOT NULL,
	            'HasTab'	INTEGER NOT NULL);";
                DataAccess.ExecuteSelect(query);

                query = @"CREATE TABLE 'TagMapping' (
	            'FileID'	INTEGER NOT NULL,
	            'TagID'	INTEGER NOT NULL,
	            PRIMARY KEY(FileID,TagID));";
                DataAccess.ExecuteSelect(query);
            }

            dt = DataAccess.ExecuteSelect("pragma table_info(Tags);").Tables[0];

            if (dt.Select("name = 'Color'").Count() == 0)
            {
                DataAccess.ExecuteNonQuery("alter table Tags add column 'HasColor' int;");
                DataAccess.ExecuteNonQuery("alter table Tags add column 'Color' int;");
            }

            dt = DataAccess.ExecuteSelect("pragma table_info(GameFiles);").Tables[0];
            //GameFile map count update
            if (dt.Select("name = 'MapCount'").Count() == 0)
            {
#if DEBUG
#else
            FileInfo fi = new FileInfo("DoomLauncher.sqlite");
            fi.CopyTo(string.Format("DoomLauncher_{0}.sqlite.bak", Guid.NewGuid().ToString()));
#endif

                DataAccess.ExecuteNonQuery("alter table GameFiles add column 'MapCount' int;");

                GameFileGetOptions options = new GameFileGetOptions();
                options.SelectFields = new GameFileFieldType[] { GameFileFieldType.GameFileID, GameFileFieldType.Map };
                IEnumerable<IGameFile> gameFiles = m_adapter.GetGameFiles(options);

                GameFileFieldType[] updateFields = new GameFileFieldType[] { GameFileFieldType.MapCount };
                float total = gameFiles.Count();
                int count = 0;

                foreach (IGameFile gameFile in gameFiles)
                {
                    if (UpdateProgress != null)
                    {
                        ProgressPercent = Convert.ToInt32(count / total * 100);
                        UpdateProgress(this, new EventArgs());
                    }

                    if (gameFile.Map != null)
                    {
                        gameFile.MapCount = gameFile.Map.Count(x => x == ',') + 1;
                        m_adapter.UpdateGameFile(gameFile, updateFields);
                    }

                    count++;
                }
            }

            if (dt.Select("name = 'SettingsSpecificFiles'").Count() == 0)
            {
                DataAccess.ExecuteNonQuery("alter table GameFiles add column 'SettingsSpecificFiles' TEXT;");
            }

            dt = DataAccess.ExecuteSelect("select * from Configuration where Name = 'Version'").Tables[0];

            WriteVersion(AppVersion.Version_1_1_0);

            if (UpdateComplete != null)
            {
                UpdateComplete(this, new EventArgs());
            }
        }

        private void Pre_1_2_0()
        {

        }

        private void Pre_2_1_0()
        {
            DataTable dt = DataAccess.ExecuteSelect("pragma table_info(Files);").Tables[0];

            if (dt.Select("name = 'FileOrder'").Count() == 0)
            {
                DataAccess.ExecuteNonQuery("alter table Files add column 'FileOrder' int;");
                DataAccess.ExecuteNonQuery("update Files set FileOrder = 2");
            }

            dt = DataAccess.ExecuteSelect("pragma table_info(GameFiles);").Tables[0];

            if (dt.Select("name = 'MinutesPlayed'").Count() == 0)
            {
                DataAccess.ExecuteNonQuery("alter table GameFiles add column 'MinutesPlayed' int;");
                DataAccess.ExecuteNonQuery("update GameFiles set MinutesPlayed = 0");
            }
        }

        private void Pre_2_2_0()
        {
            DataTable dt = DataAccess.ExecuteSelect("select name from sqlite_master where type='table' and name='Stats';").Tables[0];

            if (dt.Rows.Count == 0)
            {
                DataAccess.ExecuteNonQuery("update Configuration set UserCanModify = 1 where Name = 'GameFileDirectory'");

                string query = @"CREATE TABLE 'Stats' (
	            'StatID'	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	            'GameFileID'	INTEGER NOT NULL,
                'KillCount'	INTEGER NOT NULL,
                'TotalKills'	INTEGER NOT NULL,
                'SecretCount'	INTEGER NOT NULL,
                'TotalSecrets'	INTEGER NOT NULL,
                'LevelTime'	REAL NOT NULL,
                'ItemCount'	INTEGER NOT NULL,
                'TotalItems'	INTEGER NOT NULL,
                'SourcePortID'	INTEGER NOT NULL,
                'MapName'	TEXT NOT NULL,
                'RecordTime'	TEXT NOT NULL);";

                DataAccess.ExecuteNonQuery(query);
            }
        }

        private void Pre_2_2_1()
        {
            IEnumerable<ISourcePort> sourcePorts = m_adapter.GetSourcePorts();

            foreach (ISourcePort sourcePort in sourcePorts)
            {
                if (sourcePort.SupportedExtensions.Contains(".pk3"))
                    sourcePort.SupportedExtensions = sourcePort.SupportedExtensions.Replace(".pk3", ".pk3,.pk7");

                List<DbParameter> parameters = new List<DbParameter>();
                parameters.Add(DataAccess.DbAdapter.CreateParameter("ext", sourcePort.SupportedExtensions));
                parameters.Add(DataAccess.DbAdapter.CreateParameter("SourcePortID", sourcePort.SourcePortID));
                DataAccess.ExecuteNonQuery("update SourcePorts set SupportedExtensions = @ext where SourcePortID = @SourcePortID", parameters);
            }
        }

        private void Pre_2_3_0()
        {
            IEnumerable<IStatsData> stats = m_adapter.GetStats();
            HashSet<IStatsData> statSet = new HashSet<IStatsData>(); 

            foreach(IStatsData stat in stats)
            {
                if (!statSet.Contains(stat))
                    statSet.Add(stat);
                else
                    m_adapter.DeleteStats(stat.StatID);
            }
        }

        private void Pre_2_4_0()
        {
            DataTable dt = DataAccess.ExecuteSelect("pragma table_info(IWads);").Tables[0];

            if (dt.Select("name = 'GameFileID'").Count() == 0)
            {
                DataAccess.ExecuteNonQuery("alter table IWads add column 'GameFileID' int;");

                IEnumerable<IIWadData> iwads = m_adapter.GetIWads();
                IEnumerable<IGameFile> gameFiles = m_adapter.GetGameFiles();

                foreach(IIWadData iwad in iwads)
                {
                    IGameFile find = gameFiles.Where(x => x.FileName.ToLower() == iwad.FileName.ToLower().Replace(".wad", ".zip")).FirstOrDefault();
                    if (find != null)
                    {
                        iwad.GameFileID = find.GameFileID;
                        m_adapter.UpdateIWad(iwad);
                    }
                }
            }

            dt = DataAccess.ExecuteSelect("pragma table_info(SourcePorts);").Tables[0];

            if (dt.Select("name = 'SettingsFiles'").Count() == 0)
            {
                DataAccess.ExecuteNonQuery("alter table SourcePorts add column 'SettingsFiles' TEXT;");
            }

            //There was a bug in previous versions that would set MapCount to 1 when there were no maps found
            DataAccess.ExecuteNonQuery("update GameFiles set MapCount = null where Map is null or length(Map) = 0");
        }

        private void Pre_2_4_1()
        {
            string[] saveExts = new string[] { "*.zds", "*.dsg", "*.esg" };
            foreach (string ext in saveExts)
            {
                string[] files = Directory.GetFiles(m_appConfig.DemoDirectory.GetFullPath(), ext);
                foreach(string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    FileInfo fiTo = new FileInfo(Path.Combine(m_appConfig.SaveGameDirectory.GetFullPath(), fi.Name));
                    if (fiTo.Exists)
                        fiTo.Delete();
                    fi.MoveTo(fi.FullName);
                }
            }
        }

        private void Pre_2_6_0()
        {
            DataTable dt = DataAccess.ExecuteSelect("pragma table_info(SourcePorts);").Tables[0];

            if (dt.Select("name = 'LaunchType'").Count() == 0)
                DataAccess.ExecuteNonQuery("alter table SourcePorts add column 'LaunchType' TEXT;");
            if (dt.Select("name = 'FileOption'").Count() == 0)
                DataAccess.ExecuteNonQuery("alter table SourcePorts add column 'FileOption' TEXT;");

            DataAccess.ExecuteNonQuery(string.Format("update SourcePorts set LaunchType = {0}", (int)SourcePortLaunchType.SourcePort));
            DataAccess.ExecuteNonQuery("update SourcePorts set FileOption = '-file'");

            dt = DataAccess.ExecuteSelect("pragma table_info(GameFiles);").Tables[0];

            if (dt.Select("name = 'SettingsStat'").Count() == 0)
            {
                DataAccess.ExecuteNonQuery("alter table GameFiles add column 'SettingsStat' INTEGER;");
                DataAccess.ExecuteNonQuery("update GameFiles set SettingsStat = 1");
            }
        }

        public DataAccess DataAccess { get; set; }
    }
}