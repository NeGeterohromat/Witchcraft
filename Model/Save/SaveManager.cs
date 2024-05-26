using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace PixelRPG
{
    public static class SaveManager
    {
        private static string filePaths;

        public static void AddFilePath()=>filePaths = @"/saves/0.sav";
        
        public static void SaveGame(GameModel model)
        {
            var bf = new BinaryFormatter();
            AddFilePath();
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = appDir+filePaths;
            var fs = new FileStream(fullPath, FileMode.Create);
            var save = new Save(model);
            bf.Serialize(fs, save);
            fs.Close();
        }

        public static GameModel LoadGame() 
        {
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = appDir + filePaths;
            if (!File.Exists(fullPath))
                throw new Exception("File path is incorrect");
            var bf = new BinaryFormatter();
            var fs = new FileStream(fullPath, FileMode.Open);
            var save = (Save)bf.Deserialize(fs);
            fs.Close();
            return GameModel.GetModel(save);
        }
    }
}
