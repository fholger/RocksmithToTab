using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RocksmithToTabLib
{
    public class GpxContainer
    {
        public static Stream CreateGPXContainer(Stream score)
        {
            const int HEADER_BCFS = 1397113666;
            const int sectorSize = 0x1000;
            // The container format used in GPX is some sort of file system with a FAT.
            // I don't understand the format fully, and it seems rather ridiculous for the 
            // purpose. I only understand enough to "fake" it. The data is organised in 
            // sectors of 4096 bytes each, where a sector can be a file entry containing
            // a filename, filesize and list of other sectors containing the actual file data.
            // The very first sector after the file header is almost complete filled with 0xff,
            // except for the first four bytes, which can vary. I don't know their purpose, but
            // from tests it seems that Guitar Pro doesn't mind at all if you fill this first
            // sector with random data.
            // The next sector appears to be a root file index. I don't fully know all the details,
            // but it seems to be mostly constant.
            // Apart from that, we only need to create a single sector for a single file entry,
            // score.gpix, and as many data sectors as are needed to store the file.

            var output = new MemoryStream();
            var writer = new BinaryWriter(output);
            var firstSector = new byte[sectorSize];
            var nullData = new byte[sectorSize];
            for (var i = 0; i < sectorSize; ++i)
            {
                firstSector[i] = 0xff;
                nullData[i] = 0x00;
            }
            firstSector[0] = firstSector[1] = 0x00;

            // prepare file data
            byte[] data;
            using (var mem = new MemoryStream())
            {
                score.CopyTo(mem);
                data = mem.ToArray();
            }
            int numberOfSectors = (data.Length + sectorSize - 1) / sectorSize;

            // write header
            writer.Write(HEADER_BCFS);
            // write first sector
            writer.Write(firstSector, 0, sectorSize);
            // next sector (root dir?) is identified by the number 1
            writer.Write((int)1);
            // write filename '/'
            writer.Write('/');
            writer.Write(nullData, 0, 131);
            // I don't fully understand what the next numbers mean, but these
            // static values seem to work
            writer.Write((int)1);
            writer.Write((int)4); // might be the last sector that contains a file entry
            writer.Write((int)0);
            writer.Write((int)3); // first sector with file entry?
            writer.Write(nullData, 0, 3944); // fill up rest of sector

            // now comes the file entry for the score.gpif file, identified by number 2
            writer.Write((int)2);
            writer.Write("score.gpif".ToArray());
            writer.Write(nullData, 0, 122);
            writer.Write((int)1);
            // file size
            writer.Write(data.Length);
            writer.Write((int)0);
            // next is the list of sectors containing the file contents
            for (int i = 4; i < 4 + numberOfSectors; ++i)
                writer.Write(i);
            // fill up sector
            writer.Write(nullData, 0, 3948 - numberOfSectors * 4);

            // this next sector links to other sectors containing file entries (I think)
            // but we don't have those, so it's basically empty.
            writer.Write((int)2);
            writer.Write(nullData, 0, sectorSize - 4);

            // finally, write actual file content
            writer.Write(data, 0, data.Length);
            // fill final sector
            writer.Write(nullData, 0, sectorSize - (data.Length % sectorSize));

            // one filler byte, apparently required
            writer.Write((byte)0);

            output.Position = 0;
            return output;
        }


        public static Stream CompressGPX(Stream input)
        {
            // Compressing the GPX container is entirely optional, as Guitar Pro can read both
            // uncompressed and compressed files. The compression is a rather primitive home-made
            // algorithm and doesn't achieve stellar compression ratios.

            const int HEADER_BCFZ = 1514554178;

            byte[] content;
            using (var mem = new MemoryStream())
            {
                input.CopyTo(mem);
                content = mem.ToArray();
            }

            var output = new MemoryStream();
            var writer = new BinaryWriter(output);

            // write file header and content size
            writer.Write(HEADER_BCFZ);
            writer.Write(content.Length);

            // rest is created in a bit array
            var bits = new BitwiseWriter();
            int pos = 0;

            int maxSize = 0;

            while (pos < content.Length)
            {
                // we can write an uncompressed block of up to 3 bytes, or specify a block of the
                // already written data to be copied. not trying to be clever, we'll just start with
                // the next 4 bytes, see if they occured previously, and if so, double the search string
                // until we have found the largest previous occurence.
                // there are any number of more sophisticated approaches, one being to build a suffix
                // tree to find the largest common substring. However, I have a feeling it would not
                // dramatically improve the compression, although it might improve performance...
                int size = 4;
                int offset = Math.Max(0, pos - 0x7fff);
                int foundPos = 0;
                while ((foundPos = FindPreviousOccurence(content, offset, pos, size)) != -1)
                {
                    offset = foundPos;
                    size *= 2;
                }
                size /= 2;

                if (size > 2)
                {
                    // successful match found
                    bits.WriteBit(1);
                    offset = pos - offset;  // reverse the offset
                    // determine highest bit in size or offset
                    int bitLength = 0;
                    for (int i = 0; i < 15; ++i)
                    {
                        int shift = 1 << i;
                        if (((size & shift) != 0) || ((offset & shift) != 0))
                        {
                            bitLength = i + 1;
                        }
                    }

                    bits.WriteWordLE(bitLength, 4);
                    bits.WriteWordBE(offset, bitLength);
                    bits.WriteWordBE(size, bitLength);

                    maxSize = Math.Max(maxSize, size);
                }

                else
                {
                    size = Math.Min(3, content.Length - pos);
                    bits.WriteBit(0);
                    bits.WriteWordBE(size, 2);
                    bits.WriteBytes(content, pos, size);
                }

                pos += size;
            }

            writer.Write(bits.Finalize());
            output.Position = 0;
            return output;
        }

        static int FindPreviousOccurence(byte[] data, int start, int position, int size)
        {
            if (size > data.Length - position)
                return -1;

            for (int i = start; i < position - size; ++i)
            {
                bool found = true;
                for (int j = 0; j < size; ++j)
                {
                    if (data[i + j] != data[position + j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return i;
            }

            return -1;
        }
    }


    class BitwiseWriter
    {
        MemoryStream stream;
        byte buf;
        int pos;

        public BitwiseWriter()
        {
            stream = new MemoryStream();
            buf = 0;
            pos = 7;
        }

        public void WriteBit(byte val)
        {
            //Debug.Assert(val == 0 || val == 1);
            val = (byte)(val << (pos--));
            buf |= val;
            if (pos == -1)
            {
                stream.WriteByte(buf);
                buf = 0;
                pos = 7;
            }
        }

        public void WriteByte(byte val)
        {
            for (int i = 7; i >= 0; --i)
            {
                byte bit = (byte)((val & (1 << i)) >> i);
                WriteBit(bit);
            }
        }

        public void WriteBytes(byte[] val, int offset, int count)
        {
            for (int i = offset; i < offset + count; ++i)
            {
                WriteByte(val[i]);
            }
        }

        public void WriteWordLE(int word, int numBits)
        {
            for (int i = numBits - 1; i >= 0; --i)
            {
                byte bit = (byte)((word & (1 << i)) >> i);
                WriteBit(bit);
            }
        }

        public void WriteWordBE(int word, int numBits)
        {
            for (int i = 0; i < numBits; ++i)
            {
                byte bit = (byte)((word & (1 << i)) >> i);
                WriteBit(bit);
            }
        }

        public byte[] Finalize()
        {
            if (pos != 7)
                stream.WriteByte(buf);
            return stream.ToArray();
        }
    }
}
