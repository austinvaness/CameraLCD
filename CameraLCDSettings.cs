using Sandbox.ModAPI;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using VRage.FileSystem;

namespace avaness.CameraLCD
{
    public class CameraLCDSettings
    {
        private const string fileName = "CameraLCDSettings.xml";
        private static string FilePath => Path.Combine(MyFileSystem.UserDataPath, "Storage", fileName);

        protected CameraLCDSettings()
        { }

        public bool Enabled { get; set; } = true;
        public int Ratio { get; set; } = 2;
        public bool UpdateLOD { get; set; } = false;
        public bool HeadFix { get; set; } = true;
        public int Range { get; set; } = 40;
        public bool LockAspectRatio { get; set; } = false;

        public static CameraLCDSettings Load()
        {
            string file = FilePath;
            if(File.Exists(file))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CameraLCDSettings));
                    using (XmlReader xml = XmlReader.Create(file))
                    {
                        return (CameraLCDSettings)serializer.Deserialize(xml);
                    }
                }
                catch { }
            }

            return new CameraLCDSettings();
        }

        public void Save()
        {
            try
            {
                string file = FilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                XmlSerializer serializer = new XmlSerializer(typeof(CameraLCDSettings));
                using (StreamWriter stream = File.CreateText(file))
                {
                    serializer.Serialize(stream, this);
                }
            }
            catch { }
            
        }
    }

}