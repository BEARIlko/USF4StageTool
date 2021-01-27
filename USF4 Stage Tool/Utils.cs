using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpImageLibrary;
using static CSharpImageLibrary.ImageFormats;

namespace USF4_Stage_Tool
{

	public static class Utils
	{
		public static string shaderFilename = "./Shaders.dat";
		public static string shaderPropertiesFilename = "./ShadersProperties.dat";
		public static Dictionary<string, int> Shaders = new Dictionary<string, int>();
		public static Dictionary<string, string> ShadersProperties = new Dictionary<string, string>();

		public static UInt64 HashInts(int x, int y)
		{
			UInt64 ux = Convert.ToUInt64(x);
			UInt64 uy = Convert.ToUInt64(y);

			return (((ux + uy) * (ux + uy + 1)) / 2) + uy;
		}

		public static Matrix4x4 UnifyTransformMatrix(Matrix4x4[] MatArray)
		{
			Matrix4x4 returnArray = Matrix4x4.Multiply(MatArray[6], MatArray[7]);

			returnArray = Matrix4x4.Multiply(returnArray, MatArray[8]); //scale

			returnArray = Matrix4x4.Multiply(returnArray, MatArray[4]);
			returnArray = Matrix4x4.Multiply(returnArray, MatArray[3]);
			returnArray = Matrix4x4.Multiply(returnArray, MatArray[5]); //rotation yxz

			returnArray = Matrix4x4.Multiply(returnArray, MatArray[0]);
			returnArray = Matrix4x4.Multiply(returnArray, MatArray[1]);
			returnArray = Matrix4x4.Multiply(returnArray, MatArray[2]); //translation

			return returnArray;
		}

		public static void LeftHandToEulerAnglesXYZ(Matrix4x4 m, out float rfXAngle, out float rfYAngle, out float rfZAngle)
		{
			// +-           -+   +-                                      -+
			// | r00 r01 r02 |   |  cy*cz  cz*sx*sy-cx*sz  cx*cz*sy+sx*sz |
			// | r10 r11 r12 | = |  cy*sz  cx*cz+sx*sy*sz -cz*sx+cx*sy*sz |
			// | r20 r21 r22 |   | -sy     cy*sx           cx*cy          |
			// +-           -+   +-                                      -+

			if (m.M31 < 1.0f)
			{
				if (m.M31 > -1.0f)
				{
					// y_angle = asin(-r20)
					// z_angle = atan2(r10,r00)
					// x_angle = atan2(r21,r22)
					rfYAngle = (float)Math.Asin(-m.M31);
					rfZAngle = (float)Math.Atan2(m.M21, m.M11);
					rfXAngle = (float)Math.Atan2(m.M32, m.M33);
					return ;//EA_UNIQUE
				}
				else
				{
					// y_angle = +pi/2
					// x_angle - z_angle = atan2(r01,r02)
					// WARNING.  The solution is not unique.  Choosing x_angle = 0.
					rfYAngle = (float)Math.PI/2;
					rfZAngle = -(float)Math.Atan2(m.M12, m.M13);
					rfXAngle = 0.0f;
					return ;//EA_NOT_UNIQUE_DIF
				}
			}

			else
			{
				// y_angle = -pi/2
				// x_angle + z_angle = atan2(-r01,-r02)
				// WARNING.  The solution is not unique.  Choosing x_angle = 0;
				rfYAngle = -(float)Math.PI / 2;
				rfZAngle = (float)Math.Atan2(-m.M12, -m.M13);
				rfXAngle = 0.0f;
				return;//EA_NOT_UNIQUE_SUM
			}
		}

		public static void DecomposeMatrixNaive(Matrix4x4 matrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz)
		{
			tx = -matrix.M41;
			ty = matrix.M43;
			tz = matrix.M42;

			//double tX = -matrix[12];
			//double tY = matrix[14];
			//double tZ = matrix[13];
			//double ry = -atan2(-matrix[2], matrix[0]);
			//double rx = atan2(-matrix[9], matrix[5]);
			//double rz = asin(matrix[1]);

			ry = -Convert.ToSingle(Math.Atan2(-matrix.M13, matrix.M11));
			rx = Convert.ToSingle(Math.Atan2(-matrix.M32, matrix.M22));
			rz = Convert.ToSingle(Math.Asin(matrix.M12));
		}

		public static void DecomposeMatrixYXZ(Matrix4x4 matrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz)
        {
			Vector3 scale = new Vector3();
			Quaternion quatrotation = new Quaternion();
			Vector3 translation = new Vector3();


			Matrix4x4.Decompose(matrix, out scale, out quatrotation, out translation);

			double p0 = quatrotation.W;
			double p2 = quatrotation.X;
			double p1 = quatrotation.Y;
			double p3 = quatrotation.Z;

			double e = 1;

			/* Quaternion is (p0,p2,p1,p3)
			* e = 1;
			* 
			* Equations for decomposing a quaternion into arbitrarily ordered rotation sequences:
			* https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/Quaternions.pdf
			* Beautiful.
			*/

			double drz = Math.Atan2(2 * (p0 * p3 - e * p1 * p2), 1 - 2 * (p2 * p2 + p3 * p3));
			double drx = Math.Asin(2 * (p0 * p2 + e * p1 * p3));
			double dry = Math.Atan2(2 * (p0 * p1 - e * p2 * p3), 1 - 2 * (p1 * p1 + p2 * p2));

            //Handle singularities

            if (drx > ((Math.PI / 2f) - 0.00000001f))
            {
                drz = 0;
                dry = Math.Atan2(p1, p0);
            }
            if (drx < ((-Math.PI / 2f) + 0.00000001f))
            {
                drz = 0;
                dry = Math.Atan2(p1, p0);
            }
            //Supposedly the same holds true for +/ -90 deg, which seems... odd ?

            tx = translation.X;
			ty = translation.Y;
			tz = translation.Z;

			rx = Convert.ToSingle(drx);
			ry = Convert.ToSingle(dry);
			rz = Convert.ToSingle(drz);

			sx = scale.X;
			sy = scale.Y;
			sz = scale.Z;
		}

		public static void DecomposeMatrixXYZ(Matrix4x4 matrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz)
		{
			Vector3 scale = new Vector3();
			Quaternion quatrotation = new Quaternion();
			Vector3 translation = new Vector3();


			Matrix4x4.Decompose(matrix, out scale, out quatrotation, out translation);

			double p0 = quatrotation.W;
			double p1 = quatrotation.X;
			double p2 = quatrotation.Y;
			double p3 = quatrotation.Z;

			double e = -1;

			double drz = Math.Atan2(2 * (p0 * p3 - e * p1 * p2), 1 - 2 * (p2 * p2 + p3 * p3));
			double dry = Math.Asin(2 * (p0 * p2 + e * p1 * p3));
			double drx = Math.Atan2(2 * (p0 * p1 - e * p2 * p3), 1 - 2 * (p1 * p1 + p2 * p2));

			//Handle singularities

			if (dry > ((Math.PI / 2f) - 0.00000001f))
			{
				drz = 0;
				drx = Math.Atan2(p1, p0);
			}
			if (dry < ((-Math.PI / 2f) + 0.00000001f))
			{
				drz = 0;
				drx = Math.Atan2(p1, p0);
			}
			//Supposedly the same holds true for +/ -90 deg, which seems... odd ?

			tx = translation.X;
			ty = translation.Y;
			tz = translation.Z;

			rx = Convert.ToSingle(drx);
			ry = Convert.ToSingle(dry);
			rz = Convert.ToSingle(drz);

			sx = scale.X;
			sy = scale.Y;
			sz = scale.Z;
		}

		public static byte[] ImageToByte(Image img)
		{
			ImageConverter converter = new ImageConverter();
			Bitmap tmp = new Bitmap(img);
			byte[] byteImg = (byte[])converter.ConvertTo(tmp.Clone(), typeof(byte[]));
			return byteImg;
		}

		public static Bitmap BitmapFromBytes(byte[] Data)
		{
			MemoryStream mStream = new MemoryStream();
			mStream.Write(Data, 0, Convert.ToInt32(Data.Length));
			Bitmap bmp = new Bitmap(mStream, false);
			mStream.Dispose();
			return bmp;
		}

		public static int[] Rotate3Array(int[] Array, int steps)
		{
			//If it's not a 1x3 array, return it and error to console
			if(Array.Length % 3 != 0) { Console.WriteLine($"Wrong size array: {Array.Length}"); return Array; }
			//If steps is divisible by 3, don't do any rotating
			if(steps % 3 == 0) { return Array; }
			int[] temp = new int[3];
			if (steps % 3 == 1) { temp = new int[]{ Array[2], Array[0], Array[1] }; }
			if (steps % 3 == 2) { temp = new int[]{ Array[1], Array[2], Array[0] }; }

			return temp;
		}

		private static void SaveDDS(string filepath)
		{
			////Get the current IMG as Bytes;
			//Image img;
			////img = pictureBox1.BackgroundImage;
			////byte[] bytes = ImageToByte(img);

			//MipHandling MH = new MipHandling();
			//ImageEngineFormatDetails IEFD = new ImageEngineFormatDetails(ImageEngineFormat.DDS_DXT5);
			//ImageEngineImage e2 = new ImageEngineImage(bytes);
			//e2.Save(filepath, IEFD, MH);
		}

		//Fixes the '.' in OBJ to the whatever your System Decimal Separator is. This helps when float.Parse()
		public static string FixFloatingPoint(string FloatText) 
		{ 
			FloatText = FloatText.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Trim(); 
			FloatText = FloatText.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Trim();
			return FloatText;
		}
		public static string RestoreFloatingPoint(string FloatText) { return FloatText.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Trim(), "."); }

		#region HeX Methods
		//Returns a HEX string from float but the HEX Bytes are reversed
		public static string FloatToHex(float inputFloat)
		{
			if (inputFloat == 0) { return "00000000"; }
			uint l = BitConverter.ToUInt32(BitConverter.GetBytes(inputFloat), 0);
			string ForwardHEX = String.Format("{0:X}", l);
			char[] chars = ForwardHEX.ToCharArray();
			return String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", chars[6], chars[7], chars[4], chars[5], chars[2], chars[3], chars[0], chars[1]);
		}

		public static string IntToHex(int inputInt)
		{
			if (inputInt == 0) { return "0000"; }
			string hexValue = inputInt.ToString("X");

			while (hexValue.Length < 4)
			{
				hexValue = $"{0}{hexValue}";
			}
			char[] chars = hexValue.ToCharArray();
			string reversed = String.Format("{0}{1}{2}{3}", chars[2], chars[3], chars[0], chars[1]);
			return reversed;
		}

		public static string IntToHex2(bool IsLong, int inputInt)
		{
			string hexValue = string.Empty;
			int LoopLength;
			if (IsLong)
			{
				if (inputInt == 0) { return "00000000"; }
				hexValue = inputInt.ToString("X");
				LoopLength = 8;
			}
			else
			{
				if (inputInt == 0) { return "0000"; }
				hexValue = inputInt.ToString("X");
				LoopLength = 4;
			}
			while (hexValue.Length < LoopLength)
			{
				hexValue = $"{0}{hexValue}";
			}
			hexValue = EndianFlip(hexValue);
			return hexValue;
		}
		public static string EndianFlip(string inputHex)
		{
			string ReturnValue = string.Empty;
			if (inputHex == string.Empty) { return inputHex; } //Probably bad idea
			char[] chars = inputHex.ToCharArray();
			string reversed = string.Empty;
			if (inputHex.Length == 4)
			{
				ReturnValue = String.Format("{0}{1}{2}{3}", chars[2], chars[3], chars[0], chars[1]);
			}
			else if (inputHex.Length == 8)
			{
				ReturnValue = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", chars[6], chars[7], chars[4], chars[5], chars[2], chars[3], chars[0], chars[1]);
			}
			else
			{
				MessageBox.Show("Error in reading EMG", "Error");
				return inputHex;
			}
			return ReturnValue;
		}

		public static int HexToInt(string hex)
		{
			return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
		}


		public static string StreamToHex3(Stream stream)
		{
			stream.Seek(0, 0);
			BinaryReader reader = new BinaryReader(stream);
			StringBuilder data = new StringBuilder();
			byte[] buffer = new byte[256];
			int bytesread = reader.Read(buffer, 0, buffer.Length);
			while (bytesread != 0)
			{
				data.AppendLine(HexStr2(buffer, bytesread));
				bytesread = reader.Read(buffer, 0, buffer.Length);
			}
			return data.ToString();
		}

		public static string HexStr2(byte[] data, int length)
		{
			char[] lookup = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
			int i = 0, p = 0;
			char[] c = new char[length * 2];
			byte d;
			while (i < length)
			{
				d = data[i++];
				c[p++] = lookup[d / 0x10];
				c[p++] = lookup[d % 0x10];
			}
			return new string(c, 0, c.Length);
		}

		public static void WriteDataToStream(string FilePath, List<byte> Data)
		{
			FileStream writeStream;
			writeStream = new FileStream(FilePath, FileMode.OpenOrCreate);

			using (BinaryWriter binWriter = new BinaryWriter(writeStream))
			{
				byte[] dataArray = Data.ToArray();

				Console.WriteLine("Writing the data.");
				binWriter.Write(dataArray, 0, dataArray.Length);
			}
		}
		public static void WriteDataToStream(string FilePath, byte[] Data)
		{
			FileStream writeStream;
			writeStream = new FileStream(FilePath, FileMode.OpenOrCreate);

			using (BinaryWriter binWriter = new BinaryWriter(writeStream))
			{
				binWriter.Write(Data, 0, Data.Length);
			}
		}


		public static void AddZeroToLineEnd(List<byte> targetList)
		{
			while (targetList.Count % 0x10 != 0)
			{
				targetList.Add(0x00);
			}
		}
		#endregion HeX Methods

		#region Binary Methods
		public static void AddPaddingZeros(List<byte> targetList, int endoffset, int ListLength)
		{
			for (int i = ListLength; i < endoffset; i++)
			{
				targetList.Add(Convert.ToByte("00"));
			}
		}

		public static void AddStringAsBytes(List<byte> targetList, string Data)
		{
			string checkUp = "Nothing";
			try
			{
				if (Data == Environment.NewLine)
				{
					targetList.Add(Convert.ToByte(Environment.NewLine));
					return;
				}
				for (int i = 0; i < Data.Length / 2; i++)
				{
					string num = Data.Substring(i * 2, 2);
					checkUp = num;
					//Console.WriteLine("num:" + checkUp);
					int hexValue = int.Parse(num, System.Globalization.NumberStyles.HexNumber);
					targetList.Add(Convert.ToByte(hexValue));
				}
			}
			catch (Exception up)
			{
				Console.WriteLine(up.Message);
				Console.WriteLine($"Offending data:  {@checkUp}");
			}
		}

		public static void AddCopiedBytes(List<byte> targetList, int StartOffset, int Length, byte[] Data)
		{
			for (int i = 0; i < Length; i++)
			{
				targetList.Add(Data[StartOffset + i]);
			}
		}

		public static void AddFloatAsBytes(List<byte> targetList, float Data)
		{
			AddAllBytes(targetList, ByteArrayToList(BitConverter.GetBytes(Data)));
		}

		public static void AddIntAsBytes(List<byte> targetList, int Data, bool IsLong)
		{
			if (IsLong)
			{
				AddAllBytes(targetList, ByteArrayToList(BitConverter.GetBytes(Data)));
			}
			else
            {
				if (Data > 0xFFFF)
                {
					Console.WriteLine("AddIntAsBytes: Short out of range, using 0xFFFF instead.");
					AddAllBytes(targetList, ByteArrayToList(BitConverter.GetBytes((ushort)0xFFFF)));
				}
				else AddAllBytes(targetList, ByteArrayToList(BitConverter.GetBytes((ushort)Data)));
			}
		}

		public static void AddSignedShortAsBytes(List<byte> targetList, int Data)
        {
			AddAllBytes(targetList, ByteArrayToList(BitConverter.GetBytes((short)Data)));
        }

		public static void AddCharsAsBytes(List<byte> targetList, string Data)
		{
			char[] Chars = Data.ToCharArray();
			foreach (char c in Chars)
			{
				targetList.Add(Convert.ToByte(c));
			}
		}

		public static void AddAllBytes(List<byte> targetList, List<byte> sourceList)
		{
			foreach (byte o in sourceList)
			{
				targetList.Add(o);
			}
		}

		public static List<byte> UpdateIntAtPosition(List<byte> targetList, int Position, int newValue)
		{
			byte[] bytes = BitConverter.GetBytes(newValue);
			for(int i = 0; i < 4; i++)
			{
				targetList[Position + i] = bytes[i];
			}

			return targetList;
		}

		public static List<byte> UpdateIntAtPosition(List<byte> targetList, int Position, int newValue, out int outValue)
		{
			byte[] bytes = BitConverter.GetBytes(newValue);
			for (int i = 0; i < 4; i++)
			{
				targetList[Position + i] = bytes[i];
			}
			outValue = newValue;

			return targetList;
		}

		public static List<byte> UpdateShortAtPosition(List<byte> targetList, int Position, int newValue)
		{
			byte[] bytes = BitConverter.GetBytes(newValue);
			for (int i = 0; i < 2; i++)
			{
				targetList[Position + i] = bytes[i];
			}

			return targetList;
		}
		public static List<byte> UpdateShortAtPosition(List<byte> targetList, int Position, int newValue, out int outValue)
		{
			byte[] bytes = BitConverter.GetBytes(newValue);
			for (int i = 0; i < 2; i++)
			{
				targetList[Position + i] = bytes[i];
			}
			outValue = newValue;

			return targetList;
		}

		public static List<byte> ByteArrayToList(byte[] ByteData)
		{
			List<byte> returnList = new List<byte>();
			foreach (byte b in ByteData)
			{
				returnList.Add(b); 
			}
			return returnList;
		}
		//TODO LARGE UINT32s WILL GET BODIED BY CONVERSION TO INT32?
		public static int ReadInt(bool IsLong, int Offset, byte[] Data)
		{
			int ReturnValue;
			int HexInt;
			if (IsLong) 
			{ 
				HexInt = BitConverter.ToInt32(Data, Offset);
			}
			else 
			{ 
				HexInt = BitConverter.ToUInt16(Data, Offset);
			}	
			ReturnValue = HexInt;
			//Console.WriteLine(ReturnValue);
			return ReturnValue;
		}

		public static void ReadToNextNonNullByte(int Offset, byte[] Data, out int EndOffset, out int ByteValue)
        {
			ByteValue = 0;
			EndOffset = 0;

			for (int i = Offset; i < Data.Length; i++)
			{
				if (Data[i] != 0x00) 
				{ 
					ByteValue = Data[i];
					EndOffset = i + 0x01; //We want the next byte
					break;
				}
			}
		}

		public static int ReadSignedShort(int Offset, byte[] Data)
        {
			int ReturnValue = BitConverter.ToInt16(Data, Offset);

			return ReturnValue;
        }

		public static float ReadFloat(int Offset, byte[] Data)
		{
			float ReturnValue;
			float HexFloat;
			HexFloat = BitConverter.ToSingle(Data, Offset);
			ReturnValue = HexFloat;
			//Console.WriteLine(ReturnValue);
			return ReturnValue;
		}
		public static string ReadString(int Offset, int Length, byte[] Data) //TODO something ????!!!???
		{
			string ReturnValue;			
			ReturnValue = Data[Offset].ToString("X");
			return ReturnValue;
		}

		public static byte[] ReadStringToArray(int offset, int length, byte[] targetArray, int targetArrayLength)
		{

			int maxLen = targetArrayLength - offset;

			if (maxLen < length) { length = maxLen; }

			byte[] byteReturn = new byte[length];

			for(int i = 0; i < length; i++)
			{
				byteReturn[i] = targetArray[i + offset];
			}
			return byteReturn;
		}

		public static byte[] ReadZeroTermStringToArray(int offset, byte[] targetArray, int targetArrayLength)
		{

			int maxLen = targetArrayLength - offset;

			int length = 0;

			for (int i = 0; i < maxLen; i++)
			{
				if (targetArray[offset + i] == 0) { length = i; break; }
			}

			byte[] byteReturn = new byte[length];

			for (int i = 0; i < length; i++)
			{
				byteReturn[i] = targetArray[i + offset];
			}
			return byteReturn;
		}

		public static byte[] ChopByteArray(byte[] source, int StartOffset, int Length)
		{
			byte[] chop = new byte[Length];
			for (int i = 0; i < Length; i++)
			{
				chop[i] = source[StartOffset + i];
			}
			return chop;
		}
		#endregion Binary Methods

		public static byte[] StringToHexBytes(string Name, int Length)
		{
			byte[] bytes = new byte[Length];
			string byteString = Name.Trim();
			string leftString;
			for (int i = 0; i<bytes.Length; i++)
			{
				if (byteString.Length < 2)
					bytes[i] = 0x00;
				else
				{
					leftString = byteString.Substring(0, 2);
					byteString = byteString.Substring(2);
					bytes[i] = Byte.Parse(leftString, NumberStyles.HexNumber);
				}
			}
			return bytes;
		}

		#region Program Settings
		public static void SaveShaders()
		{
			string data = string.Empty;
			foreach (string s in Shaders.Keys)
			{
				data = data + s + Environment.NewLine;
			}
			File.WriteAllText(shaderFilename, data);
		}

		public static void ReadShaders()
		{
			if (File.Exists(shaderFilename))
			{
				Shaders = new Dictionary<string, int>();
				string[] lines = File.ReadAllLines(shaderFilename);
				for (int i=0; i< lines.Length; i++)
				{
					Shaders.Add(lines[i], i);
				}
			}
		}

		public static void SaveShadersProperties()
		{
			string data = string.Empty;
			foreach (string s in ShadersProperties.Values)
			{
				data = data + s + Environment.NewLine;
			}
			File.WriteAllText(shaderPropertiesFilename, data);
		}

		public static void ReadShadersProperties()
		{
			if (File.Exists(shaderPropertiesFilename))
			{
				ShadersProperties = new Dictionary<string, string>();
				string[] lines = File.ReadAllLines(shaderPropertiesFilename);
				for (int i=0; i< lines.Length; i++)
				{
					string[] Value = lines[i].Split(' ');
					ShadersProperties.Add(Value[0], lines[i]);
				}
			}
		}

		#endregion Program Settings
	}
}
