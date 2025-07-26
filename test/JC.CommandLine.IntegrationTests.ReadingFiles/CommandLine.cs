using System.Xml;
using System.Xml.Linq;

namespace JC.CommandLine.IntegrationTests.ReadingFiles
{
    internal class CommandLine
    {
        public XmlDocument XmlDoc { get; set; }
        public XDocument XDoc { get; set; }
        public string[] Lines { get; set; }
        public string Text { get; set; }
        public byte[] Bytes { get; set; }

        public int[] Numbers { get; set; }
    }
}
