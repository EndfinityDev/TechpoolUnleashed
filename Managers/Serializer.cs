using System;
using System.Collections.Generic;
using System.Text;

namespace TechpoolUnleashed
{
    static class Serializer
    {
        public static byte[] BuildTable(Int32 size, string name)
        {
            List<byte> bytes = new List<byte>();
            byte[] tempBytes;
            if (name == "") 
            {
                bytes.Add(0x01);
                tempBytes = Encoding.UTF8.GetBytes("T");
                foreach (byte b in tempBytes) 
                    bytes.Add(b);
            }
            else
            {
                tempBytes = BuildString(name, "");
                foreach (byte b in tempBytes)
                    bytes.Add(b);

                tempBytes = Encoding.UTF8.GetBytes("T");
                foreach (byte b in tempBytes)
                    bytes.Add(b);
            }
            tempBytes = BitConverter.GetBytes(0);
            foreach (byte b in tempBytes)
                bytes.Add(b);

            tempBytes = BitConverter.GetBytes(size);
            foreach (byte b in tempBytes)
                bytes.Add(b);

            return bytes.ToArray();
        }
        public static byte[] BuildString(string str, string name)
        {
            List<byte> bytes = new List<byte>();
            byte[] tempBytes;
            Int32 size = str.Length;

            tempBytes = Encoding.UTF8.GetBytes("S");
            foreach (byte b in tempBytes)
                bytes.Add(b);

            if (name != "")
            {
                Int32 nameSize = name.Length;
                tempBytes = BitConverter.GetBytes(nameSize);
                foreach (byte b in tempBytes)
                    bytes.Add(b);
                tempBytes = Encoding.UTF8.GetBytes(name);
                foreach (byte b in tempBytes)
                    bytes.Add(b);
                tempBytes = Encoding.UTF8.GetBytes("S");
                foreach (byte b in tempBytes)
                    bytes.Add(b);
            }
            tempBytes = BitConverter.GetBytes(size);
            foreach (byte b in tempBytes)
                bytes.Add(b);
            tempBytes = Encoding.UTF8.GetBytes(str);
            foreach (byte b in tempBytes)
                bytes.Add(b);

            return bytes.ToArray();
        }
        public static byte[] BuildNumber(double number, string name)
        {
            List<byte> bytes = new List<byte>();
            byte[] tempBytes;

            Int32 nameSize = name.Length;

            tempBytes = Encoding.UTF8.GetBytes("S");
            foreach (byte b in tempBytes)
                bytes.Add(b);
            tempBytes = BitConverter.GetBytes(nameSize);
            foreach (byte b in tempBytes)
                bytes.Add(b);
            tempBytes = Encoding.UTF8.GetBytes(name);
            foreach (byte b in tempBytes)
                bytes.Add(b);

            tempBytes = Encoding.UTF8.GetBytes("N");
            foreach (byte b in tempBytes)
                bytes.Add(b);
            tempBytes = BitConverter.GetBytes(number);
            foreach (byte b in tempBytes)
                bytes.Add(b);

            return bytes.ToArray();
        }
    }
}
