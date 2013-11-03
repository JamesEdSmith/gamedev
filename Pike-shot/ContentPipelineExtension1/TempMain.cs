using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace LevelPipeline
{
    public static class TempMain
    {
        public static void Main()
        {
            object testValue = new LevelContent();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter xmlWriter = XmlWriter.Create("test.xml", settings))
            {
                IntermediateSerializer.Serialize(xmlWriter, testValue, null);
            }
        }
    }
}