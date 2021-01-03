using DoomLauncher.Interfaces;

namespace DoomLauncher.SaveGame
{
    public interface ISaveGameReader
    {
        string GetName();
        ISaveGameFile GetInfoFromFile(IGameFile gameFile, ISourcePortData sourcePort);
    }
}
