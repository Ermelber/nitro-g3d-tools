using System.Collections.Generic;
using System.IO;
using LibEndianBinaryIO;

namespace LibNitroG3DTools.Converter.Binary.Nsbmd.Sbc
{
    public class Sbc
    {
        public Sbc()
        {

        }

        public Sbc(byte[] sbc)
        {
            using (var er = new EndianBinaryReaderEx(new MemoryStream(sbc), Endianness.LittleEndian))
            {
                while (er.BaseStream.Position < er.BaseStream.Length)
                {
                    CommandList.Add(SbcCommand.Read(er));
                }
            }
        }

        public List<SbcCommand> CommandList = new List<SbcCommand>();

        public byte[] Write()
        {
            using (var m = new MemoryStream())
            {
                var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);

                foreach (var cmd in CommandList)
                {
                    cmd.Write(ew);
                }

                return m.ToArray();
            }
        }
    }
}