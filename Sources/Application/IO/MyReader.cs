using System;
using System.Text;

namespace NRO_Server.Application.IO
{
    public class MyReader
    {
        public sbyte[] Buffer { get; set; }

		private int _posRead;

		private int _posMark;

		public MyReader()
		{
		}

		public MyReader(sbyte[] data)
		{
			Buffer = data;
		}

		public sbyte ReadSByte()
		{
			if (_posRead < Buffer.Length)
			{
				return Buffer[_posRead++];
			}
			_posRead = Buffer.Length;
			throw new Exception(" loi doc sbyte eof ");
		}

		public sbyte Readsbyte()
		{
			return ReadSByte();
		}

		public sbyte ReadByte()
		{
			return ReadSByte();
		}

		public void Mark(int readlimit)
		{
			_posMark = _posRead;
		}

		public void Reset()
		{
			_posRead = _posMark;
		}

		public byte ReadUnsignedByte()
		{
			return ConvertSbyteToByte(ReadSByte());
		}

		public short ReadShort()
		{
			short num = 0;
			for (int i = 0; i < 2; i++)
			{
				num = (short)(num << 8);
				num = (short)(num | (short)(0xFF & Buffer[_posRead++]));
			}
			return num;
		}

		public ushort ReadUnsignedShort()
		{
			ushort num = 0;
			for (int i = 0; i < 2; i++)
			{
				num = (ushort)(num << 8);
				num = (ushort)(num | (ushort)(0xFFu & (uint)Buffer[_posRead++]));
			}
			return num;
		}

		public int ReadInt()
		{
			var num = 0;
			for (var i = 0; i < 4; i++)
			{
				num <<= 8;
				num |= 0xFF & Buffer[_posRead++];
			}
			return num;
		}

		public long ReadLong()
		{
			var num = 0L;
			for (var i = 0; i < 8; i++)
			{
				num <<= 8;
				num |= (uint) (0xFF & Buffer[_posRead++]);
			}
			return num;
		}

		public bool ReadBool()
		{
			return ReadSByte() > 0;
		}

		public bool ReadBoolean()
		{
			return ReadSByte() > 0;
		}

		public string ReadString()
		{
			var num = ReadShort();
			var array = new byte[num];
			for (var i = 0; i < num; i++)
			{
				array[i] = ConvertSbyteToByte(ReadSByte());
			}
			var uTf8Encoding = new UTF8Encoding();
			return uTf8Encoding.GetString(array);
		}

		public string ReadStringUTF()
		{
			var num = ReadShort();
			var array = new byte[num];
			for (var i = 0; i < num; i++)
			{
				array[i] = ConvertSbyteToByte(ReadSByte());
			}
			var uTf8Encoding = new UTF8Encoding();
			return uTf8Encoding.GetString(array);
		}

		public string ReadUTF()
		{
			return ReadStringUTF();
		}

		public int Read()
		{
			if (_posRead < Buffer.Length)
			{
				return ReadSByte();
			}
			return -1;
		}

		public int Read(ref sbyte[] data)
		{
			if (data == null)
			{
				return 0;
			}
			var num = 0;
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = ReadSByte();
				if (_posRead > Buffer.Length)
				{
					return -1;
				}
				num++;
			}
			return num;
		}

		public void ReadFully(ref sbyte[] data)
		{
			if (data == null || data.Length + _posRead > Buffer.Length) return;
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = ReadSByte();
			}
		}

		public int Available()
		{
			return Buffer.Length - _posRead;
		}

		private static byte ConvertSbyteToByte(sbyte var)
		{
			if (var > 0)
			{
				return (byte)var;
			}
			return (byte)(var + 256);
		}

		public static byte[] ConvertSbyteToByte(sbyte[] var)
		{
			var array = new byte[var.Length];
			for (var i = 0; i < var.Length; i++)
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

		public void Read(ref sbyte[] data, int arg1, int arg2)
		{
			if (data == null)
			{
				return;
			}
			for (var i = 0; i < arg2; i++)
			{
				data[i + arg1] = ReadSByte();
				if (_posRead > Buffer.Length)
				{
					break;
				}
			}
		}
    }
}