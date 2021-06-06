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
using System.Diagnostics;
using CSharpImageLibrary;
using static CSharpImageLibrary.ImageFormats;

namespace USF4_Stage_Tool
{
    #region ZlibDecoder
    /// <summary>
    /// public domain zlib decode    
    /// original: v0.2  Sean Barrett 2006-11-18
    /// ported to C# by Tammo Hinrichs, 2012-08-02
    /// simple implementation
    /// - all input must be provided in an upfront buffer
    /// - all output is written to a single output buffer
    /// - Warning: This is SLOW. It's no miracle .NET as well as Mono implement DeflateStream natively.
    /// </summary>
    public class ZlibDecoder
	{
		/// <summary>
		/// Decode deflated data
		/// </summary>
		/// <param name="compressed">deflated input data</param>
		/// <returns>uncompressed output</returns>
		public static List<byte> Inflate(IList<byte> compressed)
		{
			return new ZlibDecoder { In = compressed }.Inflate();
		}

		#region internal

		// fast-way is faster to check than jpeg huffman, but slow way is slower
		private const int FastBits = 9; // accelerate all cases in default tables
		private const int FastMask = ((1 << FastBits) - 1);

		private static readonly int[] DistExtra = new[]
		{
			0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9,
			10, 10, 11, 11, 12, 12, 13, 13
		};

		private static readonly int[] LengthBase = new[]
		{
			3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
			15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
			67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0
		};

		private static readonly int[] LengthExtra = new[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4,
			4, 4, 4, 5, 5, 5, 5, 0, 0, 0
		};

		private static readonly int[] DistBase = new[]
		{
			1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
			257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193,
			12289, 16385, 24577, 0, 0
		};

		private static readonly int[] LengthDezigzag = new[]
		{
			16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2,
			14,
			1, 15
		};

		// @TODO: should statically initialize these for optimal thread safety
		private static readonly byte[] DefaultLength = new byte[288];
		private static readonly byte[] DefaultDistance = new byte[32];

		private List<byte> Out;
		private UInt32 CodeBuffer;
		private int NumBits;

		private Huffman Distance;
		private Huffman Length;

		private int InPos;
		private IList<byte> In;

		private static void InitDefaults()
		{
			int i; // use <= to match clearly with spec
			for (i = 0; i <= 143; ++i) DefaultLength[i] = 8;
			for (; i <= 255; ++i) DefaultLength[i] = 9;
			for (; i <= 279; ++i) DefaultLength[i] = 7;
			for (; i <= 287; ++i) DefaultLength[i] = 8;

			for (i = 0; i <= 31; ++i) DefaultDistance[i] = 5;
		}

		private static int BitReverse16(int n)
		{
			n = ((n & 0xAAAA) >> 1) | ((n & 0x5555) << 1);
			n = ((n & 0xCCCC) >> 2) | ((n & 0x3333) << 2);
			n = ((n & 0xF0F0) >> 4) | ((n & 0x0F0F) << 4);
			n = ((n & 0xFF00) >> 8) | ((n & 0x00FF) << 8);
			return n;
		}

		private static int BitReverse(int v, int bits)
		{
			Debug.Assert(bits <= 16);
			// to bit reverse n bits, reverse 16 and shift
			// e.g. 11 bits, bit reverse and shift away 5
			return BitReverse16(v) >> (16 - bits);
		}

		private int Get8()
		{
			return InPos >= In.Count ? 0 : In[InPos++];
		}

		private void FillBits()
		{
			do
			{
				Debug.Assert(CodeBuffer < (1U << NumBits));
				CodeBuffer |= (uint)(Get8() << NumBits);
				NumBits += 8;
			} while (NumBits <= 24);
		}

		private uint Receive(int n)
		{
			if (NumBits < n) FillBits();
			var k = (uint)(CodeBuffer & ((1 << n) - 1));
			CodeBuffer >>= n;
			NumBits -= n;
			return k;
		}

		private int HuffmanDecode(Huffman z)
		{
			int s;
			if (NumBits < 16) FillBits();
			int b = z.Fast[CodeBuffer & FastMask];
			if (b < 0xffff)
			{
				s = z.Size[b];
				CodeBuffer >>= s;
				NumBits -= s;
				return z.Value[b];
			}

			// not resolved by fast table, so compute it the slow way
			// use jpeg approach, which requires MSbits at top
			int k = BitReverse((int)CodeBuffer, 16);
			for (s = FastBits + 1; ; ++s)
				if (k < z.MaxCode[s])
					break;
			if (s == 16) return -1; // invalid code!
									// code size is s, so:
			b = (k >> (16 - s)) - z.FirstCode[s] + z.FirstSymbol[s];
			Debug.Assert(z.Size[b] == s);
			CodeBuffer >>= s;
			NumBits -= s;
			return z.Value[b];
		}

		private void ParseHuffmanBlock()
		{
			for (; ; )
			{
				int z = HuffmanDecode(Length);
				if (z < 256)
				{
					if (z < 0) throw new Exception("bad huffman code"); // error in huffman codes
					Out.Add((byte)z);
				}
				else
				{
					if (z == 256) return;
					z -= 257;
					int len = LengthBase[z];
					if (LengthExtra[z] != 0) len += (int)Receive(LengthExtra[z]);
					z = HuffmanDecode(Distance);
					if (z < 0) throw new Exception("bad huffman code");
					int dist = DistBase[z];
					if (DistExtra[z] != 0) dist += (int)Receive(DistExtra[z]);
					dist = Out.Count - dist;
					if (dist < 0) throw new Exception("bad dist");
					for (int i = 0; i < len; i++, dist++)
						Out.Add(Out[dist]);
				}
			}
		}

		private void ComputeHuffmanCodes()
		{
			var lenCodes = new byte[286 + 32 + 137]; //padding for maximum single op
			var codeLengthSizes = new byte[19];

			uint hlit = Receive(5) + 257;
			uint hdist = Receive(5) + 1;
			uint hclen = Receive(4) + 4;

			for (int i = 0; i < hclen; ++i)
				codeLengthSizes[LengthDezigzag[i]] = (byte)Receive(3);

			var codeLength = new Huffman(new ArraySegment<byte>(codeLengthSizes));

			int n = 0;
			while (n < hlit + hdist)
			{
				int c = HuffmanDecode(codeLength);
				Debug.Assert(c >= 0 && c < 19);
				if (c < 16)
					lenCodes[n++] = (byte)c;
				else if (c == 16)
				{
					c = (int)Receive(2) + 3;
					for (int i = 0; i < c; i++) lenCodes[n + i] = lenCodes[n - 1];
					n += c;
				}
				else if (c == 17)
				{
					c = (int)Receive(3) + 3;
					for (int i = 0; i < c; i++) lenCodes[n + i] = 0;
					n += c;
				}
				else
				{
					Debug.Assert(c == 18);
					c = (int)Receive(7) + 11;
					for (int i = 0; i < c; i++) lenCodes[n + i] = 0;
					n += c;
				}
			}
			if (n != hlit + hdist) throw new Exception("bad codelengths");
			Length = new Huffman(new ArraySegment<byte>(lenCodes, 0, (int)hlit));
			Distance = new Huffman(new ArraySegment<byte>(lenCodes, (int)hlit, (int)hdist));
		}

		private void ParseUncompressedBlock()
		{
			var header = new byte[4];
			if ((NumBits & 7) != 0)
				Receive(NumBits & 7); // discard
									  // drain the bit-packed data into header
			int k = 0;
			while (NumBits > 0)
			{
				header[k++] = (byte)(CodeBuffer & 255); // wtf this warns?
				CodeBuffer >>= 8;
				NumBits -= 8;
			}
			Debug.Assert(NumBits == 0);
			// now fill header the normal way
			while (k < 4)
				header[k++] = (byte)Get8();
			int len = header[1] * 256 + header[0];
			int nlen = header[3] * 256 + header[2];
			if (nlen != (len ^ 0xffff)) throw new Exception("zlib corrupt");
			if (InPos + len > In.Count) throw new Exception("read past buffer");

			// TODO: this sucks. DON'T USE LINQ.
			Out.AddRange(In.Skip(InPos).Take(len));
			InPos += len;
		}

		private List<byte> Inflate()
		{
			Out = new List<byte>();
			NumBits = 0;
			CodeBuffer = 0;

			bool final;
			do
			{
				final = Receive(1) != 0;
				var type = (int)Receive(2);
				if (type == 0)
				{
					ParseUncompressedBlock();
				}
				else if (type == 3)
				{
					throw new Exception("invalid block type");
				}
				else
				{
					if (type == 1)
					{
						// use fixed code lengths
						if (DefaultDistance[31] == 0) InitDefaults();
						Length = new Huffman(new ArraySegment<byte>(DefaultLength));
						Distance = new Huffman(new ArraySegment<byte>(DefaultDistance));
					}
					else
					{
						ComputeHuffmanCodes();
					}
					ParseHuffmanBlock();
				}
			} while (!final);

			return Out;
		}


		private class Huffman
		{
			public readonly UInt16[] Fast = new UInt16[1 << FastBits];
			public readonly UInt16[] FirstCode = new UInt16[16];
			public readonly UInt16[] FirstSymbol = new UInt16[16];
			public readonly int[] MaxCode = new int[17];
			public readonly Byte[] Size = new Byte[288];
			public readonly UInt16[] Value = new UInt16[288];

			public Huffman(ArraySegment<byte> sizeList)
			{
				int i;
				int k = 0;
				var nextCode = new int[16];
				var sizes = new int[17];

				// DEFLATE spec for generating codes
				for (i = 0; i < Fast.Length; i++) Fast[i] = 0xffff;
				for (i = 0; i < sizeList.Count; ++i)
					++sizes[sizeList.Array[i + sizeList.Offset]];
				sizes[0] = 0;
				for (i = 1; i < 16; ++i)
					Debug.Assert(sizes[i] <= (1 << i));
				int code = 0;
				for (i = 1; i < 16; ++i)
				{
					nextCode[i] = code;
					FirstCode[i] = (UInt16)code;
					FirstSymbol[i] = (UInt16)k;
					code = (code + sizes[i]);
					if (sizes[i] != 0)
						if (code - 1 >= (1 << i)) throw new Exception("bad codelengths");
					MaxCode[i] = code << (16 - i); // preshift for inner loop
					code <<= 1;
					k += sizes[i];
				}
				MaxCode[16] = 0x10000; // sentinel
				for (i = 0; i < sizeList.Count; ++i)
				{
					int s = sizeList.Array[i + sizeList.Offset];
					if (s != 0)
					{
						int c = nextCode[s] - FirstCode[s] + FirstSymbol[s];
						Size[c] = (byte)s;
						Value[c] = (UInt16)i;
						if (s <= FastBits)
						{
							int j = BitReverse(nextCode[s], s);
							while (j < (1 << FastBits))
							{
								Fast[j] = (UInt16)c;
								j += (1 << s);
							}
						}
						++nextCode[s];
					}
				}
			}
		}

		#endregion
	}
    #endregion

    public static class Utils
	{

		public static string shaderFilename = "./Shaders.dat";
		public static string shaderPropertiesFilename = "./ShadersProperties.dat";
		public static Dictionary<string, int> Shaders = new Dictionary<string, int>();
		public static Dictionary<string, string> ShadersProperties = new Dictionary<string, string>();

		public static byte[] MakeModelName(string Name)
		{
			byte[] bytes = new byte[0x20];
			byte[] stringBytes = Encoding.ASCII.GetBytes(Name);
			for (int i = 0; i < bytes.Length; i++)
			{
				if (i < stringBytes.Length)
				{
					bytes[i] = stringBytes[i];
				}
				else
				{
					bytes[i] = 0x00;
				}
			}
			return bytes;
		}

		public static string NormalizeWhiteSpace(string input)
		{
			int len = input.Length,
				index = 0,
				i = 0;
			var src = input.ToCharArray();
			bool skip = false;
			char ch;
			for (; i < len; i++)
			{
				ch = src[i];
				switch (ch)
				{
					case '\u0020':
					case '\u00A0':
					case '\u1680':
					case '\u2000':
					case '\u2001':
					case '\u2002':
					case '\u2003':
					case '\u2004':
					case '\u2005':
					case '\u2006':
					case '\u2007':
					case '\u2008':
					case '\u2009':
					case '\u200A':
					case '\u202F':
					case '\u205F':
					case '\u3000':
					case '\u2028':
					case '\u2029':
					case '\u0009':
					case '\u000A':
					case '\u000B':
					case '\u000C':
					case '\u000D':
					case '\u0085':
						if (skip) continue;
						src[index++] = ch;
						skip = true;
						continue;
					default:
						skip = false;
						src[index++] = ch;
						continue;
				}
			}

			return new string(src, 0, index);
		}

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

			ry = Convert.ToSingle(2 * Math.PI - Math.Atan2(-matrix.M13, matrix.M11));
			rx = Convert.ToSingle(Math.Atan2(-matrix.M32, matrix.M22));
			rz = Convert.ToSingle(Math.Asin(matrix.M12));

			if (ry != 0 || rx != 0 || rz != 0) return;
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

			double dry = -Math.Atan2(2 * (p0 * p3 - e * p1 * p2), 1 - 2 * (p2 * p2 + p3 * p3));
			double drz = Math.Asin(2 * (p0 * p2 + e * p1 * p3));
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
			ty = -translation.Z;
			tz = translation.Y;

			//rx = Convert.ToSingle(drx * 180d/Math.PI);
			//ry = Convert.ToSingle(dry * 180d / Math.PI);
			//rz = Convert.ToSingle(drz * 180d / Math.PI);

			rx = Convert.ToSingle((180 / Math.PI) * drx);
			ry = Convert.ToSingle((180 / Math.PI) * dry);
			rz = Convert.ToSingle((180 / Math.PI) * drz);

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
			if (steps % 3 == 1) return new int[]{ Array[2], Array[0], Array[1] };
			else if (steps % 3 == 2) return new int[]{ Array[1], Array[2], Array[0] };
			else return Array;
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

		public static uint ReadInt(bool IsLong, uint Offset, byte[] Data)
        {
			uint ReturnValue;
			if (IsLong)
			{
				ReturnValue = BitConverter.ToUInt32(new byte[] { Data[Offset], Data[Offset + 1], Data[Offset + 2], Data[Offset + 3] }, 0);
			}
			else
			{
				ReturnValue = BitConverter.ToUInt16(new byte[] { Data[Offset], Data[Offset + 1] }, 0);
			}
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
			return ReturnValue;
		}
		public static float ReadFloat(uint Offset, byte[] Data)
        {
			float ReturnValue;
			ReturnValue = BitConverter.ToSingle(new byte[] { Data[Offset], Data[Offset + 1], Data[Offset + 2], Data[Offset + 3] }, 0);
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
		public static byte[] ReadZeroTermStringToArray(uint offset, byte[] targetArray, uint targetArrayLength)
		{

			uint maxLen = targetArrayLength - offset;

			uint length = 0;

			for (uint i = 0; i < maxLen; i++)
			{
				if (targetArray[offset + i] == 0) { length = i; break; }
			}

			byte[] byteReturn = new byte[length];

			for (uint i = 0; i < length; i++)
			{
				byteReturn[i] = targetArray[i + offset];
			}
			return byteReturn;
		}

		public static T[] Slice<T>(this T[] source, int start, int length)
		{
			if (length == 0) length = source.Length - start;
			// Return new array.
			T[] res = new T[length];
			for (int i = 0; i < length; i++)
			{
				res[i] = source[i + start];
			}
			return res;
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
