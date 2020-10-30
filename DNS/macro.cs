using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace DNS
{
    public static class macro
    {
        public static int read(out UInt16 @out, MemoryStream stream)
        {
            long pre_position = stream.Position;

            @out = 0;
            byte[] bytes = BitConverter.GetBytes(@out);
            stream.Read(bytes, 0, bytes.Length);

            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            @out = BitConverter.ToUInt16(bytes, 0);
            return (int)(stream.Position - pre_position);
        }
        public static int write(MemoryStream stream, UInt16 @in)
        {
            long pre_position = stream.Position;

            byte[] bytes = BitConverter.GetBytes(@in);
            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            stream.Write(bytes, 0, bytes.Length);
            return (int)(stream.Position - pre_position);
        }
        public static int read(out Int16 @out, MemoryStream stream)
        {
            long pre_position = stream.Position;

            @out = 0;
            byte[] bytes = BitConverter.GetBytes(@out);
            stream.Read(bytes, 0, bytes.Length);

            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            @out = BitConverter.ToInt16(bytes, 0);
            return (int)(stream.Position - pre_position);
        }
        public static int write(MemoryStream stream, Int16 @in)
        {
            long pre_position = stream.Position;

            byte[] bytes = BitConverter.GetBytes(@in);
            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            stream.Write(bytes, 0, bytes.Length);
            return (int)(stream.Position - pre_position);
        }
        public static int read(out UInt32 @out, MemoryStream stream)
        {
            long pre_position = stream.Position;

            @out = 0;
            byte[] bytes = BitConverter.GetBytes(@out);
            stream.Read(bytes, 0, bytes.Length);

            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            @out = BitConverter.ToUInt32(bytes, 0);
            return (int)(stream.Position - pre_position);
        }
        public static int write(MemoryStream stream, UInt32 @in)
        {
            long pre_position = stream.Position;

            byte[] bytes = BitConverter.GetBytes(@in);
            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            stream.Write(bytes, 0, bytes.Length);
            return (int)(stream.Position - pre_position);
        }
        public static int read(out Int32 @out, MemoryStream stream)
        {
            long pre_position = stream.Position;

            @out = 0;
            byte[] bytes = BitConverter.GetBytes(@out);
            stream.Read(bytes, 0, bytes.Length);

            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            @out = BitConverter.ToInt32(bytes, 0);
            return (int)(stream.Position - pre_position);
        }
        public static int write(MemoryStream stream, Int32 @in)
        {
            long pre_position = stream.Position;

            byte[] bytes = BitConverter.GetBytes(@in);
            if (true == BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            stream.Write(bytes, 0, bytes.Length);
            return (int)(stream.Position - pre_position);
        }
        public static int read(out byte[] @out, MemoryStream stream, long size)
        {
            long pre_position = stream.Position;

            byte[] bytes = new byte[size];
            stream.Read(bytes, 0, bytes.Length);

            @out = bytes;
            return (int)(stream.Position - pre_position);
        }
        public static int write(MemoryStream stream, byte[] @in)
        {
            long pre_position = stream.Position;
            byte[] bytes = @in;

            stream.Write(bytes, 0, bytes.Length);
            return (int)(stream.Position - pre_position);
        }
        public static int read(out string[] @out, MemoryStream stream, long size)
        {

            const int char_compress = 0xC0;

            long pre_position = stream.Position;

            using (MemoryStream replicate_bytes = new MemoryStream(stream.ToArray(), 0, (int)(stream.Position + size)))
            {
                replicate_bytes.Position = pre_position;

                List<byte[]> bytes_list = new List<byte[]>();

                int len = 0;
                do
                {
                    if (size <= (replicate_bytes.Position - pre_position))
                        break;

                    byte[] byte_len = new byte[1];
                    replicate_bytes.Read(byte_len, 0, byte_len.Length);
                    len = BitConverter.ToInt32(new byte[4] { byte_len[0], 0x00, 0x00, 0x00 }, 0);

                    // 0xC0
                    if (char_compress == len)
                    {
                        replicate_bytes.Read(byte_len, 0, byte_len.Length);

                        int before = BitConverter.ToInt32(new byte[4] { byte_len[0], 0x00, 0x00, 0x00 }, 0);

                        long swap_pos = replicate_bytes.Position;
                        {
                            replicate_bytes.Position = before;
                            //replicate_bytes.Read(byte_len, 0, byte_len.Length);
                            //len = BitConverter.ToInt32(new byte[4] { byte_len[0], 0x00, 0x00, 0x00 }, 0);
                            //replicate_bytes.Position -= byte_len.Length;

                            string[] sub_out;
                            read(out sub_out, replicate_bytes, replicate_bytes.Length - replicate_bytes.Position);

                            foreach (string s in sub_out)
                                bytes_list.Add(Encoding.UTF8.GetBytes(s));
                        }
                        replicate_bytes.Position = swap_pos;

                        len = 0; // break;
                    }
                    else if (0 < len)
                    {
                        byte[] bytes = new byte[len];
                        replicate_bytes.Read(bytes, 0, bytes.Length);
                        bytes_list.Add(bytes);
                    }
                } while (0 < len);
                stream.Position = replicate_bytes.Position;

                List<string> strings = new List<string>();
                foreach (byte[] bytes in bytes_list)
                {
                    strings.Add(Encoding.UTF8.GetString(bytes));
                }
                @out = strings.ToArray();

                return (int)(replicate_bytes.Position - pre_position);
            }
        }
        public static int write(MemoryStream stream, string[] @in)
        {
            long pre_position = stream.Position;

            foreach (string s in @in)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                byte bytes_len = BitConverter.GetBytes(bytes.Length)[0];
                // 데이터 길이
                stream.Write(new byte[] { bytes_len }, 0, 1);
                // 스트링 데이터
                stream.Write(bytes, 0, bytes.Length);
            }
            // \0
            stream.Write(new byte[] { 0x00 }, 0, 1);

            return (int)(stream.Position - pre_position);
        }

        public static bool IsTcpPortAvailable(int tcpPort)
        {
            var ipgp = IPGlobalProperties.GetIPGlobalProperties();

            // Check ActiveConnection ports
            TcpConnectionInformation[] conns = ipgp.GetActiveTcpConnections();
            foreach (var cn in conns)
            {
                if (cn.LocalEndPoint.Port == tcpPort)
                {
                    return false;
                }
            }

            // Check LISTENING ports
            IPEndPoint[] endpoints = ipgp.GetActiveTcpListeners();
            foreach (var ep in endpoints)
            {
                if (ep.Port == tcpPort)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
