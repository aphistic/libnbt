using System;
using System.IO;
using System.IO.Compression;
using LibNbt.Tags;

namespace LibNbt
{
    public class NbtFile : IDisposable
    {
        public NbtCompound RootTag { get; protected set; }
        protected string LoadedFile { get; set; }

        public NbtFile() 
        {
            LoadedFile = "";
        }
        public NbtFile(string fileName)
        {
            LoadedFile = fileName;
        }

        public virtual void LoadFile() { LoadFile(LoadedFile); }

        public virtual void LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(string.Format("Could not find NBT file: {0}", fileName), fileName);
            }

            LoadedFile = fileName;

            using (FileStream readFileStream = File.OpenRead(fileName))
            {
                using (var decStream = new GZipStream(readFileStream, CompressionMode.Decompress))
                {
                    using (var memStream = new MemoryStream((int)readFileStream.Length))
                    {
                        var buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = decStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            memStream.Write(buffer, 0, bytesRead);
                        }

                        // Make sure the stream is at the beginning
                        memStream.Seek(0, SeekOrigin.Begin);

                        // Make sure the first byte in this file is the tag for a TAG_Compound
                        if (memStream.ReadByte() == (int)NbtTagType.TAG_Compound)
                        {
                            var rootCompound = new NbtCompound();
                            rootCompound.ReadTag(memStream);

                            RootTag = rootCompound;
                        }
                        else
                        {
                            throw new InvalidDataException("File format does not start with a TAG_Compound");
                        }
                    }
                }
            }
        }

        public virtual void SaveFile(string fileName)
        {
            using (var saveStream = new MemoryStream())
            {
                if (RootTag != null)
                {
                    RootTag.WriteTag(saveStream);

                    saveStream.Seek(0, SeekOrigin.Begin);
                    using (FileStream saveFile = File.OpenWrite(fileName))
                    {
                        using (var compressStream = new GZipStream(saveFile, CompressionMode.Compress))
                        {
                            var buffer = new byte[4096];
                            int amtSaved;
                            while ((amtSaved = saveStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                compressStream.Write(buffer, 0, amtSaved);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}
