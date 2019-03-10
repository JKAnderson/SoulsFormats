using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats.AC4
{
    /// <summary>
    /// A funky file container used in Armored Core 4.
    /// </summary>
    public class Zero3
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<File> Files;
        
        public static Zero3 Read(string path)
        {
            var containers = new List<BinaryReaderEx>();
            int index = 0;
            string containerPath = Path.ChangeExtension(path, index.ToString("D3"));
            while (System.IO.File.Exists(containerPath))
            {
                containers.Add(new BinaryReaderEx(true, System.IO.File.OpenRead(containerPath)));
                index++;
                containerPath = Path.ChangeExtension(path, index.ToString("D3"));
            }

            var result = new Zero3(containers[0], containers);
            foreach (BinaryReaderEx br in containers)
                br.Stream.Close();
            return result;
        }

        internal Zero3(BinaryReaderEx br, List<BinaryReaderEx> containers)
        {
            br.BigEndian = true;

            int fileCount = br.ReadInt32();
            br.AssertInt32(0x10);
            br.AssertInt32(0x10);
            br.AssertInt32(0x800000);
            for (int i = 0; i < 16; i++)
                br.AssertInt32(0);

            Files = new List<File>(fileCount);
            for (int i = 0; i < fileCount; i++)
                Files.Add(new File(br, containers));
        }

        public class File
        {
            public string Name;
            public byte[] Bytes;

            internal File(BinaryReaderEx br, List<BinaryReaderEx> containers)
            {
                Name = br.ReadFixStr(0x40);
                int containerIndex = br.ReadInt32();
                uint fileOffset = br.ReadUInt32();
                int paddedSize = br.ReadInt32();
                int fileSize = br.ReadInt32();

                Bytes = containers[containerIndex].GetBytes(fileOffset * 0x10, fileSize);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
