using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace USF4_Stage_Tool
{
    #region USF4 Structs

    public class USF4File
    {
		public byte[] HEXBytes;
		public byte[] Name;
		public virtual void ReadFile(byte[] Data)
        {
			HEXBytes = Data;
        }
		public virtual void GenerateBytes()
        {

        }
	}

	public class EMZ : USF4File
    {
		public int NumberOfFiles;
		public int FileListPointer;
		public int FileNamesPointer;
		public List<int> FilePointersList;
		public Dictionary<int, USF4File> Files;
		public List<int> FileLengthsList;
		public List<int> FileNamePointersList;       //Each entry in this list points to an entry in the EMZ file name list
		public List<byte[]> FileNamesList;

		public EMZ()
        {
		}
		public EMZ(byte[] Data)
		{

			HEXBytes = Data;
			NumberOfFiles = Utils.ReadInt(true, 0x0C, Data);
			FileListPointer = Utils.ReadInt(true, 0x18, Data);
			FileNamesPointer = Utils.ReadInt(true, 0x1C, Data);
			FileLengthsList = new List<int>();
			FilePointersList = new List<int>();
			Files = new Dictionary<int, USF4File>();
			FileNamesList = new List<byte[]>();
			FileNamePointersList = new List<int>();


			for (int i = 0; i < NumberOfFiles; i++)
			{
				FilePointersList.Add(Utils.ReadInt(true, FileListPointer + (i * 8), Data));
				FileLengthsList.Add(Utils.ReadInt(true, FileListPointer + (i * 8) + 4, Data));
				FileNamePointersList.Add(Utils.ReadInt(true, FileNamesPointer + (i * 4), Data));
				FileNamesList.Add(Utils.ReadZeroTermStringToArray(FileNamePointersList[i], Data, Data.Length));
				int FileType = Utils.ReadInt(true, FilePointersList[i] + FileListPointer + (i * 8), Data);

				USF4File file;

				if (FileType == USF4Methods.EMO) file = new EMO();
				else if (FileType == USF4Methods.EMM) file = new EMM();
				else if (FileType == USF4Methods.EMB) file = new EMB();
				else if (FileType == USF4Methods.LUA) file = new LUA();
				else if (FileType == USF4Methods.EMA) file = new EMA();
				else if (FileType == USF4Methods.CSB) file = new CSB();
				else file = new OtherFile();

				file.ReadFile(Utils.ChopByteArray(Data, FilePointersList[i] + FileListPointer + (i * 8), FileLengthsList[i]));
				file.Name = FileNamesList[i];
				Files.Add(i, file);
			}

		}
		public override void ReadFile(byte[] Data)
		{

			HEXBytes = Data;
			NumberOfFiles = Utils.ReadInt(true, 0x0C, Data);
			FileListPointer = Utils.ReadInt(true, 0x18, Data);
			FileNamesPointer = Utils.ReadInt(true, 0x1C, Data);
			FileLengthsList = new List<int>();
			FilePointersList = new List<int>();
			Files = new Dictionary<int, USF4File>();
			FileNamesList = new List<byte[]>();
			FileNamePointersList = new List<int>();


			for (int i = 0; i < NumberOfFiles; i++)
			{
				FilePointersList.Add(Utils.ReadInt(true, FileListPointer + (i * 8), Data));
				FileLengthsList.Add(Utils.ReadInt(true, FileListPointer + (i * 8) + 4, Data));
				FileNamePointersList.Add(Utils.ReadInt(true, FileNamesPointer + (i * 4), Data));
				FileNamesList.Add(Utils.ReadZeroTermStringToArray(FileNamePointersList[i], Data, Data.Length));
				int FileType = Utils.ReadInt(true, FilePointersList[i] + FileListPointer + (i * 8), Data);

				USF4File file;

				if (FileType == USF4Methods.EMO) file = new EMO();
				else if (FileType == USF4Methods.EMM) file = new EMM();
				else if (FileType == USF4Methods.EMB) file = new EMB();
				else if (FileType == USF4Methods.LUA) file = new LUA();
				else if (FileType == USF4Methods.EMA) file = new EMA();
				else if (FileType == USF4Methods.CSB) file = new CSB();
				else file = new OtherFile();

				file.ReadFile(Utils.ChopByteArray(Data, FilePointersList[i] + FileListPointer + (i * 8), FileLengthsList[i]));
				file.Name = FileNamesList[i];
				Files.Add(i, file);
			}

		}
		public override void GenerateBytes()
		{
			List<byte> Data = new List<byte>();
			List<int> FilePointerPositions = new List<int>();
			List<int> FileLengthPositions = new List<int>();
			List<int> FileNamePointerPositions = new List<int>();
			Utils.AddCopiedBytes(Data, 0, 0x0C, HEXBytes);
			Utils.AddIntAsBytes(Data, NumberOfFiles, true);
			Utils.AddPaddingZeros(Data, 0x18, Data.Count);
			Utils.AddIntAsBytes(Data, FileListPointer, true);
			int FileNameListPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, FileNamesPointer, true);

			for (int i = 0; i < NumberOfFiles; i++)
			{
				FilePointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, FilePointersList[i], true);
				FileLengthPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, FileLengthsList[i], true);
			}

			Utils.UpdateIntAtPosition(Data, FileNameListPointerPosition, Data.Count);

			for (int i = 0; i < NumberOfFiles; i++)
			{
				FileNamePointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, FileNamePointersList[i], true);
			}

			Utils.AddZeroToLineEnd(Data);

			for (int i = 0; i < FilePointersList.Count; i++)
			{
				USF4File file = Files[i];
				Utils.UpdateIntAtPosition(Data, FilePointerPositions[i], Data.Count - (0x20 + i * 8));
				FilePointersList[i] = Data.Count - (0x20 + i * 8);
				/////////

				FileLengthsList[i] = file.HEXBytes.Length;
				Utils.UpdateIntAtPosition(Data, FileLengthPositions[i], file.HEXBytes.Length);
				Utils.AddCopiedBytes(Data, 0x00, file.HEXBytes.Length, file.HEXBytes);

				Utils.AddZeroToLineEnd(Data);
			}

			for (int i = 0; i < FileNamesList.Count; i++)
			{
				Utils.UpdateIntAtPosition(Data, FileNamePointerPositions[i], Data.Count);
				FileNamePointersList[i] = Data.Count;

				Utils.AddCopiedBytes(Data, 0x00, FileNamesList[i].Length, FileNamesList[i]);
				Utils.AddCopiedBytes(Data, 0x00, 0x01, new byte[] { 0x00 });
			}

			HEXBytes = Data.ToArray();
		}
	}

	public class OtherFile : USF4File
	{

	}

	public class EMB : USF4File
	{
		public int NumberOfFiles;
		public int FileListPointer;
		public int FileNameListPointer;
		public List<DDS> DDSFiles;
		public List<int> FilePointerList;
		public List<int> FileLengthList;
		public List<int> FileNamePointerList;
		public List<byte[]> FileNameList; //Normally EMB packs don't have file lists, but we can add one without damaging anything!

		public EMB()
        {

        }
		public EMB(byte[] Data, byte[] name)
        {
			HEXBytes = Data;
			Name = name;
			NumberOfFiles = Utils.ReadInt(true, 0x0C, Data);
			FileListPointer = Utils.ReadInt(true, 0x18, Data);
			FileNameListPointer = Utils.ReadInt(true, 0x1C, Data);
			FileNamePointerList = new List<int>();
			FileNameList = new List<byte[]>();
			FilePointerList = new List<int>();
			FileLengthList = new List<int>();
			DDSFiles = new List<DDS>();

			for (int i = 0; i < NumberOfFiles; i++)
			{
				FilePointerList.Add(Utils.ReadInt(true, FileListPointer + i * 8, Data));
				FileLengthList.Add(Utils.ReadInt(true, FileListPointer + i * 8 + 0x04, Data));
			}
			for (int i = 0; i < NumberOfFiles; i++)
			{
				if (FileNameListPointer == 0x00) //if there wasn't a file index, add a dummy one
				{
					FileNamePointerList.Add(0x00);
					FileNameList.Add(new byte[] { 0x44, 0x44, 0x53 });
				}
				else
				{
					FileNamePointerList.Add(Utils.ReadInt(true, FileNameListPointer + i * 4, Data));
					FileNameList.Add(Utils.ReadZeroTermStringToArray(FileNamePointerList[i], HEXBytes, HEXBytes.Length));
				}
			}
			for (int i = 0; i < NumberOfFiles; i++)
			{
				DDS WorkingDDS = new DDS();
				WorkingDDS.HEXBytes = Utils.ChopByteArray(HEXBytes, FilePointerList[i] + FileListPointer + (i * 8), FileLengthList[i]);
				DDSFiles.Add(WorkingDDS);
			}
		}
        public override void ReadFile(byte[] Data)
        {
			HEXBytes = Data;
			NumberOfFiles = Utils.ReadInt(true, 0x0C, Data);
			FileListPointer = Utils.ReadInt(true, 0x18, Data);
			FileNameListPointer = Utils.ReadInt(true, 0x1C, Data);
			FileNamePointerList = new List<int>();
			FileNameList = new List<byte[]>();
			FilePointerList = new List<int>();
			FileLengthList = new List<int>();
			DDSFiles = new List<DDS>();

			for (int i = 0; i < NumberOfFiles; i++)
			{
				FilePointerList.Add(Utils.ReadInt(true, FileListPointer + i * 8, Data));
				FileLengthList.Add(Utils.ReadInt(true, FileListPointer + i * 8 + 0x04, Data));
			}
			for (int i = 0; i < NumberOfFiles; i++)
			{
				if (FileNameListPointer == 0x00) //if there wasn't a file index, add a dummy one
				{
					FileNamePointerList.Add(0x00);
					FileNameList.Add(new byte[] { 0x44, 0x44, 0x53 });
				}
				else
				{
					FileNamePointerList.Add(Utils.ReadInt(true, FileNameListPointer + i * 4, Data));
					FileNameList.Add(Utils.ReadZeroTermStringToArray(FileNamePointerList[i], HEXBytes, HEXBytes.Length));
				}
			}
			for (int i = 0; i < NumberOfFiles; i++)
			{
				DDS WorkingDDS = new DDS();
				WorkingDDS.HEXBytes = Utils.ChopByteArray(HEXBytes, FilePointerList[i] + FileListPointer + (i * 8), FileLengthList[i]);
				DDSFiles.Add(WorkingDDS);
			}
		}
        public override void GenerateBytes()
        {
			List<byte> Data = new List<byte>();

			Data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x42, 0xFE, 0xFF, 0x20, 0x00, 0x01, 0x00, 0x01, 0x00 });
			Utils.AddIntAsBytes(Data, DDSFiles.Count, true);
			Utils.AddPaddingZeros(Data, 0x18, Data.Count);
			Utils.AddIntAsBytes(Data, FileListPointer, true);

			int FileNameListPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, FileNameListPointer, true);

			List<int> FilePointerPositions = new List<int>();
			List<int> FileLengthPositions = new List<int>();
			List<int> FileNamePointerPositions = new List<int>();

			for (int i = 0; i < DDSFiles.Count; i++)
			{
				FilePointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, FilePointerList[i], true);
				FileLengthPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, FileLengthList[i], true);
			}

			Utils.UpdateIntAtPosition(Data, FileNameListPointerPosition, Data.Count, out FileNameListPointer);
			for (int i = 0; i < DDSFiles.Count; i++)
			{
				FileNamePointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, FileNamePointerList[i], true);
			}
			for (int i = 0; i < DDSFiles.Count; i++)
			{
				Utils.AddZeroToLineEnd(Data);
				Utils.UpdateIntAtPosition(Data, FilePointerPositions[i], Data.Count - FilePointerPositions[i]);
				FilePointerList[i] = Data.Count - FilePointerPositions[i];
				Utils.UpdateIntAtPosition(Data, FileLengthPositions[i], DDSFiles[i].HEXBytes.Length);
				FileLengthList[i] = DDSFiles[i].HEXBytes.Length;

				Utils.AddCopiedBytes(Data, 0x00, DDSFiles[i].HEXBytes.Length, DDSFiles[i].HEXBytes);
			}
			Utils.AddZeroToLineEnd(Data);

			for (int i = 0; i < DDSFiles.Count; i++)
			{
				Utils.UpdateIntAtPosition(Data, FileNamePointerPositions[i], Data.Count);
				FileNamePointerList[i] = Data.Count;
				Utils.AddCopiedBytes(Data, 0x00, FileNameList[i].Length, FileNameList[i]);
				Data.Add(0x00);
			}
			Utils.AddZeroToLineEnd(Data);


			HEXBytes = Data.ToArray();
		}
	}

	public struct DDS
	{
		public byte[] HEXBytes;
		public byte[] Name;
	}

	public class CSB : USF4File
	{	
		public CSB()
        {

        }
	}

	public class LUA : USF4File
	{
	}

	public class EMM : USF4File
	{
		public int MaterialCount;
		public List<int> MaterialPointersList;
		public List<Material> Materials;

		public EMM()
        {

        }

		public EMM(byte[] Data, byte[] name)
		{
			Name = name;
			HEXBytes = Data;
			MaterialCount = Utils.ReadInt(true, 0x10, Data);
			MaterialPointersList = new List<int>();
			Materials = new List<Material>();

			for (int i = 0; i < MaterialCount; i++)
			{
				MaterialPointersList.Add(Utils.ReadInt(true, 0x14 + i * 4, Data));
				Materials.Add(new Material(Utils.ChopByteArray(Data, MaterialPointersList[i] + 0x10, Data.Length - (MaterialPointersList[i] + 0x10))));
			}
		}

        public override void ReadFile(byte[] Data)
        {
			HEXBytes = Data;
			MaterialCount = Utils.ReadInt(true, 0x10, Data);
			MaterialPointersList = new List<int>();
			Materials = new List<Material>();

			for (int i = 0; i < MaterialCount; i++)
			{
				MaterialPointersList.Add(Utils.ReadInt(true, 0x14 + i * 4, Data));
				Materials.Add(new Material(Utils.ChopByteArray(Data, MaterialPointersList[i] + 0x10, Data.Length - (MaterialPointersList[i] + 0x10))));
			}
		}
        public override void GenerateBytes()
		{
			List<byte> Data = new List<byte>();
			List<int> MaterialPointerPositions = new List<int>();
			//#EMM Header
			Data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x4D, 0xFE, 0xFF, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00 });
			Utils.AddIntAsBytes(Data, MaterialCount, true);
			for (int i = 0; i < MaterialCount; i++)
			{
				MaterialPointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, MaterialPointersList[i], true);
			}
			for (int i = 0; i < MaterialCount; i++)
			{
				Utils.UpdateIntAtPosition(Data, MaterialPointerPositions[i], Data.Count - 0x10);
				Utils.AddCopiedBytes(Data, 0, 0x20, Materials[i].Name);
				Utils.AddCopiedBytes(Data, 0, 0x20, Materials[i].Shader);
				Utils.AddIntAsBytes(Data, Materials[i].PropertyCount, true);

				for (int j = 0; j < Materials[i].PropertyCount; j++)
				{
					Utils.AddCopiedBytes(Data, 0, 0x20, Materials[i].PropertyNamesList[j]);
					Utils.AddCopiedBytes(Data, 0x00, 0x08, Materials[i].PropertyValuesList[j]);
				}
			}
			HEXBytes = Data.ToArray();
			
		}
	}

	public class EME : USF4File //TODO - Still being read as "Other File" at the moment
    {	
		public int EffectCount;
		public List<Effect> Effects;
	}

	public struct Effect
	{
		public byte[] PreData;
		public byte[] Name;
		public int Pointer1;
		public int Pointer2;
		public int Pointer3;
		public List<float> Values;
	}


	public class EMA : USF4File
	{
		public int SkeletonPointer;
		public int AnimationCount;
		public int MysteryIntOS12; //Always seems to be 3?
		public List<int> AnimationPointersList;
		public List<Animation> Animations;

		public Skeleton Skeleton;

		public EMA()
        {

        }
		public EMA(byte[] Data, byte[] name)
		{
			Name = name;
			HEXBytes = Data;
			//Populate EMA header data
			AnimationPointersList = new List<int>();
			Animations = new List<Animation>();

			SkeletonPointer = Utils.ReadInt(true, 0x0C, Data);
			AnimationCount = Utils.ReadInt(false, 0x10, Data);
			MysteryIntOS12 = Utils.ReadInt(true, 0x12, Data);
			if (MysteryIntOS12 != 0x03)
			{
				Console.WriteLine($"Mystery Int OS12: {MysteryIntOS12}");
			}
			//Populate animation index and animation list
			for (int i = 0; i < AnimationCount; i++)
			{

				int indexOffset = 0x20 + (4 * i);
				AnimationPointersList.Add(Utils.ReadInt(true, indexOffset, Data));

				//Populate animation list
				int curAnimOS = AnimationPointersList[i];
				Animation WorkingAnimation = new Animation();

				WorkingAnimation.Duration = Utils.ReadInt(false, curAnimOS, Data);
				WorkingAnimation.CmdTrackCount = Utils.ReadInt(false, curAnimOS + 0x02, Data);
				WorkingAnimation.ValueCount = Utils.ReadInt(false, curAnimOS + 0x04, Data);
				WorkingAnimation.NamePointer = Utils.ReadInt(true, curAnimOS + 0x0C, Data);
				int NameLength, NameOffset;
				Utils.ReadToNextNonNullByte(WorkingAnimation.NamePointer + curAnimOS, Data, out NameOffset, out NameLength);
				WorkingAnimation.Name = Utils.ReadStringToArray(NameOffset, NameLength, Data, Data.Length);
				WorkingAnimation.ValuesListPointer = Utils.ReadInt(true, curAnimOS + 0x10, Data);

				//Populate value list
				WorkingAnimation.ValuesList = new List<float>();

				for (int j = 0; j < WorkingAnimation.ValueCount; j++)
				{
					WorkingAnimation.ValuesList.Add(Utils.ReadFloat(curAnimOS + WorkingAnimation.ValuesListPointer + 4 * j, Data));
				}

				//Populate command index and command list
				WorkingAnimation.CmdTrackPointersList = new List<int>();
				WorkingAnimation.CMDTracks = new List<CMDTrack>();

				for (int j = 0; j < WorkingAnimation.CmdTrackCount; j++)
				{
					WorkingAnimation.CmdTrackPointersList.Add(Utils.ReadInt(true, curAnimOS + 0x14 + 4 * j, Data));

					int curCmdOS = curAnimOS + WorkingAnimation.CmdTrackPointersList[j];

					CMDTrack WorkingCMD = new CMDTrack();
					WorkingCMD.BoneID = Utils.ReadInt(false, curCmdOS, Data);
					WorkingCMD.TransformType = Data[curCmdOS + 0x02];
					WorkingCMD.BitFlag = Data[curCmdOS + 0x03];
					WorkingCMD.StepCount = Utils.ReadInt(false, curCmdOS + 0x04, Data);
					WorkingCMD.IndicesListPointer = Utils.ReadInt(false, curCmdOS + 0x06, Data);

					//Populate keyframe list and indices list
					WorkingCMD.IndicesList = new List<int>();
					WorkingCMD.StepsList = new List<int>();

					if ((WorkingCMD.BitFlag & 0x10) == 0x10)
					{

					}

					for (int k = 0; k < WorkingCMD.StepCount; k++)
					{
						//populate keyframes
						if ((WorkingCMD.BitFlag & 0x20) == 0x20)
						{
							WorkingCMD.StepsList.Add(Utils.ReadInt(false, curCmdOS + 0x08 + 2 * k, Data));
						}
						else
						{
							WorkingCMD.StepsList.Add(Data[curCmdOS + 0x08 + k]);
						}

						//Populate indices
						if ((WorkingCMD.BitFlag & 0x40) == 0x40)
						{
							WorkingCMD.IndicesList.Add(Utils.ReadInt(true, curCmdOS + WorkingCMD.IndicesListPointer + 4 * k, Data));
						}
						else
						{
							WorkingCMD.IndicesList.Add(Utils.ReadInt(false, curCmdOS + WorkingCMD.IndicesListPointer + 2 * k, Data));
						}

					}

					//cmdHeader finished, push to list and start the next one...
					WorkingAnimation.CMDTracks.Add(WorkingCMD);
				}

				//Animation finished, push to list and start the next one...
				Animations.Add(WorkingAnimation);
			}

			//All animations pushed to EMA Header, read skeleton...
			if (SkeletonPointer != 0x00)
			{
				Skeleton = new Skeleton(Utils.ChopByteArray(Data, SkeletonPointer, Data.Length - SkeletonPointer));
			}
			else Skeleton = new Skeleton();
		}

        public override void ReadFile(byte[] Data)
        {
			HEXBytes = Data;
			//Populate EMA header data
			AnimationPointersList = new List<int>();
			Animations = new List<Animation>();

			SkeletonPointer = Utils.ReadInt(true, 0x0C, Data);
			AnimationCount = Utils.ReadInt(false, 0x10, Data);
			MysteryIntOS12 = Utils.ReadInt(true, 0x12, Data);
			if (MysteryIntOS12 != 0x03)
			{
				Console.WriteLine($"Mystery Int OS12: {MysteryIntOS12}");
			}
			//Populate animation index and animation list
			for (int i = 0; i < AnimationCount; i++)
			{

				int indexOffset = 0x20 + (4 * i);
				AnimationPointersList.Add(Utils.ReadInt(true, indexOffset, Data));

				//Populate animation list
				int curAnimOS = AnimationPointersList[i];
				Animation WorkingAnimation = new Animation();

				WorkingAnimation.Duration = Utils.ReadInt(false, curAnimOS, Data);
				WorkingAnimation.CmdTrackCount = Utils.ReadInt(false, curAnimOS + 0x02, Data);
				WorkingAnimation.ValueCount = Utils.ReadInt(false, curAnimOS + 0x04, Data);
				WorkingAnimation.NamePointer = Utils.ReadInt(true, curAnimOS + 0x0C, Data);
				int NameLength, NameOffset;
				Utils.ReadToNextNonNullByte(WorkingAnimation.NamePointer + curAnimOS, Data, out NameOffset, out NameLength);
				WorkingAnimation.Name = Utils.ReadStringToArray(NameOffset, NameLength, Data, Data.Length);
				WorkingAnimation.ValuesListPointer = Utils.ReadInt(true, curAnimOS + 0x10, Data);

				//Populate value list
				WorkingAnimation.ValuesList = new List<float>();

				for (int j = 0; j < WorkingAnimation.ValueCount; j++)
				{
					WorkingAnimation.ValuesList.Add(Utils.ReadFloat(curAnimOS + WorkingAnimation.ValuesListPointer + 4 * j, Data));
				}

				//Populate command index and command list
				WorkingAnimation.CmdTrackPointersList = new List<int>();
				WorkingAnimation.CMDTracks = new List<CMDTrack>();

				for (int j = 0; j < WorkingAnimation.CmdTrackCount; j++)
				{
					WorkingAnimation.CmdTrackPointersList.Add(Utils.ReadInt(true, curAnimOS + 0x14 + 4 * j, Data));

					int curCmdOS = curAnimOS + WorkingAnimation.CmdTrackPointersList[j];

					CMDTrack WorkingCMD = new CMDTrack();
					WorkingCMD.BoneID = Utils.ReadInt(false, curCmdOS, Data);
					WorkingCMD.TransformType = Data[curCmdOS + 0x02];
					WorkingCMD.BitFlag = Data[curCmdOS + 0x03];
					WorkingCMD.StepCount = Utils.ReadInt(false, curCmdOS + 0x04, Data);
					WorkingCMD.IndicesListPointer = Utils.ReadInt(false, curCmdOS + 0x06, Data);

					//Populate keyframe list and indices list
					WorkingCMD.IndicesList = new List<int>();
					WorkingCMD.StepsList = new List<int>();

					if ((WorkingCMD.BitFlag & 0x10) == 0x10)
					{

					}

					for (int k = 0; k < WorkingCMD.StepCount; k++)
					{
						//populate keyframes
						if ((WorkingCMD.BitFlag & 0x20) == 0x20)
						{
							WorkingCMD.StepsList.Add(Utils.ReadInt(false, curCmdOS + 0x08 + 2 * k, Data));
						}
						else
						{
							WorkingCMD.StepsList.Add(Data[curCmdOS + 0x08 + k]);
						}

						//Populate indices
						if ((WorkingCMD.BitFlag & 0x40) == 0x40)
						{
							WorkingCMD.IndicesList.Add(Utils.ReadInt(true, curCmdOS + WorkingCMD.IndicesListPointer + 4 * k, Data));
						}
						else
						{
							WorkingCMD.IndicesList.Add(Utils.ReadInt(false, curCmdOS + WorkingCMD.IndicesListPointer + 2 * k, Data));
						}

					}

					//cmdHeader finished, push to list and start the next one...
					WorkingAnimation.CMDTracks.Add(WorkingCMD);
				}

				//Animation finished, push to list and start the next one...
				Animations.Add(WorkingAnimation);
			}

			//All animations pushed to EMA Header, read skeleton...
			if (SkeletonPointer != 0x00)
			{
				Skeleton = new Skeleton(Utils.ChopByteArray(Data, SkeletonPointer, Data.Length - SkeletonPointer));
			}
			else Skeleton = new Skeleton();
		}
        public override void GenerateBytes()
		{
			List<Byte> Data = new List<byte>();

			//EMA + some stuff
			Utils.AddCopiedBytes(Data, 0x00, 0x0C, new byte[] { 0x23, 0x45, 0x4D, 0x41, 0xFE, 0xFF, 0x20, 0x00, 0x01, 0x00, 0x00, 0x00 });

			int SkeletonPointerPosition = Data.Count;   //Store skeleton pointer pos for later updating
			Utils.AddIntAsBytes(Data, SkeletonPointer, true);
			Utils.AddIntAsBytes(Data, AnimationCount, false);
			Utils.AddIntAsBytes(Data, MysteryIntOS12, false); //Always 0x03?
			Utils.AddZeroToLineEnd(Data); //Pad out to O/S 0x20

			List<int> AnimationPointerPositions = new List<int>(); //To store animation pointer pos for later updating
			for (int i = 0; i < AnimationCount; i++)
			{
				AnimationPointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, AnimationPointersList[i], true);
			}

			List<int> AnimationStartOSs = new List<int>();
			List<int> AnimationNamePointerPositions = new List<int>();

			for (int i = 0; i < AnimationCount; i++)
			{
				List<int> CMDTrackPointerPositions = new List<int>();
				AnimationStartOSs.Add(Data.Count);

				Utils.UpdateIntAtPosition(Data, AnimationPointerPositions[i], Data.Count);
				Utils.AddIntAsBytes(Data, Animations[i].Duration, false);
				Utils.AddIntAsBytes(Data, Animations[i].CmdTrackCount, false);
				Utils.AddIntAsBytes(Data, Animations[i].ValueCount, true);
				Utils.AddIntAsBytes(Data, 0x00, true); //Padding zeroes

				AnimationNamePointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, Animations[i].NamePointer, true);
				int ValuePointerPosition = Data.Count;
				Utils.AddIntAsBytes(Data, Animations[i].ValuesListPointer, true);

				for (int j = 0; j < Animations[i].CmdTrackCount; j++)
				{   //Write out the CMD track pointers and store the pos
					CMDTrackPointerPositions.Add(Data.Count);
					Utils.AddIntAsBytes(Data, Animations[i].CmdTrackPointersList[j], true);
				}
				for (int j = 0; j < Animations[i].CmdTrackCount; j++)
				{   //Start writing out CMD tracks, update pos
					int CMDStartOS = Data.Count;
					Utils.UpdateIntAtPosition(Data, CMDTrackPointerPositions[j], Data.Count - AnimationStartOSs[i]);
					Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].BoneID, false); //BoneID
					Data.Add(Convert.ToByte(Animations[i].CMDTracks[j].TransformType)); //Transform type
					Data.Add(Convert.ToByte(Animations[i].CMDTracks[j].BitFlag));       //Bitflag
					Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].StepCount, false);
					int IndicesPointerPosition = Data.Count; //Store position of indices pointer
					Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].IndicesListPointer, false);

					if ((Animations[i].CMDTracks[j].BitFlag & 0x20) != 0x20) //If flag not set (Step list is bytes)
					{
						for (int k = 0; k < Animations[i].CMDTracks[j].StepsList.Count; k++)
						{
							Data.Add(Convert.ToByte(Animations[i].CMDTracks[j].StepsList[k]));
						}
					}
					else //If flag is set, step list is shorts
					{
						for (int k = 0; k < Animations[i].CMDTracks[j].StepsList.Count; k++)
						{
							Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].StepsList[k], false);
						}
					}
					while (CMDStartOS + Animations[i].CMDTracks[j].IndicesListPointer > Data.Count)
					{
						Data.Add(0x00);
					}

					if ((Data.Count - CMDStartOS) % 2 != 0)
					{
						Data.Add(0x00);
					}
					Utils.UpdateShortAtPosition(Data, IndicesPointerPosition, Data.Count - CMDStartOS);
					if ((Animations[i].CMDTracks[j].BitFlag & 0x40) != 0x40) //If flag not set (Index list is short)
					{
						for (int k = 0; k < Animations[i].CMDTracks[j].IndicesList.Count; k++)
						{
							Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].IndicesList[k], false);
						}
					}
					else //If flag is set, index list is longs
					{
						for (int k = 0; k < Animations[i].CMDTracks[j].IndicesList.Count; k++)
						{
							Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].IndicesList[k], true);
						}
						Utils.AddIntAsBytes(Data, 0x00, false); //??Not sure if we need this, but the pointers will take care of it
					}

					if (j < Animations[i].CMDTracks.Count - 1)
					{   //If there's still another track left, pad out to the track pointer
						while (AnimationStartOSs[i] + Animations[i].CmdTrackPointersList[j + 1] > Data.Count)
						{
							Data.Add(0x00);
						}
					}
					else //Else pad out to the start of the value list
					{
						while (AnimationStartOSs[i] + Animations[i].ValuesListPointer > Data.Count)
						{
							Data.Add(0x00);
						}
					}
				}
				//Value list...
				Utils.UpdateIntAtPosition(Data, ValuePointerPosition, Data.Count - AnimationStartOSs[i]);
				for (int j = 0; j < Animations[i].ValuesList.Count; j++)
				{
					Utils.AddFloatAsBytes(Data, Animations[i].ValuesList[j]);
				}
			}

			Utils.AddZeroToLineEnd(Data);
			//Data.Add(0x00);
			//Utils.AddZeroToLineEnd(Data);

			//Start skeleton header
			if (Skeleton.Nodes.Count > 0) //NEED TO CHECK FOR SKELETON PRESENCE?
			{
				Utils.UpdateIntAtPosition(Data, SkeletonPointerPosition, Data.Count);
				SkeletonPointer = Data.Count;
				Skeleton.GenerateBytes();
				Data.AddRange(Skeleton.HEXBytes);
			}

			//The padding and pointers for the name list are weird, so name 0 is handled on its own, then the rest are handled in a loop.
			//I can't work out how it "really" works but this seems to do the trick.
			Utils.UpdateIntAtPosition(Data, AnimationNamePointerPositions[0], Data.Count - (AnimationStartOSs[0]));

			Utils.AddIntAsBytes(Data, 0x00, true);
			Utils.AddIntAsBytes(Data, 0x00, true);
			Utils.AddIntAsBytes(Data, 0x00, false);

			Data.Add(Convert.ToByte(Animations[0].Name.Length));
			Utils.AddCopiedBytes(Data, 0x00, Animations[0].Name.Length, Animations[0].Name);

			for (int i = 1; i < Animations.Count; i++)
			{
				Utils.AddIntAsBytes(Data, 0x00, true);
				Utils.UpdateIntAtPosition(Data, AnimationNamePointerPositions[i], Data.Count - (AnimationStartOSs[i]));
				Utils.AddIntAsBytes(Data, 0x00, true);
				Utils.AddIntAsBytes(Data, 0x00, true);
				Utils.AddIntAsBytes(Data, 0x00, false);

				Data.Add(Convert.ToByte(Animations[i].Name.Length));
				Utils.AddCopiedBytes(Data, 0x00, Animations[i].Name.Length, Animations[i].Name);
			}

			Data.Add(0x00);

			HEXBytes = Data.ToArray();
		}
	}

	public struct Animation
	{
		public byte[] Name;
		public byte[] HEXBytes;
		public int Duration;
		public int CmdTrackCount;
		public int ValueCount;
		public int NamePointer;
		public int ValuesListPointer;
		public List<int> CmdTrackPointersList;
		public List<CMDTrack> CMDTracks;
		public List<float> ValuesList;
	}

	public struct CMDTrack
	{
		public byte[] HEXBytes;
		public int BoneID;
		public byte TransformType;
		public byte BitFlag;
		public int StepCount;
		public int IndicesListPointer;
		public List<int> StepsList;
		public List<int> IndicesList;
	}

	/// <summary>EMM Material</summary>
	public struct Material
	{
		public byte[] HEXBytes;
		/// <summary>Must be Length: 0x20</summary>
		public byte[] Name;
		///<summary>Must be Length: 0x20</summary>
		public byte[] Shader;
		///<summary>Counts from 1.</summary>
		public int PropertyCount;
		///<summary>Must be Length 0x20 bytes. </summary>
		public List<byte[]> PropertyNamesList;
		///<summary>Must be Length 0x08 bytes. </summary>
		public List<byte[]> PropertyValuesList;

		public Material(byte[] Data)
		{
			HEXBytes = Data;
			Name = Utils.ReadStringToArray(0, 0x20, Data, Data.Length);
			Shader = Utils.ReadStringToArray(0x20, 0x20, Data, Data.Length);
			string shaderName = Encoding.ASCII.GetString(Shader);
			if (!Utils.Shaders.ContainsKey(shaderName)) Utils.Shaders.Add(shaderName, Utils.Shaders.Count);
			PropertyCount = Utils.ReadInt(true, 0x40, Data);
			PropertyValuesList = new List<byte[]>();
			PropertyNamesList = new List<byte[]>();
			for (int i = 0; i < PropertyCount; i++)
			{
				PropertyNamesList.Add(Utils.ReadStringToArray(0x44 + i * 0x28, 0x20, Data, Data.Length));
				PropertyValuesList.Add(Utils.ReadStringToArray(0x64 + i * 0x28, 0x08, Data, Data.Length));

				string propertyName = Encoding.ASCII.GetString(PropertyNamesList[i]).Replace("\0", "");
				string propertyValue = Utils.HexStr2(PropertyValuesList[i], 0x08);

				if (!Utils.ShadersProperties.ContainsKey(propertyName))
				{
					Utils.ShadersProperties.Add(propertyName, $"{propertyName} {propertyValue}");
				}
			}
		}
		public Material(byte[] Data, byte[] name)
		{
			HEXBytes = Data;
			Name = name;
			Shader = Utils.ReadStringToArray(0x20, 0x20, Data, Data.Length);
			string shaderName = Encoding.ASCII.GetString(Shader);
			if (!Utils.Shaders.ContainsKey(shaderName)) Utils.Shaders.Add(shaderName, Utils.Shaders.Count);
			PropertyCount = Utils.ReadInt(true, 0x40, Data);
			PropertyValuesList = new List<byte[]>();
			PropertyNamesList = new List<byte[]>();
			for (int i = 0; i < PropertyCount; i++)
			{
				PropertyNamesList.Add(Utils.ReadStringToArray(0x44 + i * 0x28, 0x20, Data, Data.Length));
				PropertyValuesList.Add(Utils.ReadStringToArray(0x64 + i * 0x28, 0x08, Data, Data.Length));

				string propertyName = Encoding.ASCII.GetString(PropertyNamesList[i]).Replace("\0", "");
				string propertyValue = Utils.HexStr2(PropertyValuesList[i], 0x08);

				if (!Utils.ShadersProperties.ContainsKey(propertyName))
				{
					Utils.ShadersProperties.Add(propertyName, $"{propertyName} {propertyValue}");
				}
			}
		}
		public void GenerateBytes()
		{
			List<byte> Data = new List<byte>();
			Utils.AddCopiedBytes(Data, 0, 0x20, Name);
			Utils.AddCopiedBytes(Data, 0, 0x20, Shader);
			Utils.AddIntAsBytes(Data, PropertyCount, true);

			for (int j = 0; j < PropertyCount; j++)
			{
				Utils.AddCopiedBytes(Data, 0, 0x20, PropertyNamesList[j]);
				Utils.AddCopiedBytes(Data, 0x00, 0x08, PropertyValuesList[j]);
			}
			HEXBytes = Data.ToArray();
		}
	}

	public class EMO : USF4File //Header
	{
		public int SkeletonPointer;
		public int EMGCount;
		public int NumberEMMMaterials; //???
		public int NamingListPointer;
		public List<int> EMGPointersList;
		public List<EMG> EMGs;
		public int temp_bitdepth;
		public List<int> NamingPointersList;
		public List<byte[]> NamesList;

		public Skeleton Skeleton;

		public EMO()
        {

        }

		public EMO(byte[] Data, byte[] name)
		{
			temp_bitdepth = 0;
			Name = name;
			HEXBytes = Data;
			SkeletonPointer = Utils.ReadInt(true, 0x10, Data);
			EMGCount = Utils.ReadInt(false, 0x20, Data);
			NumberEMMMaterials = Utils.ReadInt(false, 0x22, Data);
			NamingListPointer = Utils.ReadInt(true, 0x24, Data);
			EMGPointersList = new List<int>();
			NamingPointersList = new List<int>();
			NamesList = new List<byte[]>();
			EMGs = new List<EMG>();


			Skeleton.HEXBytes = Utils.ChopByteArray(HEXBytes, SkeletonPointer, HEXBytes.Length - SkeletonPointer);
			Skeleton = new Skeleton(Skeleton.HEXBytes);

			for (int i = 0; i < EMGCount; i++)
			{
				EMGPointersList.Add(Utils.ReadInt(true, 0x28 + (i * 4), HEXBytes));
				EMGs.Add(new EMG(Utils.ChopByteArray(HEXBytes, EMGPointersList[i] + 0x30, HEXBytes.Length - (EMGPointersList[i] + 0x30))));
				Console.WriteLine("Got EMG " + i);
			}

			/*Number of names in the index doesn't seem to be stored anywhere in the file.
			 * We take the position of the first name, and subtract the start position of the name pointer list
			 * That's the total length of the pointer list, divided by 4 = number of entries
			 */
			int NameListCount = (Utils.ReadInt(true, NamingListPointer + 0x20, HEXBytes) - NamingListPointer) / 4;

			for (int i = 0; i < NameListCount; i++)
			{
				NamingPointersList.Add(Utils.ReadInt(true, NamingListPointer + 0x20 + i * 4, HEXBytes));
				NamesList.Add(Utils.ReadZeroTermStringToArray(NamingPointersList[i] + 0x20, HEXBytes, HEXBytes.Length));
			}
		}
        public override void ReadFile(byte[] Data)
        {
			temp_bitdepth = 0;
			HEXBytes = Data;
			SkeletonPointer = Utils.ReadInt(true, 0x10, Data);
			EMGCount = Utils.ReadInt(false, 0x20, Data);
			NumberEMMMaterials = Utils.ReadInt(false, 0x22, Data);
			NamingListPointer = Utils.ReadInt(true, 0x24, Data);
			EMGPointersList = new List<int>();
			NamingPointersList = new List<int>();
			NamesList = new List<byte[]>();
			EMGs = new List<EMG>();


			Skeleton.HEXBytes = Utils.ChopByteArray(HEXBytes, SkeletonPointer, HEXBytes.Length - SkeletonPointer);
			Skeleton = new Skeleton(Skeleton.HEXBytes);

			for (int i = 0; i < EMGCount; i++)
			{
				EMGPointersList.Add(Utils.ReadInt(true, 0x28 + (i * 4), HEXBytes));
				EMGs.Add(new EMG(Utils.ChopByteArray(HEXBytes, EMGPointersList[i] + 0x30, HEXBytes.Length - (EMGPointersList[i] + 0x30))));
				Console.WriteLine("Got EMG " + i);
			}

			/*Number of names in the index doesn't seem to be stored anywhere in the file.
			 * We take the position of the first name, and subtract the start position of the name pointer list
			 * That's the total length of the pointer list, divided by 4 = number of entries
			 */
			int NameListCount = (Utils.ReadInt(true, NamingListPointer + 0x20, HEXBytes) - NamingListPointer) / 4;

			for (int i = 0; i < NameListCount; i++)
			{
				NamingPointersList.Add(Utils.ReadInt(true, NamingListPointer + 0x20 + i * 4, HEXBytes));
				NamesList.Add(Utils.ReadZeroTermStringToArray(NamingPointersList[i] + 0x20, HEXBytes, HEXBytes.Length));
			}
		}
        public override void GenerateBytes()
        {
			List<byte> Data = new List<byte>();
			List<int> EMGIndexPositions = new List<int>();
			List<int> NamesIndexPositions = new List<int>();
			//If there's no available HEXBytes, use default values, else copy
			if (HEXBytes == null || HEXBytes.Length < 0x10)
			{
				Data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x4F, 0xFE, 0xFF, 0x20, 0x00, 0x02, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00 });
			}
			else Utils.AddCopiedBytes(Data, 0x00, 0x10, HEXBytes);

			int SkeletonPosition = Data.Count;
			Utils.AddIntAsBytes(Data, SkeletonPointer, true);
			Utils.AddPaddingZeros(Data, 0x20, Data.Count);
			Utils.AddIntAsBytes(Data, EMGs.Count, false);
			Utils.AddIntAsBytes(Data, NumberEMMMaterials, false);
			int NamingListPositionInt = Data.Count;
			Utils.AddIntAsBytes(Data, NamingListPointer, true);

			for (int i = 0; i < EMGPointersList.Count; i++)
			{
				EMGIndexPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, EMGPointersList[i], true);
			}
			Utils.AddZeroToLineEnd(Data);

			//Write out EMGs and Update EMG pointers
			for (int i = 0; i < EMGs.Count; i++)
			{
				Utils.AddZeroToLineEnd(Data);
				Utils.AddCopiedBytes(Data, 0x00, 0x10, new byte[0x10] { 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

				Utils.UpdateIntAtPosition(Data, EMGIndexPositions[i], Data.Count - 0x30);
				EMGPointersList[i] = Data.Count - 0x31;
				Utils.AddCopiedBytes(Data, 0x00, EMGs[i].HEXBytes.Length, EMGs[i].HEXBytes);
			}

			Utils.AddZeroToLineEnd(Data);

			Utils.UpdateIntAtPosition(Data, NamingListPositionInt, Data.Count - 0x20, out NamingListPointer);

			for (int i = 0; i < NamesList.Count; i++)
			{
				NamesIndexPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, NamingPointersList[i], true);
			}

			for (int i = 0; i < NamesList.Count; i++)
			{
				Utils.UpdateIntAtPosition(Data, NamesIndexPositions[i], Data.Count - 0x20);
				NamingPointersList[i] = Data.Count - 0x20;
				Utils.AddCopiedBytes(Data, 0x00, NamesList[i].Length, NamesList[i]);
				Data.Add(0x00);
			}

			Utils.AddZeroToLineEnd(Data);

			Utils.UpdateIntAtPosition(Data, SkeletonPosition, Data.Count, out SkeletonPointer);
			Utils.AddCopiedBytes(Data, 0x00, Skeleton.HEXBytes.Length, Skeleton.HEXBytes);
			Utils.AddZeroToLineEnd(Data);
			
			HEXBytes = Data.ToArray();
		}
	}

	public struct Skeleton
	{
		public byte[] HEXBytes;
		public int NodeCount;
		public int IKObjectCount;
		public int IKDataCount;
		public int NodeListPointer;
		public int NameIndexPointer;
		public int IKBoneListPointer;
		public int IKObjectNameIndexPointer;
		public int RegisterPointer;
		public int SecondaryMatrixPointer;
		public int IKDataPointer;
		public int MysteryShort; //I've seen 0x00, 0x01, 0x02, 0x03. EMA and EMO don't always match.
		public float MysteryFloat1;	//These two "floats" might be a checksum to ensure the EMO and EMA skeletons match?
		public float MysteryFloat2;

		public List<Node> Nodes;
		public List<byte[]> FFList;
		public List<int> NodeNamePointersList;
		public List<byte[]> NodeNames;
		public List<IKNode> IKNodes;
		public List<int> IKNamePointersList;
		public List<byte[]> IKNodeNames;
		public List<IKDataBlock> IKDataBlocks;

		public Skeleton(byte[] Data)
		{
			HEXBytes = Data;

			Nodes = new List<Node>();
			NodeNamePointersList = new List<int>();
			NodeNames = new List<byte[]>();
			FFList = new List<byte[]>();
			IKNodes = new List<IKNode>();
			IKNamePointersList = new List<int>();
			IKNodeNames = new List<byte[]>();
			IKDataBlocks = new List<IKDataBlock>();

			NodeCount = Utils.ReadInt(false, 0x00, Data);
			IKObjectCount = Utils.ReadInt(false, 0x02, Data);
			IKDataCount = Utils.ReadInt(true, 0x04, Data);
			NodeListPointer = Utils.ReadInt(true, 0x08, Data);
			NameIndexPointer = Utils.ReadInt(true, 0x0C, Data);
			//0x10
			IKBoneListPointer = Utils.ReadInt(true, 0x10, Data);
			IKObjectNameIndexPointer = Utils.ReadInt(true, 0x14, Data);
			RegisterPointer = Utils.ReadInt(true, 0x18, Data);
			SecondaryMatrixPointer = Utils.ReadInt(true, 0x1C, Data);
			//0x20
			IKDataPointer = Utils.ReadInt(true, 0x20, Data);
			//0x30
			MysteryShort = Utils.ReadInt(false, 0x36, Data);       //1 REALLY no idea what these are
			MysteryFloat1 = Utils.ReadFloat(0x38, Data);           //2		Are these some kind of checksum to make sure EMA and EMO skels match?
			MysteryFloat2 = Utils.ReadFloat(0x3C, Data);           //3
			

			for (int i = 0; i < NodeCount; i++)
			{
				NodeNamePointersList.Add(Utils.ReadInt(true, NameIndexPointer + i * 4, Data));
				NodeNames.Add(Utils.ReadZeroTermStringToArray(NodeNamePointersList[i], Data, Data.Length));
				FFList.Add(Utils.ReadStringToArray(RegisterPointer + i * 8, 0x08, Data, Data.Length));

				Node WorkingNode = new Node
				{
					Parent = Utils.ReadSignedShort(NodeListPointer + i * 0x50, Data),
					Child1 = Utils.ReadSignedShort(NodeListPointer + i * 0x50 + 0x02, Data),
					Sibling = Utils.ReadSignedShort(NodeListPointer + i * 0x50 + 0x04, Data),
					Child3 = Utils.ReadSignedShort(NodeListPointer + i * 0x50 + 0x06, Data),
					Child4 = Utils.ReadSignedShort(NodeListPointer + i * 0x50 + 0x08, Data),
					BitFlag = Utils.ReadInt(false, NodeListPointer + i * 0x50 + 0x0A, Data), //Tells if the node is animated, IK'd, ???
					PreMatrixFloat = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x0C, Data) //???
				};

				float m11 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x10, Data);
				float m12 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x14, Data);
				float m13 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x18, Data);
				float m14 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x1C, Data);
				float m21 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x20, Data);
				float m22 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x24, Data);
				float m23 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x28, Data);
				float m24 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x2C, Data);
				float m31 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x30, Data);
				float m32 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x34, Data);
				float m33 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x38, Data);
				float m34 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x3C, Data);
				float m41 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x40, Data);
				float m42 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x44, Data);
				float m43 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x48, Data);
				float m44 = Utils.ReadFloat(NodeListPointer + i * 0x50 + 0x4C, Data);

				WorkingNode.NodeMatrix = new Matrix4x4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);

				if (SecondaryMatrixPointer != 0)
				{
					m11 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x00, Data);
					m12 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x04, Data);
					m13 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x08, Data);
					m14 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x0C, Data);
					m21 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x10, Data);
					m22 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x14, Data);
					m23 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x18, Data);
					m24 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x1C, Data);
					m31 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x20, Data);
					m32 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x24, Data);
					m33 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x28, Data);
					m34 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x2C, Data);
					m41 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x30, Data);
					m42 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x34, Data);
					m43 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x38, Data);
					m44 = Utils.ReadFloat(SecondaryMatrixPointer + i * 0x40 + 0x3C, Data);

					WorkingNode.SecondaryMatrix = new Matrix4x4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
				}

				Nodes.Add(WorkingNode);
			}

			if (IKObjectCount != 0)
			{
				for (int i = 0; i < IKObjectCount; i++)
				{
					IKNode wIK = new IKNode
					{
						BoneList = new List<int>(),
						BoneCount = Utils.ReadInt(true, IKBoneListPointer + i * 0x08, Data),
						BoneListPointer = Utils.ReadInt(true, IKBoneListPointer + i * 0x08 + 0x04, Data)
					};

					for (int j = 0; j < wIK.BoneCount; j++)
					{
						wIK.BoneList.Add(Utils.ReadInt(false, IKBoneListPointer + i * 0x08 + wIK.BoneListPointer + j * 0x02, Data));
					}

					IKNamePointersList.Add(Utils.ReadInt(true, IKObjectNameIndexPointer + i * 0x04, Data));
					IKNodeNames.Add(Utils.ReadZeroTermStringToArray(IKNamePointersList[i], Data, Data.Length));

					IKNodes.Add(wIK);
				}
			}

			if (IKDataCount != 0)
			{
				//There's no pointers to individual IKData blocks, so we initialise the position for the first data block,
				//and add the length of each data block as we read it in.
				int CurrentBlockStartPosition = IKDataPointer;
				for (int i = 0; i < IKDataCount; i++)
				{
					IKDataBlock wIKData = new IKDataBlock
					{
						IKShorts = new List<int>(),
						IKFloats = new List<float>(),

						BitFlag = Utils.ReadInt(false, CurrentBlockStartPosition, Data),
						Length = Utils.ReadInt(false, CurrentBlockStartPosition + 0x02, Data)
					};

					if (wIKData.BitFlag == 0x00)
					{
						for (int j = 0; j < wIKData.Length - 0x04; j += 2)
						{
							wIKData.IKShorts.Add(Utils.ReadInt(false, CurrentBlockStartPosition + j + 0x04, Data));
						}
					}
					else if (wIKData.BitFlag == 0x01)
					{
						for (int j = 0; j < wIKData.Length - 0x10; j += 2) //- 0x10 because first 0x04 bytes are the "header", last 0x0C bytes are the floats
						{
							wIKData.IKShorts.Add(Utils.ReadInt(false, CurrentBlockStartPosition + j + 0x04, Data));
						}
						for (int j = 0; j < 3; j++)
						{
							wIKData.IKFloats.Add(Utils.ReadFloat(CurrentBlockStartPosition + 0x04 + wIKData.IKShorts.Count * 0x02 + j * 0x04, Data));
						}
					}
					else
					{
						Console.WriteLine("UNKNOWN IKFLAG IN SKELETON IK DATA BLOCK " + i);
					}

					IKDataBlocks.Add(wIKData);

					CurrentBlockStartPosition += wIKData.Length;
				}
			}
		}

		public void GenerateBytes()
		{
			List<byte> Data = new List<byte>();
			//0x00
			Utils.AddIntAsBytes(Data, NodeCount, false);
			Utils.AddIntAsBytes(Data, IKObjectCount, false);
			Utils.AddIntAsBytes(Data, IKDataCount, true);
			int NodeListPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, NodeListPointer, true);
			int NameIndexPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, NameIndexPointer, true);
			//0x10
			int IKBoneListPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, IKBoneListPointer, true);
			int IKOBjectNameIndexPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, IKObjectNameIndexPointer, true);
			int RegisterPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, RegisterPointer, true);
			int SecondaryMatrixPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, SecondaryMatrixPointer, true);
			//0x20
			int IKDataPointerPosition = Data.Count;
			Utils.AddIntAsBytes(Data, IKDataPointer, true);
			Utils.AddZeroToLineEnd(Data);
			//0x30
			Utils.AddIntAsBytes(Data, 0x00, true); //Padding
			Utils.AddIntAsBytes(Data, 0x00, false); //Padding
			Utils.AddIntAsBytes(Data, MysteryShort, false);
			Utils.AddFloatAsBytes(Data, MysteryFloat1);
			Utils.AddFloatAsBytes(Data, MysteryFloat2);
			//0x40 - Node relationships and main matrices
			Utils.UpdateIntAtPosition(Data, NodeListPointerPosition, Data.Count);
			for (int i = 0; i < NodeCount; i++)
			{
				Utils.AddSignedShortAsBytes(Data, Nodes[i].Parent);
				Utils.AddSignedShortAsBytes(Data, Nodes[i].Child1);
				Utils.AddSignedShortAsBytes(Data, Nodes[i].Sibling);
				Utils.AddSignedShortAsBytes(Data, Nodes[i].Child3);
				Utils.AddSignedShortAsBytes(Data, Nodes[i].Child4);
				Utils.AddSignedShortAsBytes(Data, Nodes[i].BitFlag);
				Utils.AddFloatAsBytes(Data, Nodes[i].PreMatrixFloat);

				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M11);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M12);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M13);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M14);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M21);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M22);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M23);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M24);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M31);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M32);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M33);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M34);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M41);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M42);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M43);
				Utils.AddFloatAsBytes(Data, Nodes[i].NodeMatrix.M44);
			}
			//FF Register
			Utils.UpdateIntAtPosition(Data, RegisterPointerPosition, Data.Count);
			for (int i = 0; i < FFList.Count; i++)
			{
				Utils.AddCopiedBytes(Data, 0x00, FFList[i].Length, FFList[i]);
			}
			//Node Name Index
			Utils.UpdateIntAtPosition(Data, NameIndexPointerPosition, Data.Count);
			List<int> NodeNameIndexPointerPositions = new List<int>();
			for (int i = 0; i < NodeNames.Count; i++)
			{
				NodeNameIndexPointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, NodeNamePointersList[i], true);
			}
			for (int i = 0; i < NodeNames.Count; i++)
			{
				Utils.UpdateIntAtPosition(Data, NodeNameIndexPointerPositions[i], Data.Count);
				Utils.AddCopiedBytes(Data, 0x00, NodeNames[i].Length, NodeNames[i]);
				Data.Add(0x00);
			}
			//Utils.AddZeroToLineEnd(Data);

			//Secondary Matrix List TODO Check the secondary matrix position - not sure where it appears
			//when there's both secondary matrices AND IK data
			if (SecondaryMatrixPointer != 0)
			{

				Utils.UpdateIntAtPosition(Data, SecondaryMatrixPointerPosition, Data.Count);
				for (int i = 0; i < NodeCount; i++)
				{
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M11);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M12);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M13);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M14);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M21);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M22);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M23);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M24);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M31);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M32);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M33);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M34);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M41);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M42);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M43);
					Utils.AddFloatAsBytes(Data, Nodes[i].SecondaryMatrix.M44);
				}
			}

			if (IKDataCount != 0)
			{
				//IKData Blocks
				Utils.UpdateIntAtPosition(Data, IKDataPointerPosition, Data.Count);
				for (int i = 0; i < IKDataCount; i++)
				{
					Utils.AddIntAsBytes(Data, IKDataBlocks[i].BitFlag, false);
					Utils.AddIntAsBytes(Data, IKDataBlocks[i].Length, false);
					for (int j = 0; j < IKDataBlocks[i].IKShorts.Count; j++)
					{
						Utils.AddIntAsBytes(Data, IKDataBlocks[i].IKShorts[j], false);
					}
					for (int j = 0; j < IKDataBlocks[i].IKFloats.Count; j++)
					{
						Utils.AddFloatAsBytes(Data, IKDataBlocks[i].IKFloats[j]);
					}
				}
				//IK Bone Lists Index
				Utils.UpdateIntAtPosition(Data, IKBoneListPointerPosition, Data.Count);
				List<int> IKNodeBoneListPointerPositions = new List<int>();

				for (int i = 0; i < IKNodes.Count; i++)
				{
					Utils.AddIntAsBytes(Data, IKNodes[i].BoneCount, true);
					IKNodeBoneListPointerPositions.Add(Data.Count);
					Utils.AddIntAsBytes(Data, IKNodes[i].BoneListPointer, true);
				}
				//IK Bone Lists
				for (int i = 0; i < IKNodes.Count; i++)
				{
					Utils.UpdateIntAtPosition(Data, IKNodeBoneListPointerPositions[i], Data.Count - IKNodeBoneListPointerPositions[i]);
					for (int j = 0; j < IKNodes[i].BoneList.Count; j++)
					{
						Utils.AddIntAsBytes(Data, IKNodes[i].BoneList[j], false);
					}
				}
				//IKNameIndex
				Utils.UpdateIntAtPosition(Data, IKOBjectNameIndexPointerPosition, Data.Count);
				List<int> IKObjectNamePointers = new List<int>();
				for (int i = 0; i < IKObjectCount; i++)
				{
					IKObjectNamePointers.Add(Data.Count);
					Utils.AddIntAsBytes(Data, IKNamePointersList[i], true);
				}
				//IK Names
				for (int i = 0; i < IKObjectCount; i++)
				{
					Utils.UpdateIntAtPosition(Data, IKObjectNamePointers[i], Data.Count);
					Utils.AddCopiedBytes(Data, 0x00, IKNodeNames[i].Length, IKNodeNames[i]);
					Data.Add(0x00);
				}
			}
			Data.Add(0x00);

			HEXBytes = Data.ToArray();
		}
	}

	public struct Node
	{
		public string Name;
		public int Parent;
		public int Child1;
		public int Sibling; //sibling??
		public int Child3;
		public int Child4;
		public int BitFlag;
		public float PreMatrixFloat;
		public Matrix4x4 NodeMatrix;
		public Matrix4x4 SecondaryMatrix;
		public List<string> child_strings; //Used to rebuild tree relationships from Collada imports
	}

	public struct IKNode
    {
		public List<int> BoneList; //Possibly?
		public int BoneCount;
		public int BoneListPointer;
    }

	public struct IKDataBlock
    {
		public int BitFlag;
		public int Length;
		public List<int> IKShorts;
		public List<float> IKFloats;
    }

	public class EMG : USF4File
	{
		public int RootBone;
		public int ModelCount;
		public List<Model> Models;
		public List<int> ModelPointersList;
		public EMG()
        {

        }
		public EMG(byte[] Data)
		{
			HEXBytes = Data;
			RootBone = Utils.ReadInt(false, 0x04, Data);
			ModelCount = Math.Max(Utils.ReadInt(false, 0x06, Data), 1);     //If model count < 1, set to 1. TODO less hacky fix
			ModelPointersList = new List<int>();
			Models = new List<Model>();
			
			for (int i = 0; i < ModelCount; i++)
			{
				ModelPointersList.Add(Utils.ReadInt(true, 0x08 + i * 4, HEXBytes));
				Models.Add(new Model(Utils.ChopByteArray(HEXBytes, ModelPointersList[i], HEXBytes.Length - ModelPointersList[i])));
			}

			int length;
			length = Models.Last().VertexListPointer + (Models.Last().VertexCount * Models.Last().BitDepth);
			length = length + ModelPointersList.Last();
			HEXBytes = Utils.ChopByteArray(HEXBytes, 0, length);
		}

		public override void GenerateBytes()
		{
			List<byte> Data = new List<byte>();

			//#EMG
			Data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x47 });
			Utils.AddIntAsBytes(Data, RootBone, false);
			Utils.AddIntAsBytes(Data, ModelCount, false);

			List<int> ModelPointerPositions = new List<int>();
			for (int i = 0; i < ModelCount; i++)
			{
				ModelPointerPositions.Add(Data.Count);
				Utils.AddIntAsBytes(Data, ModelPointersList[i], true);
			}

			Utils.AddZeroToLineEnd(Data);

			for (int i = 0; i < ModelCount; i++)
			{
				Utils.UpdateIntAtPosition(Data, ModelPointerPositions[i], Data.Count);
				int ModelStartPosition = Data.Count;

				Utils.AddIntAsBytes(Data, Models[i].BitFlag, true);
				Utils.AddIntAsBytes(Data, Models[i].TextureCount, true);
				Utils.AddIntAsBytes(Data, 0x00, true);  //Padding
				int TextureListPointerPosition = Data.Count;
				Utils.AddIntAsBytes(Data, Models[i].TextureListPointer, true);
				Utils.AddIntAsBytes(Data, Models[i].VertexCount, false);
				Utils.AddIntAsBytes(Data, Models[i].BitDepth, false);
				int VertexListPointerPosition = Data.Count;
				Utils.AddIntAsBytes(Data, Models[i].VertexListPointer, true);
				Utils.AddIntAsBytes(Data, Models[i].ReadMode, false);
				Utils.AddIntAsBytes(Data, Models[i].SubModelsCount, false);
				int SubModelListPointerPosition = Data.Count;
				Utils.AddIntAsBytes(Data, Models[i].SubModelsListPointer, true);
				if (Models[i].CullData == null)
				{
					Data.AddRange(new List<byte> { 0x00, 0x0A, 0x1B, 0x3C, 0xC3, 0xA4, 0x9E, 0x40,
												   0x80, 0x89, 0x27, 0x3E, 0xF6, 0x79, 0x3E, 0x41,
												   0x50, 0x94, 0xA1, 0xC0, 0xFD, 0x14, 0x9D, 0xBF,
												   0xEE, 0x94, 0x0A, 0xC1, 0x43, 0xB9, 0x0D, 0x43,
												   0x5A, 0x2F, 0xA2, 0x40, 0x63, 0x47, 0x32, 0x41,
												   0x3A, 0xD1, 0x0F, 0x41, 0xF6, 0x79, 0xBE, 0x41 });
				}
				else Utils.AddCopiedBytes(Data, 0x00, 0x30, Models[i].CullData);

				Utils.UpdateIntAtPosition(Data, TextureListPointerPosition, Data.Count - ModelStartPosition);
				List<int> TexturePointerPositions = new List<int>();
				for (int j = 0; j < Models[i].TextureCount; j++)
				{
					TexturePointerPositions.Add(Data.Count);
					Utils.AddIntAsBytes(Data, Models[i].TexturePointersList[j], true);
				}
				for (int j = 0; j < Models[i].TextureCount; j++)
				{
					Utils.UpdateIntAtPosition(Data, TexturePointerPositions[j], Data.Count - ModelStartPosition);
					Utils.AddIntAsBytes(Data, Models[i].Textures[j].TextureLayers, true);
					for (int k = 0; k < Models[i].Textures[j].TextureLayers; k++)
					{
						Data.Add(0x00);
						Utils.AddIntAsBytes(Data, Models[i].Textures[j].TextureIndicesList[k], false);
						Data.Add(0x22);
						Utils.AddFloatAsBytes(Data, Models[i].Textures[j].Scales_UList[k]);
						Utils.AddFloatAsBytes(Data, Models[i].Textures[j].Scales_VList[k]);
					}
				}

				Utils.UpdateIntAtPosition(Data, SubModelListPointerPosition, Data.Count - ModelStartPosition);
				List<int> SubmodelPointerPositions = new List<int>();
				for (int j = 0; j < Models[i].SubModelsCount; j++)
				{
					SubmodelPointerPositions.Add(Data.Count);
					Utils.AddIntAsBytes(Data, Models[i].SubModelPointersList[j], true);
				}

				for (int j = 0; j < Models[i].SubModels.Count; j++)
				{
					Utils.AddZeroToLineEnd(Data);

					Utils.UpdateIntAtPosition(Data, SubmodelPointerPositions[j], Data.Count - ModelStartPosition);
					Utils.AddCopiedBytes(Data, 0x00, Models[i].SubModels[j].MysteryFloats.Length, Models[i].SubModels[j].MysteryFloats);
					Utils.AddIntAsBytes(Data, Models[i].SubModels[j].MaterialIndex, false);
					Utils.AddIntAsBytes(Data, Models[i].SubModels[j].DaisyChainLength, false);
					Utils.AddIntAsBytes(Data, Models[i].SubModels[j].BoneIntegersCount, false);
					Utils.AddCopiedBytes(Data, 0, 0x20, Models[i].SubModels[j].SubModelName);
					for (int k = 0; k < Models[i].SubModels[j].DaisyChain.Length; k++)
					{
						Utils.AddIntAsBytes(Data, Models[i].SubModels[j].DaisyChain[k], false);
					}
					if (Models[i].SubModels[j].BoneIntegersList != null)
					{
						for (int k = 0; k < Models[i].SubModels[j].BoneIntegersList.Count; k++)
						{
							Utils.AddIntAsBytes(Data, Models[i].SubModels[j].BoneIntegersList[k], false);
						}
					}
				}

				Utils.UpdateIntAtPosition(Data, VertexListPointerPosition, Data.Count - 0x10);

				for (int j = 0; j < Models[i].VertexCount; j++)
				{
					//Vertex Data - really hoping this is always true
					if ((Models[i].BitFlag & 0x01) == 0x01)
					{
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].X);
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].Y);
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].Z);
					}
					else
					{
						Console.WriteLine("Uh oh, vert flag down");
					}

					//Normals, not implemented in OBJ encoder
					if ((Models[i].BitFlag & 0x02) == 0x02)
					{
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].nX);
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].nY);
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].nZ);
					}

					//UV Co-ordinates
					if ((Models[i].BitFlag & 0x04) == 0x04)
					{
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].U);
						Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].V);
					}

					//TODO ??? Three floats. AE says something about Tex2?
					//2nd texture layer, UV mapping plus blend ratio?
					//But seems to be a lot of negative values where the UV floats should be, which isn't impossible but is weird
					if ((Models[i].BitFlag & 0x80) == 0x80)
					{
						//Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].???);
						//Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].???);
						//Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].???);
						Utils.AddFloatAsBytes(Data, 0f);
						Utils.AddFloatAsBytes(Data, 0f);
						Utils.AddFloatAsBytes(Data, 0.5f);
					}

					//UV Colour. Default to white TODO modify to accept user defined colour
					if ((Models[i].BitFlag & 0x40) == 0x40)
					{
						Data.Add(0xFF); Data.Add(0xFF); Data.Add(0xFF); Data.Add(0xFF);
					}

					//TODO Bone weights
					//4 chars listing the influencing bones from the sub-model's Bone Integer List
					//3 Floats listing the weightings. If the 3 floats don't add up to 1, the remainder is applied to bone 4?
					if ((Models[i].BitFlag & 0x0200) == 0x0200)
					{
						for (int k = 0; k < 4; k++)
						{
							if (Models[i].VertexData[j].BoneIDs.Count > k)
							{
								Data.Add(Convert.ToByte(Models[i].VertexData[j].BoneIDs[k]));
							}
							else Data.Add(0x00);
						}
						for (int k = 0; k < 3; k++)
						{
							if (Models[i].VertexData[j].BoneWeights.Count > k)
							{
								Utils.AddFloatAsBytes(Data, Models[i].VertexData[j].BoneWeights[k]);
							}
							else Utils.AddFloatAsBytes(Data, 0f);
						}
					}
				}
			}
			HEXBytes = Data.ToArray();
		}
	}

	public struct Model
	{
		public byte[] HEXBytes;
		public int BitFlag;
		public int TextureCount;
		public int TextureListPointer;          //Look for index
		public int VertexCount;
		public int BitDepth;
		public int VertexListPointer;               //Start reading Vertex data from here
		public List<Vertex> VertexData;
		public int ReadMode;                 // 1 = Plain triangles, 0 = stripped
		public int SubModelsCount;
		public int SubModelsListPointer;      //Points to list of pointers to each sub model.
		public List<int> SubModelPointersList;      //We only need offset
		public List<SubModel> SubModels;      //The actual sub model struct
		public List<int> TexturePointersList;    //Points to each individual texture block in the texture header
		public List<EMGTexture> Textures;
		public byte[] CullData;

		public Model(byte[] Data)
		{
			BitFlag = Utils.ReadInt(true, 0x00, Data);
			TextureCount = Utils.ReadInt(true, 0x04, Data);
			TextureListPointer = Utils.ReadInt(true, 0x0C, Data);
			VertexCount = Utils.ReadInt(false, 0x10, Data);
			BitDepth = Utils.ReadInt(false, 0x12, Data);
			VertexListPointer = Utils.ReadInt(true, 0x14, Data);
			ReadMode = Utils.ReadInt(false, 0x18, Data);
			CullData = Utils.ChopByteArray(Data, 0x20, 0x30);
			TexturePointersList = new List<int>();
			Textures = new List<EMGTexture>();

			if (ReadMode == 0)
			{
				Console.WriteLine("MODEL READ MODE ZERO!");
			}
			//CullData = new byte[] { 0x00, 0x0A, 0x1B, 0x3C, 0xC3, 0xA4, 0x9E, 0x40, //Generic cull data with broad display
			//							0x80, 0x89, 0x27, 0x3E, 0xF6, 0x79, 0x3E, 0x41,
			//							0x50, 0x94, 0xA1, 0xC0, 0xFD, 0x14, 0x9D, 0xBF,
			//							0xEE, 0x94, 0x0A, 0xC1, 0x43, 0xB9, 0x0D, 0x43,
			//							0x5A, 0x2F, 0xA2, 0x40, 0x63, 0x47, 0x32, 0x41,
			//							0x3A, 0xD1, 0x0F, 0x41, 0xF6, 0x79, 0xBE, 0x41 },

			for (int i = 0; i < TextureCount; i++)
			{
				TexturePointersList.Add(Utils.ReadInt(true, TextureListPointer + i * 4, Data));
				EMGTexture newEMGTexture = new EMGTexture
				{
					TextureLayers = Utils.ReadInt(true, TexturePointersList[i], Data),
					TextureIndicesList = new List<int>(),
					Scales_UList = new List<float>(),
					Scales_VList = new List<float>()
				};

				int TextureLength = 0x0C;
				for (int j = 0; j < newEMGTexture.TextureLayers; j++)
				{
					newEMGTexture.TextureIndicesList.Add(Utils.ReadInt(false, TexturePointersList[i] + 0x05 + (j * TextureLength), Data));
					newEMGTexture.Scales_UList.Add(Utils.ReadFloat(TexturePointersList[i] + 0x08 + (j * TextureLength), Data));
					newEMGTexture.Scales_VList.Add(Utils.ReadFloat(TexturePointersList[i] + 0x0C + (j * TextureLength), Data));
				}
				Textures.Add(newEMGTexture);
			}

			SubModelsCount = Utils.ReadInt(false, 0x1A, Data);
			SubModelsListPointer = Utils.ReadInt(true, 0x1C, Data);
			SubModelPointersList = new List<int>();
			SubModels = new List<SubModel>();

			for (int i = 0; i < SubModelsCount; i++)
			{
				SubModelPointersList.Add(Utils.ReadInt(true, SubModelsListPointer + i * 4, Data));
				SubModels.Add(new SubModel(Utils.ChopByteArray(Data, SubModelPointersList[i], Data.Length - SubModelPointersList[i])));
				Console.WriteLine("Got SubModel " + i);
			}

			VertexData = new List<Vertex>();
			for (int i = 0; i < VertexCount; i++)
			{
				int ReadPosition = 0;
				//After a particular data type is read form the vertex block, we advance the position by the correct length
				//This way a single function can read all vert data blocks. Relative order of all data types is fixed.

				Vertex v = new Vertex();

				//Vertex Data - really hoping this is always true
				if ((BitFlag & 0x01) == 0x01)
				{
					v.X = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x00, Data);
					v.Y = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x04, Data);
					v.Z = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x08, Data);
					//Move the read head
					ReadPosition += 0x0C;
				}
				else
				{
					Console.WriteLine("Uh oh, vert flag down");
				}

				//Normals
				if ((BitFlag & 0x02) == 0x02)
				{
					v.nX = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x00, Data);
					v.nY = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x04, Data);
					v.nZ = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x08, Data);

					ReadPosition += 0x0C;
				}

				//UV Co-ordinates
				if ((BitFlag & 0x04) == 0x04)
				{
					v.U = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x00, Data);
					v.V = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x04, Data);

					ReadPosition += 0x08;
				}

				if ((BitFlag & 0x80) == 0x80)
				{
					v.U2 = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x00, Data);
					v.V2 = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x04, Data);
					v.blend = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x08, Data);

					ReadPosition += 0x0C;
				}

				//UV Colour.
				if ((BitFlag & 0x40) == 0x40)
				{
					v.colour = Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x00, Data);

					ReadPosition += 0x04;
				}

				//Bone weighting
				if ((BitFlag & 0x0200) == 0x0200)
				{
					v.BoneIDs = new List<int>();
					v.BoneWeights = new List<float>();
					v.BoneIDs.Add(Data[VertexListPointer + i * BitDepth + ReadPosition + 0x00]);
					v.BoneIDs.Add(Data[VertexListPointer + i * BitDepth + ReadPosition + 0x01]);
					v.BoneIDs.Add(Data[VertexListPointer + i * BitDepth + ReadPosition + 0x02]);
					v.BoneIDs.Add(Data[VertexListPointer + i * BitDepth + ReadPosition + 0x03]);

					v.BoneWeights.Add(Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x04, Data));
					v.BoneWeights.Add(Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x08, Data));
					v.BoneWeights.Add(Utils.ReadFloat(VertexListPointer + i * BitDepth + ReadPosition + 0x0C, Data));
				}

				VertexData.Add(v);
			}

			HEXBytes = Utils.ChopByteArray(Data, 0x00, VertexListPointer + VertexCount * BitDepth);
		}
	}

	public struct EMGTexture
	{
		public int TextureLayers;
		public List<int> TextureIndicesList;          //The DDS index in the EMB
		public List<float> Scales_UList;
		public List<float> Scales_VList;
	}

	public struct SubModel
	{
		public int MaterialIndex;					//Index from the EMG texture list
		public int DaisyChainLength;
		public int[] DaisyChain;
		public byte[] HEXBytes;
		public int BoneIntegersCount;
		public byte[] SubModelName;             //Need to be 32 (0x20) characters in length
		public List<int> BoneIntegersList;
		//public Dictionary<int, int> BoneIntDict;
		public byte[] MysteryFloats;

		public SubModel(byte[] Data)
		{
			MysteryFloats = Utils.ChopByteArray(Data, 0x00, 0x10); //Read in "mystery float" bytes. Not storing them as floats 'cos we don't know what they do and they might not be floats at all
			BoneIntegersList = new List<int>();
			MaterialIndex = Utils.ReadInt(false, 0x10, Data);
			DaisyChainLength = Utils.ReadInt(false, 0x12, Data);
			BoneIntegersCount = Utils.ReadInt(false, 0x14, Data);
			SubModelName = Utils.ReadStringToArray(0x16, 0x20, Data, Data.Length);
			
			DaisyChain = new int[DaisyChainLength];
			for (int i = 0; i < DaisyChainLength; i++)
			{
				DaisyChain[i] = Utils.ReadInt(false, 0x36 + i * 2, Data);
			}

			for (int i = 0; i < BoneIntegersCount; i++)
			{
				BoneIntegersList.Add(Utils.ReadInt(false, 0x36 + (DaisyChainLength + i) * 2, Data));
			}

			HEXBytes = Utils.ChopByteArray(Data, 0x00, 0x36 + (DaisyChainLength + BoneIntegersCount) * 2);
		}
	}

	public struct ObjFile	//Top level file structure for OBJ
    {
		public List<ObjObject> ObjObjects;
		public List<Vertex> Verts;
		public List<UVMap> Textures;
		public List<Normal> Normals;
	}

	public struct ObjObject   //"o " grouping => .emg
	{
		public List<ObjGroup> ObjGroups;
		public string Name;
	}

	public struct ObjGroup   //"g " grouping => model
	{
		public List<ObjMaterial> ObjMaterials;
		public List<Vertex> UniqueVerts;
		public string Name;
	}

	public struct ObjMaterial   //"usemtl " grouping => submodel
	{
		public string Name;
		public int lastV; 
		public int lastT;
		public int lastN;
		public List<string> lines;
		public List<int[]> FaceIndices;
		public List<int> DaisyChain;
	}

	public struct ObjModel   //The representation of the Wavefront .OBJ file.
	{
		public List<Vertex> Verts;
		public List<UVMap> Textures;
		public List<Normal> Normals;
		public List<Vertex> UniqueVerts;
		public List<int[]> FaceIndices;
		public List<ObjMatGroup> MaterialGroups;
	}

	public struct ObjMatGroup
	{
		public int endvert;
		public List<string> lines;
		public List<int[]> FaceIndices;
		public List<int> DaisyChain;
	}

	public struct SMDModel
	{
		public byte[] HEXBytes;
		public byte[] Name;
		public List<Vertex> Verts;
		public List<SMDNode> Nodes;
		public List<SMDFrame> Frames;
		public List<int[]> FaceIndices;
		public List<byte[]> MaterialNames;
		public bool bRefPose;
		public Dictionary<string, int> MaterialDictionary;
	}

	public struct SMDNode
	{
		public int ID;
		public byte[] Name;
		public int Parent;
	}

	public struct SMDFrame
	{
		public int Time;
		public List<int> NodeIDs;
		public List<float> traX;
		public List<float> traY;
		public List<float> traZ;
		public List<float> rotX;
		public List<float> rotY;
		public List<float> rotZ;
	}

	public struct Vertex   //Your classic everyday point in 3D space.
	{
		public float X;
		public float Y;
		public float Z;

		public float U;
		public float V;

		public float U2;
		public float V2;
		public float blend;

		public float nX;
		public float nY;
		public float nZ;

		public float colour;

		public int ParentBone;
		public int BoneCount;
		public List<int> BoneIDs;
		public List<float> BoneWeights;

		public bool VertexEquals(object obj)
		{
			//Check for null and compare run-time types.
			if ((obj == null) || !GetType().Equals(obj.GetType()))
			{
				return false;
			}
			else
			{
				Vertex v = (Vertex)obj;
				return (X == v.X) && (Y == v.Y) && (Z == v.Z) && (U == v.U) && (V == v.V) && (nX == v.nX) && (nY == v.nY) && (nZ == v.nZ) && (ParentBone == v.ParentBone) && (BoneCount == v.BoneCount);
			}
		}

		public void UpdatePosition(Vertex v)
		{
			X = v.X; Y = v.Y; Z = v.Z;
		}
		public void UpdatePosition(float x, float y, float z)
		{
			X = x; Y = y; Z = z;
		}
		public void UpdateUV(UVMap uv)
		{
			U = uv.U; V = uv.V;
		}
		public void UpdateUV(float u, float v)
		{
			U = u; V = v;
		}
		public void UpdateNormals(Normal n)
		{
			nX = n.nX; nY = n.nY; nZ = n.nZ;
		}
		public void UpdateNormals(float nx, float ny, float nz)
		{
			X = nx; Y = ny; Z = nz;
		}
	}

	public struct UVMap  //UV Texture co-ordinates
	{
		public float U;
		public float V;
	}

	public struct Normal   //Your classic everyday point in 3D space.
	{
		public float nX;
		public float nY;
		public float nZ;
	}

	public struct Weights
    {
		public List<int> BoneID;
		public List<float> BoneWeights;
	}

	#endregion
}
