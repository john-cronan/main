using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common
{
    public class CompoundFileWriter
    {
        private readonly ProtocolWriter _binaryWriter;

        public CompoundFileWriter(ProtocolWriter binaryWriter)
        {
            _binaryWriter = binaryWriter;
        }

        public Task WriteHeaderAsync(string key, string value)
            => WriteHeaderAsync(_binaryWriter.TextEncoding.GetBytes(key), _binaryWriter.TextEncoding.GetBytes(value));

        public async Task WriteHeaderAsync(byte[] key, byte[] value)
        {
            if (key == null || key.Length == 0) 
                throw new ArgumentNullException(nameof(key));
            if (key.Length > 0xff)
                throw new ArgumentException($"The key must be <= {0xff} bytes", nameof(key));
            if (value == null) 
                throw new ArgumentNullException(nameof(value));
            if (value.Length > 0xffff)
                throw new ArgumentException($"The value must be <= {0xffff} bytes", nameof(value));

            await Task.CompletedTask;
        }

        public async Task WriteDataAsync(byte[] bytes)
        {
            await Task.CompletedTask;
        }
    }


    internal enum CompoundFileFrameType : ushort
    {
        StartSectionFrame = 0x01ff,
        HeadersFrame = 0x02ff,
        ContinuationFrame = 0x03ff
    }

    public abstract class CompoundFileNode
    {
        public abstract Task WriteToAsync(Stream stream);
    }

    public class CompoundFileStartSectionNode : CompoundFileNode
    {
        private readonly byte[] _name;

        public CompoundFileStartSectionNode()
            : this(Array.Empty<byte>())
        {
        }

        public CompoundFileStartSectionNode(byte[] name)
        {
            _name = name;
        }

        public byte[] Name => _name;

        public override Task WriteToAsync(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class CompoundFileHeaderNode : CompoundFileNode
    {
        public static readonly Encoding DefaultTextEncoding = Encoding.UTF8;

        private readonly byte[] _name;
        private Encoding _textEncoding;
        private readonly byte[] _value;

        public CompoundFileHeaderNode(string name, string value)
            : this(name, value, DefaultTextEncoding)
        {
        }

        public CompoundFileHeaderNode(string name, string value, Encoding textEncoding)
            : this(textEncoding.GetBytes(name), textEncoding.GetBytes(value))
        {
            TextEncoding = textEncoding;
        }

        public CompoundFileHeaderNode(byte[] name, byte[] value)
        {
            _name = name;
            _value = value;
        }

        public Encoding TextEncoding { get => _textEncoding; set => _textEncoding = value; }
        public byte[] Name => _name;
        public byte[] Value => _value;


        public string NameAsString
        {
            get
            {
                if (TextEncoding == null)
                {
                    throw new InvalidOperationException("There is no current text encoding");
                }
                return _textEncoding.GetString(_name);
            }
        }

        public string ValueAsString
        {
            get
            {
                if (TextEncoding == null)
                {
                    throw new InvalidOperationException("There is no current text encoding");
                }
                return _textEncoding.GetString(_value);
            }
        }

        public override Task WriteToAsync(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class CompoundFileDataNode : CompoundFileNode
    {
        private readonly byte[] _data;

        public CompoundFileDataNode(byte[] data)
        {
            _data = data;
        }

        public byte[] Data => _data;

        public override Task WriteToAsync(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
