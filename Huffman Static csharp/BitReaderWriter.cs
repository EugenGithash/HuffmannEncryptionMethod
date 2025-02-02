﻿using System;
using System.IO;

namespace BitReaderWriter
{
    class BitReader : IDisposable
    {
        FileStream fileStream;

        byte BufferReader; // 0 - 255
        int NumberOfReadBits;
        public long NumberOfBitsInFile;
  
        public BitReader(string filePath)
        {
            OpenFile(filePath);
            NumberOfReadBits = 0;
            BufferReader = 0;
        }

        //~BitReader()
        //{
        //    CloseFile();
        //}

        private bool IsBufferEmpty()
        {
            return NumberOfReadBits == 0 ? true : false;
        }

        private int ReadBit()
        {
            if (IsBufferEmpty())
            {
                BufferReader = (byte)fileStream.ReadByte();
                NumberOfReadBits = 8;
            }

            // take first bit
            int bitToReturn = 0;
            int mask = 1 << (NumberOfReadBits - 1);
            bitToReturn = (BufferReader & mask) >> (NumberOfReadBits - 1);

            NumberOfReadBits--;
            return bitToReturn; 
        }

        public uint ReadNBits(int nr) //nr will be a value [1..32]
        {
            uint result = 0;
            for (var i = nr - 1; i >= 0; i--)
            {
                byte bit = (byte)ReadBit();
                result = (result << 1) + bit; // add bit to result
            }
            return result;
        }

        void OpenFile(string filePath)
        {
            fileStream = new FileStream(filePath, FileMode.Open);
            NumberOfBitsInFile = fileStream.Length * 8;
        }

        public void Dispose()
        {
            fileStream.Dispose();
        }
    }

    class BitWriter : IDisposable
    {
        FileStream fileStream;

        private byte BufferWrite;
        private int NumberOfBitsWrite;

        public BitWriter(string filePath)
        {
            OpenFile(filePath);
            NumberOfBitsWrite = 0;
            BufferWrite = 0;
        }

        //~BitWriter()
        //{
        //    CloseFile();
        //}


        private bool IsBufferFull()
        {
            return NumberOfBitsWrite == 8 ? true : false;
        }


        private void WriteBit(uint value, bool check = false)
        {
            BufferWrite = (byte)(BufferWrite << 1);
            BufferWrite += (byte)value;

            NumberOfBitsWrite++;
            if (IsBufferFull())
            {
                NumberOfBitsWrite = 0;
                fileStream.WriteByte(BufferWrite);
                BufferWrite = 0;
            }
        }

        public void WriteNBits(int nr, uint value,out uint nr_of_one_bits,bool check=false) 
        {
            nr_of_one_bits = 0;
            bool get_out = false;
            for(var i = nr - 1; i >= 0; i--)
            {
                // take first bit
                uint bitToWrite = 0;
                int mask = 1 << (i);
                bitToWrite = (uint)((value & mask) >> i);
                nr_of_one_bits++;
               
                NumberOfBitsWrite++;
                if (IsBufferFull() && check)
                {
                    get_out = true;
                }
                NumberOfBitsWrite--;
                WriteBit(bitToWrite); // write bit
                if(get_out)
                {
                    return;
                }
            }
        }

        public void WriteNBits(int nr, uint value)
        {
            for (var i = nr - 1; i >= 0; i--)
            {
                // take first bit
                uint bitToWrite = 0;
                int mask = 1 << (i);
                bitToWrite = (uint)((value & mask) >> i);
                WriteBit(bitToWrite); // write bit
            }
        }


        void OpenFile(string filePath)
        {
            fileStream = new FileStream(filePath, FileMode.Create);
        }

        public void Dispose()
        {
            fileStream.Dispose();
        }
    }

    class TestReaderWriter
    {
        public static void Test(string readFilePath, string writeFilePath)
        {
            var bitRead = new BitReader(readFilePath);
            var bitWrite = new BitWriter(writeFilePath);
            long NBR = 8 * new FileInfo(readFilePath).Length; //NBR  = 8 * InputFileSizeInBytes      
            var rand = new Random();
            do
            {
                int nb = rand.Next() % 32 + 1;
                if (nb > NBR)
                {
                    nb = (int)NBR;
                }

                uint value = bitRead.ReadNBits(nb);
                bitWrite.WriteNBits(nb, value);

                NBR -= nb;
            } while (NBR > 0);
            bitWrite.WriteNBits(7, 1);
            bitWrite.Dispose();
        }
    }
}