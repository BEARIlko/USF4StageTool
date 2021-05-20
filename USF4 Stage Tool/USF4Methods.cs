using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USF4_Stage_Tool
{
	public static class USF4Methods
	{
		public static int CSB = 0x46545540;
		public static int EMA = 0x414D4523;
		public static int EMB = 0x424D4523;
		public static int EMG = 0x474D4523;
		public static int EMM = 0x4D4D4523;
		public static int EMO = 0x4F4D4523;
		public static int LUA = 0x61754C1B;
		public static int DDS = 0x20534444;

		public static void CheckFile(int FileNumber)
		{
			if (FileNumber == LUA)
			{

			}
		}
	}
}
