using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbelt;

namespace Toolbelt
{
    public class ByteRef
    {
        private byte[] _bytes;

        public ByteRef(int size)
        {
            _bytes = new byte[size];
            Position = 0;
        }

        public ByteRef(byte[] bytes)
        {
            int size = bytes.Length;
            _bytes = new byte[size];
            bytes.Skip(0).Take(size).ToArray().CopyTo(_bytes, 0);
            Position = 0;
        }

        public ByteRef(byte[] bytes, int size)
        {
            _bytes = new byte[size];
            bytes.Skip(0).Take(size).ToArray().CopyTo(_bytes, 0);
            Position = 0;
        }

        public int Length { get => _bytes.Length; }

        public int Position { get; private set; }

        public void DebugDump()
        {
            Console.WriteLine(Utility.ByteArrayToString(_bytes));
        }

        public void Reset(int size)
        {
            _bytes = new byte[size];
        }

        public void Resize(int size)
        {
            int OrigLength = Length;
            byte[] tempBuf = new byte[Length];
            _bytes.CopyTo(tempBuf, 0);
            _bytes = new byte[size];
            BlockCopy(tempBuf, 0, OrigLength);
        }

        public void SetPosition(int cursor)
        {
            Position = cursor;
        }

        public void Advance(int amt)
        {
            Position += amt;
        }

        public int GetPosition()
        {
            return Position;
        }

        public byte[] Get()
        {
            return _bytes;
        }

        public byte[] At(int start)
        {
            return _bytes.Skip(start).ToArray();
        }

        public int Push<T>(object value)
        {
            int bytesSet = Set<T>(Position, value);
            Advance(bytesSet);
            return Position;
        }

        public void BlockCopy(byte[] source, int index, int size)
        {
            for (int i = 0; i < Math.Min(source.Length, Math.Min(Length, size)); i++)
            {
                Set<byte>(index + i, source[i]);
            }
        }

        public void BlockCopy(byte[] source, int index, int start, int size)
        {
            BlockCopy(source.Skip(start).ToArray(), index, size);
        }

        public void BlockCopy(ByteRef source, int index, int size)
        {
            for (int i = 0; i < Math.Min(source.Length, Math.Min(Length, size)); i++)
            {
                Set<byte>(index + i, source.GetByte(i));
            }
        }

        public void Fill(int index, byte value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Set<byte>(index + i, value);
            }
        }

        public dynamic Pop<T>(int length = 0)
        {
            Type type = typeof(T);
            switch (true)
            {
                case bool _ when type == typeof(byte):
                    Advance(-1);
                    return GetByte(Position);
                case bool _ when type == typeof(short):
                    Advance(-2);
                    return GetInt16(Position);
                case bool _ when type == typeof(ushort):
                    Advance(-2);
                    return GetUInt16(Position);
                case bool _ when type == typeof(int):
                    Advance(-4);
                    return GetInt32(Position);
                case bool _ when type == typeof(uint):
                    Advance(-4);
                    return GetUInt32(Position);
                case bool _ when type == typeof(long):
                    Advance(-8);
                    return GetInt64(Position);
                case bool _ when type == typeof(ulong):
                    Advance(-8);
                    return GetUInt64(Position);
                case bool _ when type == typeof(float):
                    Advance(-4);
                    return GetFloat(Position);
                case bool _ when type == typeof(double):
                    Advance(-8);
                    return GetDouble(Position);
                case bool _ when type == typeof(string):
                    Advance(-length);
                    return GetString(Position, length);
            }
            return null;
        }

        public int Set<T>(int index, object value)
        {
            Type type = typeof(T);
            int iBytesSet = 0;
            try
            {
                switch (true)
                {
                    case bool _ when type == typeof(byte):
                        byte bVal = Convert.ToByte(value);
                        SetVal(index, bVal);
                        iBytesSet = 1;
                        break;
                    case bool _ when type == typeof(short):
                        short shVal = Convert.ToInt16(value);
                        SetVal(index, shVal);
                        iBytesSet = 2;
                        break;
                    case bool _ when type == typeof(int):
                        int iVal = Convert.ToInt32(value);
                        SetVal(index, iVal);
                        iBytesSet = 4;
                        break;
                    case bool _ when type == typeof(long):
                        long lVal = Convert.ToInt64(value);
                        SetVal(index, lVal);
                        iBytesSet = 8;
                        break;
                    case bool _ when type == typeof(ushort):
                        ushort usVal = Convert.ToUInt16(value);
                        SetVal(index, usVal);
                        iBytesSet = 2;
                        break;
                    case bool _ when type == typeof(uint):
                        uint uiVal = Convert.ToUInt32(value);
                        SetVal(index, uiVal);
                        iBytesSet = 4;
                        break;
                    case bool _ when type == typeof(ulong):
                        ulong ulVal = Convert.ToUInt64(value);
                        SetVal(index, ulVal);
                        iBytesSet = 8;
                        break;
                    case bool _ when type == typeof(float):
                        float fVal = (float)(double)value;
                        SetVal(index, fVal);
                        iBytesSet = 4;
                        break;
                    case bool _ when type == typeof(double):
                        double dVal = Convert.ToDouble(value);
                        SetVal(index, dVal);
                        iBytesSet = 8;
                        break;
                    case bool _ when type == typeof(string):
                        string sVal = (string)value;
                        SetVal(index, sVal);
                        iBytesSet = sVal.Length;
                        break;
                    case bool _ when type == typeof(byte[]):
                        byte[] baVal = (byte[])value;
                        BlockCopy(baVal, index, baVal.Length);
                        break;
                    default:
                        iBytesSet = 0;
                        break;
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            return iBytesSet;
        }

        public void SetVal(int index, byte value)
        {
            _bytes[index] = value;
        }

        public void SetVal(int index, short value)
        {
            BitConverter.GetBytes(value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, ushort value)
        {
            BitConverter.GetBytes(value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, int value)
        {
            BitConverter.GetBytes(value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, uint value)
        {
            BitConverter.GetBytes(value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, long value)
        {
            BitConverter.GetBytes(value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, ulong value)
        {
            BitConverter.GetBytes(value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, float value)
        {
            BitConverter.GetBytes((double)value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, double value)
        {
            BitConverter.GetBytes(value).CopyTo(_bytes, index);
        }

        public void SetVal(int index, string value)
        {
            Encoding.ASCII.GetBytes(value).CopyTo(_bytes, index);
        }

        public byte[] Append(byte[] data)
        {
            int length = _bytes.Length;
            ByteRef temp = new ByteRef(_bytes);
            _bytes = new byte[length + data.Length];
            Set<byte[]>(0, temp.Get());
            Set<byte[]>(temp.Length, data);
            return _bytes;
        }

        public byte GetByte(int index)
        {
            byte value = _bytes[index];
            return value;
        }

        public byte[] GetBytes(int index, int count)
        {
            if (index + count <= _bytes.Length)
                return _bytes.Skip(index).Take(count).ToArray();
            return null;
        }

        public short GetInt16(int index)
        {
            short value = BitConverter.ToInt16(_bytes, index);
            return value;
        }

        public ushort GetUInt16(int index)
        {
            ushort value = BitConverter.ToUInt16(_bytes, index);
            return value;
        }

        public int GetInt32(int index)
        {
            int value = BitConverter.ToInt32(_bytes, index);
            return value;
        }

        public uint GetUInt32(int index)
        {
            uint value = BitConverter.ToUInt32(_bytes, index);
            return value;
        }

        public long GetInt64(int index)
        {
            long value = BitConverter.ToInt64(_bytes, index);
            return value;
        }

        public ulong GetUInt64(int index)
        {
            ulong value = BitConverter.ToUInt64(_bytes, index);
            return value;
        }

        public float GetFloat(int index)
        {
            float value = (float)(double)BitConverter.ToDouble(_bytes, index);
            return value;
        }

        public double GetDouble(int index)
        {
            double value = BitConverter.ToDouble(_bytes, index);
            return value;
        }

        public string GetString(int index, int size)
        {
            string value = System.Text.Encoding.UTF8.GetString(_bytes, index, size);
            return value;
        }

    }
}
