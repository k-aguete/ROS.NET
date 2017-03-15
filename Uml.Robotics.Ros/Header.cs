﻿using System;
using System.Collections;
using System.Text;

namespace Uml.Robotics.Ros
{
    public class Header
    {
        public IDictionary Values = new Hashtable();

        public bool Parse(byte[] buffer, int size, ref string error_msg)
        {
            int i = 0;
            while (i < size)
            {
                int thispiece = BitConverter.ToInt32(buffer, i);
                i += 4;
                byte[] line = new byte[thispiece];
                Array.Copy(buffer, i, line, 0, thispiece);
                string thisheader = Encoding.ASCII.GetString(line);
                string[] chunks = thisheader.Split('=');
                if (chunks.Length != 2)
                {
                    i += thispiece;
                    continue;
                }
                Values[chunks[0].Trim()] = chunks[1].Trim();
                i += thispiece;
            }
            bool res = (i == size);
            if (!res)
                EDB.WriteLine("Warning: Could not parse connection header.");
            return res;
        }

        private static byte[] concat(byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length + b.Length];
            Array.Copy(a, result, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }

        public void Write(IDictionary dict, ref byte[] buffer, ref int totallength)
        {
            Values = new Hashtable(dict);
            buffer = new byte[0];
            totallength = 0;
            foreach (object k in dict.Keys)
            {
                int linelength = 0;
                byte[] key = Encoding.ASCII.GetBytes((string) k);
                byte[] val = Encoding.ASCII.GetBytes(dict[k].ToString());
                totallength += val.Length + key.Length + 1 + 4;
                linelength = val.Length + key.Length + 1;
                buffer = concat(buffer, ByteLength(linelength));
                buffer = concat(buffer, key);
                buffer = concat(buffer, Encoding.ASCII.GetBytes("="));
                buffer = concat(buffer, val);
            }
            if (totallength != buffer.Length)
                throw new InvalidOperationException("Error during header creation: mismatch in buffer length");
        }

        public static byte[] ByteLength(int num)
        {
            return ByteLength((uint) num);
        }

        public static byte[] ByteLength(uint num)
        {
            return BitConverter.GetBytes(num);
        }
    }
}