using System;
using System.Collections.Generic;
using System.Numerics;

namespace USF4_Stage_Tool
{
	#region USF4 Structs

	public struct OtherFile
	{
		public int FilePosition;
		public byte[] HEXBytes;
	}

	public struct EMZ
	{
		public int NumberOfFiles;
		public int FileListPointer;
		public int FileNameListPointer;
		public List<int> FilePointerList;
		public Dictionary<int, object> Files;
		public List<int> FileLengthList;
		public List<int> FileNamePointerList;       //Each entry in this list points to an entry in the EMZ file name list
		public List<byte[]> FileNameList;
		public byte[] HEXBytes;
	}

	public struct EMB
	{
		public int FilePosition;
		public byte[] Name;
		public int NumberOfFiles;
		public int FileListPointer;
		public int FileNameListPointer;
		public List<DDS> DDSFiles;
		public List<int> FilePointerList;
		public List<int> FileLengthList;
		public List<int> FileNamePointerList;
		public List<byte[]> FileNameList;   //No file names normally, but we can test adding names and see if it still loads
		public byte[] HEXBytes;

		public void GenerateBytes()
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
		public byte[] Name;
		public byte[] HEXBytes;
	}

	public struct LUA
	{
		public int FilePosition;
		public byte[] HEXBytes;
		public byte[] Name;
	}

	public struct EMM
	{
		public int FilePosition;
		public byte[] HEXBytes;
		public byte[] Name;
		public int MaterialCount;
		public List<int> MaterialPointerList;
		public List<Material> Materials;
	}

	public struct EME
    {
		public int FilePosition;
		public byte[] HEXBytes;
		public byte[] Name;
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


	public struct EMA
	{
		public int FilePosition;
		public byte[] HEXBytes;
		public byte[] Name;
		public int SkeletonPointer;
		public int AnimationCount;
		public int MysteryIntOS12; //Always seems to be 3?
		public List<int> AnimationPointerList;
		public List<Animation> Animations;

		public Skeleton Skeleton;
	}

	public struct Animation
	{
		public byte[] Name;
		public byte[] HEXBytes;
		public int Duration;
		public int CmdTrackCount;
		public int ValueCount;
		public int NamePointer;
		public int ValueListPointer;
		public List<int> CmdTrackPointerList;
		public List<CMDTrack> CMDTracks;
		public List<float> ValueList;
	}

	public struct CMDTrack
	{
		public byte[] HEXBytes;
		public int BoneID;
		public byte TransformType;
		public byte BitFlag;
		public int StepCount;
		public int IndiceListPointer;
		public List<int> StepList;
		public List<int> IndiceList;
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
		public List<byte[]> PropertyNames;
		///<summary>Must be Length 0x08 bytes. </summary>
		public List<byte[]> PropertyValues;
	}

	public struct EMO //Header
	{
		public int FilePosition;
		public int SkeletonPointer;
		public int EMGCount;
		public byte[] Name;
		public int NumberEMMMaterials; //???
		public int NamingListPointer;
		public List<int> EMGPointerList;
		public List<EMG> EMGList;
		public byte[] HEXBytes;
		public int temp_bitdepth;
		public List<int> NamingPointerList;
		public List<byte[]> NamingList;

		public Skeleton Skeleton;
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
		public List<int> NodeNameIndex;
		public List<byte[]> NodeNames;
		public List<IKNode> IKNodes;
		public List<int> IKNameIndex;
		public List<byte[]> IKNodeNames;
		public List<IKDataBlock> IKDataBlocks;
	}

	public struct Node
	{
		public int Parent;
		public int Child1;
		public int Sibling; //sibling??
		public int Child3;
		public int Child4;
		public int BitFlag;
		public float PreMatrixFloat;
		public Matrix4x4 NodeMatrix;
		public Matrix4x4 SecondaryMatrix;
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

	public struct EMG
	{
		public int RootBone;                            //The position of the EMG inside the EMO
		public int ModelCount;                  //Should it will be visible
		public byte[] HEXBytes;                 //The complete HEX data for the EMG
		public List<Model> Models;
		public List<int> ModelPointerList;
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
		public int ReadMode;                            //Suposed to read face data?? Write the same thing!! ?
		public int SubModelsCount;
		public int SubModeListPointer;      //Points to list of pointers to each sub model.
		public List<int> SubModelList;      //We only need offset
		public List<SubModel> SubModels;      //The actual sub model struct
		public int LengthDifference;
		public List<int> TexturePointer;    //Points to each individual texture block in the texture header
		public List<EMGTexture> TexturesList;
		public byte[] CullData;
	}

	public struct EMGTexture
	{
		public int TextureLayers;
		public List<int> TextureIndex;          //The DDS index in the EMB
		public List<float> Scales_U;
		public List<float> Scales_V;
	}

	public struct SubModel
	{
		public int MaterialIndex;                       //Index from the EMM file
		public int DaisyChainLength;
		public int[] DaisyChain;
		public byte[] HEXBytes;
		public int BoneIntegersCount;                           //Another mystery!!!! ?? 'TODO find out what the heck?
		public byte[] SubModelName;             //Need to be 32 characters in length
		public List<int> BoneIntegersList;
		public byte[] MysteryFloats;
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

	public struct CSB
	{
		public byte[] HEXBytes;
		public byte[] Name;
	}
	#endregion
}
