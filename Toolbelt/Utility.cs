using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zlib;
using System.Runtime.InteropServices;
using Medallion;
using BitStreams;

namespace Toolbelt
{
    public static class Utility
    {
        public static void TestBitStream(byte[] data)
        {
            BitStream bs = new BitStream(data);
            bs.ReadBits(208);
            byte letter = bs.ReadByte(7);
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(new[] { letter }));
        }

        public static string GetScriptPath(Type type, string script)
        {
            string path = new Uri(type.Assembly.CodeBase).AbsolutePath;
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, "Scripts");
            path = Path.Combine(path, script);
            return path;
        }

        public static byte[] Compress(byte[] data)
        {
            var compress = new Func<byte[], byte[]>(a => {
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var compressor =
                           new Ionic.Zlib.ZlibStream(ms,
                                                      CompressionMode.Compress,
                                                      CompressionLevel.Default))
                    {
                        compressor.Write(a, 0, a.Length);
                    }

                    return ms.ToArray();
                }
            });

            return compress(data);
        }

        public static byte[] Decompress(byte[] data)
        {
            var decompress = new Func<byte[], byte[]>(a => {
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var compressor =
                           new Ionic.Zlib.ZlibStream(ms,
                                                      CompressionMode.Decompress,
                                                      CompressionLevel.Default))
                    {
                        compressor.Write(a, 0, a.Length);
                    }

                    return ms.ToArray();
                }
            });

            return decompress(data);
        }

        public static int Checksum(byte[] data, int length, byte[] checkhash)
        {
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                md5.TransformFinalBlock(data, 0, length);
                hash = md5.Hash;
            }
            
            if (hash.SequenceEqual(checkhash))
                return 0;
            
            return -1;
        }

        public static bool BitfieldContains<T>(UInt32 field, T value)
        { 
            return (field & (uint)(object)value) != 0;
        }

        public static uint BitfieldSet<T>(UInt32 field, T value)
        {
            return Bits.SetBit(field,(int)(object)value);
        }

        public static byte[] Serialize(object src)
        {
            int size = Marshal.SizeOf(src);
            byte[] data = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(src, ptr, true);
            Marshal.Copy(ptr, data, 0, size);
            Marshal.FreeHGlobal(ptr);

            return data;
        }

        public static T Deserialize<T>(byte[] data) where T : struct
        {                        
            T output = default(T);

            int size = Marshal.SizeOf(output);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);
            output = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);            

            return output;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToString(byte[] ba, string Separator)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}{1}", b, Separator);
            return hex.ToString();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return ByteArrayToString(ba, " ");
        }

        public static string ReadCString(byte[] cString)
        {
            var nullIndex = Array.IndexOf(cString, (byte)0);
            nullIndex = (nullIndex == -1) ? cString.Length : nullIndex;
            return Encoding.ASCII.GetString(cString, 0, nullIndex);
        }

        public static string ReadCString(byte[] cString, int startIndex)
        {
            var nullIndex = Array.IndexOf(cString, (byte)0, startIndex);
            nullIndex = (nullIndex == -1) ? cString.Length : nullIndex;
            return Encoding.ASCII.GetString(cString, startIndex, nullIndex - startIndex);
        }

        public static string ReadCString(byte[] cString, int startIndex, int count)
        {
            var nullIndex = Array.IndexOf(cString, (byte)0, startIndex, count);
            nullIndex = (nullIndex == -1) ? count : nullIndex - startIndex;
            return Encoding.ASCII.GetString(cString, startIndex, nullIndex);
        }

        public static string MaxStr(string source, int max)
        {
            return source.PadRight(max).Substring(0, max).TrimEnd();
        }

        public static int bin2dec(string strBin)
        {
            return Convert.ToInt16(strBin, 2);
        }

        private static string dec2hex(int val)
        {
            return val.ToString("X");
        }

        public static string bin2hex(string strBin)
        {
            int decNumber = bin2dec(strBin);
            return dec2hex(decNumber);
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static string IPToStr(ulong IP, bool littleEndian)
        {
            byte[] addr = BitConverter.GetBytes(IP);
            if (!littleEndian)
                Array.Reverse(addr);
            return string.Format("{0}.{1}.{2}.{3}", addr[3], addr[2], addr[1], addr[0]);
        }

        public static uint IPToInt(string IP, bool littleEndian)
        {
            byte[] ip = IP.Split('.').Select(s => byte.Parse(s)).ToArray();
            if (littleEndian)
                Array.Reverse(ip);
            return BitConverter.ToUInt32(ip, 0);
        }

        public static bool ValidAuthString(string input, int maxLength)
        {
            // check length
            if (input.Length == 0 || input.Length > maxLength)
                return false;

            // validate valid ascii characters
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(input.ToCharArray()))
            {
                if (b < 20) return false;
            }

            return true;
        }

        static Random random = new Random();
        public static string GetRandomHexNumber(int count)
        {
            byte[] buffer = new byte[count / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (count % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        public static byte[] GenerateSessionHash(uint accountId, string username)
        {
            byte[] hashValue;
            using (var md5 = MD5.Create())
            {
                string hash = string.Format("ONYX_SESSION_HASH_{0}_{1}", accountId, username);
                hashValue = md5.ComputeHash(md5.ComputeHash(Encoding.ASCII.GetBytes(hash)));
            }
            return hashValue;
        }

        public static Dictionary<string, string> ReadConf(string filename)
        {
            Dictionary<string, string> confData = new Dictionary<string, string>();
            var lines = System.IO.File.ReadAllLines(@filename);
            List<string> validLines = new List<string>();
            foreach (var line in lines)
            {
                if (line.Trim().Length > 0 && line.Trim()[0] != '#')
                {
                    validLines.Add(line);
                }
            }
            confData = validLines.ToDictionary(c => c.Split(new[] { ':' }, 2)[0].Trim(), c => c.Split(new[] { ':' }, 2)[1].Trim());
            return confData;
        }
    }
}
