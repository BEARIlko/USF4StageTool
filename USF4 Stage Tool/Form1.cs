using CSharpImageLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using static KopiLua.Lua;
using static CSharpImageLibrary.ImageFormats;
using System.Globalization;
using Collada141;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Diagnostics;
using grendgine_collada;
using System.Xml.Serialization;

namespace USF4_Stage_Tool
{

	public partial class Form1 : Form
	{
		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		DebugOutput debugOutputForm;
		public string LastOpenFolder = string.Empty;

		//File types filters
		public const string OBJFileFilter = "Wavefront (.obj)|*.obj";
		public const string SMDFileFilter = "StudioMDL (.smd)|*.smd";
		public const string EMZFileFilter = "EMZ (.emz)|*.emz";
		public const string TEXEMZFileFilter = "TEX.EMZ (.tex.emz)|*.tex.emz";
		public const string EMGFileFilter = "EMG (.emg)|*.emg";
		public const string EMOFileFilter = "EMO (.emo)|*.emo";
		public const string EMAFileFilter = "EMO (.ema)|*.ema";
		public const string EMMFileFilter = "EMO (.emm)|*.emm";
		public const string EMBFileFilter = "EMZ (.emb)|*.emb";
		public const string LUAFileFilter = "LUA (.lua)|*.lua";
		public const string TXTFileFilter = "Text file (.txt)|*.txt";
		public const string DDSFileFilter = "Direct Draw Surface (.dds)|*.dds";
		public const string CSBFileFilter = "Sound Bank (.csb)|*.csb";

		public int TotalNewVerts;
		public bool ObjectLoaded;
		public bool EncodingInProgress;
		private const int MaxVertWarning = 100000;
		private const bool WarnForMaxVerts = false;
		private string TargetEMZFilePath;
		private string TargetEMZFileName;
		private string TargetTEXEMZFileName;
		private string TargetTEXEMZFilePath;
		public int SelectedEMZNumberInTree = 0;
		public int SelectedEMONumberInTree = 0;
		public int SelectedEMGNumberInTree = 0;
		public int SelectedModelNumberInTree = 0;
		public int SelectedSubModelNumberInTree = 0;
		TreeNode LastSelectedTreeNode;
		private StringBuilder HeXText;
		ContextMenuStrip CM;
		DateTime StartTime;     //Used for ETA/ETR Calculation
		List<int> VertexDaisyChain = new List<int>();

		EMZ WorkingEMZ;
		EMZ WorkingTEXEMZ;
		ElementHost EH3D = new ElementHost();
		HostingWpfUserControlInWf.UserControl1 uc = new HostingWpfUserControlInWf.UserControl1();
		ObjFile WorkingObjFile;
		ObjModel WorkingObject;
		string WorkingFileName;
		public bool InjectObjAfterOpen;
		public string NameSuffix = "_HeX Encoded.txt";
		public string ConsoleLineSpacer = "__________________________________";
		public InputBox IB;

		public EMB matchingemb = new EMB(); //Used to show DDS names on the model texture edit tooltips

		public Form1()
		{
			InitializeComponent();
			debugOutputForm = new DebugOutput();
			CheckForIllegalCrossThreadCalls = false;
			//Jank Version display
			string[] VersionNumbers = Regex.Split(Application.ProductVersion, "\\.");
			string Major = VersionNumbers[0];
			string Minor = VersionNumbers[1];
			this.Text = this.Text + "  v" + Major + "." + Minor;
			//End of jank!!
		}

		//Open an OBJ 3D file to start workng
		private void BtnOpenOBJ_Click(object sender, EventArgs e)
		{
			BTNOpenOBJ();
		}

		async void BTNOpenOBJ()
		{
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = OBJFileFilter;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				if (filepath.Trim() != string.Empty)
				{
					LastOpenFolder = Path.GetDirectoryName(filepath);
					WorkingFileName = diagOpenOBJ.SafeFileName;

					await Task.Run(() => { ReadOBJ(filepath); });
					if (ValidateOBJ())
					{
						
						lbOBJNameProperty.Text = Path.GetFileName(filepath);
						_ = Task.Delay(10).ContinueWith(t => EncodeTheOBJ());
						
					}
					else
					{
						ClearUpStatus();
						MessageBox.Show("OBJ Encoding canceled!", TStrings.STR_Error);
					}
				}
			}
		}

        void ReadOBJ(string obj)
        {
            ConsoleWrite(ConsoleLineSpacer);
            ConsoleWrite($"Opening OBJ file:  {obj}");
            VertexDaisyChain = new List<int>();
            ConsoleWrite(" ");
            ClearUpStatus();
            string[] lines = File.ReadAllLines(obj);

            Dictionary<UInt64, int> VertUVDictionary = new Dictionary<UInt64, int>();
            Dictionary<UInt64, int> UniqueChunkDictionary = new Dictionary<UInt64, int>(); //For use if we implement vert normal splitting

            //Prepare Input OBJ Structure
            WorkingObject = new ObjModel
            {
                Verts = new List<Vertex>(),
                Textures = new List<UVMap>(),
                Normals = new List<Normal>(),
                FaceIndices = new List<int[]>(),
                UniqueVerts = new List<Vertex>(),
                MaterialGroups = new List<ObjMatGroup>()
            };

            ObjMatGroup WorkingMat = new ObjMatGroup()
            {
                FaceIndices = new List<int[]>(),
                lines = new List<string>(),
                DaisyChain = new List<int>()
            };

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith("v "))
                {
                    float flip = 1f;
                    if (chkGeometryFlipX.Checked) { flip = -1f; }

                    string vertCoords = line.Replace("v ", "").Trim();
                    string[] vertProps = vertCoords.Trim().Split(' ');
                    Vertex vert = new Vertex()
                    {
                        X = flip * float.Parse(Utils.FixFloatingPoint(vertProps[0])),
                        Y = float.Parse(Utils.FixFloatingPoint(vertProps[1])),
                        Z = float.Parse(Utils.FixFloatingPoint(vertProps[2]))
                    };
                    WorkingObject.Verts.Add(vert);
                }
                if (line.StartsWith("vt "))
                {
                    string vertTextures = line.Replace("vt ", "").Trim();
                    vertTextures = Utils.FixFloatingPoint(vertTextures);
                    string[] vertTex = vertTextures.Trim().Split(' ');
                    UVMap tex = new UVMap()
                    {
                        U = float.Parse(vertTex[0]),
                        V = float.Parse(vertTex[1])
                    };

                    WorkingObject.Textures.Add(tex);
                }
                if (line.StartsWith("vn "))
                {   //TODO test if we need to flip normals
                    string normCoords = line.Replace("vn ", "").Trim();
                    string[] normProps = normCoords.Trim().Split(' ');
                    Normal norm = new Normal()
                    {
                        nX = float.Parse(Utils.FixFloatingPoint(normProps[0])),
                        nY = float.Parse(Utils.FixFloatingPoint(normProps[1])),
                        nZ = float.Parse(Utils.FixFloatingPoint(normProps[2]))
                    };
                    WorkingObject.Normals.Add(norm);
                }
                if (line.StartsWith("usemtl") || line.StartsWith("o "))
                {
                    //Starting a new material group, add the old material to the WorkingObj if needed
                    if (WorkingMat.lines.Count > 0)
                    {
                        WorkingMat.endvert = WorkingObject.Verts.Count;
                        WorkingObject.MaterialGroups.Add(WorkingMat);
                    }
                    //Initialise a new MatGroup
                    WorkingMat = new ObjMatGroup
                    {
                        FaceIndices = new List<int[]>(),
                        lines = new List<string>(),
                        DaisyChain = new List<int>()
                    };
                }
                if (line.StartsWith("f "))
                {   //If we find a face line, add it to our current material group
                    WorkingMat.lines.Add(line);
                }
            }
            //Once we reach the end of the file, add the final group to the OBJ
            if (WorkingMat.lines.Count > 0)
            {
                WorkingMat.endvert = WorkingObject.Verts.Count;
                WorkingObject.MaterialGroups.Add(WorkingMat);
            }

            foreach (ObjMatGroup omg in WorkingObject.MaterialGroups)
            {
                for (int i = 0; i < omg.lines.Count; i++)
                {
                    string line = omg.lines[i];
                    int[] tempFaceArray = new int[3];

                    if (!line.Contains("/"))
                    {
                        MessageBox.Show("Invalid Face data format", "Error");
                        return;
                    }
                    string vertFaces = line.Replace("f ", "").Trim(); //Remove leading f
                    string[] arFaces = vertFaces.Trim().Split(' '); //Split into chunks

                    string[] chunk1string;
                    string[] chunk2string;
                    string[] chunk3string;
                    //FACE FLIP HAPPENS HERE NOW
                    if (chkGeometryFlipX.Checked)
                    {
                        chunk1string = arFaces[2].Trim().Split('/');   //Split chunks into index components
                        chunk2string = arFaces[1].Trim().Split('/');
                        chunk3string = arFaces[0].Trim().Split('/');
                    }
                    else
                    {
                        chunk1string = arFaces[0].Trim().Split('/');
                        chunk2string = arFaces[1].Trim().Split('/');
                        chunk3string = arFaces[2].Trim().Split('/');
                    }

                    //Parse components to int MINUS ONE BECAUSE OF ZERO INDEXING
                    int[] chunk1 = new int[] { int.Parse(chunk1string[0]) - 1, int.Parse(chunk1string[1]) - 1, int.Parse(chunk1string[2]) - 1 };
                    int[] chunk2 = new int[] { int.Parse(chunk2string[0]) - 1, int.Parse(chunk2string[1]) - 1, int.Parse(chunk2string[2]) - 1 };
                    int[] chunk3 = new int[] { int.Parse(chunk3string[0]) - 1, int.Parse(chunk3string[1]) - 1, int.Parse(chunk3string[2]) - 1 };

                    for (int j = 0; j < 3; j++) //If we're dealing with an obj with negative indexing, adjust face indices
                    {
                        if (chunk1[j] < 0) chunk1[j] += omg.endvert + 1;
                        if (chunk2[j] < 0) chunk2[j] += omg.endvert + 1;
                        if (chunk3[j] < 0) chunk3[j] += omg.endvert + 1;
                    }

                    /* Simpler hashing system that only respects position and UV maps */
                    //CHUNK 1
                    UInt64 tempHash = Utils.HashInts(chunk1[0], chunk1[1]);
                    if (VertUVDictionary.TryGetValue(tempHash, out _) == false)
                    {
                        VertUVDictionary.Add(tempHash, VertUVDictionary.Count);

                        Vertex WorkingVert = new Vertex();
                        WorkingVert.UpdatePosition(WorkingObject.Verts[chunk1[0]]);
                        WorkingVert.UpdateUV(WorkingObject.Textures[chunk1[1]]);
                        WorkingVert.UpdateNormals(WorkingObject.Normals[chunk1[2]]);

                        WorkingObject.UniqueVerts.Add(WorkingVert);
                    }

                    tempFaceArray[0] = VertUVDictionary[tempHash];

                    //CHUNK 2
                    tempHash = Utils.HashInts(chunk2[0], chunk2[1]);

                    if (VertUVDictionary.TryGetValue(tempHash, out _) == false)
                    {
                        VertUVDictionary.Add(tempHash, VertUVDictionary.Count);

                        Vertex WorkingVert = new Vertex();
                        WorkingVert.UpdatePosition(WorkingObject.Verts[chunk2[0]]);
                        WorkingVert.UpdateUV(WorkingObject.Textures[chunk2[1]]);
                        WorkingVert.UpdateNormals(WorkingObject.Normals[chunk2[2]]);

                        WorkingObject.UniqueVerts.Add(WorkingVert);
                    }

                    tempFaceArray[1] = VertUVDictionary[tempHash];

                    //CHUNK 3
                    tempHash = Utils.HashInts(chunk3[0], chunk3[1]);

                    if (VertUVDictionary.TryGetValue(tempHash, out _) == false)
                    {
                        VertUVDictionary.Add(tempHash, VertUVDictionary.Count);

                        Vertex WorkingVert = new Vertex();
                        WorkingVert.UpdatePosition(WorkingObject.Verts[chunk3[0]]);
                        WorkingVert.UpdateUV(WorkingObject.Textures[chunk3[1]]);
                        WorkingVert.UpdateNormals(WorkingObject.Normals[chunk3[2]]);

                        WorkingObject.UniqueVerts.Add(WorkingVert);
                    }

                    tempFaceArray[2] = VertUVDictionary[tempHash];

                    //ADD TEMP FACE TO THE LIST
                    omg.FaceIndices.Add(tempFaceArray);
                }
            }
        }

        //void ReadOBJ(string obj)
        //{
        //	ConsoleWrite(ConsoleLineSpacer);
        //	ConsoleWrite($"Opening OBJ file:  {obj}");
        //	VertexDaisyChain = new List<int>();
        //	ConsoleWrite(" ");
        //	ClearUpStatus();
        //	string[] lines = File.ReadAllLines(obj);

        //	Dictionary<UInt64, int> VertUVDictionary;
        //	Dictionary<UInt64, int> UniqueChunkDictionary = new Dictionary<UInt64, int>(); //For use if we implement vert normal splitting

        //	//Prepare Input OBJ Structure
        //	WorkingObjFile = new ObjFile
        //	{
        //		ObjObjects = new List<ObjObject>(),
        //		Verts = new List<Vertex>(),
        //		Normals = new List<Normal>(),
        //		Textures = new List<UVMap>()
        //	};

        //	int ObjObjectIndex = -1;
        //	int ObjGroupIndex = -1;
        //	int ObjMaterialIndex = -1;

        //	int lastV = 0;
        //	int lastN = 0;
        //	int lastT = 0;

        //	for (int i = 0; i < lines.Length; i++)
        //	{
        //		string line = lines[i];

        //		if (line.StartsWith("o "))
        //		{
        //			ObjObjectIndex++;
        //			ObjGroupIndex = -1;
        //			ObjMaterialIndex = -1;
        //			WorkingObjFile.ObjObjects.Add(new ObjObject() 
        //			{ 
        //				ObjGroups = new List<ObjGroup>(),
        //				Name = line.Split(' ')[1] 
        //			});
        //		}

        //		if (line.StartsWith("g "))
        //		{
        //			ObjGroupIndex++;
        //			ObjMaterialIndex = -1;
        //			WorkingObjFile.ObjObjects[ObjObjectIndex].ObjGroups.Add(new ObjGroup() 
        //			{ 
        //				ObjMaterials = new List<ObjMaterial>(), 
        //				Name = line.Split(' ')[1], 
        //				UniqueVerts = new List<Vertex>() 
        //			});
        //		}

        //		if (line.StartsWith("usemtl "))
        //		{
        //			ObjMaterialIndex++;
        //                  WorkingObjFile.ObjObjects[ObjObjectIndex].ObjGroups[ObjGroupIndex].ObjMaterials.Add(new ObjMaterial()
        //                  {
        //                      lines = new List<string>(),
        //                      DaisyChain = new List<int>(),
        //                      FaceIndices = new List<int[]>(),
        //                      Name = line.Split(' ')[1],
        //                      lastV = lastV,
        //				lastN = lastN,
        //				lastT = lastT
        //                  });
        //              }

        //		if (line.StartsWith("v "))
        //		{
        //			lastV++;
        //			float flip = 1f;
        //			if (chkGeometryFlipX.Checked) { flip = -1f; }

        //			string vertCoords = line.Replace("v ", "").Trim();
        //			string[] vertProps = vertCoords.Trim().Split(' ');

        //			Vertex vert = new Vertex()
        //			{
        //				X = flip * float.Parse(Utils.FixFloatingPoint(vertProps[0])),
        //				Y = float.Parse(Utils.FixFloatingPoint(vertProps[1])),
        //				Z = float.Parse(Utils.FixFloatingPoint(vertProps[2]))
        //			};
        //			WorkingObjFile.Verts.Add(vert);
        //		}

        //		if (line.StartsWith("vt "))
        //		{
        //			lastT++;
        //			string vertTextures = line.Replace("vt ", "").Trim();
        //			vertTextures = Utils.FixFloatingPoint(vertTextures);
        //			string[] vertTex = vertTextures.Trim().Split(' ');
        //			UVMap tex = new UVMap()
        //			{
        //				U = float.Parse(vertTex[0]),
        //				V = float.Parse(vertTex[1])
        //			};
        //			WorkingObjFile.Textures.Add(tex);
        //		}

        //		if (line.StartsWith("vn "))
        //		{   //TODO test if we need to flip normals
        //			lastN++;
        //			string normCoords = line.Replace("vn ", "").Trim();
        //			string[] normProps = normCoords.Trim().Split(' ');
        //			Normal norm = new Normal()
        //			{
        //				nX = float.Parse(Utils.FixFloatingPoint(normProps[0])),
        //				nY = float.Parse(Utils.FixFloatingPoint(normProps[1])),
        //				nZ = float.Parse(Utils.FixFloatingPoint(normProps[2]))
        //			};
        //			WorkingObjFile.Normals.Add(norm);
        //		}

        //		if (line.StartsWith("f "))
        //		{   
        //			if (WorkingObjFile.ObjObjects.Count == 0)
        //                  {
        //				ObjObjectIndex = 0;
        //				WorkingObjFile.ObjObjects.Add(new ObjObject()
        //				{
        //					ObjGroups = new List<ObjGroup>(),
        //					Name = line.Split(' ')[1]
        //				});
        //			}
        //			if (WorkingObjFile.ObjObjects[0].ObjGroups.Count == 0)
        //			{
        //				ObjGroupIndex = 0;
        //				WorkingObjFile.ObjObjects[ObjObjectIndex].ObjGroups.Add(new ObjGroup()
        //				{
        //					ObjMaterials = new List<ObjMaterial>(),
        //					Name = line.Split(' ')[1],
        //					UniqueVerts = new List<Vertex>()
        //				});
        //			}
        //			if (WorkingObjFile.ObjObjects[0].ObjGroups[0].ObjMaterials.Count == 0)
        //			{
        //				ObjMaterialIndex = 0;
        //				WorkingObjFile.ObjObjects[ObjObjectIndex].ObjGroups[ObjGroupIndex].ObjMaterials.Add(new ObjMaterial()
        //				{
        //					lines = new List<string>(),
        //					DaisyChain = new List<int>(),
        //					FaceIndices = new List<int[]>(),
        //					Name = line.Split(' ')[1],
        //					lastV = lastV,
        //					lastN = lastN,
        //					lastT = lastT
        //				});
        //			}
        //			//If we find a face line, add it to our current material group
        //			WorkingObjFile.ObjObjects[ObjObjectIndex].ObjGroups[ObjGroupIndex].ObjMaterials[ObjMaterialIndex].lines.Add(line);
        //		}
        //	}

        //	List<EMG> testEMGlist = new List<EMG>();

        //	foreach (ObjObject o in WorkingObjFile.ObjObjects)
        //          {
        //		EMG emg = new EMG()
        //		{
        //			ModelCount = 0,
        //			ModelPointersList = new List<int>(),
        //			Models = new List<Model>(),
        //			RootBone = 0
        //		};

        //		foreach (ObjGroup g in o.ObjGroups)
        //              {
        //			Model model = new Model()
        //			{
        //				VertexData = new List<Vertex>(),
        //				BitDepth = 0x20,
        //				BitFlag = 0x07,
        //				ReadMode = 0,
        //				SubModels = new List<SubModel>(),
        //				SubModelsCount = 0,
        //				TexturePointersList = new List<int>(),
        //				Textures = new List<EMGTexture>()
        //			};

        //			VertUVDictionary = new Dictionary<ulong, int>(); //Clear the dictionary for the new model

        //			foreach (ObjMaterial m in g.ObjMaterials)
        //                  {
        //				SubModel submodel = new SubModel()
        //				{
        //					BoneIntegersCount = 0,
        //					SubModelName = Utils.MakeModelName(m.Name)
        //				};

        //				for (int i = 0; i < m.lines.Count; i++)
        //                      {
        //                          string line = m.lines[i];
        //                          int[] tempFaceArray = new int[3];

        //                          if (!line.Contains("/"))
        //                          {
        //                              MessageBox.Show("Invalid Face data format", "Error");
        //                              return;
        //                          }
        //                          string vertFaces = line.Replace("f ", "").Trim(); //Remove leading f
        //                          string[] arFaces = vertFaces.Trim().Split(' '); //Split into chunks

        //                          string[] chunk1string;
        //                          string[] chunk2string;
        //                          string[] chunk3string;
        //                          //FACE FLIP HAPPENS HERE NOW
        //                          if (chkGeometryFlipX.Checked)
        //                          {
        //                              chunk1string = arFaces[2].Trim().Split('/');   //Split chunks into index components
        //                              chunk2string = arFaces[1].Trim().Split('/');
        //                              chunk3string = arFaces[0].Trim().Split('/');
        //                          }
        //                          else
        //                          {
        //                              chunk1string = arFaces[0].Trim().Split('/');
        //                              chunk2string = arFaces[1].Trim().Split('/');
        //                              chunk3string = arFaces[2].Trim().Split('/');
        //                          }

        //                          //Parse components to int MINUS ONE BECAUSE OF ZERO INDEXING
        //                          int[] chunk1 = new int[] { int.Parse(chunk1string[0]) - 1, int.Parse(chunk1string[1]) - 1, int.Parse(chunk1string[2]) - 1 };
        //                          int[] chunk2 = new int[] { int.Parse(chunk2string[0]) - 1, int.Parse(chunk2string[1]) - 1, int.Parse(chunk2string[2]) - 1 };
        //                          int[] chunk3 = new int[] { int.Parse(chunk3string[0]) - 1, int.Parse(chunk3string[1]) - 1, int.Parse(chunk3string[2]) - 1 };

        //                          if (chunk1[0] < 0) chunk1[0] += m.lastV + 1;
        //                          if (chunk1[1] < 0) chunk1[1] += m.lastT + 1;
        //                          if (chunk1[2] < 0) chunk1[2] += m.lastN + 1;

        //					if (chunk2[0] < 0) chunk2[0] += m.lastV + 1;
        //					if (chunk2[1] < 0) chunk2[1] += m.lastT + 1;
        //					if (chunk2[2] < 0) chunk2[2] += m.lastN + 1;

        //					if (chunk3[0] < 0) chunk3[0] += m.lastV + 1;
        //					if (chunk3[1] < 0) chunk3[1] += m.lastT + 1;
        //					if (chunk3[2] < 0) chunk3[2] += m.lastN + 1;


        //					/* Simpler hashing system that only respects position and UV maps */
        //					//CHUNK 1
        //					UInt64 tempHash = Utils.HashInts(chunk1[0], chunk1[1]);
        //                          if (VertUVDictionary.TryGetValue(tempHash, out _) == false)
        //                          {
        //                              VertUVDictionary.Add(tempHash, VertUVDictionary.Count);

        //                              Vertex WorkingVert = new Vertex();
        //                              WorkingVert.UpdatePosition(WorkingObjFile.Verts[chunk1[0]]);
        //                              WorkingVert.UpdateUV(WorkingObjFile.Textures[chunk1[1]]);
        //                              WorkingVert.UpdateNormals(WorkingObjFile.Normals[chunk1[2]]);

        //                              g.UniqueVerts.Add(WorkingVert);
        //                          }

        //                          tempFaceArray[0] = VertUVDictionary[tempHash];

        //                          //CHUNK 2
        //                          tempHash = Utils.HashInts(chunk2[0], chunk2[1]);

        //                          if (VertUVDictionary.TryGetValue(tempHash, out _) == false)
        //                          {
        //                              VertUVDictionary.Add(tempHash, VertUVDictionary.Count);

        //                              Vertex WorkingVert = new Vertex();
        //                              WorkingVert.UpdatePosition(WorkingObjFile.Verts[chunk2[0]]);
        //                              WorkingVert.UpdateUV(WorkingObjFile.Textures[chunk2[1]]);
        //                              WorkingVert.UpdateNormals(WorkingObjFile.Normals[chunk2[2]]);

        //                              g.UniqueVerts.Add(WorkingVert);
        //                          }

        //                          tempFaceArray[1] = VertUVDictionary[tempHash];

        //                          //CHUNK 3
        //                          tempHash = Utils.HashInts(chunk3[0], chunk3[1]);

        //                          if (VertUVDictionary.TryGetValue(tempHash, out _) == false)
        //                          {
        //                              VertUVDictionary.Add(tempHash, VertUVDictionary.Count);

        //                              Vertex WorkingVert = new Vertex();
        //                              WorkingVert.UpdatePosition(WorkingObjFile.Verts[chunk3[0]]);
        //                              WorkingVert.UpdateUV(WorkingObjFile.Textures[chunk3[1]]);
        //                              WorkingVert.UpdateNormals(WorkingObjFile.Normals[chunk3[2]]);

        //                              g.UniqueVerts.Add(WorkingVert);
        //                          }

        //                          tempFaceArray[2] = VertUVDictionary[tempHash];

        //                          //ADD TEMP FACE TO THE LIST
        //                          m.FaceIndices.Add(tempFaceArray);
        //                      }

        //				submodel.DaisyChain = DaisyChainFromIndices(m.FaceIndices).ToArray();
        //				submodel.DaisyChainLength = submodel.DaisyChain.Length;

        //				model.SubModels.Add(submodel);
        //				model.SubModelsCount++;
        //                  }

        //			model.VertexData = g.UniqueVerts;

        //			emg.Models.Add(model);
        //			emg.ModelCount++;
        //		}

        //		testEMGlist.Add(emg);
        //          }
        //}


        List<int> DaisyChainFromIndices(List<int[]> nIndices)
		{
			SetupProgress(nIndices.Count);

			Stopwatch stopWatch = new Stopwatch();

			stopWatch.Start();

			int compression = nIndices.Count * 3;
			int compression_zero = compression;

			List<int> Chain = new List<int>();
			bool bForwards = false;
			int count = nIndices.Count;

			//Initialise start of chain
			int buffer1 = nIndices[0][2];
			int buffer2 = nIndices[0][1];
			Chain.AddRange(new List<int> { nIndices[0][0], nIndices[0][1], nIndices[0][2] });
			nIndices.RemoveAt(0);

			int[] workingArray = new int[3];

			while (nIndices.Count > 0)
			{
				for (int i = 0; i < nIndices.Count; i++)
				{
					workingArray = nIndices[i];

					for (int j = 0; j < 3; j++)
					{
                        int x1 = (j > 0) ? -3 : 0;
                        int x2 = (j == 2) ? -3 : 0;

                        if (bForwards == true && workingArray[1 + j + x2] == buffer1 && workingArray[0 + j] == buffer2)
                        {
                            compression -= 2;
                            buffer2 = buffer1;
                            buffer1 = workingArray[2 + j + x1];
                            Chain.Add(buffer1);
                            nIndices.RemoveAt(i);
                            i = -1;
                            bForwards = !bForwards;
                            progressBar1.Value += 1;
                            //TimeEstimate(TStrings.STR_ReorderingFaces, count, Chain.Count);
                            break;
                        }
                        if (bForwards == false && workingArray[1 + j + x2] == buffer1 && workingArray[2 + j + x1] == buffer2)
                        {
                            compression -= 2;
                            buffer2 = buffer1;
                            buffer1 = workingArray[0 + j];
                            Chain.Add(buffer1);
                            nIndices.RemoveAt(i);
                            i = -1;
                            bForwards = !bForwards;
                            progressBar1.Value += 1;
                            //TimeEstimate(TStrings.STR_ReorderingFaces, count, Chain.Count);
                            break;
                        }
                    }
				}
				//No match found - if we've run out of faces, great, if not, re-initialise
				if (nIndices.Count > 0)
				{
					workingArray = nIndices[0];

					compression += 2;
					Chain.Add(buffer1);
					if (bForwards)
					{
						//Create chain break
						Chain.Add(workingArray[0]);
						Chain.Add(workingArray[0]);
						Chain.Add(workingArray[1]);
						Chain.Add(workingArray[2]);
						//Re-initialise buffer
						buffer1 = workingArray[2];
						buffer2 = workingArray[1];
					}
					if (!bForwards)
					{
						//Create chain break
						Chain.Add(workingArray[2]);
						Chain.Add(workingArray[2]);
						Chain.Add(workingArray[1]);
						Chain.Add(workingArray[0]);
						//Re-initialise buffer
						buffer1 = workingArray[0];
						buffer2 = workingArray[1];
					}
					//Clear the used face and flip the flag
					nIndices.RemoveAt(0);
					bForwards = !bForwards;
					progressBar1.Value += 1;
					//TimeEstimate(TStrings.STR_ReorderingFaces, count, Chain.Count);
				}
			}

			progressBar1.Value = progressBar1.Maximum;

			if (Chain.Count > 0xFFFF) AddStatus("Warning - Encoded object has too many faces. Consider splitting into smaller sub-models to ensure correct loading.");

			Console.WriteLine($"Compression {compression}/{compression_zero} = {100 * compression / compression_zero}%");

			stopWatch.Stop();

			Console.WriteLine($"Stopwatch {stopWatch.ElapsedMilliseconds}");

			return Chain;
		}

		void ClearUpStatus()
		{
			ObjectLoaded = false;
			TotalNewVerts = 0;
			lbLoadSteps.Text = string.Empty;
			progressBar1.Value = 0;
		}

		async void EncodeTheOBJ()
		{
			EncodingInProgress = true;

			for (int i = 0; i < WorkingObject.MaterialGroups.Count; i++)
			{
				ObjMatGroup tempMatGroup = WorkingObject.MaterialGroups[i];

				tempMatGroup.DaisyChain = await Task.Run(() =>
				{
					//return GeometryIO.BruteForceChain(new List<int[]>(WorkingObject.MaterialGroups[i].FaceIndices));
					return DaisyChainFromIndices(new List<int[]>(WorkingObject.MaterialGroups[i].FaceIndices));
				});

				WorkingObject.MaterialGroups.RemoveAt(i);
				WorkingObject.MaterialGroups.Insert(i, tempMatGroup);

				lbLoadSteps.Text = TStrings.STR_EncodeComplete;
				
			}
			AddStatus(WorkingFileName + " encoded!");
			ObjectLoaded = true;
			EncodingInProgress = false;
		}

		public void SetupProgress(int steps)
		{
			StartTime = DateTime.Now;
			progressBar1.Maximum = steps;
			progressBar1.Value = 0;
		}

		void TimeEstimate(string action)
		{
			int max = progressBar1.Maximum;
			int index = progressBar1.Value;
			TimeSpan timeRemaining = TimeSpan.FromTicks(DateTime.Now.Subtract(StartTime).Ticks * (max - (index + 1)) / (index + 1));
			lbLoadSteps.Text = string.Format("{0}: {1:00}:{2:00}:{3:00}", action, timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds);
		}

		bool ValidateOBJ()
		{
			if (WorkingObject.Verts.Count <= 0) { MessageBox.Show(TStrings.STR_ERR_VertsNotFoundinOBJ, TStrings.STR_Information); return false; }
			if (WorkingObject.Textures.Count <= 0) { MessageBox.Show(TStrings.STR_ERR_TexturesNotFoundinOBJ, TStrings.STR_Information); return false; }
			if (WorkingObject.Normals.Count <= 0) { MessageBox.Show(TStrings.STR_ERR_NormalsNotFoundinOBJ, TStrings.STR_Information); return false; }
			if (WorkingObject.MaterialGroups.Count <= 0) { MessageBox.Show(TStrings.STR_ERR_FacesNotFoundinOBJ, TStrings.STR_Information); return false; }

			return true;
		}

		#region Console Dump Methods
		void DumpOBJData(ObjModel model)
		{
			ConsoleWrite($"	Verts: {model.Verts.Count}");
			ConsoleWrite($"	Textures: {model.Textures.Count}");
			ConsoleWrite($"	Normals: {model.Normals.Count}");
			ConsoleWrite($"	Faces: {model.FaceIndices.Count}");
		}

		void DumpFaceListToConsole()
		{
			ConsoleWrite(" Complete Face List");
			//foreach (Face f in WorkinObject.VertFaces)
			//{
			//	PrintDebugFacesInfo("	", f);
			//}
			ConsoleWrite(ConsoleLineSpacer);
		}

		void DumpEncodeFinished()
		{
			ConsoleWrite($"Total of {TotalNewVerts} verts added after encoding.");
			ConsoleWrite(" ");
			ConsoleWrite("Encoded OBJ:");
			ConsoleWrite(ConsoleLineSpacer);
			//DumpOBJData(EncodedObject);
			ConsoleWrite(ConsoleLineSpacer);
			DumpFaceListToConsole();
		}
		public void ConsoleWrite(string line) { debugOutputForm.AddLineToConsole(line); }

		//Open Debug Form
		private void ConsoleMessagesOutputToolStripMenuItem_Click(object sender, EventArgs e) { debugOutputForm.Show(); }
		#endregion Console Dump

		private void BtnSaveNewOBJ_Click(object sender, EventArgs e)
		{

		}

		void SaveEncodedOBJ()
		{
			if (!ObjectLoaded) { MessageBox.Show(TStrings.STR_ERR_NoOBJ, TStrings.STR_Information); return; }
			string filepath;
			string newOBJ = WorkingFileName + "_Encoded.obj";

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = newOBJ;
			saveFileDialog1.Filter = OBJFileFilter;     //"Wavefront (.obj)|*.obj";
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				filepath = saveFileDialog1.FileName;
				if (filepath.Trim() != "")
				{
					List<string> OBJData = new List<string>();

					foreach (Vertex v in WorkingObject.UniqueVerts)
					{
						OBJData.Add($"v {Utils.RestoreFloatingPoint(Convert.ToString(v.X))} {Utils.RestoreFloatingPoint(Convert.ToString(v.Y))} {Utils.RestoreFloatingPoint(Convert.ToString(v.Z))}");
					}
					foreach (Vertex v in WorkingObject.UniqueVerts)
					{
						OBJData.Add($"vt {Utils.RestoreFloatingPoint(Convert.ToString(v.U))} {Utils.RestoreFloatingPoint(Convert.ToString(v.V))}");
					}
					foreach (Vertex v in WorkingObject.UniqueVerts)
					{
						OBJData.Add($"vn {Utils.RestoreFloatingPoint(Convert.ToString(v.nX))} {Utils.RestoreFloatingPoint(Convert.ToString(v.nY))} {Utils.RestoreFloatingPoint(Convert.ToString(v.nZ))}");
					}
					foreach (int[] ar in WorkingObject.FaceIndices)
					{
						OBJData.Add($"f {ar[0] + 1}/{ar[0] + 1}/{ar[0] + 1} {ar[1] + 1}/{ar[1] + 1}/{ar[1] + 1} {ar[2] + 1}/{ar[2] + 1}/{ar[2] + 1}");
					}
					lbLoadSteps.Text = TStrings.STR_OBJSaved;
					MessageBox.Show($"New OBJ {newOBJ} saved.", "Success!");
					File.WriteAllLines(filepath, OBJData.Cast<string>().ToArray());
					ConsoleWrite(ConsoleLineSpacer);
					ConsoleWrite($"Success! New OBJ {newOBJ} saved.");
					ConsoleWrite(ConsoleLineSpacer);
				}
			}
		}

		void OutputOBJToHEX()
		{
			string DaisyChainHEX = string.Empty;

			HeXText = new StringBuilder();
			foreach (int num in VertexDaisyChain) { DaisyChainHEX = DaisyChainHEX + Utils.IntToHex(num); }
			HeXText.Append(DaisyChainHEX);
			//SetupProgress(TStrings.STR_OutputingHex, WorkingObject.UniqueVerts.Count);

			string dataLine = string.Empty;
			foreach (Vertex v in WorkingObject.UniqueVerts)
			{
				dataLine = dataLine + $"{Utils.FloatToHex(v.X)}{Utils.FloatToHex(v.Y)}{Utils.FloatToHex(v.Z)}";
				dataLine = dataLine + $"{Utils.FloatToHex(v.U)}{Utils.FloatToHex(v.V)}";
				dataLine = dataLine + $"{Utils.FloatToHex(v.nX)}{Utils.FloatToHex(v.nY)}{Utils.FloatToHex(v.nZ)}";

				HeXText.Append(dataLine);
				//progressBar1.Value += 1;
				//TimeEstimate(TStrings.STR_OutputingHex, WorkingObject.UniqueVerts.Count, progressBar1.Value);
			}
		}

		void SaveEncodedOBJHex()
		{
			if (!ObjectLoaded) { AddStatus(TStrings.STR_ERR_NoOBJ); return; }
			OutputOBJToHEX();
			string filepath;
			string newTXT = WorkingFileName + "HEX.txt";
			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = newTXT;
			saveFileDialog1.Filter = TXTFileFilter;
			filepath = saveFileDialog1.FileName;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				filepath = saveFileDialog1.FileName;
				File.WriteAllText(filepath, HeXText.ToString());
				lbLoadSteps.Text = TStrings.STR_HEXSaved;
				AddStatus($"HEX Output saved to {filepath}");
				ConsoleWrite(ConsoleLineSpacer);
				ConsoleWrite($"Success! {filepath} processed & saved.");
				ConsoleWrite(ConsoleLineSpacer);
			}
		}

		private void BtnSaveHEXFile_Click(object sender, EventArgs e)
		{

		}

		//Open USF4 Modding Google DOC
		private void USF4ModdingDocumentToolStripMenuItem_Click(object sender, EventArgs e) { System.Diagnostics.Process.Start("https://docs.google.com/document/d/1dU-uFvhQksLNEzEc4OWpj9nfEGy7gh5-HwGZ8NsOiTY"); }

		EMG NewEMGFromOBJ(EMG template, bool AddNewName)
		{
			EMG emg = new EMG();
			int RootBone = template.RootBone;
			byte[] ModelName;
			if (AddNewName)
				ModelName = Utils.MakeModelName(lbOBJNameProperty.Text.Substring(0, lbOBJNameProperty.Text.Length - 4));
			else
				ModelName = template.Models[0].SubModels[0].SubModelName;
			//EMG Header
			emg.RootBone = RootBone;
			emg.ModelCount = 1;
			emg.ModelPointersList = new List<int> { 0x10 };

            //Model Header
            Model mod = new Model
            {
                BitFlag = template.Models[0].BitFlag, //0x45;
                TextureCount = WorkingObject.MaterialGroups.Count,
                TextureListPointer = 0x50,
                Textures = new List<EMGTexture>(),
                TexturePointersList = new List<int>(),
                VertexCount = WorkingObject.UniqueVerts.Count,
                BitDepth = template.Models[0].BitDepth,
                ReadMode = 0x01,
                SubModelsCount = WorkingObject.MaterialGroups.Count,
                SubModelsListPointer = 0x64,
                SubModelPointersList = new List<int>(),
                SubModels = new List<SubModel>(),
                //Copy of the MAD_KAN cull data, can adjust if needed
                CullData = new byte[] { 0x00, 0x0A, 0x1B, 0x3C, 0xC3, 0xA4, 0x9E, 0x40,
                                        0x80, 0x89, 0x27, 0x3E, 0xF6, 0x79, 0x3E, 0x41,
                                        0x50, 0x94, 0xA1, 0xC0, 0xFD, 0x14, 0x9D, 0xBF,
                                        0xEE, 0x94, 0x0A, 0xC1, 0x43, 0xB9, 0x0D, 0x43,
                                        0x5A, 0x2F, 0xA2, 0x40, 0x63, 0x47, 0x32, 0x41,
                                        0x3A, 0xD1, 0x0F, 0x41, 0xF6, 0x79, 0xBE, 0x41 }
            };

            //Populate texture info
            for (int i = 0; i < WorkingObject.MaterialGroups.Count; i++)
			{
                EMGTexture tex = new EMGTexture
                {
                    Scales_UList = new List<float> { 1f },
                    Scales_VList = new List<float> { 1f },
                    TextureLayers = 0x01,
                    TextureIndicesList = new List<int> { template.Models[0].Textures[0].TextureIndicesList[0] }
                };

                mod.Textures.Add(tex);
				mod.TexturePointersList.Add(0x00);

				if (tbTextureIndex.Text.Trim() != string.Empty)
				{
					tex.TextureIndicesList[0] = int.Parse(tbTextureIndex.Text);
				}
				if (tbScaleU.Text.Trim() != string.Empty)
				{
					for (int j = 0; j < tex.TextureLayers; j++)
					{
						tex.Scales_UList[j] = Convert.ToSingle(tbScaleU.Text);
					}
				}
				if (tbScaleV.Text.Trim() != string.Empty)
				{
					for (int j = 0; j < tex.TextureLayers; j++)
					{
						tex.Scales_VList[j] = Convert.ToSingle(tbScaleV.Text);
					}
				}
			}

			SubModel subm = new SubModel();

			for (int i = 0; i < WorkingObject.MaterialGroups.Count; i++)
			{
				mod.SubModelPointersList.Add(0x00);

                subm = new SubModel
                {
                    MaterialIndex = i,
                    MysteryFloats = new byte[] { 0x9E, 0xDF, 0xDD, 0xBC, 0xC5, 0x2A, 0x3B, 0x3E, 0xA7, 0x68, 0x3F, 0x3C, 0x00, 0x00, 0x80, 0x3F },
                    DaisyChain = WorkingObject.MaterialGroups[i].DaisyChain.ToArray(),
                    DaisyChainLength = WorkingObject.MaterialGroups[i].DaisyChain.Count,
                    SubModelName = ModelName
                };

                mod.SubModels.Add(subm);
			}
			mod.VertexListPointer = mod.SubModelPointersList[0] + 0x36 + (subm.DaisyChainLength * 2) + (subm.BoneIntegersCount * 2);

			mod.VertexData = new List<Vertex>();
			for (int i = 0; i < WorkingObject.UniqueVerts.Count; i++)
			{
				Vertex v = WorkingObject.UniqueVerts[i];
				if (chkTextureFlipX.Checked) v.U = 1 - v.U;
				if (chkTextureFlipY.Checked) v.V = 1 - v.V;
				mod.VertexData.Add(v);
			}

			emg.Models = new List<Model> { mod };

			emg.GenerateBytes();
			return emg;
		}

		Skeleton SkeletonFromSMD(SMDModel model)
		{
            Skeleton skel = new Skeleton
            {
                FFList = new List<byte[]>(),
                Nodes = new List<Node>(),
                NodeNamePointersList = new List<int>(),
                NodeNames = new List<byte[]>(),
                //Declare generic skeleton values
                NodeCount = model.Nodes.Count,
                IKObjectCount = 0,
                IKDataCount = 0,
                NodeListPointer = 0,
                NameIndexPointer = 0,
                IKBoneListPointer = 0,
                IKObjectNameIndexPointer = 0,
                RegisterPointer = 0,
                IKDataPointer = 0,
                MysteryShort = 2,           //1 REALLY no idea what these are
                MysteryFloat1 = 0x26a1dd0a, //2
                MysteryFloat2 = 0x4d28129d  //3
            };

            //Populate nodes and register
            for (int i = 0; i < model.Nodes.Count; i++)
			{
				skel.FFList.Add(new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 });

                Node WorkingNode = new Node
                {
                    Parent = model.Nodes[i].Parent,
                    Child1 = -1,
                    Sibling = -1,
                    Child3 = -1,
                    Child4 = -1,
                    PreMatrixFloat = 0f
				};

				//Look forwards for a child, break loop when found
				for (int j = i + 1; j < model.Nodes.Count; j++)
				{
					if (model.Nodes[j].Parent == i) { WorkingNode.Child1 = j; break; }
				}
				//Look forwards for a sibling, break loop when found
				for (int j = i + 1; j < model.Nodes.Count; j++)
				{
					if (model.Nodes[j].Parent == WorkingNode.Parent) { WorkingNode.Sibling = j; break; }
				}
				//Construct node matrix
				Matrix4x4 sMatrix = Matrix4x4.CreateScale(1f, 1f, 1f); //No scale data in an SMD, so initialise it all to 1.

				Matrix4x4 ryMatrix = Matrix4x4.CreateRotationY(model.Frames[0].rotY[i]);
				Matrix4x4 rxMatrix = Matrix4x4.CreateRotationX(model.Frames[0].rotX[i]);
				Matrix4x4 rzMatrix = Matrix4x4.CreateRotationZ(model.Frames[0].rotZ[i]);
				Matrix4x4 tMatrix = Matrix4x4.CreateTranslation(model.Frames[0].traX[i], model.Frames[0].traY[i], model.Frames[0].traZ[i]);

				Matrix4x4 tempMatrix = Matrix4x4.Multiply(sMatrix, ryMatrix);    //S Ry
				tempMatrix = Matrix4x4.Multiply(tempMatrix, rxMatrix); //S Ry Rx
				tempMatrix = Matrix4x4.Multiply(tempMatrix, rzMatrix); //S Ry Rx Rz

				WorkingNode.NodeMatrix = Matrix4x4.Multiply(tempMatrix, tMatrix);//S Ry Rx Rz T

				skel.Nodes.Add(WorkingNode);
				skel.NodeNames.Add(model.Nodes[i].Name);
				skel.NodeNamePointersList.Add(0);
			}

			skel.GenerateBytes();

			return skel;
		}

		LUA ReadLUA(byte[] Data)
		{
			LUA nLUA = new LUA();
			return nLUA;
		}

		#region Tree Methods
		public void ClearTree() { tvTree.Nodes.Clear(); }

		void RefreshTree(bool AfterOpen)
		{
			var savedExpansionState = tvTree.Nodes.GetExpansionState();
			tvTree.BeginUpdate();
			ClearTree();
			if (WorkingEMZ != null && WorkingEMZ.Files != null) FillTreeEMZ(WorkingEMZ, TargetEMZFilePath);
			if (WorkingTEXEMZ != null && WorkingTEXEMZ.Files != null) FillTreeEMZ(WorkingTEXEMZ, TargetTEXEMZFilePath);
			if (!AfterOpen)
			{
				tvTree.Nodes.SetExpansionState(savedExpansionState);
			}
			tvTree.EndUpdate();
			tvTree.SelectedNode = TreeViewExtensions.SelectedNodeBeforeRefresh; //Not working??
		}

		public void FillTreeEMZ(EMZ sourceEMZ, string filepath)
		{
			TreeNode NodeEMZ = new TreeNode("EMZ - " + filepath);
			NodeEMZ.Tag = "EMZ";
			if (filepath.Contains("tex.emz")) NodeEMZ.Tag = "TEX";
			tvTree.Nodes.Add(NodeEMZ);
			for (int i = 0; i < sourceEMZ.Files.Count; i++)
			{
				USF4File file = sourceEMZ.Files[i];
				string nodeName = Encoding.UTF8.GetString(sourceEMZ.FileNamesList[i]);
				if (file.GetType() == typeof(EMO))
				{
					TreeNode nodeEMO = AddTreeNode(NodeEMZ, nodeName, "EMO");
					EMO emo = (EMO)sourceEMZ.Files[i];
					for (int j = 0; j < emo.EMGCount; j++)
					{
						TreeNode nodeEMG = AddTreeNode(nodeEMO, "EMG " + j, "EMG");
						for (int m = 0; m < emo.EMGs[j].ModelCount; m++)
						{
							Model mod = emo.EMGs[j].Models[m];
							TreeNode nodeModel = AddTreeNode(nodeEMG, "Model " + m, "Model");
							for (int sm = 0; sm < mod.SubModelsCount; sm++)
							{
								SubModel sMod = mod.SubModels[sm];
								AddTreeNode(nodeModel, Encoding.UTF8.GetString(sMod.SubModelName), "SubModel");
							}
						}
						//nodeEMG.Text =$"{nodeEMG.Text} - [{Encoding.UTF8.GetString(emo.EMGList[j].Models[0].SubModels[0].SubModelName)} ----]";

					}
					if (emo.Skeleton.Nodes != null)
					{
						TreeNode nodeSkeleton = AddTreeNode(nodeEMO, "Skeleton", "Skeleton");
						for (int k = 0; k < emo.Skeleton.Nodes.Count; k++)
						{
							AddTreeNode(nodeSkeleton, k + " " + Encoding.ASCII.GetString(emo.Skeleton.NodeNames[k]), "Node");
						}
					}
					else { AddTreeNode(nodeEMO, "No skeleton data", "No skeleton data"); }
				}
					
				if (file.GetType() == typeof(EMM))
				{
					TreeNode nodeEMM = AddTreeNode(NodeEMZ, nodeName, "EMM");
					EMM emm = (EMM)sourceEMZ.Files[i];
					for (int j = 0; j < emm.MaterialCount; j++)
					{
						AddTreeNode(nodeEMM, Encoding.UTF8.GetString(emm.Materials[j].Name), "Material");
					}
				}
				if (file.GetType() == typeof(LUA))
				{
					AddTreeNode(NodeEMZ, nodeName, "LUA");
				}

				if (file.GetType() == typeof(EMB))
				{
					TreeNode nodeEMB = AddTreeNode(NodeEMZ, nodeName, "EMB");
					EMB emb = (EMB)sourceEMZ.Files[i];
					for (int j = 0; j < emb.DDSFiles.Count; j++)
					{
						AddTreeNode(nodeEMB, j + "  " + Encoding.UTF8.GetString(emb.FileNameList[j]), "DDS");
					}
				}
				if (file.GetType() == typeof(CSB))
				{
					AddTreeNode(NodeEMZ, nodeName, "CSB");
				}
				if (file.GetType() == typeof(EMA))
				{
					TreeNode nodeEMA = AddTreeNode(NodeEMZ, nodeName, "EMA");
					EMA ema = (EMA)sourceEMZ.Files[i];
					for (int j = 0; j < ema.Animations.Count; j++)
					{
						AddTreeNode(nodeEMA, j + "  " + Encoding.ASCII.GetString(ema.Animations[j].Name), "Animation");
					}

					if (ema.Skeleton.Nodes != null)
					{
						TreeNode nodeSkeleton = AddTreeNode(nodeEMA, "Skeleton", "Skeleton");
						for (int k = 0; k < ema.Skeleton.Nodes.Count; k++)
						{
							AddTreeNode(nodeSkeleton, k + " " + Encoding.ASCII.GetString(ema.Skeleton.NodeNames[k]), "Node");
						}
					}
                    else { AddTreeNode(nodeEMA, "No skeleton data", "No skeleton data"); }

				}
				if (file.GetType() == typeof(OtherFile))
				{
					AddTreeNode(NodeEMZ, nodeName, "OtherFile");
				}
			}
			NodeEMZ.Expand();
		}
		//Add and tag EMO as Child to the target EMZ
		TreeNode AddTreeNode(TreeNode Root, string Name, string tag)
		{
			TreeNode Child = new TreeNode(Name); Child.Tag = tag;
			Root.Nodes.Add(Child);
			return Child;
		}

		private void TvTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			string title = string.Empty;
			string sep = " / ";
			CM = cmEmpty;
			LastSelectedTreeNode = tvTree.SelectedNode;
			Console.WriteLine("Last Node Index: " + LastSelectedTreeNode.Index);
			tvTree.SelectedNode = e.Node;
			pbPreviewDDS.Visible = false;
			pbPreviewDDS.SendToBack();

			//Hide edit panels
			pnlEO_EMG.Visible = false;
			pnlEO_MOD.Visible = false;
			pnlEO_SUBMOD.Visible = false;
			pnlEO_MaterialEdit.Visible = false;
			lbSelNODE_ListData.Visible = true;

			//Do the selected index stuff
			if (e.Node.Tag.ToString() == "EMZ")
			{
				CM = emzContext;
				SelectedEMZNumberInTree = e.Node.Index;
				TreeDisplayEMZData();
			}
			if (e.Node.Tag.ToString() == "TEX")
			{
				CM = emzContext;
				SelectedEMZNumberInTree = e.Node.Index;
				TreeDisplayEMZData();
			}
			if (e.Node.Tag.ToString() == "EMM")
			{
				CM = emmContext;
				title = e.Node.Text;
				TreeDisplayEMMData((EMM)WorkingEMZ.Files[e.Node.Index]);
			}
			if (e.Node.Tag.ToString() == "EMB")
			{
				CM = embContext;
				title = e.Node.Text;
				//TreeDisplayEMBData((EMB)WorkingEMZ.Files[e.Node.Index]);
			}
			if (e.Node.Tag.ToString() == "EMA")
			{
				CM = emaContext;
				title = e.Node.Text;
				//TreeDisplayEMBData((EMA)WorkingEMZ.Files[e.Node.Index]);
			}
			if (e.Node.Tag.ToString() == "Material")
			{
				pnlEO_MaterialEdit.Visible = true;
				lbSelNODE_ListData.Visible = false;
				CM = matContext;
				title = e.Node.Text;
				TreeDisplayEMMDetails();
			}
			if (e.Node.Tag.ToString() == "Animation")
			{
				CM = emaContext;
				title = e.Node.Text;
				lbSelNODE_ListData.Items.Clear();
				//TreeDisplayEMMData((EMM)WorkingEMZ.Files[e.Node.Index]);
			}
			if (e.Node.Tag.ToString() == "DDS")
			{
				try
				{
					CM = ddsContext;
					title = e.Node.Text;
					pbPreviewDDS.Visible = true;
					pbPreviewDDS.BringToFront();
					EMB emb = (EMB)WorkingTEXEMZ.Files[e.Node.Parent.Index];
					DDS dds = emb.DDSFiles[e.Node.Index];
					ImageEngineImage IE = new ImageEngineImage(dds.HEXBytes);
					pbPreviewDDS.BackgroundImage = Utils.BitmapFromBytes(IE.Save(new ImageEngineFormatDetails(ImageEngineFormat.PNG), new MipHandling()));
					IE.Dispose();
				}
				catch
				{
					AddStatus($"Error while trying to read this DDS.");
				}
			}
			if (e.Node.Tag.ToString() == "LUA")
			{
				CM = luaContext;
				title = e.Node.Text;
				lbSelNODE_ListData.Items.Clear();
			}
			if (e.Node.Tag.ToString() == "CSB")
			{
				CM = csbContext;
				title = e.Node.Text;
				lbSelNODE_ListData.Items.Clear();
			}
			if (e.Node.Tag.ToString() == "EMO")
			{
				CM = emoContext;
				SelectedEMONumberInTree = e.Node.Index;
				TreeDisplayEMOData((EMO)WorkingEMZ.Files[e.Node.Index]);

				title = e.Node.Text;
			}
			if (e.Node.Tag.ToString() == "EMA")
			{
				CM = emaContext;
				TreeDisplayEMAData((EMA)WorkingEMZ.Files[e.Node.Index]);

				title = e.Node.Text;
			}
			if (e.Node.Tag.ToString() == "EMG")
			{
				CM = emgContext;
				SelectedEMGNumberInTree = e.Node.Index;
				SelectedEMONumberInTree = e.Node.Parent.Index;  //Get The Parrent EMO Index
				EMO sEMO = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
				TreeDisplayEMGData(sEMO.EMGs[SelectedEMGNumberInTree]);
				pnlEO_EMG.Visible = true;
				title = $"{e.Node.Parent.Text}{sep}{e.Node.Text}";
			}
			if (e.Node.Tag.ToString() == "Skeleton")
            {
				CM = cmEmpty;
				lbSelNODE_ListData.Items.Clear();
			}
			if (e.Node.Tag.ToString() == "Node")
			{
				CM = cmEmpty;
				Node node;
				if(LastSelectedTreeNode.Parent.Parent.Tag.ToString() == "EMA")
                {
					EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Parent.Index];
					node = ema.Skeleton.Nodes[LastSelectedTreeNode.Index];
                }
				else
                {
					EMO emo = (EMO)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Parent.Index];
					node = emo.Skeleton.Nodes[LastSelectedTreeNode.Index];
				}
				
				TreeDisplaySkelNodeData(node);

				title = e.Node.Text;
			}
			if (e.Node.Tag.ToString() == "Model")
			{
				CM = emgContext;
				SelectedEMGNumberInTree = e.Node.Parent.Index;
				SelectedEMONumberInTree = e.Node.Parent.Parent.Index;
				SelectedModelNumberInTree = e.Node.Index;
				EMO sEMO = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
				TreeDisplayModelData(sEMO.EMGs[SelectedEMGNumberInTree].Models[e.Node.Index]);
				pnlEO_MOD.Visible = true;
				title = $"{e.Node.Parent.Parent.Text}{sep}{e.Node.Parent.Text}{sep}{e.Node.Text}";
			}
			if (e.Node.Tag.ToString() == "SubModel")
			{
				CM = emgContext;
				SelectedSubModelNumberInTree = e.Node.Index;
				SelectedModelNumberInTree = e.Node.Parent.Index;
				SelectedEMGNumberInTree = e.Node.Parent.Parent.Index;
				SelectedEMONumberInTree = e.Node.Parent.Parent.Parent.Index;
				EMO sEMO = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
				TreeDisplaySubModelData(sEMO.EMGs[SelectedEMGNumberInTree].Models[e.Node.Parent.Index].SubModels[e.Node.Index]);
				pnlEO_SUBMOD.Visible = true;
				tbEO_SubModName.Text = $"{e.Node.Text}";
				tbEO_SubModMaterial.Text = $"{sEMO.EMGs[SelectedEMGNumberInTree].Models[e.Node.Parent.Index].SubModels[e.Node.Index].MaterialIndex}";
				title = $"{e.Node.Text}";
			}
			lbSelNODE_Title.Text = title;
			Console.WriteLine($"EMO {SelectedEMONumberInTree} EMG {SelectedEMGNumberInTree} MOD {SelectedModelNumberInTree} SubModel {SelectedSubModelNumberInTree}");
		}

		private void TvTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			tvTree.SelectedNode = tvTree.GetNodeAt(e.Location);
			if (e.Button == MouseButtons.Right) { CM.Show(tvTree, e.Location); }
		}

		void TreeDisplayEMMDetails()
		{
			lvShaderProperties.Clear();
			Utils.ReadShaders();
			FillShaderComboBox();
			EMM emm = (EMM)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];
			Material mat = emm.Materials[LastSelectedTreeNode.Index];
			Console.WriteLine("Shader Name: " + Encoding.ASCII.GetString(mat.Name));

			if (!Utils.Shaders.TryGetValue(Encoding.ASCII.GetString(mat.Shader), out _))
            {
				Utils.Shaders.Add(Encoding.ASCII.GetString(mat.Shader), Utils.Shaders.Count);
				Utils.SaveShaders();
            }

			cbShaders.SelectedIndex = Utils.Shaders[Encoding.ASCII.GetString(mat.Shader)];
			for (int i = 0; i < mat.PropertyCount; i++)
			{
				byte[] name = mat.PropertyNamesList[i];
				byte[] value = mat.PropertyValuesList[i];
				string sName = Encoding.ASCII.GetString(name);
				string sValue = Utils.HexStr2(value, value.Length);
				AddPropertyLineToTextBox($"{sName.Replace("\0", "")} {sValue}");
			}
		}

		void AddPropertyLineToTextBox(string Line)
		{
			lvShaderProperties.Text = lvShaderProperties.Text + Line + Environment.NewLine;
		}

		void GetNodeParent(TreeNode node, int ParrentUP)
		{
			//node.
		}
		void OpenEMONode(bool SelectTheEMG) //false for deleting so we select only the EMO
		{
			tvTree.SelectedNode = tvTree.Nodes[0].Nodes[SelectedEMONumberInTree]; //FIX
			tvTree.SelectedNode.Expand();
			if (SelectTheEMG) tvTree.SelectedNode = tvTree.SelectedNode.Nodes[SelectedEMGNumberInTree];
		}
		#endregion Tree Methods

		#region Tree Display Related
		void TreeDisplayEMZData()
		{
			lbSelNODE_ListData.Items.Clear();
			if ((string)tvTree.Nodes[SelectedEMZNumberInTree].Tag == "EMZ") lbSelNODE_ListData.Items.Add($"File Count: {WorkingEMZ.Files.Count}");
			if ((string)tvTree.Nodes[SelectedEMZNumberInTree].Tag == "TEX") lbSelNODE_ListData.Items.Add($"File Count: {WorkingTEXEMZ.Files.Count}");
		}
		void TreeDisplayEMOData(EMO emo)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"EMG Count: {emo.EMGCount}");
			lbSelNODE_ListData.Items.Add($"Skeleton node count: {emo.Skeleton.NodeCount}");
		}
		void TreeDisplayEMAData(EMA ema)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Animation Count: {ema.AnimationCount}");
			lbSelNODE_ListData.Items.Add($"Skeleton node count: {ema.Skeleton.NodeCount}");
		}
		void TreeDisplaySkelNodeData(Node node)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Parent ID: {node.Parent}");
			lbSelNODE_ListData.Items.Add($"Child ID: {node.Child1}");
			lbSelNODE_ListData.Items.Add($"Sibling ID: {node.Sibling}");

			Utils.DecomposeMatrixXYZ(node.NodeMatrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out _, out _, out _);
			lbSelNODE_ListData.Items.Add($"Position: ({tx.ToString("0.0000")}, {ty.ToString("0.0000")}, {tz.ToString("0.0000")})");
			lbSelNODE_ListData.Items.Add($"Rotation: ({rx.ToString("0.0000")}, {ry.ToString("0.0000")}, {rz.ToString("0.0000")})");
		}
		void TreeDisplayEMGData(EMG emg)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Model Count: {emg.ModelCount}");
			lbSelNODE_ListData.Items.Add($"Root Bone: {emg.RootBone}");
			tbEMGRootBone.Text = $"{emg.RootBone}";
		}
		void TreeDisplayModelData(Model m)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"SubModel Count: {m.SubModelsCount}");
			lbSelNODE_ListData.Items.Add($"Bit Flag: {m.BitFlag}");
			lbSelNODE_ListData.Items.Add($"Bit Depth: {m.BitDepth}");
			lbSelNODE_ListData.Items.Add($"---------------------------");
			lbSelNODE_ListData.Items.Add($"Model Components:");
			if ((m.BitFlag & 0x01) == 0x01) { lbSelNODE_ListData.Items.Add($"    Vertex Co-ordinates"); }
			if ((m.BitFlag & 0x02) == 0x02) { lbSelNODE_ListData.Items.Add($"    Vertex Normals"); }
			if ((m.BitFlag & 0x04) == 0x04) { lbSelNODE_ListData.Items.Add($"    UV Map"); }
			if ((m.BitFlag & 0x80) == 0x80) { lbSelNODE_ListData.Items.Add($"    UV Map 2"); }
			if ((m.BitFlag & 0x40) == 0x40) { lbSelNODE_ListData.Items.Add($"    Vertex Color"); }
			if ((m.BitFlag & 0x200) == 0x200) { lbSelNODE_ListData.Items.Add($"    Bone Weights"); }
			lbSelNODE_ListData.Items.Add($"---------------------------");

			lbSelNODE_ListData.Items.Add($"Read Mode: {m.ReadMode}");
			lbSelNODE_ListData.Items.Add($"Texture Count: {m.TextureCount}");

			
			for (int i = 0; i < m.TextureCount; i++)
			{
				string s = string.Empty;
				for(int j = 0; j < m.Textures[i].TextureLayers; j++)
                {
					if(j == m.Textures[i].TextureLayers -1) s += $"{m.Textures[i].TextureIndicesList[j]}";
					else s += $"{m.Textures[i].TextureIndicesList[j]} / ";
				}
				lbSelNODE_ListData.Items.Add($"Texture: {i} Index: {s}");
			}

			//Try to find the EMB matching the current model...
			matchingemb = new EMB();

			if (WorkingTEXEMZ != null && WorkingTEXEMZ.Files != null)
			{
				for (int i = 0; i < WorkingTEXEMZ.Files.Count; i++)
				{
					object file = WorkingTEXEMZ.Files[i];
					if (file.GetType() == typeof(EMB))
					{
						byte[] targetemo = Utils.ChopByteArray(WorkingEMZ.FileNamesList[SelectedEMONumberInTree], 0, WorkingEMZ.FileNamesList[SelectedEMONumberInTree].Length - 1);
						byte[] targetemb = Utils.ChopByteArray(WorkingTEXEMZ.FileNamesList[i], 0, WorkingTEXEMZ.FileNamesList[i].Length - 1);

						if (Encoding.ASCII.GetString(targetemo) == Encoding.ASCII.GetString(targetemb))
						{
							matchingemb = (EMB)WorkingTEXEMZ.Files[i];
						}
					}
				}
			}
			//Clear the grid and re-populate
			modelTextureGrid.Rows.Clear();			
			if (m.Textures.Count > 0)
            {
				for (int i = 0; i < m.TextureCount; i++)
				{
					for(int j = 0; j < m.Textures[i].TextureLayers; j++)
                    {
						string[] row = { $"{i}", $"{j}", $"{m.Textures[i].TextureIndicesList[j]}", $"{m.Textures[i].Scales_UList[j].ToString("0.00")}", $"{m.Textures[i].Scales_VList[j].ToString("0.00")}" };
						modelTextureGrid.Rows.Add(row);
						//If we have a matching EMB, set the tooltip to be the indexed DDS name
						try
						{
							if (matchingemb.FileNameList != null)
							{
								modelTextureGrid.Rows[i + j].Cells[2].ToolTipText = Encoding.ASCII.GetString(matchingemb.FileNameList[m.Textures[i].TextureIndicesList[j]]).Replace('\0', ' ').Trim();
							}
						}//If the index is out of range (ie higher than number of DDSs in the emb) catch and clear the string
                        catch { modelTextureGrid.Rows[i + j].Cells[2].ToolTipText = string.Empty; }
                    }
				}
			}
			lbSelNODE_ListData.Items.Add($"Vertex Count: {m.VertexCount}");
			//lbSelNODE_ListData.Items.Add($"Face encoding mode: {m.ReadMode}");
		}

		void TreeDisplaySubModelData(SubModel m)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Bone Integers Count: {m.BoneIntegersCount}");
			lbSelNODE_ListData.Items.Add($"Daisy Chain Length: {m.DaisyChainLength}");
			lbSelNODE_ListData.Items.Add($"Daisy Chain Compression: {100 * m.DaisyChainLength / (GeometryIO.FaceIndicesFromDaisyChain(m.DaisyChain).Count * 3)}%");
			lbSelNODE_ListData.Items.Add($"Material Index: {m.MaterialIndex}");
		}

		void TreeDisplayEMMData(EMM e)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Materials Count: {e.MaterialCount}");
		}
		#endregion Tree Display Related		

		private void BtnSaveEMO_Click(object sender, EventArgs e)
		{
			//EMG OutputEMG = new EMG(); TODO probably the better idea but longer
			string filepath;
			string newEMG = "Final Ouput EMO.emo";
			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = newEMG;
			saveFileDialog1.Filter = EMOFileFilter;
			filepath = saveFileDialog1.FileName;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				filepath = saveFileDialog1.FileName;
				//WriteEMGToNewFile(filepath, true);
			}
		}

		private void BtnOpenEMZ_Click(object sender, EventArgs e)
		{
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = EMZFileFilter;
			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				string friendlyName = diagOpenOBJ.SafeFileName;
				if (filepath.Trim() != string.Empty)
				{
					try
					{
						FileStream fsSource = new FileStream(filepath, FileMode.Open, FileAccess.Read);
						byte[] bytes;
						using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }

						if (Encoding.ASCII.GetString(Utils.ChopByteArray(bytes, 0x00, 0x04)) == "#EMZ")
						{
							Console.WriteLine("File looks compressed, attempting inflation...");

							bytes = Utils.ChopByteArray(bytes, 0x10, bytes.Length - 0x10);

							List<byte> zipbytes = bytes.ToList();

							bytes = ZlibDecoder.Inflate(zipbytes).ToArray();
						}

						if (filepath.Contains("tex.emz"))
						{
							WorkingTEXEMZ = new EMZ(bytes);
							TargetTEXEMZFilePath = filepath;
							TargetTEXEMZFileName = friendlyName;
							RefreshTree(true);
							AddStatus($"Opened {filepath}");
						}
						else
						{
							WorkingEMZ = new EMZ(bytes);
							TargetEMZFilePath = filepath;
							TargetEMZFileName = friendlyName;
							FillShaderComboBox();
							FillShaderPropertiesComboBox();
							Utils.SaveShaders();
							Utils.SaveShadersProperties();
							RefreshTree(true);
							AddStatus($"Opened {filepath}");
						}
					}
					catch
					{
						MessageBox.Show("Error opening EMZ. Please confirm file is valid.", TStrings.STR_Information);
                        if (filepath.Contains("tex.emz")) { WorkingTEXEMZ = new EMZ(); }
                        else { WorkingEMZ = new EMZ(); }
                        RefreshTree(true);
                    }
				}
			}
		}

		void FillShaderComboBox()
		{
			cbShaders.Items.Clear();
			cbShaders.Items.AddRange(Utils.Shaders.Keys.ToArray());
		}

		void FillShaderPropertiesComboBox()
		{
			cbShaderProperties.Items.Clear();
			cbShaderProperties.Items.AddRange(Utils.ShadersProperties.Keys.ToArray());
		}

		void SaveEMZToFile(string Tag)
		{
			string filepath;

			EMZ targetEMZ;
			saveFileDialog1.InitialDirectory = string.Empty;

			if (Tag == "EMZ")
			{
				targetEMZ = WorkingEMZ;
				if (targetEMZ == null || targetEMZ.HEXBytes == null) return;
				saveFileDialog1.Filter = EMZFileFilter;
				saveFileDialog1.FileName = TargetEMZFileName;
			}
			else
			{
				targetEMZ = WorkingTEXEMZ;
				if (targetEMZ == null || targetEMZ.HEXBytes == null) return;
				saveFileDialog1.Filter = TEXEMZFileFilter;
				saveFileDialog1.FileName = TargetTEXEMZFileName.Replace(".tex.emz", "");
			}

			if (targetEMZ == null || targetEMZ.HEXBytes == null) return;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				filepath = saveFileDialog1.FileName;
				File.Delete(filepath); //Not very good but works
				targetEMZ.GenerateBytes();
				File.WriteAllBytes(filepath, targetEMZ.HEXBytes);
				AddStatus($"Saved {filepath}");
			}
		}

		void WriteEMZToNewFile(string filepath, string Tag)
		{

			EMZ SourceEMZ;
			if (Tag == "EMZ")
				SourceEMZ = WorkingEMZ;
			else
				SourceEMZ = WorkingTEXEMZ;
		}

		void TreeContextInjectOBJ_Click(object sender, EventArgs e)
		{
			if (ObjectLoaded && !EncodingInProgress)
			{
				InjectOBJ();
			}
			else if (!ObjectLoaded && EncodingInProgress)
            {
				MessageBox.Show("OBJ Encoding still in progress.", TStrings.STR_Error);
			}
			else if (!ObjectLoaded)
			{
				InjectObjAfterOpen = true;
				BTNOpenOBJ();
			}
		}

		void InjectOBJ()
		{
			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
			EMG targetEMG = emo.EMGs[SelectedEMGNumberInTree];
			EMG newEMG = NewEMGFromOBJ(targetEMG, false);
			emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
			emo.EMGs.Insert(SelectedEMGNumberInTree, newEMG);
			emo.GenerateBytes();
			WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
			WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);

			RefreshTree(false);
			OpenEMONode(true);
			InjectObjAfterOpen = false;
			AddStatus("OBJ " + WorkingFileName + " injected in " + Encoding.ASCII.GetString(emo.Name) + " EMG " + SelectedEMGNumberInTree);
		}

		void ExtractOtherFile()
		{
			OtherFile OF = (OtherFile)WorkingEMZ.Files[LastSelectedTreeNode.Index];
			saveFileDialog1.Filter = "";
			saveFileDialog1.FileName = Encoding.ASCII.GetString(WorkingEMZ.FileNamesList[LastSelectedTreeNode.Index]);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, OF.HEXBytes);
			}
		}

		void RawDumpCSB()
		{
			CSB csb = (CSB)WorkingEMZ.Files[LastSelectedTreeNode.Index];
			saveFileDialog1.Filter = CSBFileFilter;
			saveFileDialog1.FileName = Encoding.ASCII.GetString(csb.Name);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, csb.HEXBytes);
				AddStatus($"Extracted {Encoding.ASCII.GetString(csb.Name)}");
			}
		}

		void InjectCSB()
		{
			diagOpenOBJ.Filter = CSBFileFilter;
			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				CSB csb = (CSB)WorkingEMZ.Files[LastSelectedTreeNode.Index];
				FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
				byte[] bytes;
				using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }
				csb.HEXBytes = bytes;
				WorkingEMZ.Files.Remove(LastSelectedTreeNode.Index);
				WorkingEMZ.Files.Add(LastSelectedTreeNode.Index, csb);
				AddStatus($"Injected {diagOpenOBJ.SafeFileName} into {LastSelectedTreeNode.Text}");
				RefreshTree(false);
			}
		}

		private void InsertOBJAsNewEMGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddOBJAsNewEMG();
		}

		private void AddOBJAsNewEMG()
		{
			if (!ObjectLoaded && !EncodingInProgress)
			{
				BTNOpenOBJ();
			}
			else if(EncodingInProgress)
            {
				MessageBox.Show("OBJ Encoding still in progress.", TStrings.STR_Error);
			}
			else if (ObjectLoaded)
			{
				EMO targetEMO = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
				EMG newEMG = new EMG(targetEMO.EMGs[0].HEXBytes);
				newEMG = NewEMGFromOBJ(newEMG, true);
				int NamingListPointer = targetEMO.NamingListPointer;
				targetEMO.EMGCount += 1;
				targetEMO.EMGPointersList.Add(NamingListPointer + 0x10);
				targetEMO.EMGs.Add(newEMG);
				targetEMO.GenerateBytes();
				WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
				WorkingEMZ.Files.Add(SelectedEMONumberInTree, targetEMO);
				RefreshTree(false);
				OpenEMONode(true);
				AddStatus("OBJ " + WorkingFileName + " added as new  EMG " + (targetEMO.EMGCount - 1) + " in " + Encoding.ASCII.GetString(targetEMO.Name));
			}
		}

		private void BntEO_EMGSave_Click(object sender, EventArgs e)
		{
			if (tbEMGRootBone.Text.Trim() != string.Empty)
			{
				int newRootBone = int.Parse(tbEMGRootBone.Text.Trim());
				EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
				EMG emg = emo.EMGs[SelectedEMGNumberInTree];
				emg.RootBone = newRootBone;
				emg.GenerateBytes();
				emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
				emo.EMGs.Insert(SelectedEMGNumberInTree, emg);
				emo.GenerateBytes();
				WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
				WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);
				TreeDisplayEMGData(emg);
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			pnlEO_EMG.Visible = false;
			pnlEO_MOD.Visible = false;

			EH3D = new ElementHost();
			EH3D.Dock = DockStyle.None;
			EH3D.Location = new Point()
			{
				X = pSelectedTreeNodeData.Location.X + pnlOBJECTS.Location.X,
				Y = pSelectedTreeNodeData.Location.Y + pnlOBJECTS.Location.Y
			};
			
			EH3D.Width = pSelectedTreeNodeData.Width;
			EH3D.Height = pSelectedTreeNodeData.Height;
			EH3D.Visible = false;

			// Create the WPF UserControl.
			uc = new HostingWpfUserControlInWf.UserControl1();
			// Assign the WPF UserControl to the ElementHost control's
			// Child property.
			EH3D.Child = uc;
			// Add the ElementHost control to the form's
			// collection of child controls.
			Controls.Add(EH3D);

			#region Set up tooltips
			foreach (ToolStripItem ts in emgContext.Items)
			{
					Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
					ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in emzContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in emmContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in emoContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in embContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in ddsContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in emaContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in luaContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}
			foreach (ToolStripItem ts in csbContext.Items)
			{
                    Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
                    ts.ToolTipText = tooltip;
            }
			foreach (ToolStripItem ts in matContext.Items)
			{
				Tooltips.tooltips.TryGetValue(ts.Name, out string tooltip);
				ts.ToolTipText = tooltip;
			}

			#endregion

#if !DEBUG
				injectSMDAsEMGExperimentalToolStripMenuItem.Visible = false;
				duplicateEMGToolStripMenuItem.Visible = false;
				duplicateModelToolStripMenuItem.Visible = false;
				duplicateUSAMAN01BToolStripMenuItem.Visible = false;
				InjectEMO.Visible = false;
				rawDumpEMOAsSMDToolStripMenuItem.Visible = false;
				InjectAnimationtoolStripMenuItem1.Visible = false;
				AddAnimationtoolStripMenuItem2.Visible = false;
				dumpRefPoseToSMDToolStripMenuItem.Visible = false;
				rawDumpEMAToolStripMenuItem.Visible = false;
				injectColladaAsEMGExperimentalToolStripMenuItem.Visible = false;
				
#endif

			lbSelNODE_Title.Text = string.Empty;
			IB = new InputBox();
			Utils.ReadShaders();
			Utils.ReadShadersProperties();


		}

		private void BntEO_ModSave_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
			EMG emg = emo.EMGs[SelectedEMGNumberInTree];
			Model model = emg.Models[SelectedModelNumberInTree];

			List<EMGTexture> temptexlist = new List<EMGTexture>();
			List<int> temppointerlist = new List<int>();

			EMGTexture tex = new EMGTexture() { TextureIndicesList = new List<int>(), TextureLayers = 0, Scales_UList = new List<float>(), Scales_VList = new List<float>() };
			int lastID = 0;

            for (int i = 0; i < modelTextureGrid.Rows.Count; i++)
            {
				string sID = (string)modelTextureGrid.Rows[i].Cells[0].Value;
				string sLayers = (string)modelTextureGrid.Rows[i].Cells[1].Value;
				string sInd = (string)modelTextureGrid.Rows[i].Cells[2].Value;
				string sU = (string)modelTextureGrid.Rows[i].Cells[3].Value;
				string sV = (string)modelTextureGrid.Rows[i].Cells[4].Value;

				if (sID + sLayers + sInd + sU + sV == string.Empty) continue; //Skip empty rows

				bool IDtest = int.TryParse(sID, out int ID);
                bool Layerstest = int.TryParse(sLayers, out int Layers);
				bool Indextest = int.TryParse(sInd, out int Index);
				bool Utest = float.TryParse(sU, out float U);
				bool Vtest = float.TryParse(sV, out float V);

				
				if (IDtest && Layerstest && Indextest && Utest && Vtest && Layers < 2)
				{
					if (tex.TextureLayers == 0)
					{
						tex.TextureLayers++;
						tex.TextureIndicesList.Add(Index);
						tex.Scales_UList.Add(U);
						tex.Scales_VList.Add(V);

						lastID = ID;
					}
					else if (ID == lastID)
					{
						tex.TextureLayers++;
						tex.TextureIndicesList.Add(Index);
						tex.Scales_UList.Add(U);
						tex.Scales_VList.Add(V);
					}
					else
					{
						temptexlist.Add(tex);
						temppointerlist.Add(0);
						tex = new EMGTexture() { TextureIndicesList = new List<int>(), TextureLayers = 0, Scales_UList = new List<float>(), Scales_VList = new List<float>() };
						tex.TextureLayers++;
						tex.TextureIndicesList.Add(Index);
						tex.Scales_UList.Add(U);
						tex.Scales_VList.Add(V);

						lastID = ID;
					}
				}
				else
				{
					AddStatus("Unable to save model changes due to formatting error. Reverted to previous values.");
					if(Layers > 2) { AddStatus("Texture entry can't have more than 2 layers."); }
					TreeDisplayModelData(model);
					return;
				}

			}

			if (tex.TextureLayers > 0)
			{
				temptexlist.Add(tex);
				temppointerlist.Add(0);
			}

			model.Textures = temptexlist;
			model.TextureCount = model.Textures.Count;
			model.TexturePointersList = temppointerlist;

			emg.Models.RemoveAt(SelectedModelNumberInTree);
			emg.Models.Insert(SelectedModelNumberInTree, model);
			emg.GenerateBytes();
			emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
			emo.EMGs.Insert(SelectedEMGNumberInTree, emg);
			emo.GenerateBytes();
			WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
			WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);
			TreeDisplayModelData(model);
			AddStatus("Model changes saved.");
		}

		private void DeleteEMGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Delete EMG?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				DeleteEMG((EMO)WorkingEMZ.Files[SelectedEMONumberInTree], SelectedEMGNumberInTree);
				AddStatus("EMG deleted.");
			}
		}

		void DeleteEMG(EMO emo, int EMGIndex)
		{
			emo.EMGCount -= 1;
			if (emo.EMGCount == 1)
			{
				emo.temp_bitdepth = emo.EMGs[0].Models[0].BitDepth;
			}
			emo.EMGs.RemoveAt(EMGIndex);
			emo.EMGPointersList.RemoveAt(EMGIndex); //Doesn't matter which one we remove, they all get re-generated later
			emo.GenerateBytes();
			WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
			WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);
			RefreshTree(false);
		}

		private void SaveEncodedOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveEncodedOBJ();
		}

		private void SaveEncodedOBJToHEXToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveEncodedOBJHex();
		}

		private void BntEO_SubModSave_Click(object sender, EventArgs e)
		{
			if (tbEO_SubModName.Text.Trim() != string.Empty && Int32.TryParse(tbEO_SubModMaterial.Text.Trim(), out int smMaterial))
			{
				string value = tbEO_SubModName.Text.Trim();
				byte[] newName = Utils.MakeModelName(value);
				byte[] oldName;
				EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
				EMG emg = emo.EMGs[SelectedEMGNumberInTree];
				Model model = emg.Models[SelectedModelNumberInTree];
				SubModel sm = model.SubModels[SelectedSubModelNumberInTree];
				oldName = sm.SubModelName;
				if (Encoding.ASCII.GetString(oldName) == Encoding.ASCII.GetString(newName) && sm.MaterialIndex == smMaterial)
				{
					AddStatus("No changes applied.");
					return;
				}
				sm.MaterialIndex = smMaterial;

				if (Encoding.ASCII.GetString(oldName) == Encoding.ASCII.GetString(newName))
				{
					AddStatus("Material index updated.");
					return;
				}
				sm.SubModelName = newName;

				AddStatus("Submodel info updated.");

				EMM emm = new EMM();
				bool found = false;
				bool alreadyExists = false;
				int emmPosition = 0;
				for (int i = 0; i < WorkingEMZ.Files.Count; i++)
				{
					object file = WorkingEMZ.Files[i];
					if (file.GetType() == typeof(EMM))
					{
						emm = (EMM)file;
						byte[] subName = Utils.ChopByteArray(emm.Name, 0x00, emm.Name.Length - 1);
						byte[] targetName = Utils.ChopByteArray(emo.Name, 0x00, emo.Name.Length - 1);
						Console.WriteLine($" Comparing: {Encoding.ASCII.GetString(subName)} to {Encoding.ASCII.GetString(targetName)}");
						if (Encoding.ASCII.GetString(subName) == Encoding.ASCII.GetString(targetName))
						{
							Console.WriteLine("Matching EMM found!");
							emmPosition = i;

							foreach (Material m in emm.Materials)
							{
								if (Encoding.ASCII.GetString(m.Name) == Encoding.ASCII.GetString(newName))
								{
									alreadyExists = true;
									break;
								}
							}
							for (int j = 0; j < emm.MaterialCount; j++)
							{
								if (alreadyExists) break;
								Console.WriteLine($" Comparing Material name to old SubModel Name: {Encoding.ASCII.GetString(emm.Materials[j].Name)} to {Encoding.ASCII.GetString(oldName)}");
								if (Encoding.ASCII.GetString(emm.Materials[j].Name) == Encoding.ASCII.GetString(oldName))
								{
									Console.WriteLine("Match material found!");
									emm.Materials.Add(new Material(emm.Materials[j].HEXBytes,newName));
									Console.WriteLine("New Material Name: " + Encoding.ASCII.GetString(newName));
									found = true;
									break;
								}
							}
							if (!alreadyExists)
							{
								emm.MaterialCount += 1;
								emm.MaterialPointersList.Add(0);
							}
							break;
						}
					}
					else
					{
						Console.WriteLine("Found nothing");
					}
				}
				if (!found & !alreadyExists) Console.Write("Did not find material");
				if (alreadyExists) Console.Write("Found existing material");
				if (found)
				{
					emm.GenerateBytes();
					WorkingEMZ.Files.Remove(emmPosition);
					WorkingEMZ.Files.Add(emmPosition, emm);
				}

				model.SubModels.RemoveAt(SelectedSubModelNumberInTree);
				model.SubModels.Insert(SelectedSubModelNumberInTree, sm);

				emg.Models.RemoveAt(SelectedModelNumberInTree);
				emg.Models.Insert(SelectedModelNumberInTree, model);
				emg.GenerateBytes();

				emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
				emo.EMGs.Insert(SelectedEMGNumberInTree, emg);
				if (found) emo.NumberEMMMaterials += 1;
				emo.GenerateBytes();

				WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
				WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);

				lbSelNODE_Title.Text = Encoding.ASCII.GetString(sm.SubModelName);

				RefreshTree(false);
				TreeDisplaySubModelData(sm);
			}
		}

		async void button1_Click(object sender, EventArgs e)
		{
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = SMDFileFilter;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				if (filepath.Trim() != string.Empty)
				{
					LastOpenFolder = Path.GetDirectoryName(filepath);
					WorkingFileName = diagOpenOBJ.SafeFileName;
					SMDModel someSMD = new SMDModel();
					await Task.Run(() => { someSMD = ReadSMD(filepath); });

					EMA testEMA = SimpleEMAFromSMD(someSMD);

					WorkingEMZ.Files.Add(WorkingEMZ.Files.Count, testEMA);
					WorkingEMZ.FileNamesList.Add(new byte[11] { 0x54, 0x52, 0x4E, 0x5F, 0x44, 0x52, 0x4D, 0x2E, 0x65, 0x6D, 0x61 });
					WorkingEMZ.FileLengthsList.Add(testEMA.HEXBytes.Length);
					WorkingEMZ.FileNamePointersList.Add(0x00);
					WorkingEMZ.FilePointersList.Add(0x00);
					WorkingEMZ.NumberOfFiles++;

					RefreshTree(false);
				}
			}
		}

		SMDModel ReadSMD(string smd)
		{
			ConsoleWrite(ConsoleLineSpacer);
			ConsoleWrite($"Opening SMD file:  {smd}");
			ConsoleWrite(" ");
			string[] lines = File.ReadAllLines(smd);

			//Prepare Input SMD Structure
			SMDModel WorkingSMD = new SMDModel();
			WorkingSMD.Verts = new List<Vertex>();
			WorkingSMD.Nodes = new List<SMDNode>();
			WorkingSMD.Frames = new List<SMDFrame>();
			WorkingSMD.FaceIndices = new List<int[]>();
			WorkingSMD.MaterialDictionary = new Dictionary<string, int>();
			WorkingSMD.MaterialNames = new List<byte[]>();

			//Declare Frame
			SMDFrame WorkingFrame = new SMDFrame();

			//Declare Vertex for use later
			Vertex WorkingVert = new Vertex();

			//Replace tabs with spaces, strip quote marks, and strip in-line comments, which can start with # OR ;
			char[] delim = { '#', ';' };
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Replace(@"""", "");
				lines[i] = lines[i].Replace("\t", " ");
				lines[i] = lines[i].Replace("  ", " ");
				lines[i] = lines[i].Trim().Split(delim)[0];
			}

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				if (line.StartsWith("nodes")) //Node block starts here
				{
					while (!lines[i + 1].StartsWith("end")) //Read lines until the next line is the end-of-block marker
					{
						i++;
						//If we get to the end of the file and the block hasn't ended, break and error
						if (i == lines.Length) { ConsoleWrite("Invalid SMD - No end to Node block."); break; }
						//Node names are in "quotes" so strip them out before reading
						string[] node = lines[i].Trim().Split(' ');

						SMDNode WorkingNode = new SMDNode();
						//Id, Name, Parent node
						WorkingNode.ID = int.Parse(node[0]);
						WorkingNode.Name = Encoding.ASCII.GetBytes(node[1]);
						WorkingNode.Parent = int.Parse(node[2]);

						//Done, add it to the list
						WorkingSMD.Nodes.Add(WorkingNode);
					}
				}

				if (line.StartsWith("skeleton")) //Skeleton block starts here
				{
					while (lines[i + 1] != "end") //Read lines until the next line is the end-of-block marker
					{
						string[] time;

						i++;
						//If we get to the end of the file and the block hasn't ended, break and error
						if (i == lines.Length) { ConsoleWrite("Invalid SMD - No end to Skeleton block."); break; }
						if (lines[i].StartsWith("time"))
						{
							//Start a new frame/re-initialise lists
							WorkingFrame = new SMDFrame();
							WorkingFrame.NodeIDs = new List<int>();
							WorkingFrame.traX = new List<float>();
							WorkingFrame.traY = new List<float>();
							WorkingFrame.traZ = new List<float>();
							WorkingFrame.rotX = new List<float>();
							WorkingFrame.rotY = new List<float>();
							WorkingFrame.rotZ = new List<float>();

							time = lines[i].Trim().Split(' ');
							WorkingFrame.Time = int.Parse(time[1]);
						}
						else
						{
							string[] nodeline = lines[i].Trim().Split(' ');
							WorkingFrame.NodeIDs.Add(int.Parse(nodeline[0]));
							WorkingFrame.traX.Add(float.Parse(nodeline[1]));
							WorkingFrame.traY.Add(float.Parse(nodeline[2]));
							WorkingFrame.traZ.Add(float.Parse(nodeline[3]));
							WorkingFrame.rotX.Add(float.Parse(nodeline[4]));
							WorkingFrame.rotY.Add(float.Parse(nodeline[5]));
							WorkingFrame.rotZ.Add(float.Parse(nodeline[6]));

							//If the next line is the start of a new frame or the end of the skeleton block, push the current frame to the list
							if (lines[i + 1].StartsWith("time") || lines[i + 1].StartsWith("end"))
							{
								WorkingSMD.Frames.Add(WorkingFrame);
							}
						}
					}

					if (WorkingSMD.Frames.Count == 1)
					{
						WorkingSMD.bRefPose = true;
					}
				}

				if (line.StartsWith("triangles")) //Face/vertex block starts here
				{
					while (lines[i + 1] != "end") //Read lines until the next line is the end-of-block marker
					{
						string[] vert;

						i++;

						//Read in material name and store it in the main list, so Face i uses Material i. TODO wire up a way to link MaterialName to DDS/EMM contents?
						WorkingSMD.MaterialNames.Add(Encoding.ASCII.GetBytes(lines[i]));

						if (!WorkingSMD.MaterialDictionary.TryGetValue(lines[i], out _))
						{
							WorkingSMD.MaterialDictionary.Add(lines[i], WorkingSMD.MaterialDictionary.Count);
						}

						int[] tempFaceArray = new int[3];

						//Start reading faces, looping over the number of verts in the face
						for (int j = 0; j < 3; j++)
						{
							i++;
							vert = lines[i].Trim().Split(' ');
							//Initialise the new vertex
							WorkingVert = new Vertex();
							WorkingVert.BoneIDs = new List<int>();
							WorkingVert.BoneWeights = new List<float>();

							//Minimum vertex content is ParentBone + XYZ + nXYX + UV, throw error and break if it's not long enough
							if (vert.Length < 9) { ConsoleWrite("Invalid Vertex - Not enough values."); break; }

							WorkingVert.ParentBone = int.Parse(vert[0]);
							WorkingVert.X = float.Parse(vert[1]);
							WorkingVert.Y = float.Parse(vert[2]);
							WorkingVert.Z = float.Parse(vert[3]);
							WorkingVert.nX = float.Parse(vert[4]);
							WorkingVert.nY = float.Parse(vert[5]);
							WorkingVert.nZ = float.Parse(vert[6]);
							WorkingVert.U = float.Parse(vert[7]);
							WorkingVert.V = float.Parse(vert[8]);

							//If it's longer, we've got bone weight data.
							//Bone weight is in the format Num. Influencing Bones + <BoneID> + <BoneWeight>
							//Where ID and Weight are repeated for however many bones. If the Weights don't add up to 1, remainder is applied to Parent
							if (vert.Length > 9)
							{
								WorkingVert.BoneCount = int.Parse(vert[9]);

								//Check we've got enough values, error and break otherwise
								if (vert.Length < 9 + 2 * WorkingVert.BoneCount) { ConsoleWrite("Invalid Weights - Bone count/values mismatch."); break; }
								//Loop to get all the <BoneID> + <BoneWeight> values...
								for (int k = 0; k < WorkingVert.BoneCount; k++)
								{
									WorkingVert.BoneIDs.Add(int.Parse(vert[10 + 2 * k]));
									WorkingVert.BoneWeights.Add(float.Parse(vert[11 + 2 * k]));
								}
							}
							//Compare current vert to each previous vert to generate a unique vert list
							int match = -1;

							//TODO re-enable vert splits
							//for (int k = 0; k < WorkingSMD.Verts.Count; k++)
							//{
							//	if (WorkingSMD.Verts[k].VertexEquals(WorkingVert)) { match = k; }
							//}
							if (match == -1)
							{
								tempFaceArray[j] = WorkingSMD.Verts.Count; //If we didn't find a match, our index is our current number of verts
								WorkingSMD.Verts.Add(WorkingVert); //Add the vert to the unique list
							}
							else tempFaceArray[j] = match; //If we found a match, use the index of the match
						}
						//Finished face, add it to the model and move the loop forward
						WorkingSMD.FaceIndices.Add(tempFaceArray);
						if (i == lines.Length) { ConsoleWrite("Invalid SMD - No end to face/vertex block."); break; }
					}
				}
			}

			return WorkingSMD;
		}

		EMA SimpleEMAFromSMD(SMDModel model)
		{
			EMA WorkingEMA = new EMA()
			{
				Animations = new List<Animation>(),
				AnimationPointersList = new List<int>(),
				AnimationCount = 1
			};
			WorkingEMA.AnimationPointersList.Add(0);

			Animation WorkingAnimation = new Animation()
			{
				Duration = model.Frames.Count,
				Name = new byte[] { 0x45, 0x4C, 0x56, 0x5F, 0x4D, 0x41, 0x4E, 0x30, 0x31, 0x5F, 0x30, 0x30, 0x30 },
				ValuesList = new List<float>()
			};

			Dictionary<float, int> ValueDict = new Dictionary<float, int>();

			//Populate the float dictionary
			for (int i = 0; i < model.Nodes.Count; i++)
			{
				float CumulativeRot_X = 0;
				float CumulativeRot_Y = 0;
				float CumulativeRot_Z = 0;

				for (int k = 0; k < model.Frames.Count; k++)
				{
					float ActualRot_X = model.Frames[k].rotX[i] - CumulativeRot_X;
					CumulativeRot_X = CumulativeRot_X + ActualRot_X;

					float ActualRot_Y = model.Frames[k].rotY[i] - CumulativeRot_Y;
					CumulativeRot_Y = CumulativeRot_Y + ActualRot_Y;

					float ActualRot_Z = model.Frames[k].rotZ[i] - CumulativeRot_Z;
					CumulativeRot_Z = CumulativeRot_Z + ActualRot_Z;

					float Rad2Deg = Convert.ToSingle(180 / Math.PI);
					int dumVal; //TryGetValue outputs the dictionary result, but we don't care right now
					if (!ValueDict.TryGetValue(model.Frames[k].traX[i], out dumVal)) { ValueDict.Add(model.Frames[k].traX[i], ValueDict.Count); }
					if (!ValueDict.TryGetValue(model.Frames[k].traY[i], out dumVal)) { ValueDict.Add(model.Frames[k].traY[i], ValueDict.Count); }
					if (!ValueDict.TryGetValue(model.Frames[k].traZ[i], out dumVal)) { ValueDict.Add(model.Frames[k].traZ[i], ValueDict.Count); }
					if (!ValueDict.TryGetValue(Rad2Deg * model.Frames[k].rotX[i], out dumVal)) { ValueDict.Add(Rad2Deg * model.Frames[k].rotX[i], ValueDict.Count); }
					if (!ValueDict.TryGetValue(Rad2Deg * model.Frames[k].rotY[i], out dumVal)) { ValueDict.Add(Rad2Deg * model.Frames[k].rotY[i], ValueDict.Count); }
					if (!ValueDict.TryGetValue(Rad2Deg * model.Frames[k].rotZ[i], out dumVal)) { ValueDict.Add(Rad2Deg * model.Frames[k].rotZ[i], ValueDict.Count); }

					//if (!ValueDict.TryGetValue(Rad2Deg * ActualRot_X, out dumVal)) { ValueDict.Add(Rad2Deg * ActualRot_X, ValueDict.Count); }
					//if (!ValueDict.TryGetValue(Rad2Deg * ActualRot_Y, out dumVal)) { ValueDict.Add(Rad2Deg * ActualRot_Y, ValueDict.Count); }
					//if (!ValueDict.TryGetValue(Rad2Deg * ActualRot_Z, out dumVal)) { ValueDict.Add(Rad2Deg * ActualRot_Z, ValueDict.Count); }
				}
			}
			WorkingAnimation.ValueCount = ValueDict.Count;
			//Populate the Value list from the dictionary
			for (int i = 0; i < ValueDict.Count; i++)
			{
				WorkingAnimation.ValuesList.Add(ValueDict.ElementAt(i).Key);
			}

			//Set flags for later use - byte IndexValues only have 14 "useable" bits, so max is 3FFF
			int KTimes = 0x00; if (WorkingAnimation.Duration > 0xFF) { KTimes = 0x20; }
			int IndType = 0x00; if (WorkingAnimation.ValueCount > 0x3FFF) { IndType = 0x40; }
			int Absolute = 0x00; //SMD animations ONLY support relative animation, but chuck this here for later

			//SMD contains translation and rotation, so up to 6 tracks per node
			WorkingAnimation.CmdTrackCount = model.Nodes.Count * 6;
			WorkingAnimation.CMDTracks = new List<CMDTrack>();
			WorkingAnimation.CmdTrackPointersList = new List<int>();

			//Loop bone list
			for (int i = 0; i < model.Nodes.Count; i++)
			{
				//j = transform axis
				for (int j = 0; j < 3; j++)
				{
					CMDTrack WorkingCMD = new CMDTrack();
					WorkingCMD.StepsList = new List<int>();
					WorkingCMD.IndicesList = new List<int>();
					WorkingCMD.BoneID = i;
					WorkingCMD.TransformType = 0;

					WorkingCMD.BitFlag = Convert.ToByte(j + KTimes + IndType + Absolute);
					WorkingCMD.StepCount = model.Frames.Count;

					//k = keyframe count
					for (int k = 0; k < model.Frames.Count; k++)
					{
						WorkingCMD.StepsList.Add(k);
						if (j == 0) { WorkingCMD.IndicesList.Add(ValueDict[model.Frames[k].traX[i]]); }
						if (j == 1) { WorkingCMD.IndicesList.Add(ValueDict[model.Frames[k].traY[i]]); }
						if (j == 2) { WorkingCMD.IndicesList.Add(ValueDict[model.Frames[k].traZ[i]]); }
					}
					WorkingAnimation.CMDTracks.Add(WorkingCMD);
					WorkingAnimation.CmdTrackPointersList.Add(0);
				}
			}

			for (int i = 0; i < model.Nodes.Count; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					CMDTrack WorkingCMD = new CMDTrack();
					WorkingCMD = new CMDTrack();
					WorkingCMD.StepsList = new List<int>();
					WorkingCMD.IndicesList = new List<int>();
					WorkingCMD.BoneID = i;
					WorkingCMD.TransformType = 1;
					WorkingCMD.BitFlag = Convert.ToByte(j + KTimes + IndType + Absolute);
					WorkingCMD.StepCount = model.Frames.Count;

					//k = keyframe count
					for (int k = 0; k < model.Frames.Count; k++)
					{
						float Rad2Deg = Convert.ToSingle(180 / Math.PI);

						WorkingCMD.StepsList.Add(k);
						if (j == 0)
						{
							float valueDeg = Rad2Deg * model.Frames[k].rotX[i];
							WorkingCMD.IndicesList.Add(ValueDict[valueDeg]);
						}
						if (j == 1)
						{
							float valueDeg = Rad2Deg * model.Frames[k].rotY[i];
							WorkingCMD.IndicesList.Add(ValueDict[valueDeg]);
						}
						if (j == 2)
						{
							float valueDeg = Rad2Deg * model.Frames[k].rotZ[i];
							WorkingCMD.IndicesList.Add(ValueDict[valueDeg]);
						}
					}

					WorkingAnimation.CMDTracks.Add(WorkingCMD);
					WorkingAnimation.CmdTrackPointersList.Add(0);
				}
			}

			WorkingEMA.Animations.Add(WorkingAnimation);

			WorkingEMA.Skeleton = SkeletonFromSMD(model);

			WorkingEMA.GenerateBytes();

			return WorkingEMA;
		}

		private List<string> WriteSMDNodesFromSkeleton(Skeleton skel)
		{
			List<string> skeldata = new List<string>() { "nodes" };

			for (int i = 0; i < skel.Nodes.Count; i++)
			{

				string pname = string.Empty;

				if (skel.Nodes[i].Parent >= 0)
				{
					pname = Encoding.ASCII.GetString(skel.NodeNames[skel.Nodes[i].Parent]);
				}

				skeldata.Add($"{i} \"{Encoding.ASCII.GetString(skel.NodeNames[i])}\" {skel.Nodes[i].Parent} #{pname}");
			}
			skeldata.Add("end");

			skeldata.Add("skeleton");
			skeldata.Add("time 0");

			for (int i = 0; i < skel.Nodes.Count; i++)
			{
				Matrix4x4 transposedMatrix = Matrix4x4.Transpose(skel.Nodes[i].NodeMatrix);

				float tx, ty, tz, rx, ry, rz;

				//Utils.DecomposeMatrixXYZ(skel.Nodes[i].NodeMatrix, out tx, out ty, out tz, out rx, out ry, out rz, out sx, out sy, out sz);
				Utils.DecomposeMatrixNaive(skel.Nodes[i].NodeMatrix, out tx, out ty, out tz, out rx, out ry, out rz);

				Utils.LeftHandToEulerAnglesXYZ(skel.Nodes[i].NodeMatrix, out rx, out ry, out rz);

				skeldata.Add($"{i} {String.Format("{0:0.000000}", -tx)} {String.Format("{0:0.000000}", tz)} {String.Format("{0:0.000000}", ty)} {String.Format("{0:0.000000}", -rx)} {String.Format("{0:0.000000}", rz)} {String.Format("{0:0.000000}", ry)}");

				//skeldata.Add($"{i} {String.Format("{0:0.000000}", tx)} {String.Format("{0:0.000000}", ty)} {String.Format("{0:0.000000}", tz)} {String.Format("{0:0.000000}", rx)} {String.Format("{0:0.000000}", ry)} {String.Format("{0:0.000000}", rz)} #{sx} {sy} {sz}");

			}
			skeldata.Add("end");

			return skeldata;
		}

		private void EMAAnimationtoSMD(EMA ema, int index)
		{
            List<string> SMDData = new List<string> { "version 1" };

            SMDData.AddRange(WriteSMDNodesFromSkeleton(ema.Skeleton));
		}

		private void InitialPoseFromEMA(EMA ema)
		{
			List<string> SMDData = new List<string>();
			Skeleton skel = ema.Skeleton;

			SMDData.Add("version 1");

			SMDData.Add("nodes");
			for (int i = 0; i < skel.Nodes.Count; i++)
			{
				string pname = string.Empty;

				if (skel.Nodes[i].Parent >= 0)
				{
					pname = Encoding.ASCII.GetString(skel.NodeNames[skel.Nodes[i].Parent]);
				}

				SMDData.Add($"{i} \"{Encoding.ASCII.GetString(skel.NodeNames[i])}\" {skel.Nodes[i].Parent} #{pname}");
			}
			SMDData.Add("end");
			SMDData.Add("skeleton");
			SMDData.Add("time 0");

			//Simplified animation extractor for calculating reference poses
			Animation anim = ema.Animations[0];

			for (int i = 0; i < ema.Skeleton.Nodes.Count; i++)
			{
				Matrix4x4 NoT = Matrix4x4.CreateTranslation(0, 0, 0);
				Matrix4x4[] NodeCMDMatrices = new Matrix4x4[9] { NoT, NoT, NoT, NoT, NoT, NoT, NoT, NoT, NoT };
				float[] NodeCMDValues = new float[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

				Matrix4x4 AnimMatrix;

				foreach (CMDTrack cmd in anim.CMDTracks)
				{
					if (cmd.BoneID == i)
					{
						int tType = (cmd.TransformType * 3) + (cmd.BitFlag & 0x03);

						int MaskedIndex = cmd.IndicesList[0] & 0b0011111111111111;

						float value = anim.ValuesList[MaskedIndex];

						if (tType > 2 && tType < 6) //If it's a rotation value, convert to radians
						{
							value = value * Convert.ToSingle(Math.PI) / 180f;
						}

						NodeCMDValues[tType] = value;

						switch (tType)
						{
							case 0:
								NodeCMDMatrices[0] = Matrix4x4.CreateTranslation(value, 0, 0);
								break;
							case 1:
								NodeCMDMatrices[1] = Matrix4x4.CreateTranslation(0, value, 0);
								break;
							case 2:
								NodeCMDMatrices[2] = Matrix4x4.CreateTranslation(0, 0, value);
								break;

							case 3:
								NodeCMDMatrices[3] = Matrix4x4.CreateRotationX(value);
								break;
							case 4:
								NodeCMDMatrices[4] = Matrix4x4.CreateRotationY(value);
								break;
							case 5:
								NodeCMDMatrices[5] = Matrix4x4.CreateRotationZ(value);
								break;

							case 6:
								NodeCMDMatrices[6] = Matrix4x4.CreateScale(value, 0, 0);
								break;
							case 7:
								NodeCMDMatrices[7] = Matrix4x4.CreateScale(0, value, 0);
								break;
							case 8:
								NodeCMDMatrices[8] = Matrix4x4.CreateScale(0, 0, value);
								break;

							default:
								break;
						}
					}
				}

				AnimMatrix = NodeCMDMatrices[6] * NodeCMDMatrices[7] * NodeCMDMatrices[8] * NodeCMDMatrices[3] * NodeCMDMatrices[4] * NodeCMDMatrices[5] * NodeCMDMatrices[0] * NodeCMDMatrices[1] * NodeCMDMatrices[2];

				Matrix4x4 tempMatrix = skel.Nodes[i].NodeMatrix;// * AnimMatrix;

				float tx, ty, tz, rx, ry, rz, sx, sy, sz;

				Utils.DecomposeMatrixXYZ(tempMatrix, out tx, out ty, out tz, out rx, out ry, out rz, out sx, out sy, out sz);

				SMDData.Add($"{i} {String.Format("{0:0.000000}", tx)} {String.Format("{0:0.000000}", ty)} {String.Format("{0:0.000000}", tz)} {String.Format("{0:0.000000}", rx)} {String.Format("{0:0.000000}", ry)} {String.Format("{0:0.000000}", rz)}");

			}

			SMDData.Add("end");

			File.WriteAllLines($"{Encoding.ASCII.GetString(ema.Name)}.smd", SMDData.Cast<string>().ToArray());
		}

		private void EMOtoRefSMD(EMO emo)
		{
			string filepath;
			string newSMD = $"{Encoding.ASCII.GetString(emo.Name)}.smd";

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = newSMD;
			saveFileDialog1.Filter = SMDFileFilter;     //"Wavefront (.obj)|*.obj";
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				filepath = saveFileDialog1.FileName;
				if (filepath.Trim() != "")
				{
					List<string> SMDData = new List<string>() { "version 1" };

					SMDData.AddRange(WriteSMDNodesFromSkeleton(emo.Skeleton));
					//SMDData.AddRange(WriteSMDNodesFromSkeleton(Anim.DuplicateSkeleton(emo.Skeleton)));

					SMDData.Add("triangles");
					for (int i = 0; i < emo.EMGs.Count; i++)
					{
						for (int j = 0; j < emo.EMGs[i].Models.Count; j++)
						{
							Model wMod = emo.EMGs[i].Models[j];

							for (int k = 0; k < emo.EMGs[i].Models[j].SubModels.Count; k++)
							{
								SubModel wSM = wMod.SubModels[k];
								List<int[]> smFaces = GeometryIO.FaceIndicesFromDaisyChain(wSM.DaisyChain);

								for (int f = 0; f < smFaces.Count; f++)
								{
									SMDData.Add(Encoding.ASCII.GetString(wSM.SubModelName).Replace(Convert.ToChar(0x00), ' ')); //Use as material name

									float weightTotal = 0;

									//TODO crash - exporting an EMG to SMD crashes if the EMG was created from an SMD import
									string v1 = $"{emo.EMGs[i].RootBone} ";
									v1 += $"{wMod.VertexData[smFaces[f][0]].X} {wMod.VertexData[smFaces[f][0]].Y} {wMod.VertexData[smFaces[f][0]].Z} ";
									v1 += $"{wMod.VertexData[smFaces[f][0]].nX} {wMod.VertexData[smFaces[f][0]].nY} {wMod.VertexData[smFaces[f][0]].nZ} ";
									v1 += $"{wMod.VertexData[smFaces[f][0]].U} {wMod.VertexData[smFaces[f][0]].V} ";
									if (wMod.VertexData[smFaces[f][0]].BoneIDs != null && wMod.VertexData[smFaces[f][0]].BoneIDs.Count > 0)
									{
										v1 += "4 ";
										v1 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[0]]} {wMod.VertexData[smFaces[f][0]].BoneWeights[0]} ";
										v1 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[1]]} {wMod.VertexData[smFaces[f][0]].BoneWeights[1]} ";
										v1 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[2]]} {wMod.VertexData[smFaces[f][0]].BoneWeights[2]} ";
										weightTotal += wMod.VertexData[smFaces[f][0]].BoneWeights[0];
										weightTotal += wMod.VertexData[smFaces[f][0]].BoneWeights[1];
										weightTotal += wMod.VertexData[smFaces[f][0]].BoneWeights[2];
										v1 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[3]]} {Math.Max(1 - weightTotal, 0)} ";
									}

									SMDData.Add(v1);
									weightTotal = 0;

									string v2 = $"{emo.EMGs[i].RootBone} ";
									v2 += $"{wMod.VertexData[smFaces[f][1]].X} {wMod.VertexData[smFaces[f][1]].Y} {wMod.VertexData[smFaces[f][1]].Z} ";
									v2 += $"{wMod.VertexData[smFaces[f][1]].nX} {wMod.VertexData[smFaces[f][1]].nY} {wMod.VertexData[smFaces[f][1]].nZ} ";
									v2 += $"{wMod.VertexData[smFaces[f][1]].U} {wMod.VertexData[smFaces[f][1]].V} ";
									if (wMod.VertexData[smFaces[f][1]].BoneIDs != null && wMod.VertexData[smFaces[f][1]].BoneIDs.Count > 0)
									{
										v2 += "4 ";
										v2 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[0]]} {wMod.VertexData[smFaces[f][1]].BoneWeights[0]} ";
										v2 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[1]]} {wMod.VertexData[smFaces[f][1]].BoneWeights[1]} ";
										v2 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[2]]} {wMod.VertexData[smFaces[f][1]].BoneWeights[2]} ";
										weightTotal += wMod.VertexData[smFaces[f][1]].BoneWeights[0];
										weightTotal += wMod.VertexData[smFaces[f][1]].BoneWeights[1];
										weightTotal += wMod.VertexData[smFaces[f][1]].BoneWeights[2];
										v2 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[3]]} {Math.Max(1 - weightTotal, 0)} ";
									}

									SMDData.Add(v2);
									weightTotal = 0;

									string v3 = $"{emo.EMGs[i].RootBone} ";
									v3 += $"{wMod.VertexData[smFaces[f][2]].X} {wMod.VertexData[smFaces[f][2]].Y} {wMod.VertexData[smFaces[f][2]].Z} ";
									v3 += $"{wMod.VertexData[smFaces[f][2]].nX} {wMod.VertexData[smFaces[f][2]].nY} {wMod.VertexData[smFaces[f][2]].nZ} ";
									v3 += $"{wMod.VertexData[smFaces[f][2]].U} {wMod.VertexData[smFaces[f][2]].V} ";
									if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
									{
										v3 += "4 ";
										v3 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[0]]} {wMod.VertexData[smFaces[f][2]].BoneWeights[0]} ";
										v3 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[1]]} {wMod.VertexData[smFaces[f][2]].BoneWeights[1]} ";
										v3 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[2]]} {wMod.VertexData[smFaces[f][2]].BoneWeights[2]} ";
										weightTotal += wMod.VertexData[smFaces[f][2]].BoneWeights[0];
										weightTotal += wMod.VertexData[smFaces[f][2]].BoneWeights[1];
										weightTotal += wMod.VertexData[smFaces[f][2]].BoneWeights[2];
										v3 += $"{wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[3]]} {Math.Max(1 - weightTotal, 0)} ";
									}

									SMDData.Add(v3);
								}
							}
						}
					}

					SMDData.Add("end");

					//File.WriteAllLines($"{Encoding.ASCII.GetString(emo.Name)}.smd", SMDData.Cast<string>().ToArray());
					File.WriteAllLines(filepath, SMDData.Cast<string>().ToArray());
				}
			}


		}

		EMO EMOFromSMD(SMDModel model)
		{
			EMO nEMO = new EMO();

			nEMO.EMGCount = 0x01;
			nEMO.EMGs = new List<EMG> { NewEMGFromSMD(model) };
			nEMO.EMGPointersList = new List<int> { 0x00 };

			nEMO.Name = new byte[11] { 0x54, 0x52, 0x4E, 0x5F, 0x44, 0x52, 0x4D, 0x2E, 0x65, 0x6D, 0x6F };

			nEMO.NamesList = new List<byte[]>();
			nEMO.NamingPointersList = new List<int>();

			foreach (string s in model.MaterialDictionary.Keys)
			{
				nEMO.NamesList.Add(Encoding.ASCII.GetBytes(s));
				nEMO.NamingPointersList.Add(0x00);
			}

			nEMO.NamingListPointer = 0x00;

			nEMO.NumberEMMMaterials = model.MaterialDictionary.Count;

			nEMO.SkeletonPointer = 0x00;

			nEMO.Skeleton = SkeletonFromSMD(model);

			nEMO.GenerateBytes();

			return nEMO;
		}

		EMG NewEMGFromSMD(SMDModel smd)
		{
			EMG nEMG = new EMG();

			//nEMG.RootBone = smd.Nodes.Count;
			nEMG.RootBone = 0x01;
			nEMG.ModelCount = 0x01;
			nEMG.ModelPointersList = new List<int> { 0x00 };
			nEMG.Models = new List<Model>();

			Model nModel = new Model();
			nModel.BitFlag = 0x0247;

			//Each unique material in the smd needs to be a single sub-model so it can use its own texture
			nModel.TextureCount = smd.MaterialDictionary.Count;

			nModel.TextureListPointer = 0x00;
			nModel.TexturePointersList = new List<int>();
			nModel.Textures = new List<EMGTexture>();
			nModel.VertexCount = smd.Verts.Count;
			nModel.BitDepth = 0x34;
			nModel.VertexListPointer = 0x00;
			nModel.VertexData = smd.Verts;
			nModel.ReadMode = 0x01;
			nModel.SubModelsCount = smd.MaterialDictionary.Count;
			nModel.SubModelsListPointer = 0x00;
			nModel.SubModelPointersList = new List<int>();
			nModel.SubModels = new List<SubModel>();
			nModel.CullData = new byte[] { 0x00, 0x0A, 0x1B, 0x3C, 0xC3, 0xA4, 0x9E, 0x40,
										   0x80, 0x89, 0x27, 0x3E, 0xF6, 0x79, 0x3E, 0x41,
										   0x50, 0x94, 0xA1, 0xC0, 0xFD, 0x14, 0x9D, 0xBF,
										   0xEE, 0x94, 0x0A, 0xC1, 0x43, 0xB9, 0x0D, 0x43,
										   0x5A, 0x2F, 0xA2, 0x40, 0x63, 0x47, 0x32, 0x41,
										   0x3A, 0xD1, 0x0F, 0x41, 0xF6, 0x79, 0xBE, 0x41 };
			for (int i = 0; i < nModel.TextureCount; i++)
			{
				nModel.TexturePointersList.Add(0x00);

				EMGTexture tex = new EMGTexture();
				tex.TextureIndicesList = new List<int>();
				tex.Scales_UList = new List<float>();
				tex.Scales_VList = new List<float>();

				tex.TextureLayers = 1;
				tex.TextureIndicesList.Add(0);
				tex.Scales_UList.Add(1);
				tex.Scales_VList.Add(1);

				nModel.Textures.Add(tex);
			}
			for (int i = 0; i < nModel.SubModelsCount; i++)
			{
				nModel.SubModelPointersList.Add(0x00);

				List<int[]> SubmodelFaceIndices = new List<int[]>();

				for (int j = 0; j < smd.FaceIndices.Count; j++)
				{
					//Check if our current face indice j belongs to submodel i using the dictionary
					if (smd.MaterialDictionary[Encoding.ASCII.GetString(smd.MaterialNames[j])] == i)
					{
						SubmodelFaceIndices.Add(smd.FaceIndices[j]);
					}
				}

				//Submodel indices list complete

				//BONE INTEGERS???
				//TODO something better. At the moment each submodel just adds the entire skeleton to its integer list
				//Fine for small models but...

				SubModel sm = new SubModel();
				sm.MysteryFloats = new byte[] { 0x9E, 0xDF, 0xDD, 0xBC, 0xC5, 0x2A, 0x3B, 0x3E,
											  0xA7, 0x68, 0x3F, 0x3C, 0x00, 0x00, 0x80, 0x3F };
				sm.MaterialIndex = i;

				sm.DaisyChain = DaisyChainFromIndices(new List<int[]>(SubmodelFaceIndices)).ToArray();
				sm.DaisyChainLength = sm.DaisyChain.Length;

				sm.BoneIntegersCount = smd.Nodes.Count;
				sm.BoneIntegersList = new List<int>();

				for (int j = 0; j < smd.Nodes.Count; j++)
				{
					sm.BoneIntegersList.Add(j);
				}

				sm.SubModelName = Utils.MakeModelName(smd.MaterialDictionary.ElementAt(i).Key);

				nModel.SubModels.Add(sm);
			}

			nEMG.Models.Add(nModel);

			nEMG.GenerateBytes();

			return nEMG;
		}

		bool ValidateSMD()
		{
			//TODO validate SMD
			return true;
		}

		private void SaveEMZToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveEMZToFile((string)tvTree.Nodes[SelectedEMZNumberInTree].Tag);
		}

		private void CloseEMZToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if ((string)tvTree.Nodes[SelectedEMZNumberInTree].Tag == "EMZ")
			{
				WorkingEMZ = new EMZ();
				AddStatus(".EMZ closed.");
			}
			if ((string)tvTree.Nodes[SelectedEMZNumberInTree].Tag == "TEX")
			{
				WorkingTEXEMZ = new EMZ();
				AddStatus(".TEX.EMZ closed.");
			}
			tvTree.Nodes.RemoveAt(SelectedEMZNumberInTree);
		}

		private void deleteMaterialToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Are you sure", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				EMM emm = (EMM)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];
				emm.MaterialCount -= 1;
				emm.Materials.RemoveAt(LastSelectedTreeNode.Index);
				emm.MaterialPointersList.RemoveAt(0);
				emm.GenerateBytes();
				WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
				WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, emm);
				RefreshTree(false);
				AddStatus("Material '" + LastSelectedTreeNode.Text.Replace("\0", "") + "' deleted from '" + Encoding.ASCII.GetString(emm.Name).Replace("\0", "") + "'");
			}
		}

		private void injectDDSToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			try
			{
				EMB emb = (EMB)WorkingTEXEMZ.Files[LastSelectedTreeNode.Parent.Index];
				DDS dds = emb.DDSFiles[LastSelectedTreeNode.Index];
				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;
				diagOpenOBJ.Filter = DDSFileFilter;
				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{
					//DAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMNNNNNNNNNNNN!!!!!
					//ImageEngineImage imageFile = new ImageEngineImage(diagOpenOBJ.FileName);
					//Console.WriteLine($"W: {imageFile.Width} H: {imageFile.Height}");
					//MipHandling MH = MipHandling.Default;
					//dds.HEXBytes = imageFile.Save(new ImageEngineFormatDetails(ImageEngineFormat.DDS_DXT5), MH);
					//Well we tried!!

					//If DDS read the RAW bytes and use that no need to convert
					FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
					byte[] bytes;
					using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }
					dds.HEXBytes = bytes;
					emb.DDSFiles.RemoveAt(LastSelectedTreeNode.Index);
					emb.DDSFiles.Insert(LastSelectedTreeNode.Index, dds);
					emb.GenerateBytes();
					WorkingTEXEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
					WorkingTEXEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, emb);
					RefreshTree(false);
					ImageEngineImage IE = new ImageEngineImage(dds.HEXBytes);
					pbPreviewDDS.BackgroundImage = Utils.BitmapFromBytes(IE.Save(new ImageEngineFormatDetails(ImageEngineFormat.PNG), new MipHandling()));
					IE.Dispose();

					
				}
			}
			catch
			{
				MessageBox.Show("Something went wrong while encoding/injecting texture!", TStrings.STR_Error);
			}
		}

		private void addDDSToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			AddNewDDSToEMB();
		}

		private void injectDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddNewDDSToEMB();
		}

		void AddNewDDSToEMB()
		{
			try
			{
				int index;

				if (LastSelectedTreeNode.Tag.ToString() == "EMB") index = LastSelectedTreeNode.Index;
				else index = LastSelectedTreeNode.Parent.Index;
				EMB emb = (EMB)WorkingTEXEMZ.Files[index];
				DDS dds = new DDS();
				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;
				diagOpenOBJ.Filter = DDSFileFilter;
				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{
					//If DDS read the RAW bytes and use that no need to convert
					FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
					byte[] bytes;
					using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }
					dds.HEXBytes = bytes;
					emb.NumberOfFiles += 1;
					emb.FileLengthList.Add(0);
					emb.FileNameList.Add(Encoding.ASCII.GetBytes(diagOpenOBJ.SafeFileName));
					emb.FilePointerList.Add(0);
					emb.FileNamePointerList.Add(0);
					emb.DDSFiles.Add(dds);
					emb.GenerateBytes();
					WorkingTEXEMZ.Files.Remove(index);
					WorkingTEXEMZ.Files.Add(index, emb);
					RefreshTree(false);
					ImageEngineImage IE = new ImageEngineImage(dds.HEXBytes);
					pbPreviewDDS.BackgroundImage = Utils.BitmapFromBytes(IE.Save(new ImageEngineFormatDetails(ImageEngineFormat.PNG), new MipHandling()));
					IE.Dispose();
				}
			}
			catch
			{
				MessageBox.Show("Something went wrong while encoding/injecting texture!", TStrings.STR_Error);
			}
		}

		private void deleteDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMB emb = (EMB)WorkingTEXEMZ.Files[LastSelectedTreeNode.Parent.Index];
			emb.NumberOfFiles -= 1;
			emb.FileLengthList.RemoveAt(0);
			emb.FileNameList.RemoveAt(LastSelectedTreeNode.Index);
			emb.FilePointerList.RemoveAt(0);
			emb.FileNamePointerList.RemoveAt(0);
			emb.DDSFiles.RemoveAt(LastSelectedTreeNode.Index);
			emb.GenerateBytes();
			WorkingTEXEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			WorkingTEXEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, emb);
			RefreshTree(false);
		}

		private void btnSaveEMZ_Click_1(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			SaveEMZToFile((string)btn.Tag);
		}

		private void btnSaveTEXEMZ_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			SaveEMZToFile((string)btn.Tag);
		}

		private void cbShaders_SelectedIndexChanged(object sender, EventArgs e)
		{
			//Console.WriteLine("Property fishing: " + SelectedShaderID);
		}

		private void btnEO_ShaderEditSave_Click(object sender, EventArgs e)
		{
			int SelectedShaderID = cbShaders.SelectedIndex;

			string ShaderName = Utils.Shaders.ElementAt(SelectedShaderID).Key;
			byte[] StringBytes = Encoding.ASCII.GetBytes(ShaderName);
			EMM emm = (EMM)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];
			Material mat = emm.Materials[LastSelectedTreeNode.Index]; //Saved for index
			string[] PropertyLines = Regex.Split(lvShaderProperties.Text, Environment.NewLine);
			Material newMat = new Material();
			newMat.PropertyNamesList = new List<byte[]>();
			newMat.PropertyValuesList = new List<byte[]>();
			newMat.Shader = StringBytes;
			newMat.Name = mat.Name;
			for (int i = 0; i < PropertyLines.Length; i++)
			{
				if (PropertyLines[i].Trim() == string.Empty) continue;
				string[] Property = Regex.Split(PropertyLines[i], " ");
				newMat.PropertyNamesList.Add(Utils.MakeModelName(Property[0]));
				newMat.PropertyValuesList.Add(Utils.StringToHexBytes(Property[1], 0x08));
			}
			newMat.PropertyCount = newMat.PropertyNamesList.Count;
			newMat.GenerateBytes();
			emm.Materials.RemoveAt(LastSelectedTreeNode.Index);
			emm.Materials.Insert(LastSelectedTreeNode.Index, newMat);
			emm.GenerateBytes();
			WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, emm);
			RefreshTree(false);
			AddStatus("Material '" + Encoding.ASCII.GetString(newMat.Name).Replace("\0", "") + "' saved with shader '" + ShaderName.Replace("\0", "") + "'");
		}

		private void addMaterialToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddMaterial();
		}

		void AddMaterial()
		{
			IB.StartPosition = FormStartPosition.CenterParent; IB.ShowDialog(this);
			if (IB.EnteredValue.Trim() != string.Empty)
			{
				TreeNode emmNode;
				EMO emo;
				EMM emm;
				string newMatName = IB.EnteredValue.Trim();
				if ((string)LastSelectedTreeNode.Tag == "EMM")
					emmNode = LastSelectedTreeNode;
				else
					emmNode = LastSelectedTreeNode.Parent;

				foreach (TreeNode n in emmNode.Nodes)
				{
					if (n.Text.Replace("\0", "") == newMatName)
					{
						MessageBox.Show("Name already exists!", TStrings.STR_Error);
						return;
					}
				}

				emm = (EMM)WorkingEMZ.Files[emmNode.Index];

				Material newMat = new Material();
				newMat.Name = Utils.MakeModelName(IB.EnteredValue.Trim());
				newMat.Shader = Utils.MakeModelName("T1"); //Default shader
				newMat.PropertyNamesList = new List<byte[]>();
				newMat.PropertyValuesList = new List<byte[]>();
				newMat.GenerateBytes();
				emm.Materials.Add(newMat);
				emm.MaterialCount += 1;
				emm.MaterialPointersList.Add(0);
				emm.GenerateBytes();
				WorkingEMZ.Files.Remove(emmNode.Index);
				WorkingEMZ.Files.Add(emmNode.Index, emm);

				//Find matching EMO
				for (int i = 0; i < WorkingEMZ.Files.Count; i++)
				{
					int FileType = Utils.ReadInt(true, WorkingEMZ.FilePointersList[i] + WorkingEMZ.FileListPointer + (i * 8), WorkingEMZ.HEXBytes);
					if (FileType == USF4Methods.EMO)
					{
						emo = (EMO)WorkingEMZ.Files[i];
						byte[] subName = Utils.ChopByteArray(emo.Name, 0x04, 0x03);
						byte[] targetName = Utils.ChopByteArray(emm.Name, 0x04, 0x03);
						if (Encoding.ASCII.GetString(subName) == Encoding.ASCII.GetString(targetName))
						{
							emo.NumberEMMMaterials = emm.MaterialCount;
							emo.GenerateBytes();
							WorkingEMZ.Files.Remove(i);
							WorkingEMZ.Files.Add(i, emo);
							break;
						}
					}
				}
				RefreshTree(false);
				AddStatus("Material '" + newMatName + "' added to '" + Encoding.ASCII.GetString(emm.Name).Replace("\0", "") + "'");
			}
			else
			{
				return;
			}
		}

		static void UnluacMain(string bytefile, string plainfile)
		{
			unluac.Main.decompile(bytefile, plainfile);
			//string[] args = { $"{bytefile}"};
			//unluac.Main.main(args);
		}

		LUA LUAScriptToBytecode(string target_lua)
		{
            string ChunkSpy_script = CodeStrings.ChunkSpy1;

			lua_State L = null;
			LUA nLUA = (LUA)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			try
			{	
				string[] lines = File.ReadAllLines(diagOpenOBJ.FileName);

				File.WriteAllLines(diagOpenOBJ.SafeFileName, lines, Encoding.ASCII);

				//this is the loadfile method
				string luac_script =
						@"f=assert(io.open(""native_lua_chunk.out"",""wb""))" +
						@"assert(f:write(string.dump(assert(loadfile(""" + diagOpenOBJ.SafeFileName + @""")))))" +
						"assert(f:close())";

				//LUAC implementation
				try
				{
					// initialization
					L = lua_open();
					luaL_openlibs(L);

					int loaderror = luaL_loadbuffer(L, luac_script, (uint)luac_script.Length, "program");
					int error = lua_pcall(L, 0, 0, 0);

					if(loaderror == 0 && loaderror == 0)
                    {
						AddStatus("Luascript compiled to bytecode...");
                    }
					else if (loaderror != 0 || error != 0)
					{
						AddStatus("Luascript failed to compile.");
					}
				}
				finally
				{
					// cleanup
					lua_close(L);
					File.Delete("plaintext.lua");
				}


				//ChunkSpy implementation
				try
				{
					// initialization
					L = lua_open();
					luaL_openlibs(L);

					int loaderror = luaL_loadbuffer(L, ChunkSpy_script, (uint)ChunkSpy_script.Length, "program");
					int error = lua_pcall(L, 0, 0, 0);

					if (loaderror == 0 && loaderror == 0)
					{
						AddStatus("Bytecode converted to USF4-native format...");
					}
					else if (loaderror != 0 || error != 0)
					{
						AddStatus("Bytecode conversion failed.");
					}
				}
				finally
				{
					// cleanup
					lua_close(L);
					File.Delete(CodeStrings.infile);
				}

				//Read in the newly created bytecode file and inject into EMZ
				FileStream fsSource = new FileStream("output_usf4.out", FileMode.Open, FileAccess.Read);
				byte[] bytes;
				using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }
				nLUA.HEXBytes = bytes;
				nLUA.Name = Encoding.ASCII.GetBytes(target_lua);

				File.Delete(CodeStrings.outfile);

				
			}
			catch
			{
				MessageBox.Show("Something went wrong while injecting LUA!", TStrings.STR_Error);
			}

			return nLUA;
		}

		private void addNewMaterialToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddMaterial();
		}

		private void cbShaderProperties_SelectedIndexChanged(object sender, EventArgs e)
		{
			string Value = (string)cbShaderProperties.Items[cbShaderProperties.SelectedIndex];
			Value = Utils.ShadersProperties[Value].Replace("\0", "");
			AddPropertyLineToTextBox($"{Value}");
			lvShaderProperties.Focus();
			lvShaderProperties.DeselectAll();
		}

		private void btnCalculateFloat_Click(object sender, EventArgs e)
		{
			if (SPINFloat.Text.Trim() == string.Empty) return;
			try
			{
				float Input = float.Parse(Utils.FixFloatingPoint(SPINFloat.Text.Trim()));
				string Out;
				Out = "00000000" + Utils.FloatToHex(Input);
				SPOutFloat.Text = Out;
				SPOutFloat.Focus();
				SPOutFloat.SelectAll();
			}
			catch
			{
				SPOutFloat.Text = string.Empty;
				AddStatus("Invalid float input!");
			}
		}

		private void renameDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IB.StartPosition = FormStartPosition.CenterParent; IB.ShowDialog(this);
			if (IB.EnteredValue.Trim() != string.Empty)
			{
				EMB emb = (EMB)WorkingTEXEMZ.Files[LastSelectedTreeNode.Parent.Index];
				emb.FileNameList.RemoveAt(LastSelectedTreeNode.Index);
				emb.FileNameList.Insert(LastSelectedTreeNode.Index, Encoding.ASCII.GetBytes(IB.EnteredValue));
				emb.GenerateBytes();
				WorkingTEXEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
				WorkingTEXEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, emb);
				RefreshTree(false);
			}
		}

		private void extractDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMB emb = (EMB)WorkingTEXEMZ.Files[LastSelectedTreeNode.Parent.Index];
			DDS dds = emb.DDSFiles[LastSelectedTreeNode.Index];
			saveFileDialog1.Filter = DDSFileFilter;
			saveFileDialog1.FileName = Encoding.ASCII.GetString(emb.FileNameList[LastSelectedTreeNode.Index]);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string filepath = saveFileDialog1.FileName;
				ExtractDDSFromEMB(dds.HEXBytes,filepath);
				AddStatus($"DDS {Path.GetFileName(filepath)} Extracted");
			}
		}
		/// <summary>Extracts all DDS textures</summary>
		void ExtractTEXEMZ(string BasePath)
		{
			for (int i = 0; i < WorkingTEXEMZ.Files.Count; i++)
			{
				EMB emb = (EMB)WorkingTEXEMZ.Files[i];
				string SubFolder = Encoding.ASCII.GetString(emb.Name);
				SubFolder = SubFolder.Substring(0, SubFolder.Length - 4); 
				Directory.CreateDirectory($"{BasePath}\\{SubFolder}"); //Create a sub folder witht the EMB name without .emb at the end
				ExtractAllDDSFromEMB(emb, $"{BasePath}\\{SubFolder}", true);
			}
		}

		/// <summary>Extract All DDS textures from the supplied EMB and write them to the target path</summary>
		void ExtractAllDDSFromEMB(EMB emb, string InputPath, bool alltex = false)
		{
			string FullFilePath;
			for (int i = 0; i < emb.DDSFiles.Count; i++)
			{
				DDS dds = emb.DDSFiles[i];
				string DDSName = Encoding.ASCII.GetString(emb.FileNameList[i]);
				if (!DDSName.ToLower().Contains(".dds")){	DDSName += ".dds";	}
				if (DDSName.ToLower() == "dds.dds") DDSName = Path.GetFileName(InputPath) + ".dds";
				FullFilePath = $"{InputPath}\\{DDSName}";
				if (File.Exists(FullFilePath)) DDSName = $"{i} {DDSName}";
				ExtractDDSFromEMB(dds.HEXBytes, $"{InputPath}\\{DDSName}");
				if(!alltex) AddStatus($"DDS {DDSName} Extracted");
			}
		}


		void ExtractDDSFromEMB(byte[] DDSData, string Filepath)
		{
			Utils.WriteDataToStream(Filepath, DDSData);
		}

		void AddStatus(string Value)
		{
			DateTime dt =DateTime.Now;
			lvStatus.Items.Add("["+dt.ToString("HH:mm:ss") + "] " +  Value);
			lvStatus.SelectedIndex = lvStatus.Items.Count - 1;
			lvStatus.ClearSelected();
		}

		private void lvStatus_MouseDown(object sender, MouseEventArgs e)
		{
			lvStatus.ClearSelected();
		}

		private void exctractAllDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				EMB emb = (EMB)WorkingTEXEMZ.Files[LastSelectedTreeNode.Index];
				Console.WriteLine("Selected Path " + folderBrowserDialog1.SelectedPath);
				ExtractAllDDSFromEMB(emb, folderBrowserDialog1.SelectedPath);
			}
		}

		private void extractTEXEMZTexturesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (WorkingTEXEMZ.HEXBytes == null)
			{
				MessageBox.Show("No .tex.emz texture pack loaded.", TStrings.STR_Error);
				return;
			}
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				ExtractTEXEMZ(folderBrowserDialog1.SelectedPath);
				AddStatus($"All textures exported to {folderBrowserDialog1.SelectedPath}");
			}
		}

		private void rawDumpCSBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RawDumpCSB();
		}

		private void injectCSBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			InjectCSB();
		}

		private void emgContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
			extractModelAsOBJToolStripMenuItem.Visible = true;
			extractSubmodelAsOBJToolStripMenuItem.Visible = true;
			rawDumpEMGToolStripMenuItem.Visible = true;

			if ((string)LastSelectedTreeNode.Tag == "EMG")
            {
				extractModelAsOBJToolStripMenuItem.Visible = false;
				extractSubmodelAsOBJToolStripMenuItem.Visible = false;
            }
			else if ((string)LastSelectedTreeNode.Tag == "Model")
            {
				extractSubmodelAsOBJToolStripMenuItem.Visible = false;
				rawDumpEMGToolStripMenuItem.Visible = false;
			}
			else if ((string)LastSelectedTreeNode.Tag == "SubModel")
            {
				rawDumpEMGToolStripMenuItem.Visible = false;
			}
		}

        private void csbContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void animationContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
			bool emaclick = false;

			if ((string)LastSelectedTreeNode.Tag == "EMA")
			{
				emaclick = true;
			}

			DeleteAnimaiontoolStripMenuItem3.Visible = !emaclick;
			InjectAnimationtoolStripMenuItem1.Visible = !emaclick;
		}

        private void DeleteAnimaiontoolStripMenuItem3_Click(object sender, EventArgs e)
        {
			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];
			Animation anim = (Animation)ema.Animations[LastSelectedTreeNode.Index];

			DeleteAnimation(ema, LastSelectedTreeNode.Index);	
        }
		
		void DeleteAnimation(EMA ema, int AnimIndex)
        {
			ema.AnimationPointersList.RemoveAt(0);
			ema.Animations.RemoveAt(AnimIndex);
			ema.AnimationCount = ema.Animations.Count;

			ema.GenerateBytes();

			WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, ema);
			AddStatus("Animation deleted.");
			RefreshTree(false);
        }

        private void injectLUAScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
			InjectLua();
        }

		private void InjectLua()
        {
			try
			{
				LUA lua = (LUA)WorkingEMZ.Files[LastSelectedTreeNode.Index];

				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;
				diagOpenOBJ.Filter = LUAFileFilter;

				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{
					FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
					byte[] bytes;
					using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }
					lua.HEXBytes = bytes;

					WorkingEMZ.Files.Remove(LastSelectedTreeNode.Index);
					WorkingEMZ.Files.Add(LastSelectedTreeNode.Index, lua);
					RefreshTree(false);
					AddStatus("Lua bytecode injected.");
				}
			}
			catch
			{
				MessageBox.Show("Something went wrong while injecting LUA!", TStrings.STR_Error);
			}

		}

        private void emmContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void InjectEMO_Click(object sender, EventArgs e)
        {
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = SMDFileFilter;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				if (filepath.Trim() != string.Empty)
				{
					LastOpenFolder = Path.GetDirectoryName(filepath);
					WorkingFileName = diagOpenOBJ.SafeFileName;
					SMDModel nSMD = new SMDModel();
					nSMD = ReadSMD(filepath);

					EMO nEMO = EMOFromSMD(nSMD);

					WorkingEMZ.Files.Remove(LastSelectedTreeNode.Index);
					WorkingEMZ.Files.Add(LastSelectedTreeNode.Index, nEMO);
					RefreshTree(false);
				}
			}
		}

        private void embContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void InjectAnimationtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
			InjectAnimation();
			//DuplicateAnimationDown();
        }

		private void DuplicateAnimationDown()
        {
			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];

			Animation anim = ema.Animations[LastSelectedTreeNode.Index];

			Animation anim2 = ema.Animations[LastSelectedTreeNode.Index + 1];

			anim2.Name = anim.Name;

			ema.Animations.RemoveAt(LastSelectedTreeNode.Index);
			ema.Animations.Insert(LastSelectedTreeNode.Index, anim2);
			ema.GenerateBytes();
			WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, ema);
			RefreshTree(false);
		}

		private void InjectAnimation()
		{
			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];

			Animation anim = ema.Animations[LastSelectedTreeNode.Index];

			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = SMDFileFilter;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				SMDModel model = ReadSMD(diagOpenOBJ.FileName);

				Animation WorkingAnimation = new Animation();

				WorkingAnimation.Duration = model.Frames.Count;
				WorkingAnimation.Name = anim.Name;

				WorkingAnimation.ValuesList = new List<float>();
				Dictionary<float, int> ValueDict = new Dictionary<float, int>();

				//Populate the float dictionary
				for (int i = 0; i < model.Nodes.Count; i++)
				{

					for (int k = 0; k < model.Frames.Count; k++)
					{
						int xflip = -1;

						float Rad2Deg = Convert.ToSingle(180 / Math.PI);
						int dumVal; //TryGetValue outputs the dictionary result, but we don't care right now
						if (!ValueDict.TryGetValue(xflip*model.Frames[k].traX[i], out dumVal)) { ValueDict.Add(xflip*model.Frames[k].traX[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(model.Frames[k].traY[i], out dumVal)) { ValueDict.Add(model.Frames[k].traY[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(model.Frames[k].traZ[i], out dumVal)) { ValueDict.Add(model.Frames[k].traZ[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(Rad2Deg * xflip*model.Frames[k].rotX[i], out dumVal)) { ValueDict.Add(Rad2Deg * xflip*model.Frames[k].rotX[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(Rad2Deg * model.Frames[k].rotY[i], out dumVal)) { ValueDict.Add(Rad2Deg * model.Frames[k].rotY[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(Rad2Deg * model.Frames[k].rotZ[i], out dumVal)) { ValueDict.Add(Rad2Deg * model.Frames[k].rotZ[i], ValueDict.Count); }
					}
				}
				WorkingAnimation.ValueCount = ValueDict.Count;
				//Populate the Value list from the dictionary
				for (int i = 0; i < ValueDict.Count; i++)
				{
					WorkingAnimation.ValuesList.Add(ValueDict.ElementAt(i).Key);
				}

				//Set flags for later use - short IndexValues only have 14 "useable" bits due to masking, so max is 3FFF
				int KTimes = 0x00; if (WorkingAnimation.Duration > 0xFF) { KTimes = 0x20; }
				int IndType = 0x00; if (WorkingAnimation.ValueCount > 0x3FFF) { IndType = 0x40; }
				int Absolute = 0x00; //SMD animations ONLY support relative animation, but chuck this here for later

				//SMD contains translation and rotation, so up to 6 tracks per node
				WorkingAnimation.CmdTrackCount = model.Nodes.Count * 6;
				WorkingAnimation.CMDTracks = new List<CMDTrack>();
				WorkingAnimation.CmdTrackPointersList = new List<int>();

				//Loop bone list
				for (int i = 0; i < model.Nodes.Count; i++)
				{
					//j = transform axis
					for (int j = 0; j < 3; j++)
					{
						CMDTrack WorkingCMD = new CMDTrack();
						WorkingCMD.StepsList = new List<int>();
						WorkingCMD.IndicesList = new List<int>();
						WorkingCMD.BoneID = i;
						WorkingCMD.TransformType = 0;

						WorkingCMD.BitFlag = Convert.ToByte(j + KTimes + IndType + Absolute);
						WorkingCMD.StepCount = model.Frames.Count;

						//k = keyframe count
						for (int k = 0; k < model.Frames.Count; k++)
						{
							WorkingCMD.StepsList.Add(k);
							if (j == 0) { WorkingCMD.IndicesList.Add(ValueDict[model.Frames[k].traX[i]]); }
							if (j == 1) { WorkingCMD.IndicesList.Add(ValueDict[model.Frames[k].traY[i]]); }
							if (j == 2) { WorkingCMD.IndicesList.Add(ValueDict[model.Frames[k].traZ[i]]); }
						}
						WorkingAnimation.CMDTracks.Add(WorkingCMD);
						WorkingAnimation.CmdTrackPointersList.Add(0);
					}
				}

				for (int i = 0; i < model.Nodes.Count; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						CMDTrack WorkingCMD = new CMDTrack();
						WorkingCMD = new CMDTrack();
						WorkingCMD.StepsList = new List<int>();
						WorkingCMD.IndicesList = new List<int>();
						WorkingCMD.BoneID = i;
						WorkingCMD.TransformType = 1;
						WorkingCMD.BitFlag = Convert.ToByte(j + KTimes + IndType + Absolute);
						WorkingCMD.StepCount = model.Frames.Count;

						//k = keyframe count
						for (int k = 0; k < model.Frames.Count; k++)
						{
							float Rad2Deg = Convert.ToSingle(180 / Math.PI);

							WorkingCMD.StepsList.Add(k);
							if (j == 0)
							{
								float valueDeg = Rad2Deg * model.Frames[k].rotX[i];
								WorkingCMD.IndicesList.Add(ValueDict[valueDeg]);
							}
							if (j == 1)
							{
								float valueDeg = Rad2Deg * model.Frames[k].rotY[i];
								WorkingCMD.IndicesList.Add(ValueDict[valueDeg]);
							}
							if (j == 2)
							{
								float valueDeg = Rad2Deg * model.Frames[k].rotZ[i];
								WorkingCMD.IndicesList.Add(ValueDict[valueDeg]);
							}
						}

						WorkingAnimation.CMDTracks.Add(WorkingCMD);
						WorkingAnimation.CmdTrackPointersList.Add(0);
					}
				}

				

				ema.Animations.RemoveAt(LastSelectedTreeNode.Index);
				ema.Animations.Insert(LastSelectedTreeNode.Index, WorkingAnimation);
				ema.GenerateBytes();
				WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
				WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, ema);
			}
		}

        private void dumpRefPoseToSMDToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];

			InitialPoseFromEMA(ema);
        }

        private void rawDumpEMOAsSMDToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMO emo = (EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			EMOtoRefSMD(emo);
		}

        private void rawDumpLUAToolStripMenuItem_Click(object sender, EventArgs e)
        {
			LUA lua = (LUA)WorkingEMZ.Files[LastSelectedTreeNode.Index];
			saveFileDialog1.Filter = LUAFileFilter;
			saveFileDialog1.FileName = Encoding.ASCII.GetString(lua.Name);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, lua.HEXBytes);
				AddStatus($"Extracted {Encoding.ASCII.GetString(lua.Name)}");
			}
        }

		private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tvTree.SelectedNode.ExpandAll();
		}

		private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tvTree.SelectedNode.Collapse(false);
		}

		private void insertOBJAsNewEMGToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			AddOBJAsNewEMG();
		}

        async private void injectSMDAsEMGExperimentalToolStripMenuItem_Click(object sender, EventArgs e)
        {
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = SMDFileFilter;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				if (filepath.Trim() != string.Empty)
				{
					LastOpenFolder = Path.GetDirectoryName(filepath);
					WorkingFileName = diagOpenOBJ.SafeFileName;
					SMDModel someSMD = new SMDModel();
					await Task.Run(() => { someSMD = ReadSMD(filepath); });

					EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
					EMG targetEMG = emo.EMGs[SelectedEMGNumberInTree];
					EMG newEMG = NewEMGFromSMD(someSMD);
					emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
					emo.EMGs.Insert(SelectedEMGNumberInTree, newEMG);
					emo.GenerateBytes();
					WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
					WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);

					RefreshTree(false);
					OpenEMONode(true);
				}
			}
		}

        private void injectLUAScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
			CultureInfo.CurrentCulture = new CultureInfo("en-GB", false);

			LUA lua = (LUA)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			try
			{
				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;
				diagOpenOBJ.Filter = LUAFileFilter;
				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{

					lua = LUAScriptToBytecode(diagOpenOBJ.SafeFileName);

					WorkingEMZ.Files.Remove(LastSelectedTreeNode.Index);
					WorkingEMZ.Files.Add(LastSelectedTreeNode.Index, lua);
					RefreshTree(false);
					AddStatus($"Bytecode injected into {Encoding.ASCII.GetString(WorkingEMZ.FileNamesList[LastSelectedTreeNode.Index])}.");
				}
			}
			catch
			{
				MessageBox.Show("Couldn't open file.", TStrings.STR_Error);
			}

		}

		private void injectFileExperimentalToolStripMenuItem_Click(object sender, EventArgs e)
		{
	
		}

		private void emzContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
			bool emzclick = false;

			if((string)LastSelectedTreeNode.Tag == "EMZ")
            {
				emzclick = true;
            }

			addEMOToolStripMenuItem.Visible = emzclick;
			addEMMToolStripMenuItem.Visible = emzclick;
			addEMAToolStripMenuItem.Visible = emzclick;
			addCSBToolStripMenuItem.Visible = emzclick;
			addLuaScriptToolStripMenuItem.Visible = emzclick;

			addEMBToolStripMenuItem.Visible = !emzclick;
			extractTEXEMZTexturesToolStripMenuItem.Visible = !emzclick;
        }

        private void addLUAScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
			LUA lua = new LUA();

			try
			{
				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;
				diagOpenOBJ.Filter = LUAFileFilter;
				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{

					lua = LUAScriptToBytecode(diagOpenOBJ.SafeFileName);
					lua.Name = Encoding.ASCII.GetBytes(diagOpenOBJ.SafeFileName);

					WorkingEMZ.Files.Add(WorkingEMZ.Files.Count, lua);

					WorkingEMZ.NumberOfFiles++;
					WorkingEMZ.FileNamePointersList.Add(0x00);
					WorkingEMZ.FileLengthsList.Add(0x00);
					WorkingEMZ.FilePointersList.Add(0x00);
					WorkingEMZ.FileNamesList.Add(lua.Name);

					AddStatus($"Bytecode added as {diagOpenOBJ.SafeFileName}");

					RefreshTree(false);
				}
			}
			catch
			{
				MessageBox.Show("Couldn't open file.", TStrings.STR_Error);
			}
		}

        private void emoContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void rawDumpEMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMO targetEMO = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];

			EMO nEMO = targetEMO;

			nEMO.GenerateBytes();

			saveFileDialog1.Filter = EMOFileFilter;
			saveFileDialog1.FileName = Encoding.ASCII.GetString(nEMO.Name);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, nEMO.HEXBytes);
				AddStatus($"Extracted {Encoding.ASCII.GetString(nEMO.Name)}");
			}
		}

        private void rawDumpEMAToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMA targetEMA = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			EMA nEMA = targetEMA;

			nEMA.GenerateBytes();

			saveFileDialog1.Filter = EMAFileFilter;
			saveFileDialog1.FileName = Encoding.ASCII.GetString(nEMA.Name);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, nEMA.HEXBytes);
				AddStatus($"Extracted {Encoding.ASCII.GetString(nEMA.Name)}");
			}

		}

        private void duplicateEMGToolStripMenuItem_Click(object sender, EventArgs e)
        {
			
			EMO targetEMO = (EMO)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];
			EMG targetEMG = new EMG(targetEMO.EMGs[LastSelectedTreeNode.Index].HEXBytes);
			List<int> newbones = new List<int>();

			for (int i = 0; i < targetEMG.Models[0].SubModels[0].BoneIntegersList.Count; i++)
            {
				newbones.Add(targetEMG.Models[0].SubModels[0].BoneIntegersList[i] + 21);
            }

			//TODO need to recalculate the bone integers count
			targetEMG.Models[0].SubModels[0].BoneIntegersList.Clear();
			targetEMG.Models[0].SubModels[0].BoneIntegersList.AddRange(newbones);
			targetEMG.GenerateBytes();

			//targetEMO.Skeleton = Anim.DuplicateSkeleton(targetEMO.Skeleton, 1, 20);
			//targetEMO.Skeleton.HEXBytes = HexDataFromSkeleton(targetEMO.Skeleton);
			targetEMO.EMGs.Add(targetEMG);
			targetEMO.EMGCount++;
			targetEMO.EMGPointersList.Add(0x00);

			targetEMO.GenerateBytes();

			WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, targetEMO);
			RefreshTree(false);
		}

        private void duplicateModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMO targetEMO = (EMO)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];
			EMG targetEMG = new EMG(targetEMO.EMGs[LastSelectedTreeNode.Index].HEXBytes);

			Model targetModel = targetEMG.Models[0];
			SubModel targetSM = targetModel.SubModels[0];

			List<int> daisychain1 = targetSM.DaisyChain.ToList();
			List<int> daisychain2 = new List<int>();

			for (int i = 0; i < daisychain1.Count; i++)
            {
				daisychain2.Add(daisychain1[i] + targetModel.VertexCount);
            }

			daisychain1.Add(daisychain1.Last());   //Last entry from daisychain1
			daisychain1.Add(daisychain2[2]);
			daisychain1.Add(daisychain2[2]);
			daisychain1.Add(daisychain2[1]);

			daisychain1.AddRange(daisychain2);

			targetSM.DaisyChain = daisychain1.ToArray();
			targetSM.DaisyChainLength = daisychain1.Count;
			
			int bone_count = targetSM.BoneIntegersList.Count;
			for(int i = 0; i < bone_count; i++)
            {
				//targetSM.BoneIntegersList.Add(targetSM.BoneIntegersList[i] + 20);
            }
			targetSM.BoneIntegersCount = targetSM.BoneIntegersList.Count;

			//Return Submodel
			targetModel.SubModels.RemoveAt(0);
			targetModel.SubModels.Insert(0, targetSM);
			
			//Duplicate verts and increase the bone IDs
			int vert_count = targetModel.VertexData.Count;
			
			for(int i = 0; i < vert_count; i++)
            {
				Vertex v = targetModel.VertexData[i];
				List<int> new_BoneIDs = new List<int>();

				//for (int j = 0; j < v.BoneIDs.Count; j++)
    //            {
				//	if(j > 0 && v.BoneIDs[j] == 0)
    //                {
				//		new_BoneIDs.Add(0x00);
    //                }
				//	else
    //                {
				//		new_BoneIDs.Add(v.BoneIDs[j] + 19);
    //                }
						
    //            }
				//v.BoneIDs = new_BoneIDs;
				v.X += 0f;
				targetModel.VertexData.Add(v);
            }

			targetModel.VertexCount = targetModel.VertexData.Count;

			targetEMG.Models.RemoveAt(0);
			targetEMG.Models.Insert(0, targetModel);

			targetEMG.GenerateBytes();

			targetEMO.EMGs.RemoveAt(0);
			targetEMO.EMGs.Insert(0, targetEMG);

			//targetEMO.Skeleton = Anim.DuplicateSkeleton(targetEMO.Skeleton, 1,20);
			//targetEMO.Skeleton.HEXBytes = HexDataFromSkeleton(targetEMO.Skeleton);

			targetEMO.GenerateBytes();

			WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, targetEMO);
			RefreshTree(false);

		}

		public void DuplicateUSA_MAN01_B()
		{
			EMA ema = (EMA)WorkingEMZ.Files[8];
			EMO emo = (EMO)WorkingEMZ.Files[10];

			ema.Skeleton = Anim.DuplicateSkeleton(ema.Skeleton, 1,20);
			emo.Skeleton = Anim.DuplicateSkeleton(emo.Skeleton, 1,20);

			Animation nAnim = new Animation();

			//for (int i = 0; i < ema.AnimationCount; i++)
			for (int i = 0; i < 1; i++)
			{
				nAnim = Anim.DuplicateAnimation(ema.Animations[i], 1, 20);
				ema.Animations.RemoveAt(i);
				ema.Animations.Insert(i, nAnim);
			}

			emo.Skeleton.GenerateBytes();
			ema.Skeleton.GenerateBytes();
			ema.GenerateBytes();
			emo.GenerateBytes();

			WorkingEMZ.Files.Remove(8);
			WorkingEMZ.Files.Add(8, ema);
			WorkingEMZ.Files.Remove(10);
			WorkingEMZ.Files.Add(10, emo);
			RefreshTree(false);

		}

        private void dupliacteUSAMAN01BToolStripMenuItem_Click(object sender, EventArgs e)
        {
			DuplicateUSA_MAN01_B();
        }

        private void extractLUAScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
			LUA nlua = (LUA)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			string intermediatefile = "plain_" + Encoding.ASCII.GetString(nlua.Name).Replace("\0", "");

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = intermediatefile;
			saveFileDialog1.Filter = LUAFileFilter;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string filepath = saveFileDialog1.FileName;

				if (filepath.Trim() != string.Empty)
				{
					string tempfile1 = "temp_luabytes.out";
					string tempfile2 = "temp_luaplain.out";

					Utils.WriteDataToStream(tempfile1, nlua.HEXBytes);
					
					UnluacMain(tempfile1, tempfile2);

					if (File.Exists(tempfile2))
					{
						File.WriteAllLines(saveFileDialog1.FileName, File.ReadAllLines(tempfile2));
						AddStatus("Lua bytecode extracted to plain text.");
					}
				}
			}
		}

        private void AddAnimationtoolStripMenuItem2_Click(object sender, EventArgs e)
        {
			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			ema.AnimationCount++;
			ema.Animations.Add(GeometryIO.AnimationFromColladaStruct());
			ema.AnimationPointersList.Add(0);
			ema.GenerateBytes();

			WorkingEMZ.Files.Remove(LastSelectedTreeNode.Index);
			WorkingEMZ.Files.Add(LastSelectedTreeNode.Index, ema);
			RefreshTree(false);
        }

		private void extractEMOAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = string.Empty;
			saveFileDialog1.Filter = OBJFileFilter;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				List<string> lines = new List<string>();

				for (int i = 0; i < emo.EMGs.Count; i++)
				{
					//lines.AddRange(GeometryIO.EMGtoOBJ(emo.EMGs[i], Encoding.ASCII.GetString(emo.Name).Split('\0')[0] + $"_EMG_{i}", true));
					lines.AddRange(GeometryIO.EMGtoOBJ(emo.EMGs[i], $"EMG_{i}", emo.EMGs.Count > 1)); //If there's more than 1 EMG, force inverted indices
				}

				File.WriteAllLines(saveFileDialog1.FileName, lines);
				AddStatus($"{Encoding.ASCII.GetString(emo.Name).Split('\0')[0]} extracted to {saveFileDialog1.FileName}");
			}
		}
		
		private void extractEMGAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
			EMG emg = emo.EMGs[SelectedEMGNumberInTree];

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = string.Empty;
			saveFileDialog1.Filter = OBJFileFilter;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				List<string> lines = GeometryIO.EMGtoOBJ(emg, Encoding.ASCII.GetString(emo.Name).Split('\0')[0] + $"_EMG_{SelectedEMGNumberInTree}");

				File.WriteAllLines(saveFileDialog1.FileName, lines);
			}
		}

        private void extractModelAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
			EMG emg = emo.EMGs[SelectedEMGNumberInTree];

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = string.Empty;
			saveFileDialog1.Filter = OBJFileFilter;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				List<string> lines = GeometryIO.ModeltoOBJ(emg, SelectedModelNumberInTree, Encoding.ASCII.GetString(emo.Name).Split('\0')[0] + $"_EMG_{SelectedEMGNumberInTree}");

				File.WriteAllLines(saveFileDialog1.FileName, lines);
			}
		}

        private void extractSubmodelAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
			EMG emg = emo.EMGs[SelectedEMGNumberInTree];

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = string.Empty;
			saveFileDialog1.Filter = OBJFileFilter;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				List<string> lines = GeometryIO.SubmodeltoOBJ(emg, SelectedModelNumberInTree, SelectedSubModelNumberInTree, true);

				File.WriteAllLines(saveFileDialog1.FileName, lines);
			}
		}

		private void AddFile(string filetype)
        {
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = filetype;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				string extension = diagOpenOBJ.FileName.Split('.').Last();
				string name = diagOpenOBJ.SafeFileName;

				EMZ targetEMZ = WorkingEMZ;

				if (filepath.Trim() != string.Empty)
				{
					FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
					byte[] bytes;
					using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }

					USF4File file;

					if (extension == "emo") file = new EMO();
					else if (extension == "emm") file = new EMM();
					else if (extension == "emb") file = new EMB();
					else if (extension == "lua") file = new LUA();
					else if (extension == "ema") file = new EMA();
					else if (extension == "csb") file = new CSB();
					else file = new OtherFile();

					file.ReadFile(bytes);
					file.Name = Encoding.ASCII.GetBytes(name);

					if (extension == "emb")
					{
						targetEMZ = WorkingTEXEMZ;
						AddStatus($"File {name} added to .TEX.EMZ");
					}
					else AddStatus($"File {name} added to .EMZ");

					targetEMZ.Files.Add(WorkingEMZ.Files.Count, file);
					targetEMZ.NumberOfFiles++;
					targetEMZ.FileNamePointersList.Add(0x00);
					targetEMZ.FileLengthsList.Add(0x00);
					targetEMZ.FilePointersList.Add(0x00);
					targetEMZ.FileNamesList.Add(Encoding.ASCII.GetBytes(name));

					RefreshTree(false);
				}
			}
		}

        private void addLuaScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
			AddFile(LUAFileFilter);
        }

        private void addEMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
			AddFile(EMOFileFilter);
		}

        private void addEMMToolStripMenuItem_Click(object sender, EventArgs e)
        {
			AddFile(EMMFileFilter);
		}

        private void addEMAToolStripMenuItem_Click(object sender, EventArgs e)
        {
			AddFile(EMAFileFilter);
		}

        private void addCSBToolStripMenuItem_Click(object sender, EventArgs e)
        {
			AddFile(CSBFileFilter);
		}

        private void addEMBToolStripMenuItem_Click(object sender, EventArgs e)
        {
			AddFile(EMBFileFilter);
		}

		private void DeleteFileEMZ(int index)
        {
			WorkingEMZ.Files.Remove(index);

			Dictionary<int, USF4File> temp = new Dictionary<int, USF4File>();
			for (int i = 0; i < index; i++)
            {
				temp.Add(i, WorkingEMZ.Files[i]);
            }
			for (int i = index; i < WorkingEMZ.Files.Count; i++)
			{
				temp.Add(i, WorkingEMZ.Files[i+1]);
			}

			WorkingEMZ.Files = temp;

			WorkingEMZ.FileNamesList.RemoveAt(index);
			WorkingEMZ.FileLengthsList.RemoveAt(index);
			WorkingEMZ.FileNamePointersList.RemoveAt(index);
			WorkingEMZ.FilePointersList.RemoveAt(index);
			WorkingEMZ.NumberOfFiles--;
        }

		private void DeleteFileTEXEMZ(int index)
		{
			WorkingTEXEMZ.Files.Remove(index);

			Dictionary<int, USF4File> temp = new Dictionary<int, USF4File>();
			for (int i = 0; i < index; i++)
			{
				temp.Add(i, WorkingTEXEMZ.Files[i]);
			}
			for (int i = index; i < WorkingTEXEMZ.Files.Count; i++)
			{
				temp.Add(i, WorkingTEXEMZ.Files[i + 1]);
			}

			WorkingTEXEMZ.Files = temp;

			WorkingTEXEMZ.FileNamesList.RemoveAt(index);
			WorkingTEXEMZ.FileLengthsList.RemoveAt(index);
			WorkingTEXEMZ.FileNamePointersList.RemoveAt(index);
			WorkingTEXEMZ.FilePointersList.RemoveAt(index);
			WorkingTEXEMZ.NumberOfFiles--;
		}

		private void deleteEMAToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Delete EMA?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{Encoding.ASCII.GetString(WorkingEMZ.FileNamesList[LastSelectedTreeNode.Index]).Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNode.Index);
				RefreshTree(false);
			}
		}

		private void deleteEMMToolStripMenuItem_Click(object sender, EventArgs e)
        {
			DialogResult = MessageBox.Show("Delete EMM?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{Encoding.ASCII.GetString(WorkingEMZ.FileNamesList[LastSelectedTreeNode.Index]).Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNode.Index);
				RefreshTree(false);
			}
		}

        private void deleteEMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
			DialogResult = MessageBox.Show("Delete EMO?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{Encoding.ASCII.GetString(WorkingEMZ.FileNamesList[LastSelectedTreeNode.Index]).Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNode.Index);
				RefreshTree(false);
			}
		}

        private void deleteEMBToolStripMenuItem_Click(object sender, EventArgs e)
        {
			DialogResult = MessageBox.Show("Delete EMB?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{Encoding.ASCII.GetString(WorkingTEXEMZ.FileNamesList[LastSelectedTreeNode.Index]).Split('\0')[0]} deleted from .TEX.EMZ");
				DeleteFileTEXEMZ(LastSelectedTreeNode.Index);
				RefreshTree(false);
			}
		}

        private void deleteLUAToolStripMenuItem_Click(object sender, EventArgs e)
        {
			DialogResult = MessageBox.Show("Delete LUA?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{Encoding.ASCII.GetString(WorkingEMZ.FileNamesList[LastSelectedTreeNode.Index]).Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNode.Index);
				RefreshTree(false);
			}
		}

        private void deleteCSBToolStripMenuItem_Click(object sender, EventArgs e)
        {
			DialogResult = MessageBox.Show("Delete CSB?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{Encoding.ASCII.GetString(WorkingEMZ.FileNamesList[LastSelectedTreeNode.Index]).Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNode.Index);
				RefreshTree(false);
			}
		}

        private void rawDumpEMMToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMM emm = (EMM)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			emm.GenerateBytes();

			saveFileDialog1.Filter = EMMFileFilter;
			saveFileDialog1.FileName = Encoding.ASCII.GetString(emm.Name);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, emm.HEXBytes);
				AddStatus($"Extracted {Encoding.ASCII.GetString(emm.Name)}");
			}
		}

        private void rawDumpEMBToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMB emb = (EMB)WorkingTEXEMZ.Files[LastSelectedTreeNode.Index];

			emb.GenerateBytes();

			saveFileDialog1.Filter = EMBFileFilter;
			saveFileDialog1.FileName = Encoding.ASCII.GetString(emb.Name);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, emb.HEXBytes);
				AddStatus($"Extracted {Encoding.ASCII.GetString(emb.Name)}");
			}
		}

        private void injectColladaAsEMGExperimentalToolStripMenuItem_Click(object sender, EventArgs e)
        {

			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];

			Skeleton skel = emo.Skeleton;

			EMG emg = GeometryIO.GrendgineCollada(out List<Node> tempnodes);
			
			for(int i = 0; i < tempnodes.Count; i++)
            {
				Node n = skel.Nodes[i];
				n.NodeMatrix = tempnodes[i].NodeMatrix;
				n.SkinBindPoseMatrix = tempnodes[i].SkinBindPoseMatrix;

				skel.Nodes.RemoveAt(i);
				skel.Nodes.Insert(i, n);
            }
			
			skel.GenerateBytes();

			emo.Skeleton = skel;
			emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
			emo.EMGs.Insert(SelectedEMGNumberInTree, emg);
			emo.GenerateBytes();
			WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
			WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);
			RefreshTree(false);
        }

        private void modelTextureGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
			if(e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
				if (matchingemb.FileNameList != null)
				{
					for(int i = 0; i < modelTextureGrid.Rows.Count - 1; i++)
                    {
						try
						{
							modelTextureGrid.Rows[i].Cells[2].ToolTipText = Encoding.ASCII.GetString(matchingemb.FileNameList[int.Parse((string)modelTextureGrid.Rows[i].Cells[2].Value)]).Replace('\0', ' ').Trim();
						}
						catch
                        {
							modelTextureGrid.Rows[i].Cells[2].ToolTipText = string.Empty;
						}
					}
				}
			}
        }

        private void renameMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMM emm = (EMM)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];
			Material mat = emm.Materials[LastSelectedTreeNode.Index];

			IB.StartPosition = FormStartPosition.CenterParent; IB.ShowDialog(this);

			if (IB.EnteredValue.Trim() != string.Empty)
			{
				string newMatName = IB.EnteredValue.Trim();
				string oldname = Encoding.ASCII.GetString(mat.Name).Replace("\0", "");

				foreach (TreeNode n in LastSelectedTreeNode.Parent.Nodes)
				{
					if (n.Text.Replace("\0", "") == newMatName)
					{
						MessageBox.Show("Name already exists!", TStrings.STR_Error);
						return;
					}
				}

				mat.Name = Utils.MakeModelName(IB.EnteredValue.Trim());
				mat.GenerateBytes();
				emm.Materials.RemoveAt(LastSelectedTreeNode.Index);
				emm.Materials.Insert(LastSelectedTreeNode.Index, mat);
				emm.GenerateBytes();
				WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
				WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, emm);

				RefreshTree(false);
				AddStatus($"Material '{oldname}' renamed to '{newMatName}' in {Encoding.ASCII.GetString(emm.Name).Replace("\0", "")}");
			}
			else
			{
				return;
			}
		}

        private void rawDumpEMGToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
			EMO emo = (EMO)WorkingEMZ.Files[LastSelectedTreeNode.Parent.Index];

			EMG emg = emo.EMGs[LastSelectedTreeNode.Index];

			emg.GenerateBytes();

			saveFileDialog1.Filter = EMGFileFilter;
			saveFileDialog1.FileName = $"{Encoding.ASCII.GetString(emo.Name)}_EMG_{LastSelectedTreeNode.Index}";
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, emg.HEXBytes);
				AddStatus($"Extracted {Encoding.ASCII.GetString(emo.Name)}_EMG_{LastSelectedTreeNode.Index}");
			}
		}

        private void addEMGToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMO emo = (EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index];

			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = EMGFileFilter;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				if (filepath.Trim() != string.Empty)
				{
					FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
					byte[] bytes;
					using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }

					emo.EMGs.Add(new EMG(bytes));
					emo.EMGCount = emo.EMGs.Count;
					emo.EMGPointersList.Add(0x00);
					emo.GenerateBytes();

					WorkingEMZ.Files.Remove(LastSelectedTreeNode.Index);
					WorkingEMZ.Files.Add(LastSelectedTreeNode.Index, emo);

					RefreshTree(false);
				}
			}

		}

		private void AddEMGtoPreview(EMO emo, EMG emg)
        {
			Node n = emo.Skeleton.Nodes[emg.RootBone];
			Matrix4x4 m = n.NodeMatrix;

			while(n.Parent != -1)
            {
				m = m * emo.Skeleton.Nodes[n.Parent].NodeMatrix;
				n = emo.Skeleton.Nodes[n.Parent];
			}

			matchingemb = new EMB();

			if (WorkingTEXEMZ != null && WorkingTEXEMZ.Files != null)
			{
				for (int i = 0; i < WorkingTEXEMZ.Files.Count; i++)
				{
					USF4File file = WorkingTEXEMZ.Files[i];
					if (file.GetType() == typeof(EMB))
					{
						byte[] targetemo = Utils.ChopByteArray(WorkingEMZ.FileNamesList[SelectedEMONumberInTree], 0x00, WorkingEMZ.FileNamesList[SelectedEMONumberInTree].Length - 1);
						byte[] targetemb = Utils.ChopByteArray(WorkingTEXEMZ.FileNamesList[i], 0x00, WorkingTEXEMZ.FileNamesList[i].Length - 1);

						if (Encoding.ASCII.GetString(targetemo) == Encoding.ASCII.GetString(targetemb))
						{
							matchingemb = (EMB)WorkingTEXEMZ.Files[i];
						}
					}
				}
			}

			//Gather mesh and texture data for each model
			for (int i = 0; i < emg.Models.Count; i++)
			{
				Point3DCollection pos = new Point3DCollection();
				Vector3DCollection norm = new Vector3DCollection();
				PointCollection tex = new PointCollection();

				for (int j = 0; j < emg.Models[i].VertexData.Count; j++)
				{
					Vertex v = emg.Models[i].VertexData[j];
					pos.Add(new Point3D(v.X, v.Y, v.Z));
					if ((emg.Models[i].BitFlag & 0x02) == 0x02) norm.Add(new Vector3D(v.nX, v.nY, v.nZ));
					tex.Add(new System.Windows.Point(v.U, v.V));
				}

				for (int j = 0; j < emg.Models[i].SubModels.Count; j++)
				{
					BitmapSource bitmapSource;
					int textureindex;
					DDS dds;
					List<int[]> tempfaceindices;
					Int32Collection faces = new Int32Collection();
					MaterialGroup mg = new MaterialGroup();
					List<int> ddsindices = new List<int>();

					bool readmode = (emg.Models[i].ReadMode == 1) ? false : true;

					tempfaceindices = GeometryIO.FaceIndicesFromDaisyChain(emg.Models[i].SubModels[j].DaisyChain, readmode);
					textureindex = emg.Models[i].SubModels[j].MaterialIndex;
					if (emg.Models[i].Textures != null && emg.Models[i].Textures.Count != 0)
					{
						for (int k = 0; k < emg.Models[i].Textures[textureindex].TextureIndicesList.Count; k++)
						{
							ddsindices.Add(emg.Models[i].Textures[textureindex].TextureIndicesList[k]);
						}
					}

					for (int k = 0; k < tempfaceindices.Count; k++)
					{
						faces.Add(tempfaceindices[k][2]);
						faces.Add(tempfaceindices[k][1]);
						faces.Add(tempfaceindices[k][0]);
					}

					//Try to convert the matched DDS file into BMP
					try
					{
						if (matchingemb.DDSFiles != null && ddsindices.Max() < matchingemb.DDSFiles.Count)
						{
							for (int k = 0; k < ddsindices.Count; k++)
							{
								dds = matchingemb.DDSFiles[ddsindices[k]];

								using (ImageEngineImage IE = new ImageEngineImage(dds.HEXBytes)) //Use IEI to convert dds bytes to System.Drawing.Image
								{
									Bitmap image = Utils.BitmapFromBytes(IE.Save(new ImageEngineFormatDetails(ImageEngineFormat.PNG), new MipHandling()));

									using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image)) //Use Interop to convert to a BitmapSource to use as a WPF brush
									{
										IntPtr hBitmap = bmp.GetHbitmap();
										try
										{
											bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
										}
										finally
										{
											DeleteObject(hBitmap);
										}
									}
								}

								double opacity = 1;

								if (k == 1) opacity = 0.5;

								mg.Children.Add(new DiffuseMaterial()
								{
									Brush = new System.Windows.Media.ImageBrush()
									{
										ImageSource = bitmapSource,
										TileMode = TileMode.Tile,
										Transform = new ScaleTransform(
											1d / emg.Models[i].Textures[textureindex].Scales_UList[k],
											1d / emg.Models[i].Textures[textureindex].Scales_VList[k]),
										ViewportUnits = BrushMappingMode.Absolute,
										Opacity = opacity
									}
								});
							}
						}
						else //Default to solid color Red if there's no matching dds
						{
							mg.Children.Add(new DiffuseMaterial()
							{
								Brush = new SolidColorBrush()
								{
									Color = System.Windows.Media.Color.FromRgb(255, 0, 0),
									Opacity = 1
								}
							});
						}
					}
					catch //Default to purple if an exception is thrown (eg bad DDS data)
					{
						mg.Children.Add(new DiffuseMaterial()
						{
							Brush = new SolidColorBrush()
							{
								Color = System.Windows.Media.Color.FromRgb(255, 0, 255),
								Opacity = 1
							}
						});
					}

					//Add the completed model before moving on to the next
					uc.AddModel(new GeometryModel3D()
					{
						Transform = new Transform3DGroup() 
						{ 
							Children = new Transform3DCollection() 
							{ 
								new MatrixTransform3D(new Matrix3D() 
								{ 
									M11 = m.M11, M12 = m.M12, M13 = m.M13, M14 = m.M14,
									M21 = m.M21, M22 = m.M22, M23 = m.M23, M24 = m.M24,
									M31 = m.M31, M32 = m.M32, M33 = m.M33, M34 = m.M34,
                                    OffsetX = m.M41, OffsetY = m.M42, OffsetZ = m.M43, M44 = m.M44,
								}),
								new ScaleTransform3D(-1,1,1),
								//new TranslateTransform3D(m.M41, m.M42, m.M43)
							} 
						},
						 //Needs un x-flipping AGAIN but at least Transform makes it easy this time...
						Geometry = new MeshGeometry3D()
						{
							Positions = pos,
							Normals = norm,
							TextureCoordinates = tex,
							TriangleIndices = faces
						},
						Material = mg,
					});
				}
			}
		}

		private void previewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EH3D.Visible = true;
			EH3D.BringToFront();

			uc.ClearModels();

			TreeNode node = LastSelectedTreeNode;
			//Get the EMO node
			while(!((string)node.Tag == "EMO"))
            {
				node = node.Parent;
            }

			EMO emo = (EMO)WorkingEMZ.Files[node.Index];
			EMG emg = emo.EMGs[SelectedEMGNumberInTree];

			AddEMGtoPreview(emo, emg);      
		}

        private void closePreviewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
			uc.ClearModels();
			EH3D.Visible = false;
        }

        private void btnEO_CompressSM_Click(object sender, EventArgs e)
        {
			EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
			EMG emg = emo.EMGs[SelectedEMGNumberInTree];
			Model model = emg.Models[SelectedModelNumberInTree];
			SubModel sm = model.SubModels[SelectedSubModelNumberInTree];

			using (CompressDaisyChain Compress = new CompressDaisyChain(sm))
			{
				if (Compress.ShowDialog(this) == DialogResult.OK)
				{
					model.SubModels.RemoveAt(SelectedSubModelNumberInTree);
					model.SubModels.Insert(SelectedSubModelNumberInTree, Compress.sm);
					emg.Models.RemoveAt(SelectedModelNumberInTree);
					emg.Models.Insert(SelectedModelNumberInTree, model);
					emg.GenerateBytes();
					emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
					emo.EMGs.Insert(SelectedEMGNumberInTree, emg);
					emo.GenerateBytes();
					WorkingEMZ.Files.Remove(SelectedEMONumberInTree);
					WorkingEMZ.Files.Add(SelectedEMONumberInTree, emo);
					TreeDisplaySubModelData(Compress.sm);
					AddStatus("Daisychain updated.");
				}
				else AddStatus("Compression cancelled.");
			}
        }

        private void previewEMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EH3D.Visible = true;
			EH3D.BringToFront();
			uc.ClearModels();

			EMO emo = (EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index];
			foreach (EMG emg in emo.EMGs)
            {
				AddEMGtoPreview(emo, emg);
            }

		}

        private void eMOToLibraryControllerToolStripMenuItem_Click(object sender, EventArgs e)
        {
			GeometryIO.EMOtoCollada_Library_Controller((EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index]);
        }

        private void eMOToLibraryGeometryToolStripMenuItem_Click(object sender, EventArgs e)
        {
			//GeometryIO.EMOtoCollada_Library_Geometries((EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index]);
			//test
			//GeometryIO.EMOtoCollada_Library_Controller((EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index]);

			//GeometryIO.EMOtoCollada_Library_Visual_Scenes((EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index]);

			//grendgine_collada.Grendgine_Collada template = Grendgine_Collada.Grendgine_Load_File("untitled.dae");

			EMA balrog_ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNode.Index - 2];

			grendgine_collada.Grendgine_Collada collada = new grendgine_collada.Grendgine_Collada()
			{
				Collada_Version = "1.4.1",
				Library_Controllers = GeometryIO.EMOtoCollada_Library_Controller((EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index]),
				Library_Geometries = GeometryIO.EMOtoCollada_Library_Geometries((EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index]),
				Library_Visual_Scene = GeometryIO.EMOtoCollada_Library_Visual_Scenes((EMO)WorkingEMZ.Files[LastSelectedTreeNode.Index]),
				Library_Animations = GeometryIO.EMAtoCollada_Library_Animations(balrog_ema)
			};

			//collada = Grendgine_Collada.Grendgine_Load_File("untitled.dae");

			try
			{
				Grendgine_Collada col_scenes = collada;

				XmlSerializer sr = new XmlSerializer(typeof(Grendgine_Collada));
				TextWriter tw = new StreamWriter("outputtest.dae");
				sr.Serialize(tw, col_scenes);

				tw.Close();

				
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.ReadLine();
				
			}
		}
    }
}


