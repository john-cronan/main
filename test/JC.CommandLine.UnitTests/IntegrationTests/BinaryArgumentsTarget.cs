using System.ComponentModel;

namespace JC.CommandLine.UnitTests.IntegrationTests
{
    internal class BinaryArgumentsTarget : ISupportInitialize
    {
        public byte[] Hex { get; set; }
        public byte[] HexNoPreamble { get; set; }
        public byte[] Base64 { get; set; }
        public byte[] Base64NoFlags { get; set; }
        public byte SingleByte { get; set; }
        public string UnparsedString { get; set; }

        public bool BeginInitCalled { get; private set; }
        public bool EndInitCalled { get; private set; }

        void ISupportInitialize.BeginInit()
        {
            BeginInitCalled = true;
        }

        void ISupportInitialize.EndInit()
        {
            EndInitCalled = true;
        }
    }
}
