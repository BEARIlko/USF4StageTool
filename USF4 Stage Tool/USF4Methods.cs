using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USF4_Stage_Tool
{
	public static class USF4Methods
	{
        public static Dictionary<int, int> GetModelFlag = new Dictionary<int, int>
        { 
			{ 0x14, 0x0005 },
			{ 0x18, 0x0045 },
			{ 0x20, 0x0007 },
			{ 0x24, 0x0047 },
			{ 0x28, 0x0203 },
			{ 0x34, 0x0247 },
			{ 0x40, 0x02C7 },
		};
		public static Dictionary<int, int> GetModelBitDepth = new Dictionary<int, int>
		{
			{ 0x0005, 0x14 },
			{ 0x0045, 0x18 },
			{ 0x0007, 0x20 },
			{ 0x0047, 0x24 },
			{ 0x0203, 0x28 },
			{ 0x0247, 0x34 },
			{ 0x02C7, 0x40 }
		};

		public static int CSB = 0x46545540;
		public static int EMA = 0x414D4523;
		public static int EMB = 0x424D4523;
		public static int EMG = 0x474D4523;
		public static int EMM = 0x4D4D4523;
		public static int EMO = 0x4F4D4523;
		public static int LUA = 0x61754C1B;
		public static int DDS = 0x20534444;
		public static int BSR = 0x52534223;

		public static USF4File CheckFile(byte[] Data)
        {
			return CheckFile(Utils.ReadInt(true, 0, Data));
        }

		public static USF4File CheckFile(int FileNumber)
		{
			if (FileNumber == CSB) return new CSB();
			else if (FileNumber == EMA) return new EMA();
			else if (FileNumber == EMB) return new EMB();
			else if (FileNumber == EMG) return new EMG();
			else if (FileNumber == EMM) return new EMM();
			else if (FileNumber == EMO) return new EMO();
			else if (FileNumber == LUA) return new LUA();
			else if (FileNumber == DDS) return new DDS();
			else if (FileNumber == BSR) return new BSR();
			else return new USF4File();
		}
	}
}
