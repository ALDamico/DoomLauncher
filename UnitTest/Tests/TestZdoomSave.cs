using System;
using System.Threading.Tasks;
using DoomLauncher.SaveGame;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest.Tests
{
    [TestClass]
    public class TestZdoomSave
    {
        [TestMethod]
        public void TestZdoomSaveBinary()
        {
            string save1 = "zdoomsave_v1.zds";
            TestUtil.CopyResourceFile(save1);

            ZDoomSaveGameReader reader = new ZDoomSaveGameReader(save1);

            Assert.AreEqual("Autosave Aug 21 13:30", reader.GetName());
        }

        [TestMethod]
        public void TestZdoomSaveJson()
        {
            string save1 = "zdoomsave_v2.zds";
            TestUtil.CopyResourceFile(save1);

            ZDoomSaveGameReader reader = new ZDoomSaveGameReader(save1);

            Assert.AreEqual("Autosave Aug 21 13:12", reader.GetName());
        }

        [TestMethod]
        public void TestZdoomSaveJson_3_5()
        {
            string save1 = "zdoomsave_v3.zds";
            TestUtil.CopyResourceFile(save1);

            ZDoomSaveGameReader reader = new ZDoomSaveGameReader(save1);

            Assert.AreEqual("Autosave Aug 28 06:25", reader.GetName());
        }

        [TestMethod]
        public void TestZdoomGlobals()
        {
            string save1 = "save63.zds";
            TestUtil.CopyResourceFile(save1);

            ZDoomSaveGameReader reader = new ZDoomSaveGameReader(save1);

            var saveGame = reader.GetInfoFromFile(null, null);

            Assert.IsNotNull(saveGame.Picture);
            Assert.AreEqual(3, saveGame.FoundSecrets);
            Assert.AreEqual(3, saveGame.FoundItems);
            Assert.AreEqual(104, saveGame.KilledMonsters);
            Assert.AreEqual(182, saveGame.TotalMonsters);
            Assert.AreEqual(10, saveGame.TotalSecrets);
            Assert.AreEqual(3, saveGame.FoundSecrets);
            Assert.AreEqual(3, saveGame.FoundItems);
            Assert.AreEqual(8, saveGame.TotalItems);
            Assert.AreEqual(TimeSpan.FromSeconds(1), saveGame.MapTime);
        }

        [TestMethod]
        public async Task TestZdoomSaveAsync()
        {
            string save1 = "save63.zds";
            TestUtil.CopyResourceFile(save1);

            ZDoomSaveGameReader reader = new ZDoomSaveGameReader(save1);

            var saveGame = await reader.GetInfoFromFileAsync(null, null);

            Assert.IsNotNull(saveGame.Picture);
            Assert.AreEqual(3, saveGame.FoundSecrets);
            Assert.AreEqual(3, saveGame.FoundItems);
            Assert.AreEqual(104, saveGame.KilledMonsters);
            Assert.AreEqual(182, saveGame.TotalMonsters);
            Assert.AreEqual(10, saveGame.TotalSecrets);
            Assert.AreEqual(3, saveGame.FoundSecrets);
            Assert.AreEqual(3, saveGame.FoundItems);
            Assert.AreEqual(8, saveGame.TotalItems);
            Assert.AreEqual(TimeSpan.FromSeconds(1), saveGame.MapTime);
        }
    }
}
