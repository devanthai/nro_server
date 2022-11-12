using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Org.BouncyCastle.Math;

namespace NRO_Server.Application.IO
{
    public class MyWriter
    {
        public sbyte[] Buffer { get; set; }

		private int _posWrite;

		private int _lenght;

		public MyWriter()
		{
			Buffer = new sbyte[2048];
			_lenght = 2048;
			_posWrite = 0;
		}

		public void WriteSByte(sbyte value)
		{
			CheckLenght(0);
			Buffer[_posWrite++] = value;
		}

		public void WriteSByteUncheck(sbyte value)
		{
			Buffer[_posWrite++] = value;
		}

		public void WriteByte(sbyte value)
		{
			WriteSByte(value);
		}

		public void WriteByte(int value)
		{
			WriteSByte((sbyte)value);
		}

		public void WriteChar(char value)
		{
			WriteSByte(0);
			WriteSByte((sbyte)value);
		}

		public void WriteUnsignedByte(byte value)
		{
			WriteSByte((sbyte)value);
		}

		public void WriteUnsignedByte(byte[] value)
		{
			CheckLenght(value.Length);
			for (int i = 0; i < value.Length; i++)
			{
				WriteSByteUncheck((sbyte)value[i]);
			}
		}

		public void WriteSByte(sbyte[] value)
		{
			CheckLenght(value.Length);
			for (int i = 0; i < value.Length; i++)
			{
				WriteSByteUncheck(value[i]);
			}
		}

		public void WriteShort(short value)
		{
			CheckLenght(2);
			for (int num = 1; num >= 0; num--)
			{
				WriteSByteUncheck((sbyte)(value >> num * 8));
			}
		}

		public void WriteShort(int value)
		{
			CheckLenght(2);
			short num = (short)value;
			for (int num2 = 1; num2 >= 0; num2--)
			{
				WriteSByteUncheck((sbyte)(num >> num2 * 8));
			}
		}

		public void WriteUnsignedShort(ushort value)
		{
			CheckLenght(2);
			for (int num = 1; num >= 0; num--)
			{
				WriteSByteUncheck((sbyte)(value >> num * 8));
			}
		}

		public void WriteInt(int value)
		{
			CheckLenght(4);
			for (int num = 3; num >= 0; num--)
			{
				WriteSByteUncheck((sbyte)(value >> num * 8));
			}
		}

		public void WriteLong(long value)
		{
			CheckLenght(8);
			for (int num = 7; num >= 0; num--)
			{
				WriteSByteUncheck((sbyte)(value >> num * 8));
			}
		}

		public void WriteBoolean(bool value)
		{
			WriteSByte((sbyte)(value ? 1 : 0));
		}

		public void WriteBool(bool value)
		{
			WriteSByte((sbyte)(value ? 1 : 0));
		}

		public void WriteString(string value)
		{
			char[] array = value.ToCharArray();
			WriteShort((short)array.Length);
			CheckLenght(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				WriteSByteUncheck((sbyte)array[i]);
			}
		}

		public void WriteUTF(string value)
		{
			Encoding unicode = Encoding.Unicode;
			Encoding encoding = Encoding.GetEncoding(65001);
			byte[] bytes = unicode.GetBytes(value);
			byte[] array = Encoding.Convert(unicode, encoding, bytes);
			WriteShort((short)array.Length);
			CheckLenght(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				sbyte value2 = (sbyte)array[i];
				WriteSByteUncheck(value2);
			}
		}

		public void Write(sbyte value)
		{
			WriteSByte(value);
		}

		public void Write(ref sbyte[] data, int arg1, int arg2)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < arg2; i++)
			{
				WriteSByte(data[i + arg1]);
				if (_posWrite > Buffer.Length)
				{
					break;
				}
			}
		}

		public void Write(sbyte[] value)
		{
			WriteSByte(value);
		}

		public void Write(List<sbyte> value)
		{
			WriteSByte(value.ToArray());
		}

		public void Write(byte[] value)
		{
			WriteSByte(ServerUtils.ConvertArrayByteToSByte(value));
		}

		public sbyte[] GetData()
		{
			if (_posWrite <= 0)
			{
				return null;
			}
			sbyte[] array = new sbyte[_posWrite];
			for (int i = 0; i < _posWrite; i++)
			{
				array[i] = Buffer[i];
			}
			return array;
		}
																 
		public void CheckLenght(int ltemp)
		{
			if (_posWrite + ltemp >= _lenght)
			{
				sbyte[] array = new sbyte[_lenght + 1024 + ltemp];
				for (int i = 0; i < _lenght; i++)
				{
					array[i] = Buffer[i];
				}
				Buffer = null;
				Buffer = array;
				_lenght += 1024 + ltemp;
			}
		}

		private static void ConvertString(string[] args)
		{
			string path = args[0];
			string path2 = args[1];
			using StreamReader input = new StreamReader(path, Encoding.Unicode);
			using StreamWriter output = new StreamWriter(path2, append: false, Encoding.UTF8);
			CopyContents(input, output);
		}

		private static void CopyContents(TextReader input, TextWriter output)
		{
			char[] array = new char[8192];
			int count;
			while ((count = input.Read(array, 0, array.Length)) != 0)
			{
				output.Write(array, 0, count);
			}
			output.Flush();
			string message = output.ToString();
		}

		public byte ConvertSbyteToByte(sbyte var)
		{
			if (var > 0)
			{
				return (byte)var;
			}
			return (byte)(var + 256);
		}

		public byte[] ConvertSbyteToByte(sbyte[] var)
		{
			byte[] array = new byte[var.Length];
			for (int i = 0; i < var.Length; i++)
			{
				if (var[i] > 0)
				{
					array[i] = (byte)var[i];
				}
				else
				{
					array[i] = (byte)(var[i] + 256);
				}
			}
			return array;
		}

		public void Close()
		{
			Buffer = null;
		}
    }
}