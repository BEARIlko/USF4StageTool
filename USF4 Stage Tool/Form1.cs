using CSharpImageLibrary;
using grendgine_collada;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
using static CSharpImageLibrary.ImageFormats;
using static KopiLua.Lua;
using Quaternion = System.Numerics.Quaternion;

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
		public const string EMAFileFilter = "EMA (.ema)|*.ema";
		public const string EMMFileFilter = "EMM (.emm)|*.emm";
		public const string EMBFileFilter = "EMZ (.emb)|*.emb";
		public const string LUAFileFilter = "LUA (.lua)|*.lua";
		public const string TXTFileFilter = "Text file (.txt)|*.txt";
		public const string DDSFileFilter = "Direct Draw Surface (.dds)|*.dds";
		public const string CSBFileFilter = "Sound Bank (.csb)|*.csb";

		public int TotalNewVerts;
		public bool ObjectLoaded;
		public bool EncodingInProgress;
		public bool ShiftPressed;
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
		TreeNode LastSelectedTreeNodeU;
		TreeNode LastSelectedTreeNodeM;
		DateTime StartTime;     //Used for ETA/ETR Calculation

		EMB WorkingEMZ;
		EMB WorkingTEXEMZ;
		public List<USF4File> master_USF4FileList = new List<USF4File>();
		public List<ModelFile> master_ModelFileList = new List<ModelFile>();
		ElementHost EH3D = new ElementHost();
		HostingWpfUserControlInWf.UserControl1 uc = new HostingWpfUserControlInWf.UserControl1();
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

					master_ModelFileList.Add(new ObjFile().ReadFile(filepath, diagOpenOBJ.SafeFileName));

					RefreshTree(tvTreeModel);

					//await Task.Run(() => { ReadOBJ(filepath); });
					//if (ValidateOBJ())
					//{
					//	lbOBJNameProperty.Text = Path.GetFileName(filepath);
					//	await Task.Run(() =>
					//	{
					//		EncodeTheOBJ();
					//	});
					//}
					//else
					//{
					//	ClearUpStatus();
					//	MessageBox.Show("OBJ Encoding canceled!", TStrings.STR_Error);
					//}
				}
			}
		}

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

		#region Console Dump Methods

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
		private void BtnSaveHEXFile_Click(object sender, EventArgs e)
		{

		}

		//Open USF4 Modding Google DOC
		private void USF4ModdingDocumentToolStripMenuItem_Click(object sender, EventArgs e) { System.Diagnostics.Process.Start("https://docs.google.com/document/d/1dU-uFvhQksLNEzEc4OWpj9nfEGy7gh5-HwGZ8NsOiTY"); }
		private void userGuideToolStripMenuItem_Click(object sender, EventArgs e) { System.Diagnostics.Process.Start("https://docs.google.com/document/d/1MS_3PxLq1Q-zPYWutZz6AoTLeGhdUa_GJ_pm-rP6zOc"); }
		private void sourcecodeOnGitHubToolStripMenuItem_Click(object sender, EventArgs e) { System.Diagnostics.Process.Start("https://github.com/BEARIlko/USF4StageTool"); }


		Skeleton SkeletonFromSMD(SMDFile model)
		{
			Skeleton skel = new Skeleton
			{
				FFList = new List<byte[]>(),
				Nodes = new List<Node>(),
				NodeNamePointersList = new List<int>(),
				NodeNames = new List<string>(),
				//Declare generic skeleton values
				NodeCount = model.Skeleton.Nodes.Count,
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
			for (int i = 0; i < model.Skeleton.Nodes.Count; i++)
			{
				skel.FFList.Add(new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 });

				Node WorkingNode = new Node
				{
					Parent = model.Skeleton.Nodes[i].Parent,
					Child1 = -1,
					Sibling = -1,
					Child3 = -1,
					Child4 = -1,
					PreMatrixFloat = 0f
				};

				//Look forwards for a child, break loop when found
				for (int j = i + 1; j < model.Skeleton.Nodes.Count; j++)
				{
					if (model.Skeleton.Nodes[j].Parent == i) { WorkingNode.Child1 = j; break; }
				}
				//Look forwards for a sibling, break loop when found
				for (int j = i + 1; j < model.Skeleton.Nodes.Count; j++)
				{
					if (model.Skeleton.Nodes[j].Parent == WorkingNode.Parent) { WorkingNode.Sibling = j; break; }
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
				skel.NodeNames.Add(model.Skeleton.Nodes[i].Name);
				skel.NodeNamePointersList.Add(0);
			}

			skel.GenerateBytes();

			return skel;
		}

		#region Tree Methods
		public void ClearTree(TreeView tree) 
		{
			tree.Nodes.Clear();
		}

		void RefreshTree(TreeView tree)
		{
			var savedExpansionState = tree.Nodes.GetExpansionState();
			tree.BeginUpdate();
			ClearTree(tree);
			if (tree == tvTreeUSF4) FillTreeUSF4();
			else if (tree == tvTreeModel) FillTreeModel();

			tree.Nodes.SetExpansionState(savedExpansionState);
			
			//If either of the trees is empty, reset the relevant context menu
			if (master_USF4FileList == null || master_USF4FileList.Count == 0)
            {
				tvTreeUSF4.ContextMenuStrip = new ContextMenuStrip();
				tvTreeUSF4.ContextMenuStrip.Items.Add(new ToolStripMenuItem($"Open file...", null, cmUNIVopenFileToolStripMenuItem_Click));
			}

			if (master_ModelFileList == null || master_ModelFileList.Count == 0)
			{
				tvTreeModel.ContextMenuStrip = new ContextMenuStrip();
				tvTreeModel.ContextMenuStrip.Items.Add(new ToolStripMenuItem($"Open file...", null, cmMODopenFileToolStripMenuItem_Click));
			}

			tree.SelectedNode = TreeViewExtensions.SelectedNodeBeforeRefresh; //Not working??
			tree.EndUpdate();
		}

		public void FillTreeModel()
        {
			foreach (ModelFile mf in master_ModelFileList)
            {
				tvTreeModel.Nodes.Add(GenerateModelNode(mf));
            }
        }

		public TreeNode GenerateModelNode(ModelFile mf)
        {
			TreeNode n = new TreeNode()
			{
				Text = mf.Name,
				Tag = mf
			};

			if (mf.GetType() == typeof(ObjFile))
            {
				foreach (ObjObject o in ((ObjFile)mf).ObjObjects)
                {
					TreeNode oNode = new TreeNode()
					{
						Text = o.Name,
						Tag = o
					};
					foreach (ObjGroup g in o.ObjGroups)
                    {
						TreeNode gNode = new TreeNode()
						{
							Text = g.Name,
							Tag = g
						};

						foreach (ObjMaterial m in g.ObjMaterials)
                        {
							gNode.Nodes.Add(new TreeNode()
							{
								Text = m.Name,
								Tag = m
							});
                        }

						oNode.Nodes.Add(gNode);
					}
					n.Nodes.Add(oNode);
				}
            }

			if(mf.GetType() == typeof(SMDFile))
            {
				foreach (string s in ((SMDFile)mf).MaterialDictionary.Keys)
                {
					n.Nodes.Add(new TreeNode()
					{
						Text = s,
						Tag = s 
					});
                }

				TreeNode skelNode = new TreeNode()
				{
					Text = "Skeleton",
					Tag = ((SMDFile)mf).Skeleton
				};

				foreach (SMDNode sn in ((SMDFile)mf).Skeleton.Nodes)
                {
					skelNode.Nodes.Add(new TreeNode()
					{
						Text = $"{sn.ID} {sn.Name}",
						Tag = sn
					});
                }

				n.Nodes.Add(skelNode);
            }

			return n;
        }

		public void FillTreeUSF4()
		{
			foreach (USF4File uf in master_USF4FileList)
			{
				tvTreeUSF4.Nodes.Add(GenerateUSF4Node(uf));
			}
		}

		public TreeNode GenerateUSF4Node(USF4File uf)
		{
			TreeNode n = new TreeNode()
			{
				Text = uf.Name,
				Tag = uf
			};
			if (uf.GetType() == typeof(EMM))
			{
				EMM emm = (EMM)uf;
				foreach (Material m in emm.Materials)
				{
					n.Nodes.Add(new TreeNode()
					{
						Text = Encoding.ASCII.GetString(m.Name),
						Tag = m
					});
				}
			}
			else if (uf.GetType() == typeof(BSR))
			{
				BSR bsr = (BSR)uf;

				for (int i = 0; i < bsr.Physics.Count; i++)
                {
					TreeNode pn = new TreeNode()
					{
						Text = $"Physics Object {i}",
						Tag = bsr.Physics[i]
					};

					for (int j = 0; j < bsr.Physics[i].NodeDataBlocks.Count; j++)
                    {
						pn.Nodes.Add(new TreeNode()
						{
							Text = $"{bsr.NodeNames[bsr.Physics[i].NodeDataBlocks[j].ID]}",
							Tag = bsr.Physics[i].NodeDataBlocks[j]
						});
                    }
					for (int j = 0; j < bsr.Physics[i].LimitDataBlocks.Count; j++)
					{
						pn.Nodes.Add(new TreeNode()
						{
							Text = $"Limit {bsr.Physics[i].LimitDataBlocks[j].ID1} > {bsr.Physics[i].LimitDataBlocks[j].ID2}",
							Tag = bsr.Physics[i].LimitDataBlocks[j]
						});
					}

					n.Nodes.Add(pn);
                }

            }
			else if (uf.GetType() == typeof(EMB))
			{
				EMB emb = (EMB)uf;
				foreach (USF4File child in emb.Files)
				{
					n.Nodes.Add(GenerateUSF4Node(child));
				}
			}
			else if (uf.GetType() == typeof(EMO))
			{
				EMO emo = (EMO)uf;
				for (int i = 0; i < emo.EMGs.Count; i++)
				{
					EMG emg = emo.EMGs[i];
					TreeNode EMGn = new TreeNode()
					{
						Text = $"EMG {i}",
						Tag = emg
					};

					for (int j = 0; j < emg.Models.Count; j++)
					{
						Model mod = emg.Models[j];
						TreeNode Modn = new TreeNode()
						{
							Text = $"Model {j}",
							Tag = mod
						};

						for (int k = 0; k < mod.SubModels.Count; k++)
						{
							SubModel sm = mod.SubModels[k];
							TreeNode SMn = new TreeNode()
							{
								Text = Encoding.ASCII.GetString(sm.SubModelName).Replace('\0',' ').Trim(),
								Tag = sm
							};
							Modn.Nodes.Add(SMn);
						}
						EMGn.Nodes.Add(Modn);
					}
					n.Nodes.Add(EMGn);
				}

				if (emo.Skeleton.Nodes != null)
				{
					TreeNode nodeSkeleton = new TreeNode()
					{
						Text = "Skeleton",
						Tag = emo.Skeleton
					};
					for (int i = 0; i < emo.Skeleton.Nodes.Count; i++)
					{
						nodeSkeleton.Nodes.Add(new TreeNode()
						{
							Text = $"{i} {emo.Skeleton.NodeNames[i]}",
							Tag = emo.Skeleton.Nodes[i]
						});
					}

					n.Nodes.Add(nodeSkeleton);
				}
				else
				{
					n.Nodes.Add(new TreeNode()
					{
						Text = "No skeleton data",
						Tag = new Skeleton()
					});
				}
			}

			else if (uf.GetType() == typeof(EMA))
			{
				EMA ema = (EMA)uf;

				for (int i = 0; i < ema.Animations.Count; i++)
				{
					n.Nodes.Add(new TreeNode()
					{
						Text = $"{i} {Encoding.ASCII.GetString(ema.Animations[i].Name)}",
						Tag = ema.Animations[i]
					});
				}
				if (ema.Skeleton.Nodes != null)
				{
					TreeNode nodeSkeleton = new TreeNode()
					{
						Text = "Skeleton",
						Tag = ema.Skeleton
					};
					for (int i = 0; i < ema.Skeleton.Nodes.Count; i++)
					{
						nodeSkeleton.Nodes.Add(new TreeNode()
						{
							Text = $"{i} {ema.Skeleton.NodeNames[i]}",
							Tag = ema.Skeleton.Nodes[i]
						});
					}
					if (ema.Skeleton.IKNodes != null && ema.Skeleton.IKNodes.Count > 0)
					{
						for (int i = 0; i < ema.Skeleton.IKNodes.Count; i++)
						{
							nodeSkeleton.Nodes.Add(new TreeNode()
							{
								Text = $"{i} {ema.Skeleton.IKNodeNames[i]}",
								Tag = ema.Skeleton.IKNodes[i]
							});
						}
					}
					n.Nodes.Add(nodeSkeleton);
				}
				else
				{
					n.Nodes.Add(new TreeNode()
					{
						Text = "No skeleton data",
						Tag = new Skeleton()
					});
				}
			}
			return n;
		}

		private void TvTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			string title = string.Empty;
			string sep = " / ";
			LastSelectedTreeNodeU = tvTreeUSF4.SelectedNode;
			LastSelectedTreeNodeM = tvTreeModel.SelectedNode;
			pbPreviewDDS.Visible = false;
			pbPreviewDDS.SendToBack();

			//Hide edit panels
			pnlEO_EMO.Visible = false;
			pnlEO_EMG.Visible = false;
			pnlEO_MOD.Visible = false;
			pnlEO_SUBMOD.Visible = false;
			pnlEO_MaterialEdit.Visible = false;
			lbSelNODE_ListData.Visible = true;


			ContextMenuStrip newCM = new ContextMenuStrip();

			if (sender == tvTreeUSF4)
			{
				//Find top level node for current item
				TreeNode n = LastSelectedTreeNodeU;
				while (n.Parent != null) n = n.Parent;
				string topnode = n.Text;

				//Do the selected index stuff
				if (e.Node.Tag.GetType() == typeof(EMB))
				{
					ToolStripMenuItem tsmiAddFile = new ToolStripMenuItem($"Add file to EMB...");
						tsmiAddFile.DropDownItems.Add(new ToolStripMenuItem("EMA", null, cmEMBaddFileToolStripMenuItem_Click));
						tsmiAddFile.DropDownItems.Add(new ToolStripMenuItem("EMM", null, cmEMBaddFileToolStripMenuItem_Click));
						tsmiAddFile.DropDownItems.Add(new ToolStripMenuItem("EMO", null, cmEMBaddFileToolStripMenuItem_Click));
						tsmiAddFile.DropDownItems.Add(new ToolStripMenuItem("EMB", null, cmEMBaddFileToolStripMenuItem_Click));
						tsmiAddFile.DropDownItems.Add(new ToolStripMenuItem("DDS", null, cmEMBaddFileToolStripMenuItem_Click));
						tsmiAddFile.DropDownItems.Add(new ToolStripMenuItem("LUA", null, cmEMBaddFileToolStripMenuItem_Click));
						tsmiAddFile.DropDownItems.Add(new ToolStripMenuItem("CSB", null, cmEMBaddFileToolStripMenuItem_Click));
					newCM.Items.Add(tsmiAddFile);
					title = e.Node.Text;
				}
				if (e.Node.Tag.GetType() == typeof(EMM))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Add new Material to {LastSelectedTreeNodeU.Text}", null, cmEMMaddNewMaterialToolStripMenuItem_Click));
					title = e.Node.Text;
					TreeDisplayEMMData((EMM)e.Node.Tag);
				}
				if (e.Node.Tag.GetType() == typeof(Material))
				{
					pnlEO_MaterialEdit.Visible = true;
					lbSelNODE_ListData.Visible = false;
					newCM.Items.Add(new ToolStripMenuItem($"Add new Material to {LastSelectedTreeNodeU.Parent.Text}", null, cmEMMaddNewMaterialToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Rename {LastSelectedTreeNodeU.Text}", null, cmMATrenameMaterialToolStripMenuItem_Click));

					title = e.Node.Text;
					TreeDisplayEMMDetails((Material)e.Node.Tag);
				}
				if (e.Node.Tag.GetType() == typeof(PhysNode))
                {
					title = e.Node.Text;
					TreeDisplayPhysNodeData((PhysNode)e.Node.Tag);
                }
				if (e.Node.Tag.GetType() == typeof(LimitData))
				{
					title = e.Node.Text;
					TreeDisplayLimitData((LimitData)e.Node.Tag);
				}
				if (e.Node.Tag.GetType() == typeof(EMA))
				{
					title = e.Node.Text;
					TreeDisplayEMAData((EMA)e.Node.Tag);
				}
				if (e.Node.Tag.GetType() == typeof(Animation))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Export {LastSelectedTreeNodeU.Text} to Collada", null, cmEMAexportToColladaToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Generate KFs {LastSelectedTreeNodeU.Parent.Text}", null, cmEMAgenerateKeyFramesToolStripMenuItem_Click));
					
					title = e.Node.Text;
					TreeDisplayAnimationData((Animation)e.Node.Tag);
				}
				if (e.Node.Tag.GetType() == typeof(DDS))
				{
					if (e.Node.Parent != null)
					{
						newCM.Items.Add(new ToolStripMenuItem($"Add DDS to {LastSelectedTreeNodeU.Parent.Text}", null, cmEMBaddDDSToolStripMenuItem1_Click));
						newCM.Items.Add(new ToolStripMenuItem($"Inject DDS into {LastSelectedTreeNodeU.Text}", null, cmDDSinjectDDSToolStripMenuItem1_Click));
					}
					newCM.Items.Add(new ToolStripMenuItem($"Rename {LastSelectedTreeNodeU.Text}", null, cmDDSrenameDDSToolStripMenuItem_Click));
					title = e.Node.Text;
					try
					{
						pbPreviewDDS.Visible = true;
						pbPreviewDDS.BringToFront();
						DDS dds = (DDS)e.Node.Tag;
						ImageEngineImage IE = new ImageEngineImage(dds.HEXBytes);
						pbPreviewDDS.BackgroundImage = Utils.BitmapFromBytes(IE.Save(new ImageEngineFormatDetails(ImageEngineFormat.PNG), new MipHandling()));
						IE.Dispose();
					}
					catch
					{
						AddStatus($"Error while trying to read this DDS.");
					}
				}
				if (e.Node.Tag.GetType() == typeof(LUA))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Inject LUA script into {LastSelectedTreeNodeU.Text}", null, cmLUAinjectLUAScriptToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Extract LUA script from {LastSelectedTreeNodeU.Text}", null, cmLUAextractLUAScriptToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Inject LUA bytecode into {LastSelectedTreeNodeU.Text}", null, cmLUAinjectLUAbytecodeToolStripMenuItem_Click));
					title = e.Node.Text;
					lbSelNODE_ListData.Items.Clear();
				}
				if (e.Node.Tag.GetType() == typeof(CSB))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Inject CSB into {LastSelectedTreeNodeU.Text}", null, cmCSBinjectCSBToolStripMenuItem_Click));
					title = e.Node.Text;
					lbSelNODE_ListData.Items.Clear();
				}
				if (e.Node.Tag.GetType() == typeof(EMO))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Add OBJ as new EMG in {LastSelectedTreeNodeU.Text}", null, cmEMOinsertOBJAsNewEMGToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Add EMG to {LastSelectedTreeNodeU.Text}", null, cmEMOaddEMGToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Export {LastSelectedTreeNodeU.Text} to OBJ", null, cmEMOexportEMOToOBJToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Export {LastSelectedTreeNodeU.Text} to SMD", null, cmEMOextractEMOtoSMDToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Preview {LastSelectedTreeNodeU.Text}", null, cmEMOpreviewEMOToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Expand {LastSelectedTreeNodeU.Text}", null, cmEMOexpandAllToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Collapse {LastSelectedTreeNodeU.Text}", null, cmEMOcollapseAllToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Preview Skeleton {LastSelectedTreeNodeU.Text}", null, cmEMOpreviewSkeletonToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Export {LastSelectedTreeNodeU.Text} to Collada", null, cmEMOemoToColladaToolStripMenuItem_Click));

					pnlEO_EMO.Visible = true;

					TreeDisplayEMOData((EMO)e.Node.Tag);
					title = e.Node.Text;
				}
				if (e.Node.Tag.GetType() == typeof(EMG))
				{
					ToolStripMenuItem tsmiInjectFile = new ToolStripMenuItem($"Inject model into {LastSelectedTreeNodeU.Text} from...");
					foreach (ModelFile f in master_ModelFileList)
                    {
						if(f.GetType() == typeof(ObjFile))
                        {
							tsmiInjectFile.DropDownItems.Add(new ToolStripMenuItem($"{f.Name}..."));
							foreach (ObjObject o in ((ObjFile)f).ObjObjects)
                            {
								ToolStripMenuItem tsmiSelectedObjObject = new ToolStripMenuItem(o.Name, null, cmEMGinjectOBJToolStripMenuItem_Click);
								tsmiSelectedObjObject.Tag = o;
								((ToolStripMenuItem)tsmiInjectFile.DropDownItems[tsmiInjectFile.DropDownItems.Count-1]).DropDownItems.Add(tsmiSelectedObjObject);
							}
                        }
						else if (f.GetType() == typeof(SMDFile))
                        {
							tsmiInjectFile.DropDownItems.Add(new ToolStripMenuItem(f.Name, null, cmEMGinjectOBJToolStripMenuItem_Click));
						}
                    }
					newCM.Items.Add(tsmiInjectFile);

					if (e.Node.Parent != null)
					{
						newCM.Items.Add(new ToolStripMenuItem($"Add OBJ as new EMG in {LastSelectedTreeNodeU.Parent.Text}", null, cmEMGaddOBJAsNewEMGToolStripMenuItem_Click));
						title = $"{e.Node.Parent.Text}{sep}{e.Node.Text}";
					}
					else title = $"{e.Node.Text}";
					newCM.Items.Add(new ToolStripMenuItem($"Export {LastSelectedTreeNodeU.Text} to OBJ", null, cmEMGexportEMGAsOBJToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Preview {LastSelectedTreeNodeU.Text}", null, cEMGpreviewToolStripMenuItem_Click));
					TreeDisplayEMGData((EMG)e.Node.Tag);
					pnlEO_EMG.Visible = true;

				}
				if (e.Node.Tag.GetType() == typeof(Model))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Export {LastSelectedTreeNodeU.Text} to OBJ", null, cmMODexportModelAsOBJToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Preview {LastSelectedTreeNodeU.Text}", null, cEMGpreviewToolStripMenuItem_Click));
					TreeDisplayModelData((Model)e.Node.Tag);
					pnlEO_MOD.Visible = true;
					title = $"{e.Node.Parent.Parent.Text}{sep}{e.Node.Parent.Text}{sep}{e.Node.Text}";
				}
				if (e.Node.Tag.GetType() == typeof(SubModel))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Export {LastSelectedTreeNodeU.Text} to OBJ", null, cmSUBMexportSubmodelAsOBJToolStripMenuItem_Click));
					newCM.Items.Add(new ToolStripMenuItem($"Preview {LastSelectedTreeNodeU.Text}", null, cEMGpreviewToolStripMenuItem_Click));
					TreeDisplaySubModelData((SubModel)e.Node.Tag);
					pnlEO_SUBMOD.Visible = true;
					tbEO_SubModName.Text = $"{e.Node.Text}";
					tbEO_SubModMaterial.Text = $"{((SubModel)e.Node.Tag).MaterialIndex}";
					title = $"{e.Node.Text}";
				}
				if (e.Node.Tag.GetType() == typeof(Skeleton))
				{
					TreeDisplaySkeletonData((Skeleton)e.Node.Tag);
					title = $"{e.Node.Parent.Text} Skeleton";
				}
				if (e.Node.Tag.GetType() == typeof(Node))
				{

					TreeDisplaySkelNodeData((Node)e.Node.Tag);

					title = e.Node.Text;
				}
				if (e.Node.Tag.GetType() == typeof(IKNode))
				{
					TreeDisplaySkelIKNodeData((IKNode)e.Node.Tag);

					title = e.Node.Text;
				}

				//Set up local node universal options, if appropriate
				//If current node is a USF4 file, we can dump it:
				if (typeof(USF4File).IsAssignableFrom(LastSelectedTreeNodeU.Tag.GetType()))
                {
					newCM.Items.Add(new ToolStripMenuItem($"Rename {LastSelectedTreeNodeU.Text}", null, cmUNIrenameFileToolStripMenuItem_Click));
                }
				if (LastSelectedTreeNodeU.Parent != null && typeof(USF4File).IsAssignableFrom(LastSelectedTreeNodeU.Tag.GetType()))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Raw dump {LastSelectedTreeNodeU.Text}", null, cmUNIVrawDumpFileToolStripMenuItem_Click));
				}
				//If current node PARENT is a USF4 file, we can call generic USF4File.DeleteSubfile():
				if (LastSelectedTreeNodeU.Tag.GetType() != typeof(Skeleton) && LastSelectedTreeNodeU.Parent != null && typeof(USF4File).IsAssignableFrom(LastSelectedTreeNodeU.Parent.Tag.GetType()))
				{
					newCM.Items.Add(new ToolStripMenuItem($"Delete {LastSelectedTreeNodeU.Text}", null, cmUNIVdeleteFileToolStripMenuItem_Click));
				}
				//Add seperator before listing "top node" options
				if (newCM.Items.Count > 0) newCM.Items.Add("-");
				//Setup "top node" universal options
				newCM.Items.Add(new ToolStripMenuItem($"Save {topnode}", null, cmUNIVsaveFileToolStripMenuItem_Click));
				newCM.Items.Add(new ToolStripMenuItem($"Close {topnode}", null, cmUNIVcloseFileToolStripMenuItem_Click));
				newCM.Items.Add("-");
				newCM.Items.Add(new ToolStripMenuItem($"Open file...", null, cmUNIVopenFileToolStripMenuItem_Click));

				tvTreeUSF4.ContextMenuStrip = newCM;
			}
			else if (sender == tvTreeModel)
            {
				//Find top level node for current item
				TreeNode n = LastSelectedTreeNodeM;
				while (n.Parent != null) n = n.Parent;
				string topnode = n.Text;

				if (e.Node.Tag.GetType() == typeof(ObjFile) || e.Node.Tag.GetType() == typeof(SMDFile))
				{
					ToolStripMenuItem tsmiGenEMO = new ToolStripMenuItem($"Generate EMO with vertex size...");
						tsmiGenEMO.DropDownItems.Add(new ToolStripMenuItem("0x14", null, cmMODgenerateEMOToolStripMenuItem_Click));
						tsmiGenEMO.DropDownItems.Add(new ToolStripMenuItem("0x18", null, cmMODgenerateEMOToolStripMenuItem_Click));
						tsmiGenEMO.DropDownItems.Add(new ToolStripMenuItem("0x20", null, cmMODgenerateEMOToolStripMenuItem_Click));
						tsmiGenEMO.DropDownItems.Add(new ToolStripMenuItem("0x24", null, cmMODgenerateEMOToolStripMenuItem_Click));
						tsmiGenEMO.DropDownItems.Add(new ToolStripMenuItem("0x28", null, cmMODgenerateEMOToolStripMenuItem_Click));
						tsmiGenEMO.DropDownItems.Add(new ToolStripMenuItem("0x34", null, cmMODgenerateEMOToolStripMenuItem_Click));
						tsmiGenEMO.DropDownItems.Add(new ToolStripMenuItem("0x40", null, cmMODgenerateEMOToolStripMenuItem_Click));
					newCM.Items.Add(tsmiGenEMO);
					title = e.Node.Text;
				}
				newCM.Items.Add("-");
				newCM.Items.Add(new ToolStripMenuItem($"Close {topnode}", null, cmMODcloseFileToolStripMenuItem_Click));
				newCM.Items.Add("-");
				newCM.Items.Add(new ToolStripMenuItem($"Open file...", null, cmMODopenFileToolStripMenuItem_Click));

				tvTreeModel.ContextMenuStrip = newCM;
			}

			lbSelNODE_Title.Text = title;
		}

		private void TvTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (sender == tvTreeUSF4) tvTreeUSF4.SelectedNode = tvTreeUSF4.GetNodeAt(e.Location);
			else if (sender == tvTreeModel) tvTreeModel.SelectedNode = tvTreeModel.GetNodeAt(e.Location);
			//if (e.Button == MouseButtons.Right) { CM.Show(tvTreeUSF4, e.Location); }
		}

		void TreeDisplayEMMDetails(Material mat)
		{
			lvShaderProperties.Clear();
			Utils.ReadShaders();
			Utils.ReadShadersProperties();
			FillShaderComboBox();
			FillShaderPropertiesComboBox();
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
		void TreeDisplayPhysNodeData(PhysNode pn)
        {
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"ID: {pn.ID}");
			lbSelNODE_ListData.Items.Add($"Float: {pn.LimitValues[0].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Float: {pn.LimitValues[1].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Float: {pn.LimitValues[2].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Float: {pn.LimitValues[3].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Float: {pn.LimitValues[4].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Float: {pn.LimitValues[5].ToString("0.000000")}");
		}
		void TreeDisplayLimitData(LimitData ld)
        {
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Link: {ld.ID1} > {ld.ID2}");
			lbSelNODE_ListData.Items.Add($"Bitflag: {ld.bitflag}");
			lbSelNODE_ListData.Items.Add($"Limit Value: {ld.LimitValues[0].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Limit Value: {ld.LimitValues[1].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Limit Value: {ld.LimitValues[2].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Limit Value: {ld.LimitValues[3].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Limit Value: {ld.LimitValues[4].ToString("0.000000")}");
			lbSelNODE_ListData.Items.Add($"Limit Value: {ld.LimitValues[5].ToString("0.000000")}");
		}
		void TreeDisplaySkeletonData(Skeleton skel)
        {
			lbSelNODE_ListData.Items.Clear();

			if (skel.Nodes != null)
			{
				for (int i = 0; i < skel.Nodes.Count; i++)
				{
					lbSelNODE_ListData.Items.Add($"{i}:\t{skel.NodeNames[i]}");
				}
			}

			if (skel.IKDataBlocks != null)
            {
				for (int i = 0; i < skel.IKDataBlocks.Count; i++)
                {
					string ikstring = string.Empty;
					for (int j = 0; j < skel.IKDataBlocks[i].IKShorts.Count; j++)
                    {
						ikstring += $"{skel.IKDataBlocks[i].IKShorts[j]} ";
                    }

					lbSelNODE_ListData.Items.Add($"IK {i}: {ikstring}");
				}
            }
        }

		void AddPropertyLineToTextBox(string Line)
		{
			lvShaderProperties.Text = lvShaderProperties.Text + Line + Environment.NewLine;
		}

		void OpenEMONode(bool SelectTheEMG) //false for deleting so we select only the EMO
		{
			//tvTree.SelectedNode = LastSe; //FIX
			//tvTree.SelectedNode.Expand();
			//if (SelectTheEMG) tvTree.SelectedNode = tvTree.SelectedNode.Nodes[SelectedEMGNumberInTree];
		}
		#endregion Tree Methods

		#region Tree Display Related

		void TreeDisplayEMOData(EMO emo)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"EMG Count: {emo.EMGCount}");
			lbSelNODE_ListData.Items.Add($"Matching material Count: {emo.NumberEMMMaterials}");
			lbSelNODE_ListData.Items.Add($"Skeleton node count: {emo.Skeleton.NodeCount}");
			tbEMOmatcount.Text = $"{emo.NumberEMMMaterials}";
		}
		void TreeDisplayEMAData(EMA ema)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Animation Count: {ema.AnimationCount}");
			lbSelNODE_ListData.Items.Add($"Skeleton node count: {ema.Skeleton.NodeCount}");
		}
		void TreeDisplayAnimationData(Animation a)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Duration: {a.Duration}");
			lbSelNODE_ListData.Items.Add($"CMD Track Count: {a.CMDTracks.Count}");
			lbSelNODE_ListData.Items.Add($"Value Count: {a.ValueCount}");
			lbSelNODE_ListData.Items.Add($"CMD Tracks:");

			Skeleton s = ((EMA)LastSelectedTreeNodeU.Parent.Tag).Skeleton;



			for (int i = 0; i < a.CMDTracks.Count; i++)
			{
				string axis;
				if ((a.CMDTracks[i].BitFlag & 0x03) == 0x00) axis = "X";
				else if ((a.CMDTracks[i].BitFlag & 0x03) == 0x01) axis = "Y";
				else axis = "Z";

				string ttype;
				if (a.CMDTracks[i].TransformType == 0x00) ttype = "T";
				else if (a.CMDTracks[i].TransformType == 0x01) ttype = "R";
				else ttype = "S";

				if (s.Nodes != null)
				{
						lbSelNODE_ListData.Items.Add($"{i} - {ttype} / {axis} / {a.CMDTracks[i].BoneID} {s.NodeNames[a.CMDTracks[i].BoneID]} / {String.Format("{0:X}", a.CMDTracks[i].BitFlag)}");
				}
				else
				{
					lbSelNODE_ListData.Items.Add($"{i} - {ttype} / {axis} / NA / {String.Format("{0:X}", a.CMDTracks[i].BitFlag)}");
				}
			}
		}
		void TreeDisplaySkelNodeData(Node node)
		{
			Skeleton s = (Skeleton)LastSelectedTreeNodeU.Parent.Tag;

			Node pnode = new Node();

			if (node.Parent != -1)
			{
				pnode = s.Nodes[node.Parent];
			}

			Matrix4x4 iRest = Matrix4x4.Transpose(node.SkinBindPoseMatrix);

			Matrix4x4.Invert(pnode.SkinBindPoseMatrix, out Matrix4x4 pnodeInvSBP);

			pnodeInvSBP = Matrix4x4.Transpose(pnodeInvSBP);

			int SiblingOf = -1;

			for (int i = 0; i < s.NodeCount; i++)
            {
				if (s.Nodes[i].Sibling == LastSelectedTreeNodeU.Index) SiblingOf = i;
            }

			string P = string.Empty;
			string C1 = string.Empty;
			string S = string.Empty;
			string C3 = string.Empty;
			string C4 = string.Empty;
			string SO = string.Empty;

			if (node.Parent != -1) P = s.NodeNames[node.Parent];
			if (node.Child1 != -1) C1 = s.NodeNames[node.Child1];
			if (node.Child3 != -1) C3 = s.NodeNames[node.Child3];
			if (node.Child4 != -1) C4 = s.NodeNames[node.Child4];
			if (node.Sibling != -1) S = s.NodeNames[node.Sibling];
			if (SiblingOf != -1) SO = s.NodeNames[SiblingOf];

			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Parent ID: {node.Parent} {P}");
			lbSelNODE_ListData.Items.Add($"Child ID: {node.Child1} {C1}");
			lbSelNODE_ListData.Items.Add($"Child ID3: {node.Child3} {C3}");
			lbSelNODE_ListData.Items.Add($"Child ID4: {node.Child4} {C4}");
			lbSelNODE_ListData.Items.Add($"Sibling ID: {node.Sibling} {S}");
			lbSelNODE_ListData.Items.Add($"Sibling of...: {SiblingOf} {SO}");
			lbSelNODE_ListData.Items.Add($"Bitflag: {node.BitFlag}");
			lbSelNODE_ListData.Items.Add($"PreMartix Float: {node.PreMatrixFloat}");
			lbSelNODE_ListData.Items.Add($"PreMartix Float: {node.PreMatrixFloat}");

			Matrix4x4 original_matrix = node.SkinBindPoseMatrix;

			Matrix4x4 m = node.NodeMatrix;

			string FFstring = string.Empty;
			byte[] ff = s.FFList[LastSelectedTreeNodeU.Index];
			foreach (byte b in ff)
            {
				FFstring += $" {b}";
            }
			lbSelNODE_ListData.Items.Add($"FF: {FFstring}");


			//Utils.DecomposeMatrixNaive(node.NodeMatrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz);
			Utils.DecomposeMatrixToDegrees(node.NodeMatrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out _, out _, out _);
			lbSelNODE_ListData.Items.Add($"Position: ({tx.ToString("0.000000")}, {ty.ToString("0.000000")}, {tz.ToString("0.000000")})");
			lbSelNODE_ListData.Items.Add($"Rotation: ({rx.ToString("0.000000")}, {ry.ToString("0.000000")}, {rz.ToString("0.000000")})");

			Matrix4x4.Invert(node.SkinBindPoseMatrix, out Matrix4x4 inverse);

			Utils.DecomposeMatrixToDegrees(inverse, out tx, out ty, out tz, out rx, out ry, out rz, out _, out _, out _);
			lbSelNODE_ListData.Items.Add($"SBP Position: ({tx.ToString("0.0000")}, {ty.ToString("0.0000")}, {tz.ToString("0.0000")})");
			lbSelNODE_ListData.Items.Add($"SBP Rotation: ({rx.ToString("0.0000")}, {ry.ToString("0.0000")}, {rz.ToString("0.0000")})");
		}
		void TreeDisplaySkelIKNodeData(IKNode iknode)
        {
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Nodes?:");
			Skeleton s = (Skeleton)LastSelectedTreeNodeU.Parent.Tag;
			for (int i = 0; i < iknode.BoneList.Count; i++)
            {
				lbSelNODE_ListData.Items.Add($"{i} {s.NodeNames[iknode.BoneList[i]]} {s.Nodes[iknode.BoneList[i]].BitFlag}");
			}
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
				for (int j = 0; j < m.Textures[i].TextureLayers; j++)
				{
					if (j == m.Textures[i].TextureLayers - 1) s += $"{m.Textures[i].TextureIndicesList[j]}";
					else s += $"{m.Textures[i].TextureIndicesList[j]} / ";
				}
				lbSelNODE_ListData.Items.Add($"Texture: {i} Index: {s}");
			}

			//Try to find the EMB matching the current model...
			matchingemb = new EMB();

			//Find the parent EMO, if there is one:
			TreeNode n = LastSelectedTreeNodeU;
			while (n.Tag.GetType() != typeof(EMO) && n.Parent != null)
			{
				n = n.Parent;
			}
			//If we found an EMO, search the file list for a matching EMB
			if (n.Tag.GetType() == typeof(EMO))
			{
				USF4File result = FindMatch(master_USF4FileList, typeof(EMB), ((EMO)n.Tag).Name);
				if (result.GetType() == typeof(EMB)) matchingemb = (EMB)result;
			}

			//Clear the grid and re-populate
			modelTextureGrid.Rows.Clear();
			if (m.Textures.Count > 0)
			{
				for (int i = 0; i < m.TextureCount; i++)
				{
					for (int j = 0; j < m.Textures[i].TextureLayers; j++)
					{
						string[] row = { $"{i}", $"{j}", $"{m.Textures[i].TextureIndicesList[j]}", $"{m.Textures[i].Scales_UList[j].ToString("0.00")}", $"{m.Textures[i].Scales_VList[j].ToString("0.00")}" };
						modelTextureGrid.Rows.Add(row);
						//If we have a matching EMB, set the tooltip to be the indexed DDS name
						try
						{
							if (matchingemb.FileNamesList != null)
							{
								modelTextureGrid.Rows[i + j].Cells[2].ToolTipText = matchingemb.FileNamesList[m.Textures[i].TextureIndicesList[j]].Replace('\0', ' ').Trim();
							}
						}//If the index is out of range (ie higher than number of DDSs in the emb) catch and clear the string
						catch { modelTextureGrid.Rows[i + j].Cells[2].ToolTipText = string.Empty; }
					}
				}
			}
			lbSelNODE_ListData.Items.Add($"Vertex Count: {m.VertexCount}");
			//lbSelNODE_ListData.Items.Add($"Face encoding mode: {m.ReadMode}");
		}

		void TreeDisplaySubModelData(SubModel sm)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Bone Integers Count: {sm.BoneIntegersCount}");
			lbSelNODE_ListData.Items.Add($"Daisy Chain Length: {sm.DaisyChainLength}");
			lbSelNODE_ListData.Items.Add($"Daisy Chain Compression: {100 * sm.DaisyChainLength / (GeometryIO.FaceIndicesFromDaisyChain(sm.DaisyChain).Count * 3)}%");
			lbSelNODE_ListData.Items.Add($"Material Index: {sm.MaterialIndex}");

			if (sm.BoneIntegersList != null && sm.BoneIntegersList.Count > 0)
			{
				Skeleton skel = new Skeleton();
				if(LastSelectedTreeNodeU.Parent.Parent.Parent != null)
                {
					skel = ((EMO)LastSelectedTreeNodeU.Parent.Parent.Parent.Tag).Skeleton;
                }
				lbSelNODE_ListData.Items.Add($"Bone integers:");
				foreach (int i in sm.BoneIntegersList)
				{
					lbSelNODE_ListData.Items.Add($"{i}:\t{skel.NodeNames[i]}");
				}
			}
		}

		void TreeDisplayEMMData(EMM e)
		{
			lbSelNODE_ListData.Items.Clear();
			lbSelNODE_ListData.Items.Add($"Materials Count: {e.MaterialCount}");
		}
		#endregion Tree Display Related		

		public USF4File FindMatch(List<USF4File> target_list, Type type, string t_name)
		{
			USF4File match = new USF4File();

			string target_name = t_name.Substring(0, t_name.Length - 1);

			foreach (USF4File uf in target_list)
			{
				string check_name = uf.Name.Substring(0, uf.Name.Length - 1);
				if (uf.GetType() == type && check_name == target_name)
				{
					return uf;
				}

				if (uf.GetType() == typeof(EMB))
				{
					match = FindMatch(((EMB)uf).Files, type, t_name);
					if (match.GetType() == type)
					{
						return match;
					}
				}
			}
			return match;
		}

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

							byte[] recompress = Utils.Compress(bytes);

						}

						if (filepath.Contains("tex.emz"))
						{
							WorkingTEXEMZ = new EMB(bytes, diagOpenOBJ.SafeFileName);
							TargetTEXEMZFilePath = filepath;
							TargetTEXEMZFileName = friendlyName;
							RefreshTree(tvTreeUSF4);
							AddStatus($"Opened {filepath}");
						}
						else
						{
							WorkingEMZ = new EMB(bytes, diagOpenOBJ.SafeFileName);
							TargetEMZFilePath = filepath;
							TargetEMZFileName = friendlyName;
							FillShaderComboBox();
							FillShaderPropertiesComboBox();
							Utils.SaveShaders();
							Utils.SaveShadersProperties();
							RefreshTree(tvTreeUSF4);
							AddStatus($"Opened {filepath}");
						}
					}
					catch
					{
						MessageBox.Show("Error opening EMZ. Please confirm file is valid.", TStrings.STR_Information);
						if (filepath.Contains("tex.emz")) { WorkingTEXEMZ = new EMB(); }
						else { WorkingEMZ = new EMB(); }
						RefreshTree(tvTreeUSF4);
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

			EMB targetEMZ;
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

		void cmEMGinjectOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ObjObject o = (ObjObject)((ToolStripMenuItem)sender).Tag;

			EMG emg = (EMG)LastSelectedTreeNodeU.Tag;

			emg = o.GenerateEMG(emg.Models[0].BitDepth, emg.RootBone);

			if (LastSelectedTreeNodeU.Parent != null)
            {
				EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Tag;
				emo.EMGs.RemoveAt(LastSelectedTreeNodeU.Index);
				emo.EMGs.Insert(LastSelectedTreeNodeU.Index, emg);
				emo.GenerateBytes();
            }

			RefreshTree(tvTreeUSF4);
		}

		void InjectOBJ()
		{
			
		}

		void InjectCSB()
		{
			diagOpenOBJ.Filter = CSBFileFilter;
			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				CSB csb = (CSB)LastSelectedTreeNodeU.Tag;
				FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
				byte[] bytes;
				using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }
				csb.HEXBytes = bytes;
				
				if(LastSelectedTreeNodeU.Parent != null)
                {
					EMB emb = (EMB)LastSelectedTreeNodeU.Parent.Tag;
					emb.Files.RemoveAt(LastSelectedTreeNodeU.Index);
					emb.Files.Insert(LastSelectedTreeNodeU.Index, csb);
                }

				AddStatus($"Injected {diagOpenOBJ.SafeFileName} into {LastSelectedTreeNodeU.Text}");
				RefreshTree(tvTreeUSF4);
			}
		}

		private void cmEMGaddOBJAsNewEMGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddOBJAsNewEMG();
		}

		private void AddOBJAsNewEMG()
		{
		}

		private void BntEO_EMGSave_Click(object sender, EventArgs e)
		{
			if (tbEMGRootBone.Text.Trim() != string.Empty)
			{
				int newRootBone = int.Parse(tbEMGRootBone.Text.Trim());
				EMG emg = (EMG)LastSelectedTreeNodeU.Tag;
				emg.RootBone = newRootBone;
				emg.GenerateBytes();

				if(LastSelectedTreeNodeU.Parent != null)
                {
					EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Tag;
					emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
					emo.EMGs.Insert(SelectedEMGNumberInTree, emg);
					emo.GenerateBytes();
                }
				AddStatus("EMG changes saved.");
				TreeDisplayEMGData(emg);
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			pnlEO_EMO.Visible = false;
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

			tvTreeUSF4.ContextMenuStrip = new ContextMenuStrip();
			tvTreeUSF4.ContextMenuStrip.Items.Add(new ToolStripMenuItem($"Open file...", null, cmUNIVopenFileToolStripMenuItem_Click));
			tvTreeModel.ContextMenuStrip = new ContextMenuStrip();
			tvTreeModel.ContextMenuStrip.Items.Add(new ToolStripMenuItem($"Open file...", null, cmMODopenFileToolStripMenuItem_Click));

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
				injectColladaAsEMGExperimentalToolStripMenuItem.Visible = false;
				eMOToLibraryControllerToolStripMenuItem.Visible = false;
				eMOToLibraryGeometryToolStripMenuItem.Visible = false;
				setColourToolStripMenuItem.Visible = false;
				closePreviewWindowToolStripMenuItem.Visible = false;
				
#endif

			lbSelNODE_Title.Text = string.Empty;
			IB = new InputBox();
			Utils.ReadShaders();
			Utils.ReadShadersProperties();


		}

		private void BntEO_ModSave_Click(object sender, EventArgs e)
		{
			EMG emg = (EMG)LastSelectedTreeNodeU.Parent.Tag;
			Model model = (Model)LastSelectedTreeNodeU.Tag;

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
					if (Layers > 2) { AddStatus("Texture entry can't have more than 2 layers."); }
					TreeDisplayModelData(model);
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

			emg.Models.RemoveAt(LastSelectedTreeNodeU.Index);
			emg.Models.Insert(LastSelectedTreeNodeU.Index, model);
			emg.GenerateBytes();
			if(LastSelectedTreeNodeU.Parent.Parent != null)
            {
				EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Parent.Tag;
				emo.EMGs.RemoveAt(LastSelectedTreeNodeU.Parent.Index);
				emo.EMGs.Insert(LastSelectedTreeNodeU.Parent.Index, emg);
				emo.GenerateBytes();
			}
			RefreshTree(tvTreeUSF4);
			TreeDisplayModelData(model);
			AddStatus("Model changes saved.");
		}

		private void DeleteEMGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (LastSelectedTreeNodeU.Parent == null)
			{
				AddStatus("Couldn't delete EMG as it is not part of an EMO.");
				return;
			}
			DialogResult = MessageBox.Show("Delete EMG?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				((EMO)LastSelectedTreeNodeU.Parent.Tag).DeleteSubfile(LastSelectedTreeNodeU.Index);
				RefreshTree(tvTreeUSF4);
				AddStatus("EMG deleted.");
			}
		}

		private void SaveEncodedOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void SaveEncodedOBJToHEXToolStripMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void BtnEO_SubModSave_Click(object sender, EventArgs e)
		{
			if (tbEO_SubModName.Text.Trim() != string.Empty && Int32.TryParse(tbEO_SubModMaterial.Text.Trim(), out int smMaterial))
			{
				string value = tbEO_SubModName.Text.Trim();
				byte[] newName = Utils.MakeModelName(value);
				byte[] oldName;

				TreeNode n = LastSelectedTreeNodeU;
				EMO emo = new EMO();
				while (n.Tag.GetType() != typeof(EMO) && n.Parent != null) n = n.Parent;
				if(n.Tag.GetType() == typeof(EMO)) emo = (EMO)n.Tag;

				EMG emg = (EMG)LastSelectedTreeNodeU.Parent.Parent.Tag;
				Model model = (Model)LastSelectedTreeNodeU.Parent.Tag;
				SubModel sm = (SubModel)LastSelectedTreeNodeU.Tag;
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
				}
				sm.SubModelName = newName;

				AddStatus("Submodel info updated.");

				model.SubModels.RemoveAt(LastSelectedTreeNodeU.Index);
				model.SubModels.Insert(LastSelectedTreeNodeU.Index, sm);

				emg.Models.RemoveAt(LastSelectedTreeNodeU.Parent.Index);
				emg.Models.Insert(LastSelectedTreeNodeU.Parent.Index, model);
				emg.GenerateBytes();

				if (emo.EMGs != null)
				{
					emo.EMGs.RemoveAt(LastSelectedTreeNodeU.Parent.Parent.Index);
					emo.EMGs.Insert(LastSelectedTreeNodeU.Parent.Parent.Index, emg);
					emo.GenerateBytes();
				}

				lbSelNODE_Title.Text = Encoding.ASCII.GetString(sm.SubModelName);

				RefreshTree(tvTreeUSF4);
				TreeDisplaySubModelData(sm);
			}
		}

		public void button1_Click(object sender, EventArgs e)
		{
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = string.Empty;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
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

						int FileType = Utils.ReadInt(true, 0, bytes);

						USF4File file;

						if (FileType == USF4Methods.EMO) file = new EMO();
						else if (FileType == USF4Methods.EMM) file = new EMM();
						else if (FileType == USF4Methods.EMB) file = new EMB();
						else if (FileType == USF4Methods.LUA) file = new LUA();
						else if (FileType == USF4Methods.EMA) file = new EMA();
						else if (FileType == USF4Methods.CSB) file = new CSB();
						else if (FileType == USF4Methods.DDS) file = new DDS();
						else file = new OtherFile();

						file.ReadFile(bytes);
						file.Name = diagOpenOBJ.SafeFileName;
						master_USF4FileList.Add(file);

						RefreshTree(tvTreeUSF4);
					}
					catch
					{
						MessageBox.Show("Error opening file. Please confirm file is valid.", TStrings.STR_Information);
						RefreshTree(tvTreeUSF4);
					}
				}
			}
		}

		SMDFile ReadSMD(string smd)
		{
			ConsoleWrite(ConsoleLineSpacer);
			ConsoleWrite($"Opening SMD file:  {smd}");
			ConsoleWrite(" ");
			string[] lines = File.ReadAllLines(smd);

			//Prepare Input SMD Structure
			SMDFile WorkingSMD = new SMDFile();
			WorkingSMD.Verts = new List<Vertex>();
			WorkingSMD.Skeleton.Nodes = new List<SMDNode>();
			WorkingSMD.Frames = new List<SMDFrame>();
			WorkingSMD.FaceIndices = new List<int[]>();
			WorkingSMD.MaterialDictionary = new Dictionary<string, int>();
			WorkingSMD.MaterialNames = new List<string>();

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
				lines[i] = lines[i].Replace("   ", " ");
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
						WorkingNode.Name = node[1];
						WorkingNode.Parent = int.Parse(node[2]);

						//Done, add it to the list
						WorkingSMD.Skeleton.Nodes.Add(WorkingNode);
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
						Console.WriteLine($"i = {i}");

						string[] vert;

						i++;

						//Read in material name and store it in the main list, so Face i uses Material i. TODO wire up a way to link MaterialName to DDS/EMM contents?
						WorkingSMD.MaterialNames.Add(lines[i]);

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

		EMA SimpleEMAFromSMD(SMDFile model)
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
			for (int i = 0; i < model.Skeleton.Nodes.Count; i++)
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
			WorkingAnimation.CmdTrackCount = model.Skeleton.Nodes.Count * 6;
			WorkingAnimation.CMDTracks = new List<CMDTrack>();
			WorkingAnimation.CmdTrackPointersList = new List<int>();

			//Loop bone list
			for (int i = 0; i < model.Skeleton.Nodes.Count; i++)
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

			for (int i = 0; i < model.Skeleton.Nodes.Count; i++)
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
					pname = skel.NodeNames[skel.Nodes[i].Parent];
				}

				skeldata.Add($"{i} \"{skel.NodeNames[i]}\" {skel.Nodes[i].Parent} #{pname}");
			}
			skeldata.Add("end");

			skeldata.Add("skeleton");
			skeldata.Add("time 0");

			for (int i = 0; i < skel.Nodes.Count; i++)
			{
				Matrix4x4 transposedMatrix = Matrix4x4.Transpose(skel.Nodes[i].NodeMatrix);

				float tx, ty, tz, rx, ry, rz;

				Utils.DecomposeMatrixToRadians(skel.Nodes[i].NodeMatrix, out tx, out ty, out tz, out rx, out ry, out rz, out _, out _, out _);

				skeldata.Add($"{i} {String.Format("{0:0.000000}", tx)} {String.Format("{0:0.000000}", -tz)} {String.Format("{0:0.000000}", ty)} {String.Format("{0:0.000000}", rz)} {String.Format("{0:0.000000}", -rx)} {String.Format("{0:0.000000}", ry)}");
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
					pname = skel.NodeNames[skel.Nodes[i].Parent];
				}

				SMDData.Add($"{i} \"{skel.NodeNames[i]}\" {skel.Nodes[i].Parent} #{pname}");
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

			File.WriteAllLines($"{ema.Name}.smd", SMDData.Cast<string>().ToArray());
		}

		private List<string> EMAtoSMDFrames(EMA ema, int anim_index)
        {
			Animation a = ema.Animations[anim_index];
			Skeleton s = ema.Skeleton;

			List<string> Data = new List<string>() { "skeleton" };
			//Each frame...
			for (int i = 0; i < a.Duration; i++)
            {
				Data.Add($"time {i}");
				//Each Node...
				for (int j = 0; j < s.Nodes.Count; j++)
                {
					Anim.InterpolateRelativeKeyFrames(a, j, i, out float atx, out float aty, out float atz, out float arx, out float ary, out float arz, out _, out _, out _);

					//Generate animation matrix


					//InterpolateRelativeKeyFrames returns degrees - convert to radians for SMD
					arx *= (float)(Math.PI / 180d);
					ary *= (float)(Math.PI / 180d);
					arz *= (float)(Math.PI / 180d);

					Data.Add($"{j} {atx.ToString("0.000000")} {aty.ToString("0.000000")} {atz.ToString("0.000000")}  {arx.ToString("0.000000")}  {ary.ToString("0.000000")} {arz.ToString("0.000000")}");
                }
            }
			Data.Add("end");
			return Data;
        }

		public List<string> EMOtoSMDTriangles(EMO emo)
        {
			List<string> Data = new List<string>() { "triangles" };

			Data.Add("triangles");
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
							Data.Add(Encoding.ASCII.GetString(wSM.SubModelName).Replace(Convert.ToChar(0x00), ' ')); //Use as material name

							//TODO crash - exporting an EMG to SMD crashes if the EMG was created from an SMD import
							string v1 = "0 ";
							v1 += $"{wMod.VertexData[smFaces[f][0]].X} {-wMod.VertexData[smFaces[f][0]].Z} {wMod.VertexData[smFaces[f][0]].Y} ";
							v1 += $"{wMod.VertexData[smFaces[f][0]].nX} {-wMod.VertexData[smFaces[f][0]].nZ} {wMod.VertexData[smFaces[f][0]].nY} ";
							v1 += $"{wMod.VertexData[smFaces[f][0]].U} {-wMod.VertexData[smFaces[f][0]].V} ";
							if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
							{
								List<int> local_IDs = new List<int>();
								List<float> local_weights = new List<float>();

								local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[0]]);
								local_weights.Add(wMod.VertexData[smFaces[f][0]].BoneWeights[0]);

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[1]]);
									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][0]].BoneWeights[1]));
								}

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[2]]);
									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][0]].BoneWeights[2]));
								}

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[3]]);
									local_weights.Add(1 - local_weights.Sum());
								}

								//Skip g = 0, then check for "bad" IDs
								for (int g = 1; g < local_IDs.Count; g++)
								{
									if (local_IDs[g] == wSM.BoneIntegersList[0])
									{
										local_IDs.RemoveRange(g, local_IDs.Count - g);
										local_weights.RemoveRange(g, local_weights.Count - g);
									}
								}

								string weight_string1 = string.Empty;
								for (int g = 0; g < local_IDs.Count; g++)
								{
									weight_string1 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
								}
								v1 += $"{local_IDs.Count} " + weight_string1;
							}

							Data.Add(v1);

							//string v2 = $"{emo.EMGs[i].RootBone} ";
							string v2 = "0 ";
							v2 += $"{wMod.VertexData[smFaces[f][1]].X} {-wMod.VertexData[smFaces[f][1]].Z} {wMod.VertexData[smFaces[f][1]].Y} ";
							v2 += $"{wMod.VertexData[smFaces[f][1]].nX} {-wMod.VertexData[smFaces[f][1]].nZ} {wMod.VertexData[smFaces[f][1]].nY} ";
							v2 += $"{wMod.VertexData[smFaces[f][1]].U} {-wMod.VertexData[smFaces[f][1]].V} ";
							if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
							{
								List<int> local_IDs = new List<int>();
								List<float> local_weights = new List<float>();

								local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[0]]);
								local_weights.Add(wMod.VertexData[smFaces[f][1]].BoneWeights[0]);

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[1]]);
									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][1]].BoneWeights[1]));
								}

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[2]]);
									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][1]].BoneWeights[2]));
								}

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[3]]);
									local_weights.Add(1 - local_weights.Sum());
								}
								//Skip g = 0, then check for "bad" IDs
								for (int g = 1; g < local_IDs.Count; g++)
								{
									if (local_IDs[g] == wSM.BoneIntegersList[0])
									{
										local_IDs.RemoveRange(g, local_IDs.Count - g);
										local_weights.RemoveRange(g, local_weights.Count - g);
									}
								}
								string weight_string2 = string.Empty;
								for (int g = 0; g < local_IDs.Count; g++)
								{
									weight_string2 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
								}
								v2 += $"{local_IDs.Count} " + weight_string2;
							}

							Data.Add(v2);

							//string v3 = $"{emo.EMGs[i].RootBone} ";
							string v3 = "0 ";
							v3 += $"{wMod.VertexData[smFaces[f][2]].X} {-wMod.VertexData[smFaces[f][2]].Z} {wMod.VertexData[smFaces[f][2]].Y} ";
							v3 += $"{wMod.VertexData[smFaces[f][2]].nX} {-wMod.VertexData[smFaces[f][2]].nZ} {wMod.VertexData[smFaces[f][2]].nY} ";
							v3 += $"{wMod.VertexData[smFaces[f][2]].U} {-wMod.VertexData[smFaces[f][2]].V} ";
							if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
							{
								List<int> local_IDs = new List<int>();
								List<float> local_weights = new List<float>();

								local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[0]]);
								local_weights.Add(wMod.VertexData[smFaces[f][2]].BoneWeights[0]);

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[1]]);
									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][2]].BoneWeights[1]));
								}

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[2]]);
									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][2]].BoneWeights[2]));
								}

								if (local_weights.Sum() < 1f)
								{
									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[3]]);
									local_weights.Add(1 - local_weights.Sum());
								}
								//Skip g = 0, then check for "bad" IDs
								for (int g = 1; g < local_IDs.Count; g++)
								{
									if (local_IDs[g] == wSM.BoneIntegersList[0])
									{
										local_IDs.RemoveRange(g, local_IDs.Count - g);
										local_weights.RemoveRange(g, local_weights.Count - g);
									}
								}
								string weight_string3 = string.Empty;
								for (int g = 0; g < local_IDs.Count; g++)
								{
									weight_string3 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
								}
								//string weight_string3 = string.Empty;
								v3 += $"{local_IDs.Count} " + weight_string3;
							}

							Data.Add(v3);
						}
					}
				}
			}

			Data.Add("end");

			return Data;
		}

		private List<string> EMAtoSMDAnimation(EMA ema, int anim_index)
        {
			List<string> Data = new List<string>() { "version 1" };

			//Node list
			Data.Add("nodes");
			Skeleton s = ema.Skeleton;
			for (int i = 0; i < s.Nodes.Count; i++)
			{
				string pname = string.Empty;

				if (s.Nodes[i].Parent >= 0)
				{
					pname = s.NodeNames[s.Nodes[i].Parent];
				}

				Data.Add($"{i} \"{s.NodeNames[i]}\" {s.Nodes[i].Parent} #{pname}");
			}
			Data.Add("end");

			//Animation data
			Data.AddRange(EMAtoSMDFrames(ema, anim_index));

			//No triangles in animation files!

			return Data;
        }

		private List<string> EMOtoRefSMD(EMO emo)
        {
			List<string> SMDData = new List<string>() { "version 1" };

			SMDData.AddRange(WriteSMDNodesFromSkeleton(emo.Skeleton));

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

                            //TODO crash - exporting an EMG to SMD crashes if the EMG was created from an SMD import
                            //string v1 = $"{emo.EMGs[i].RootBone} ";
                            string v1 = "0 ";
                            v1 += $"{wMod.VertexData[smFaces[f][0]].X} {-wMod.VertexData[smFaces[f][0]].Z} {wMod.VertexData[smFaces[f][0]].Y} ";
                            v1 += $"{wMod.VertexData[smFaces[f][0]].nX} {-wMod.VertexData[smFaces[f][0]].nZ} {wMod.VertexData[smFaces[f][0]].nY} ";
                            v1 += $"{wMod.VertexData[smFaces[f][0]].U} {-wMod.VertexData[smFaces[f][0]].V} ";
                            if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
                            {
                                List<int> local_IDs = new List<int>();
                                List<float> local_weights = new List<float>();

                                local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[0]]);
                                local_weights.Add(wMod.VertexData[smFaces[f][0]].BoneWeights[0]);

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[1]]);
                                    local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][0]].BoneWeights[1]));
                                }

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[2]]);
                                    local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][0]].BoneWeights[2]));
                                }

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[3]]);
                                    local_weights.Add(1 - local_weights.Sum());
                                }

                                //Skip g = 0, then check for "bad" IDs
                                for (int g = 1; g < local_IDs.Count; g++)
                                {
                                    if (local_IDs[g] == wSM.BoneIntegersList[0])
                                    {
                                        local_IDs.RemoveRange(g, local_IDs.Count - g);
                                        local_weights.RemoveRange(g, local_weights.Count - g);
                                    }
                                }

                                string weight_string1 = string.Empty;
                                for (int g = 0; g < local_IDs.Count; g++)
                                {
                                    weight_string1 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
                                }
                                v1 += $"{local_IDs.Count} " + weight_string1;
                            }

                            SMDData.Add(v1);

                            //string v2 = $"{emo.EMGs[i].RootBone} ";
                            string v2 = "0 ";
                            v2 += $"{wMod.VertexData[smFaces[f][1]].X} {-wMod.VertexData[smFaces[f][1]].Z} {wMod.VertexData[smFaces[f][1]].Y} ";
                            v2 += $"{wMod.VertexData[smFaces[f][1]].nX} {-wMod.VertexData[smFaces[f][1]].nZ} {wMod.VertexData[smFaces[f][1]].nY} ";
                            v2 += $"{wMod.VertexData[smFaces[f][1]].U} {-wMod.VertexData[smFaces[f][1]].V} ";
                            if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
                            {
                                List<int> local_IDs = new List<int>();
                                List<float> local_weights = new List<float>();

                                local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[0]]);
                                local_weights.Add(wMod.VertexData[smFaces[f][1]].BoneWeights[0]);

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[1]]);
                                    local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][1]].BoneWeights[1]));
                                }

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[2]]);
                                    local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][1]].BoneWeights[2]));
                                }

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[3]]);
                                    local_weights.Add(1 - local_weights.Sum());
                                }
                                //Skip g = 0, then check for "bad" IDs
                                for (int g = 1; g < local_IDs.Count; g++)
                                {
                                    if (local_IDs[g] == wSM.BoneIntegersList[0])
                                    {
                                        local_IDs.RemoveRange(g, local_IDs.Count - g);
                                        local_weights.RemoveRange(g, local_weights.Count - g);
                                    }
                                }
                                string weight_string2 = string.Empty;
                                for (int g = 0; g < local_IDs.Count; g++)
                                {
                                    weight_string2 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
                                }
                                v2 += $"{local_IDs.Count} " + weight_string2;
                            }

                            SMDData.Add(v2);

                            //string v3 = $"{emo.EMGs[i].RootBone} ";
                            string v3 = "0 ";
                            v3 += $"{wMod.VertexData[smFaces[f][2]].X} {-wMod.VertexData[smFaces[f][2]].Z} {wMod.VertexData[smFaces[f][2]].Y} ";
                            v3 += $"{wMod.VertexData[smFaces[f][2]].nX} {-wMod.VertexData[smFaces[f][2]].nZ} {wMod.VertexData[smFaces[f][2]].nY} ";
                            v3 += $"{wMod.VertexData[smFaces[f][2]].U} {-wMod.VertexData[smFaces[f][2]].V} ";
                            if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
                            {
                                List<int> local_IDs = new List<int>();
                                List<float> local_weights = new List<float>();

                                local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[0]]);
                                local_weights.Add(wMod.VertexData[smFaces[f][2]].BoneWeights[0]);

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[1]]);
                                    local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][2]].BoneWeights[1]));
                                }

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[2]]);
                                    local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][2]].BoneWeights[2]));
                                }

                                if (local_weights.Sum() < 1f)
                                {
                                    local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[3]]);
                                    local_weights.Add(1 - local_weights.Sum());
                                }
                                //Skip g = 0, then check for "bad" IDs
                                for (int g = 1; g < local_IDs.Count; g++)
                                {
                                    if (local_IDs[g] == wSM.BoneIntegersList[0])
                                    {
                                        local_IDs.RemoveRange(g, local_IDs.Count - g);
                                        local_weights.RemoveRange(g, local_weights.Count - g);
                                    }
                                }
                                string weight_string3 = string.Empty;
                                for (int g = 0; g < local_IDs.Count; g++)
                                {
                                    weight_string3 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
                                }
                                //string weight_string3 = string.Empty;
                                v3 += $"{local_IDs.Count} " + weight_string3;
                            }

                            SMDData.Add(v3);
                        }
                    }
                }
            }

            SMDData.Add("end");

            return SMDData;
		}


		//private void EMOtoRefSMD(EMO emo)
		//{
		//	string filepath;
		//	string newSMD = $"{emo.Name}.smd";

		//	saveFileDialog1.InitialDirectory = string.Empty;
		//	saveFileDialog1.FileName = newSMD;
		//	saveFileDialog1.Filter = SMDFileFilter;
		//	if (saveFileDialog1.ShowDialog() == DialogResult.OK)
		//	{
		//		filepath = saveFileDialog1.FileName;
		//		if (filepath.Trim() != "")
		//		{
		//			List<string> SMDData = new List<string>() { "version 1" };

		//			SMDData.AddRange(WriteSMDNodesFromSkeleton(emo.Skeleton));

		//			SMDData.Add("triangles");
		//			for (int i = 0; i < emo.EMGs.Count; i++)
		//			{
		//				for (int j = 0; j < emo.EMGs[i].Models.Count; j++)
		//				{
		//					Model wMod = emo.EMGs[i].Models[j];

		//					for (int k = 0; k < emo.EMGs[i].Models[j].SubModels.Count; k++)
		//					{
		//						SubModel wSM = wMod.SubModels[k];
		//						List<int[]> smFaces = GeometryIO.FaceIndicesFromDaisyChain(wSM.DaisyChain);

		//						for (int f = 0; f < smFaces.Count; f++)
		//						{
		//							SMDData.Add(Encoding.ASCII.GetString(wSM.SubModelName).Replace(Convert.ToChar(0x00), ' ')); //Use as material name

		//							//TODO crash - exporting an EMG to SMD crashes if the EMG was created from an SMD import
		//							//string v1 = $"{emo.EMGs[i].RootBone} ";
		//							string v1 = "0 ";
		//							v1 += $"{wMod.VertexData[smFaces[f][0]].X} {-wMod.VertexData[smFaces[f][0]].Z} {wMod.VertexData[smFaces[f][0]].Y} ";
		//							v1 += $"{wMod.VertexData[smFaces[f][0]].nX} {-wMod.VertexData[smFaces[f][0]].nZ} {wMod.VertexData[smFaces[f][0]].nY} ";
		//							v1 += $"{wMod.VertexData[smFaces[f][0]].U} {-wMod.VertexData[smFaces[f][0]].V} ";
		//							if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
		//							{
		//								List<int> local_IDs = new List<int>();
		//								List<float> local_weights = new List<float>();

		//								local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[0]]);
		//								local_weights.Add(wMod.VertexData[smFaces[f][0]].BoneWeights[0]);

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[1]]);
		//									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][0]].BoneWeights[1]));
		//								}

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[2]]);
		//									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][0]].BoneWeights[2]));
		//								}

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][0]].BoneIDs[3]]);
		//									local_weights.Add(1 - local_weights.Sum());
		//								}

		//								//Skip g = 0, then check for "bad" IDs
		//								for (int g = 1; g < local_IDs.Count; g++)
		//								{
		//									if (local_IDs[g] == wSM.BoneIntegersList[0])
  //                                          {
		//										local_IDs.RemoveRange(g, local_IDs.Count - g);
		//										local_weights.RemoveRange(g, local_weights.Count - g);
		//									}
		//								}

		//								string weight_string1 = string.Empty;
		//								for (int g = 0; g < local_IDs.Count; g++)
		//								{
		//									weight_string1 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
		//								}
		//								v1 += $"{local_IDs.Count} " + weight_string1;
		//							}

		//							SMDData.Add(v1);

		//							//string v2 = $"{emo.EMGs[i].RootBone} ";
		//							string v2 = "0 ";
		//							v2 += $"{wMod.VertexData[smFaces[f][1]].X} {-wMod.VertexData[smFaces[f][1]].Z} {wMod.VertexData[smFaces[f][1]].Y} ";
		//							v2 += $"{wMod.VertexData[smFaces[f][1]].nX} {-wMod.VertexData[smFaces[f][1]].nZ} {wMod.VertexData[smFaces[f][1]].nY} ";
		//							v2 += $"{wMod.VertexData[smFaces[f][1]].U} {-wMod.VertexData[smFaces[f][1]].V} ";
		//							if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
		//							{
		//								List<int> local_IDs = new List<int>();
		//								List<float> local_weights = new List<float>();

		//								local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[0]]);
		//								local_weights.Add(wMod.VertexData[smFaces[f][1]].BoneWeights[0]);

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[1]]);
		//									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][1]].BoneWeights[1]));
		//								}

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[2]]);
		//									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][1]].BoneWeights[2]));
		//								}

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][1]].BoneIDs[3]]);
		//									local_weights.Add(1 - local_weights.Sum());
		//								}
		//								//Skip g = 0, then check for "bad" IDs
		//								for (int g = 1; g < local_IDs.Count; g++)
		//								{
		//									if (local_IDs[g] == wSM.BoneIntegersList[0])
		//									{
		//										local_IDs.RemoveRange(g, local_IDs.Count - g);
		//										local_weights.RemoveRange(g, local_weights.Count - g);
		//									}
		//								}
		//								string weight_string2 = string.Empty;
		//								for (int g = 0; g < local_IDs.Count; g++)
		//								{
		//									weight_string2 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
		//								}
		//								v2 += $"{local_IDs.Count} " + weight_string2;
		//							}

		//							SMDData.Add(v2);

		//							//string v3 = $"{emo.EMGs[i].RootBone} ";
		//							string v3 = "0 ";
		//							v3 += $"{wMod.VertexData[smFaces[f][2]].X} {-wMod.VertexData[smFaces[f][2]].Z} {wMod.VertexData[smFaces[f][2]].Y} ";
		//							v3 += $"{wMod.VertexData[smFaces[f][2]].nX} {-wMod.VertexData[smFaces[f][2]].nZ} {wMod.VertexData[smFaces[f][2]].nY} ";
		//							v3 += $"{wMod.VertexData[smFaces[f][2]].U} {-wMod.VertexData[smFaces[f][2]].V} ";
		//							if (wMod.VertexData[smFaces[f][2]].BoneIDs != null && wMod.VertexData[smFaces[f][2]].BoneIDs.Count > 0)
		//							{
		//								List<int> local_IDs = new List<int>();
		//								List<float> local_weights = new List<float>();

		//								local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[0]]);
		//								local_weights.Add(wMod.VertexData[smFaces[f][2]].BoneWeights[0]);

		//								if (local_weights.Sum() < 1f)
  //                                      {
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[1]]);
		//									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][2]].BoneWeights[1]));
  //                                      }

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[2]]);
		//									local_weights.Add(Math.Min(1 - local_weights.Sum(), wMod.VertexData[smFaces[f][2]].BoneWeights[2]));
		//								}

		//								if (local_weights.Sum() < 1f)
		//								{
		//									local_IDs.Add(wSM.BoneIntegersList[wMod.VertexData[smFaces[f][2]].BoneIDs[3]]);
		//									local_weights.Add(1 - local_weights.Sum());
		//								}
		//								//Skip g = 0, then check for "bad" IDs
		//								for (int g = 1; g < local_IDs.Count; g++)
		//								{
		//									if (local_IDs[g] == wSM.BoneIntegersList[0])
		//									{
		//										local_IDs.RemoveRange(g, local_IDs.Count - g);
		//										local_weights.RemoveRange(g, local_weights.Count - g);
		//									}
		//								}
		//								string weight_string3 = string.Empty;
		//								for (int g = 0; g < local_IDs.Count; g++)
		//								{
		//									weight_string3 += $"{local_IDs[g]} {local_weights[g].ToString("0.000000")} ";
		//								}
		//								//string weight_string3 = string.Empty;
		//								v3 += $"{local_IDs.Count} " + weight_string3;
		//							}

		//							SMDData.Add(v3);
		//						}
		//					}
		//				}
		//			}

		//			SMDData.Add("end");

		//			//File.WriteAllLines($"{Encoding.ASCII.GetString(emo.Name)}.smd", SMDData.Cast<string>().ToArray());
		//			File.WriteAllLines(filepath, SMDData.Cast<string>().ToArray());
		//		}
		//	}


		//}

		EMO EMOFromSMD(SMDFile model)
		{
			EMO nEMO = new EMO();

			nEMO.EMGCount = 0x01;
			nEMO.EMGs = new List<EMG> { NewEMGFromSMD(model) };
			nEMO.EMGPointersList = new List<int> { 0x00 };

			nEMO.Name = "EMOfromSMD";

			nEMO.NamesList = new List<string>();
			nEMO.NamingPointersList = new List<int>();

			foreach (string s in model.MaterialDictionary.Keys)
			{
				nEMO.NamesList.Add(s);
				nEMO.NamingPointersList.Add(0x00);
			}

			nEMO.NamingListPointer = 0x00;

			nEMO.NumberEMMMaterials = model.MaterialDictionary.Count;

			nEMO.SkeletonPointer = 0x00;

			nEMO.Skeleton = SkeletonFromSMD(model);

			nEMO.GenerateBytes();

			return nEMO;
		}

		EMG NewEMGFromSMD(SMDFile smd)
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
					if (smd.MaterialDictionary[smd.MaterialNames[j]] == i)
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

				sm.BoneIntegersCount = smd.Skeleton.Nodes.Count;
				sm.BoneIntegersList = new List<int>();

				for (int j = 0; j < smd.Skeleton.Nodes.Count; j++)
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
			SaveEMZToFile((string)tvTreeUSF4.Nodes[SelectedEMZNumberInTree].Tag);
		}

		private void CloseEMZToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if ((string)tvTreeUSF4.Nodes[SelectedEMZNumberInTree].Tag == "EMZ")
			{
				WorkingEMZ = new EMB();
				AddStatus(".EMZ closed.");
			}
			if ((string)tvTreeUSF4.Nodes[SelectedEMZNumberInTree].Tag == "TEX")
			{
				WorkingTEXEMZ = new EMB();
				AddStatus(".TEX.EMZ closed.");
			}
			tvTreeUSF4.Nodes.RemoveAt(SelectedEMZNumberInTree);
		}

		private void deleteMaterialToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Are you sure?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				EMM emm = (EMM)LastSelectedTreeNodeU.Parent.Tag;
				emm.DeleteSubfile(LastSelectedTreeNodeU.Index);

				RefreshTree(tvTreeUSF4);
				AddStatus("Material '" + LastSelectedTreeNodeU.Text.Replace("\0", "") + "' deleted from '" + emm.Name.Replace("\0", "") + "'");
			}
		}

		private void cmDDSinjectDDSToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			EMB emb = (EMB)LastSelectedTreeNodeU.Parent.Tag;
			DDS dds = (DDS)LastSelectedTreeNodeU.Tag;
			try
			{
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
					emb.Files.RemoveAt(LastSelectedTreeNodeU.Index);
					emb.Files.Insert(LastSelectedTreeNodeU.Index, dds);
					emb.GenerateBytes();
					RefreshTree(tvTreeUSF4);
				}
			}
			catch
            {
				MessageBox.Show("Something went wrong while encoding/injecting texture!", TStrings.STR_Error);
			}

			try
            {
				{ 
					ImageEngineImage IE = new ImageEngineImage(dds.HEXBytes);
					pbPreviewDDS.BackgroundImage = Utils.BitmapFromBytes(IE.Save(new ImageEngineFormatDetails(ImageEngineFormat.PNG), new MipHandling()));
					IE.Dispose();
				}
			}
			catch
			{
				AddStatus($"Error while trying to read this DDS.");
			}
		}

		private void cmEMBaddDDSToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			AddNewDDSToEMB();
		}

		private void injectDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddNewDDSToEMB();
		}

		void AddNewDDSToEMB()
		{
			EMB emb;
			if (LastSelectedTreeNodeU.Tag.GetType() == typeof(EMB)) emb = (EMB)LastSelectedTreeNodeU.Tag;
			else emb = (EMB)LastSelectedTreeNodeU.Parent.Tag;
			DDS dds = new DDS();
			
			try
			{
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
					dds.Name = diagOpenOBJ.SafeFileName;
					emb.NumberOfFiles += 1;
					emb.FileLengthsList.Add(0);
					emb.FileNamesList.Add(diagOpenOBJ.SafeFileName);
					emb.FilePointersList.Add(0);
					emb.FileNamePointersList.Add(0);
					emb.Files.Add(dds);
					emb.GenerateBytes();
					RefreshTree(tvTreeUSF4);					
				}
			}
			catch
            {
				AddStatus($"Error while trying to read this DDS.");
			}
		
			try
            {
				ImageEngineImage IE = new ImageEngineImage(dds.HEXBytes);
				pbPreviewDDS.BackgroundImage = Utils.BitmapFromBytes(IE.Save(new ImageEngineFormatDetails(ImageEngineFormat.PNG), new MipHandling()));
				IE.Dispose();
			}
			catch
			{
				MessageBox.Show("Something went wrong while encoding/injecting texture!", TStrings.STR_Error);
			}

		}

		private void deleteDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			((EMB)LastSelectedTreeNodeU.Parent.Tag).DeleteSubfile(LastSelectedTreeNodeU.Index);
			RefreshTree(tvTreeUSF4);
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
			EMM emm = (EMM)LastSelectedTreeNodeU.Parent.Tag;
			Material mat = (Material)LastSelectedTreeNodeU.Tag; //Saved for index
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
			emm.Materials.RemoveAt(LastSelectedTreeNodeU.Index);
			emm.Materials.Insert(LastSelectedTreeNodeU.Index, newMat);
			emm.GenerateBytes();
			RefreshTree(tvTreeUSF4);
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
				if (LastSelectedTreeNodeU.Tag.GetType() == typeof(EMM))
				{
					emmNode = LastSelectedTreeNodeU;
					emm = (EMM)LastSelectedTreeNodeU.Tag;
				}
				else
				{
					emmNode = LastSelectedTreeNodeU.Parent;
					emm = (EMM)LastSelectedTreeNodeU.Parent.Tag;
				}

				foreach (TreeNode n in emmNode.Nodes)
				{
					if (n.Text.Replace("\0", "") == newMatName)
					{
						MessageBox.Show("Name already exists!", TStrings.STR_Error);
						return;
					}
				}

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

				//Find matching EMO
				USF4File match = FindMatch(master_USF4FileList, typeof(EMO), emm.Name);
				if (match.GetType() == typeof(EMO))
				{
					emo = (EMO)match;
					emo.NumberEMMMaterials = emm.MaterialCount;
					emo.GenerateBytes();
				}

				RefreshTree(tvTreeUSF4);
				AddStatus("Material '" + newMatName + "' added to '" + emm.Name.Replace("\0", "") + "'");
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
			LUA nLUA = (LUA)LastSelectedTreeNodeU.Tag;

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

					if (loaderror == 0 && loaderror == 0)
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
				nLUA.Name = target_lua;

				File.Delete(CodeStrings.outfile);
			}
			catch
			{
				MessageBox.Show("Something went wrong while injecting LUA!", TStrings.STR_Error);
			}

			return nLUA;
		}

		private void cmEMMaddNewMaterialToolStripMenuItem_Click(object sender, EventArgs e)
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

		private void cmDDSrenameDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IB.StartPosition = FormStartPosition.CenterParent; IB.ShowDialog(this);
			if (IB.EnteredValue.Trim() != string.Empty)
			{
				DDS dds = (DDS)LastSelectedTreeNodeU.Tag;
				dds.Name = IB.EnteredValue;
				if (LastSelectedTreeNodeU.Parent != null)
				{
					EMB emb = (EMB)LastSelectedTreeNodeU.Parent.Tag;
					emb.Files.RemoveAt(LastSelectedTreeNodeU.Index);
					emb.Files.Insert(LastSelectedTreeNodeU.Index, dds);
					emb.FileNamesList.RemoveAt(LastSelectedTreeNodeU.Index);
					emb.FileNamesList.Insert(LastSelectedTreeNodeU.Index, IB.EnteredValue);
					emb.GenerateBytes();
				}
				RefreshTree(tvTreeUSF4);
			}
		}

		private void extractDDSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMB emb = (EMB)LastSelectedTreeNodeU.Parent.Tag;
			DDS dds = (DDS)LastSelectedTreeNodeU.Tag;
			saveFileDialog1.Filter = DDSFileFilter;
			saveFileDialog1.FileName = emb.FileNamesList[LastSelectedTreeNodeU.Index];
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string filepath = saveFileDialog1.FileName;
				ExtractDDSFromEMB(dds.HEXBytes, filepath);
				AddStatus($"DDS {Path.GetFileName(filepath)} Extracted");
			}
		}
		/// <summary>Extracts all DDS textures</summary>
		void ExtractTEXEMZ(string BasePath)
		{
			for (int i = 0; i < WorkingTEXEMZ.Files.Count; i++)
			{
				EMB emb = (EMB)WorkingTEXEMZ.Files[i];
				string SubFolder = emb.Name;
				SubFolder = SubFolder.Substring(0, SubFolder.Length - 4);
				Directory.CreateDirectory($"{BasePath}\\{SubFolder}"); //Create a sub folder witht the EMB name without .emb at the end
				ExtractAllDDSFromEMB(emb, $"{BasePath}\\{SubFolder}", true);
			}
		}

		/// <summary>Extract All DDS textures from the supplied EMB and write them to the target path</summary>
		void ExtractAllDDSFromEMB(EMB emb, string InputPath, bool alltex = false)
		{
			string FullFilePath;
			for (int i = 0; i < emb.Files.Count; i++)
			{
				DDS dds = (DDS)emb.Files[i];
				string DDSName = emb.FileNamesList[i];
				if (!DDSName.ToLower().Contains(".dds")) { DDSName += ".dds"; }
				if (DDSName.ToLower() == "dds.dds") DDSName = Path.GetFileName(InputPath) + ".dds";
				FullFilePath = $"{InputPath}\\{DDSName}";
				if (File.Exists(FullFilePath)) DDSName = $"{i} {DDSName}";
				ExtractDDSFromEMB(dds.HEXBytes, $"{InputPath}\\{DDSName}");
				if (!alltex) AddStatus($"DDS {DDSName} Extracted");
			}
		}


		void ExtractDDSFromEMB(byte[] DDSData, string Filepath)
		{
			Utils.WriteDataToStream(Filepath, DDSData);
		}

		void AddStatus(string Value)
		{
			DateTime dt = DateTime.Now;
			lvStatus.Items.Add("[" + dt.ToString("HH:mm:ss") + "] " + Value);
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
				EMB emb = (EMB)LastSelectedTreeNodeU.Tag;
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
			
		}

		private void cmCSBinjectCSBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			InjectCSB();
		}

		private void emgContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			extractModelAsOBJToolStripMenuItem.Visible = true;
			extractSubmodelAsOBJToolStripMenuItem.Visible = true;
			rawDumpEMGToolStripMenuItem.Visible = true;

			if (LastSelectedTreeNodeU.Tag.GetType() == typeof(EMG))
			{
				extractModelAsOBJToolStripMenuItem.Visible = false;
				extractSubmodelAsOBJToolStripMenuItem.Visible = false;
			}
			else if (LastSelectedTreeNodeU.Tag.GetType() == typeof(Model))
			{
				extractSubmodelAsOBJToolStripMenuItem.Visible = false;
				rawDumpEMGToolStripMenuItem.Visible = false;
			}
			else if (LastSelectedTreeNodeU.Tag.GetType() == typeof(SubModel))
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

			if (LastSelectedTreeNodeU.Tag.GetType() == typeof(EMA))
			{
				emaclick = true;
			}

			DeleteAnimaiontoolStripMenuItem3.Visible = !emaclick;
		}

		private void cmEMAdeleteAnimationtoolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMA ema = (EMA)LastSelectedTreeNodeU.Parent.Tag;

			DeleteAnimation(ema, LastSelectedTreeNodeU.Index);
		}

		void DeleteAnimation(EMA ema, int AnimIndex)
		{
			ema.AnimationPointersList.RemoveAt(0);
			ema.Animations.RemoveAt(AnimIndex);
			ema.AnimationCount = ema.Animations.Count;
			ema.GenerateBytes();

			AddStatus("Animation deleted.");
			RefreshTree(tvTreeUSF4);
		}

		private void cmLUAinjectLUAbytecodeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			InjectLua();
		}

		private void InjectLua()
		{
			try
			{
				LUA lua = (LUA)LastSelectedTreeNodeU.Tag;

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

					RefreshTree(tvTreeUSF4);
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
					SMDFile nSMD = new SMDFile();
					nSMD = ReadSMD(filepath);

					EMO nEMO = EMOFromSMD(nSMD);

					RefreshTree(tvTreeUSF4);
				}
			}
		}

		private void embContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{

		}

		private void InjectAnimationtoolStripMenuItem1_Click(object sender, EventArgs e)
		{
			GeometryIO.AnimationFromColladaStruct(Grendgine_Collada.Grendgine_Load_File("outputtest.dae"));

			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNodeU.Parent.Index];

			ema.Animations.RemoveAt(LastSelectedTreeNodeU.Index);

			InjectAnimation();
			//DuplicateAnimationDown();
		}

		private void InjectAnimation()
		{
			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNodeU.Parent.Index];

			Animation anim = ema.Animations[LastSelectedTreeNodeU.Index];

			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = SMDFileFilter;

			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				SMDFile model = ReadSMD(diagOpenOBJ.FileName);

				Animation WorkingAnimation = new Animation();

				WorkingAnimation.Duration = model.Frames.Count;
				WorkingAnimation.Name = anim.Name;

				WorkingAnimation.ValuesList = new List<float>();
				Dictionary<float, int> ValueDict = new Dictionary<float, int>();

				//Populate the float dictionary
				for (int i = 0; i < model.Skeleton.Nodes.Count; i++)
				{

					for (int k = 0; k < model.Frames.Count; k++)
					{
						int xflip = -1;

						float Rad2Deg = Convert.ToSingle(180 / Math.PI);
						int dumVal; //TryGetValue outputs the dictionary result, but we don't care right now
						if (!ValueDict.TryGetValue(xflip * model.Frames[k].traX[i], out dumVal)) { ValueDict.Add(xflip * model.Frames[k].traX[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(model.Frames[k].traY[i], out dumVal)) { ValueDict.Add(model.Frames[k].traY[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(model.Frames[k].traZ[i], out dumVal)) { ValueDict.Add(model.Frames[k].traZ[i], ValueDict.Count); }
						if (!ValueDict.TryGetValue(Rad2Deg * xflip * model.Frames[k].rotX[i], out dumVal)) { ValueDict.Add(Rad2Deg * xflip * model.Frames[k].rotX[i], ValueDict.Count); }
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
				WorkingAnimation.CmdTrackCount = model.Skeleton.Nodes.Count * 6;
				WorkingAnimation.CMDTracks = new List<CMDTrack>();
				WorkingAnimation.CmdTrackPointersList = new List<int>();

				//Loop bone list
				for (int i = 0; i < model.Skeleton.Nodes.Count; i++)
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

				for (int i = 0; i < model.Skeleton.Nodes.Count; i++)
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



				ema.Animations.RemoveAt(LastSelectedTreeNodeU.Index);
				ema.Animations.Insert(LastSelectedTreeNodeU.Index, WorkingAnimation);
				ema.GenerateBytes();
			}
		}

		private void dumpRefPoseToSMDToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMA ema = (EMA)LastSelectedTreeNodeU.Tag;

			InitialPoseFromEMA(ema);
		}

		private void cmEMOextractEMOtoSMDToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Tag;
			EMA ema = (EMA)((EMB)LastSelectedTreeNodeU.Parent.Tag).Files[LastSelectedTreeNodeU.Index - 2];

			File.WriteAllLines("testSMDref.smd", EMOtoRefSMD(emo).ToArray());
			File.WriteAllLines("testSMDanim.smd", EMAtoSMDAnimation(ema, 0).ToArray());

			//EMOtoRefSMD(emo);
		}

		private void rawDumpLUAToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LUA lua = (LUA)LastSelectedTreeNodeU.Tag;
			saveFileDialog1.Filter = LUAFileFilter;
			saveFileDialog1.FileName = lua.Name;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, lua.HEXBytes);
				AddStatus($"Extracted {lua.Name}");
			}
		}

		private void cmEMOexpandAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tvTreeUSF4.SelectedNode.ExpandAll();
			foreach (TreeNode n in tvTreeUSF4.SelectedNode.Nodes)
            {
				if (n.Tag.GetType() == typeof(Skeleton))
                {
					n.Collapse();
                }
            }
		}

		private void cmEMOcollapseAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tvTreeUSF4.SelectedNode.Collapse(false);
		}

		private void cmEMOinsertOBJAsNewEMGToolStripMenuItem_Click(object sender, EventArgs e)
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
					SMDFile someSMD = new SMDFile();
					await Task.Run(() => { someSMD = ReadSMD(filepath); });

					EMO emo = (EMO)WorkingEMZ.Files[SelectedEMONumberInTree];
					EMG targetEMG = emo.EMGs[SelectedEMGNumberInTree];
					EMG newEMG = NewEMGFromSMD(someSMD);
					emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
					emo.EMGs.Insert(SelectedEMGNumberInTree, newEMG);
					emo.GenerateBytes();

					RefreshTree(tvTreeUSF4);
					OpenEMONode(true);
				}
			}
		}

		private void cmLUAinjectLUAScriptToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-GB", false);

			LUA lua = (LUA)LastSelectedTreeNodeU.Tag;

			try
			{
				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;
				diagOpenOBJ.Filter = LUAFileFilter;
				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{

					lua = LUAScriptToBytecode(diagOpenOBJ.SafeFileName);

					RefreshTree(tvTreeUSF4);
					AddStatus($"Bytecode injected into {((LUA)LastSelectedTreeNodeU.Tag).Name}.");
				}
			}
			catch
			{
				MessageBox.Show("Couldn't open file.", TStrings.STR_Error);
			}

		}

		private void cmEMBaddFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(sender.GetType() == typeof(ToolStripMenuItem))
            {
				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;

				USF4File f = new USF4File();

				if (((ToolStripMenuItem)sender).Text == "EMA")
				{
					diagOpenOBJ.Filter = EMAFileFilter;
					f = new EMA();
				}
				else if (((ToolStripMenuItem)sender).Text == "EMM")
				{
					diagOpenOBJ.Filter = EMMFileFilter;
					f = new EMM();
				}
				else if(((ToolStripMenuItem)sender).Text == "EMO")
				{
					diagOpenOBJ.Filter = EMOFileFilter;
					f = new EMO();
				}
				else if (((ToolStripMenuItem)sender).Text == "EMB")
				{
					diagOpenOBJ.Filter = EMBFileFilter;
					f = new EMB();
				}
				else if (((ToolStripMenuItem)sender).Text == "DDS")
				{
					diagOpenOBJ.Filter = DDSFileFilter;
					f = new DDS();
				}
				else if (((ToolStripMenuItem)sender).Text == "LUA")
				{
					diagOpenOBJ.Filter = LUAFileFilter;
					f = new LUA();
				}
				else if (((ToolStripMenuItem)sender).Text == "CSB")
				{
					diagOpenOBJ.Filter = CSBFileFilter;
					f = new CSB();
				}

				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{

					f.ReadFile(File.ReadAllBytes(diagOpenOBJ.FileName));
					f.Name = diagOpenOBJ.SafeFileName;

					((EMB)LastSelectedTreeNodeU.Tag).AddSubfile(f);

					AddStatus($"{diagOpenOBJ.SafeFileName} added to {LastSelectedTreeNodeU.Text}.");

					RefreshTree(tvTreeUSF4);
				}
			}
		}

		private void addLUAScriptToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LUA lua = new LUA();
			EMB emb = (EMB)LastSelectedTreeNodeU.Tag;

			try
			{
				diagOpenOBJ.RestoreDirectory = true;
				diagOpenOBJ.FileName = string.Empty;
				diagOpenOBJ.InitialDirectory = LastOpenFolder;
				diagOpenOBJ.Filter = LUAFileFilter;
				if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
				{

					lua = LUAScriptToBytecode(diagOpenOBJ.SafeFileName);
					lua.Name = diagOpenOBJ.SafeFileName;

					emb.NumberOfFiles++;
					emb.FileNamePointersList.Add(0x00);
					emb.FileLengthsList.Add(0x00);
					emb.FilePointersList.Add(0x00);
					emb.FileNamesList.Add(lua.Name);

					AddStatus($"Bytecode added as {diagOpenOBJ.SafeFileName}");

					RefreshTree(tvTreeUSF4);
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
			EMO targetEMO = (EMO)LastSelectedTreeNodeU.Tag;

			EMO nEMO = targetEMO;

			nEMO.GenerateBytes();

			saveFileDialog1.Filter = EMOFileFilter;
			saveFileDialog1.FileName = nEMO.Name;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, nEMO.HEXBytes);
				AddStatus($"Extracted {nEMO.Name}");
			}
		}

		private void rawDumpEMAToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMA targetEMA = (EMA)LastSelectedTreeNodeU.Tag;

			EMA nEMA = targetEMA;

			nEMA.GenerateBytes();

			saveFileDialog1.Filter = EMAFileFilter;
			saveFileDialog1.FileName = nEMA.Name;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, nEMA.HEXBytes);
				AddStatus($"Extracted {nEMA.Name}");
			}

		}

		private void duplicateEMGToolStripMenuItem_Click(object sender, EventArgs e)
		{

			EMO targetEMO = (EMO)WorkingEMZ.Files[LastSelectedTreeNodeU.Parent.Index];
			EMG targetEMG = new EMG(targetEMO.EMGs[LastSelectedTreeNodeU.Index].HEXBytes);
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

			//WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			//WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, targetEMO);
			RefreshTree(tvTreeUSF4);
		}

		private void duplicateModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO targetEMO = (EMO)WorkingEMZ.Files[LastSelectedTreeNodeU.Parent.Index];
			EMG targetEMG = new EMG(targetEMO.EMGs[LastSelectedTreeNodeU.Index].HEXBytes);

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
			for (int i = 0; i < bone_count; i++)
			{
				//targetSM.BoneIntegersList.Add(targetSM.BoneIntegersList[i] + 20);
			}
			targetSM.BoneIntegersCount = targetSM.BoneIntegersList.Count;

			//Return Submodel
			targetModel.SubModels.RemoveAt(0);
			targetModel.SubModels.Insert(0, targetSM);

			//Duplicate verts and increase the bone IDs
			int vert_count = targetModel.VertexData.Count;

			for (int i = 0; i < vert_count; i++)
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

			//WorkingEMZ.Files.Remove(LastSelectedTreeNode.Parent.Index);
			//WorkingEMZ.Files.Add(LastSelectedTreeNode.Parent.Index, targetEMO);
			RefreshTree(tvTreeUSF4);

		}

		public void DuplicateUSA_MAN01_B()
		{
			EMA ema = (EMA)WorkingEMZ.Files[8];
			EMO emo = (EMO)WorkingEMZ.Files[10];

			ema.Skeleton = Anim.DuplicateSkeleton(ema.Skeleton, 1, 20);
			emo.Skeleton = Anim.DuplicateSkeleton(emo.Skeleton, 1, 20);

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

			//WorkingEMZ.Files.Remove(8);
			//WorkingEMZ.Files.Add(8, ema);
			//WorkingEMZ.Files.Remove(10);
			//WorkingEMZ.Files.Add(10, emo);
			RefreshTree(tvTreeUSF4);

		}

		private void dupliacteUSAMAN01BToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DuplicateUSA_MAN01_B();
		}

		private void cmLUAextractLUAScriptToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LUA nlua = (LUA)LastSelectedTreeNodeU.Tag;

			string intermediatefile = "plain_" + nlua.Name.Replace("\0", "");

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
			EMA ema = (EMA)WorkingEMZ.Files[LastSelectedTreeNodeU.Index];

			ema.AnimationCount++;
			//ema.Animations.Add(GeometryIO.AnimationFromColladaStruct());
			ema.AnimationPointersList.Add(0);
			ema.GenerateBytes();

			//WorkingEMZ.Files.Remove(LastSelectedTreeNode.Index);
			//WorkingEMZ.Files.Add(LastSelectedTreeNode.Index, ema);
			RefreshTree(tvTreeUSF4);
		}

		private void cmEMOexportEMOToOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Tag;

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
				AddStatus($"{emo.Name.Split('\0')[0]} extracted to {saveFileDialog1.FileName}");
			}
		}

		private void cmEMGexportEMGAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			
			EMG emg = (EMG)LastSelectedTreeNodeU.Tag;

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = string.Empty;
			saveFileDialog1.Filter = OBJFileFilter;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				List<string> lines;
				if(LastSelectedTreeNodeU.Parent != null)
                {
					EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Tag;
					lines = GeometryIO.EMGtoOBJ(emg, emo.Name.Split('\0')[0] + $"_EMG_{SelectedEMGNumberInTree}");
				}
				else
                {
					lines = GeometryIO.EMGtoOBJ(emg, "EMG");
				}
				File.WriteAllLines(saveFileDialog1.FileName, lines);
			}
		}

		private void cmMODexportModelAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Parent.Tag;
			EMG emg = (EMG)LastSelectedTreeNodeU.Parent.Tag;

			saveFileDialog1.InitialDirectory = string.Empty;
			saveFileDialog1.FileName = string.Empty;
			saveFileDialog1.Filter = OBJFileFilter;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				List<string> lines = GeometryIO.ModeltoOBJ(emg, SelectedModelNumberInTree, emo.Name.Split('\0')[0] + $"_EMG_{SelectedEMGNumberInTree}");

				File.WriteAllLines(saveFileDialog1.FileName, lines);
			}
		}

		private void cmSUBMexportSubmodelAsOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Parent.Parent.Tag;
			EMG emg = (EMG)LastSelectedTreeNodeU.Parent.Parent.Tag;

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

				EMB targetEMZ;

				if (LastSelectedTreeNodeU.Tag.ToString() == "EMZ") targetEMZ = WorkingEMZ;
				else targetEMZ = WorkingTEXEMZ;


				if (filepath.Trim() != string.Empty)
				{
					FileStream fsSource = new FileStream(diagOpenOBJ.FileName, FileMode.Open, FileAccess.Read);
					byte[] bytes;
					using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII)) { bytes = br.ReadBytes((int)fsSource.Length); }

					USF4File file;

					if (extension == "emo") file = new EMO();
					else if (extension == "emm") file = new EMM();
					else if (extension == "emb") file = new EMB();
					else if (extension == "ema") file = new EMA();
					else if (extension == "csb") file = new CSB();
					else if (extension == "lua") file = new LUA();
					else file = new OtherFile();

					file.ReadFile(bytes);

					//Check if we've got a lua SCRIPT instead of lua BYTECODE
					//At some point we want to handle this properly, but for now just throw an error and cancel
					if (extension == "lua" && Utils.ReadInt(true, 0, bytes) != USF4Methods.LUA)
					{
						AddStatus("Add File cancelled. LUA file must be in bytecode format to be added.");
						return;
					}

					file.Name = name;

					if (extension == "emb")
					{
						targetEMZ = WorkingTEXEMZ;
						AddStatus($"File {name} added to .TEX.EMZ");
					}
					else AddStatus($"File {name} added to .EMZ");

					targetEMZ.Files.Insert(targetEMZ.Files.Count, file);
					targetEMZ.NumberOfFiles++;
					targetEMZ.FileNamePointersList.Add(0x00);
					targetEMZ.FileLengthsList.Add(0x00);
					targetEMZ.FilePointersList.Add(0x00);
					targetEMZ.FileNamesList.Add(name);

					RefreshTree(tvTreeUSF4);
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
			WorkingEMZ.Files.RemoveAt(index);

			List<USF4File> temp = new List<USF4File>();
			for (int i = 0; i < index; i++)
			{
				temp.Insert(i, WorkingEMZ.Files[i]);
			}
			for (int i = index; i < WorkingEMZ.Files.Count; i++)
			{
				temp.Insert(i, WorkingEMZ.Files[i + 1]);
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
			WorkingTEXEMZ.Files.RemoveAt(index);

			List<USF4File> temp = new List<USF4File>();
			for (int i = 0; i < index; i++)
			{
				temp.Insert(i, WorkingTEXEMZ.Files[i]);
			}
			for (int i = index; i < WorkingTEXEMZ.Files.Count; i++)
			{
				temp.Insert(i, WorkingTEXEMZ.Files[i + 1]);
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
				AddStatus($"{WorkingEMZ.FileNamesList[LastSelectedTreeNodeU.Index].Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNodeU.Index);
				RefreshTree(tvTreeUSF4);
			}
		}

		private void deleteEMMToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Delete EMM?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				cmUNIVdeleteFileToolStripMenuItem_Click(sender, e);
			}
		}

		private void deleteEMOToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Delete EMO?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{WorkingEMZ.FileNamesList[LastSelectedTreeNodeU.Index].Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNodeU.Index);
				RefreshTree(tvTreeUSF4);
			}
		}

		private void deleteEMBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Delete EMB?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{WorkingTEXEMZ.FileNamesList[LastSelectedTreeNodeU.Index].Split('\0')[0]} deleted from .TEX.EMZ");
				DeleteFileTEXEMZ(LastSelectedTreeNodeU.Index);
				RefreshTree(tvTreeUSF4);
			}
		}

		private void deleteLUAToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Delete LUA?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{WorkingEMZ.FileNamesList[LastSelectedTreeNodeU.Index].Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNodeU.Index);
				RefreshTree(tvTreeUSF4);
			}
		}

		private void deleteCSBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult = MessageBox.Show("Delete CSB?", TStrings.STR_Information, MessageBoxButtons.OKCancel);
			if (DialogResult == DialogResult.OK)
			{
				AddStatus($"{WorkingEMZ.FileNamesList[LastSelectedTreeNodeU.Index].Split('\0')[0]} deleted from .EMZ");
				DeleteFileEMZ(LastSelectedTreeNodeU.Index);
				RefreshTree(tvTreeUSF4);
			}
		}

		private void rawDumpEMMToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMM emm = (EMM)WorkingEMZ.Files[LastSelectedTreeNodeU.Index];

			emm.GenerateBytes();

			saveFileDialog1.Filter = EMMFileFilter;
			saveFileDialog1.FileName = emm.Name;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, emm.HEXBytes);
				AddStatus($"Extracted {emm.Name}");
			}
		}

		private void rawDumpEMBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMB emb = (EMB)LastSelectedTreeNodeU.Tag;

			emb.GenerateBytes();

			saveFileDialog1.Filter = EMBFileFilter;
			saveFileDialog1.FileName = emb.Name;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, emb.HEXBytes);
				AddStatus($"Extracted {emb.Name}");
			}
		}

		private void injectColladaAsEMGExperimentalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Tag;

			Skeleton skel = emo.Skeleton;

			EMG tempemg = GeometryIO.GrendgineCollada2(skel, out List<Node> tempnodes);

			EMG emg = emo.EMGs[SelectedEMGNumberInTree];
			Model m = emg.Models[0];
			SubModel sm = m.SubModels[0];

			m.VertexData = new List<Vertex>(tempemg.Models[0].VertexData);
			m.VertexCount = m.VertexData.Count;

			sm.BoneIntegersList = tempemg.Models[0].SubModels[0].BoneIntegersList;
			sm.BoneIntegersCount = sm.BoneIntegersList.Count;
			sm.DaisyChain = tempemg.Models[0].SubModels[0].DaisyChain;
			sm.DaisyChainLength = sm.DaisyChain.Length;

			m.SubModels = new List<SubModel>() { sm };
			m.SubModelsCount = 1;
			m.SubModelPointersList = new List<int>() { 0 };

			emg.Models = new List<Model>() { m };
			emg.ModelPointersList = new List<int>() { 0 };
			emg.ModelCount = 1;
			emg.GenerateBytes();

			emo.EMGs.RemoveAt(SelectedEMGNumberInTree);
			emo.EMGs.Insert(SelectedEMGNumberInTree, emg);
			emo.GenerateBytes();

			RefreshTree(tvTreeUSF4);
		}

		private void modelTextureGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
			{
				if (matchingemb.FileNamesList != null)
				{
					for (int i = 0; i < modelTextureGrid.Rows.Count - 1; i++)
					{
						try
						{
							modelTextureGrid.Rows[i].Cells[2].ToolTipText = matchingemb.FileNamesList[int.Parse((string)modelTextureGrid.Rows[i].Cells[2].Value)].Replace('\0', ' ').Trim();
						}
						catch
						{
							modelTextureGrid.Rows[i].Cells[2].ToolTipText = string.Empty;
						}
					}
				}
			}
		}

		private void cmMATrenameMaterialToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMM emm = (EMM)LastSelectedTreeNodeU.Parent.Tag;
			Material mat = (Material)LastSelectedTreeNodeU.Tag;

			IB.StartPosition = FormStartPosition.CenterParent; IB.ShowDialog(this);

			if (IB.EnteredValue.Trim() != string.Empty)
			{
				string newMatName = IB.EnteredValue.Trim();
				string oldname = Encoding.ASCII.GetString(mat.Name).Replace("\0", "");

				foreach (TreeNode n in LastSelectedTreeNodeU.Parent.Nodes)
				{
					if (n.Text.Replace("\0", "") == newMatName)
					{
						MessageBox.Show("Name already exists!", TStrings.STR_Error);
						return;
					}
				}

				mat.Name = Utils.MakeModelName(IB.EnteredValue.Trim());
				mat.GenerateBytes();
				emm.Materials.RemoveAt(LastSelectedTreeNodeU.Index);
				emm.Materials.Insert(LastSelectedTreeNodeU.Index, mat);
				emm.GenerateBytes();

				RefreshTree(tvTreeUSF4);
				AddStatus($"Material '{oldname}' renamed to '{newMatName}' in {emm.Name.Replace("\0", "")}");
			}
			else
			{
				return;
			}
		}

		private void rawDumpEMGToolStripMenuItem_Click_1(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Tag;

			EMG emg = (EMG)LastSelectedTreeNodeU.Tag;

			emg.GenerateBytes();

			saveFileDialog1.Filter = EMGFileFilter;
			saveFileDialog1.FileName = $"{emo.Name}_EMG_{LastSelectedTreeNodeU.Index}";
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Utils.WriteDataToStream(saveFileDialog1.FileName, emg.HEXBytes);
				AddStatus($"Extracted {emo.Name}_EMG_{LastSelectedTreeNodeU.Index}");
			}
		}

		private void cmEMOaddEMGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Tag;

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
					RefreshTree(tvTreeUSF4);
				}
			}

		}

		private void AddProcessedSkeletonToPreview(EMAProcessor emap)
		{
			AnimatedNode[] anodes = emap.ReturnAnimatedNodes();

			for (int i = 0; i < anodes.Length; i++)
			{
				//Find world transform
				Matrix4x4 m = anodes[i].animatedMatrix;

				uc.AddModel(new GeometryModel3D()
				{
					Transform = new Transform3DGroup()
					{
						Children = new Transform3DCollection()
						{
                            //scale,
                            //rotY,
                            //rotX,
                            //rotZ,
                            //translate,

                            new MatrixTransform3D(new Matrix3D()
							{
								M11 = m.M11, M12 = m.M12, M13 = m.M13, M14 = m.M14,
								M21 = m.M21, M22 = m.M22, M23 = m.M23, M24 = m.M24,
								M31 = m.M31, M32 = m.M32, M33 = m.M33, M34 = m.M34,
								OffsetX = m.M41, OffsetY = m.M42, OffsetZ = m.M43, M44 = m.M44,
							}),
							new ScaleTransform3D(-1, 1, 1),
							//new TranslateTransform3D(m.M41, m.M42, m.M43)
						}
					},
					Geometry = new MeshGeometry3D()
					{
						Positions = new Point3DCollection()
						{
							new Point3D(0.0100000,0.0100000,-0.0100000),
							new Point3D(0.0100000,-0.0100000,-0.0100000),
							new Point3D(0.0100000,0.0100000,0.0100000),
							new Point3D(0.0100000,-0.0100000,0.0100000),
							new Point3D(-0.0100000,0.0100000,-0.0100000),
							new Point3D(-0.0100000,-0.0100000,-0.0100000),
							new Point3D(-0.0100000,0.0100000,0.0100000),
							new Point3D(-0.0100000,-0.0100000,0.0100000),
						},
						TriangleIndices = new Int32Collection()
						{
							4,2,0,
							2,7,3,
							6,5,7,
							1,7,5,
							0,3,1,
							4,1,5,
							4,6,2,
							2,6,7,
							6,4,5,
							1,3,7,
							0,2,3,
							4,0,1
						}
					},
					Material = new DiffuseMaterial()
					{
						Brush = new SolidColorBrush()
						{
							Color = System.Windows.Media.Color.FromRgb(0, 255, 0),
							Opacity = 1
						}
					}
				});
			}
		}

		private void AddEMOSkeletontoPreview(EMO emo)
		{
            foreach (Node n in emo.Skeleton.Nodes)
            {
                //Find world transform
                Node node = n;
                Matrix4x4 m = n.NodeMatrix;

                while (node.Parent != -1)
                {
                    node = emo.Skeleton.Nodes[node.Parent];
                    m = Matrix4x4.Multiply(m, node.NodeMatrix);
                }

                //m = Matrix4x4.Transpose(m);

                Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz);

                ScaleTransform3D scale = new ScaleTransform3D(sx, sy, sz);
                RotateTransform3D rotY = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), ry));
                RotateTransform3D rotX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rx));
                RotateTransform3D rotZ = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rz));
                TranslateTransform3D translate = new TranslateTransform3D(tx, ty, tz);

                uc.AddModel(new GeometryModel3D()
                {
                    Transform = new Transform3DGroup()
                    {
                        Children = new Transform3DCollection()
                        {
                            //scale,
                            //rotY,
                            //rotX,
                            //rotZ,
                            //translate,

                            new MatrixTransform3D(new Matrix3D()
                            {
                                M11 = m.M11, M12 = m.M12, M13 = m.M13, M14 = m.M14,
                                M21 = m.M21, M22 = m.M22, M23 = m.M23, M24 = m.M24,
                                M31 = m.M31, M32 = m.M32, M33 = m.M33, M34 = m.M34,
                                OffsetX = m.M41, OffsetY = m.M42, OffsetZ = m.M43, M44 = m.M44,
                            }),
							new ScaleTransform3D(-1, 1, 1),
							//new TranslateTransform3D(m.M41, m.M42, m.M43)
						}
                    },
                    Geometry = new MeshGeometry3D()
                    {
                        Positions = new Point3DCollection()
                        {
                            new Point3D(0.0100000,0.0100000,-0.0100000),
                            new Point3D(0.0100000,-0.0100000,-0.0100000),
                            new Point3D(0.0100000,0.0100000,0.0100000),
                            new Point3D(0.0100000,-0.0100000,0.0100000),
                            new Point3D(-0.0100000,0.0100000,-0.0100000),
                            new Point3D(-0.0100000,-0.0100000,-0.0100000),
                            new Point3D(-0.0100000,0.0100000,0.0100000),
                            new Point3D(-0.0100000,-0.0100000,0.0100000),
                        },
                        TriangleIndices = new Int32Collection()
                        {
                            4,2,0,
                            2,7,3,
                            6,5,7,
                            1,7,5,
                            0,3,1,
                            4,1,5,
                            4,6,2,
                            2,6,7,
                            6,4,5,
                            1,3,7,
                            0,2,3,
                            4,0,1
                        }
                    },
                    Material = new DiffuseMaterial()
                    {
                        Brush = new SolidColorBrush()
                        {
                            Color = System.Windows.Media.Color.FromRgb(0, 255, 0),
                            Opacity = 1
                        }
                    }
                });
            }
        }

		private void AddEMGtoPreview(EMO emo, EMG emg)
		{
			Node n = emo.Skeleton.Nodes[emg.RootBone];
			Matrix4x4 m = n.NodeMatrix;

			while (n.Parent != -1)
			{
				m = m * emo.Skeleton.Nodes[n.Parent].NodeMatrix;
				n = emo.Skeleton.Nodes[n.Parent];
			}			

			//Try to find the EMB matching the current model...
			matchingemb = new EMB();

			USF4File result = FindMatch(master_USF4FileList, typeof(EMB), emo.Name);
			if (result.GetType() == typeof(EMB)) matchingemb = (EMB)result;

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
						if (matchingemb.Files != null && ddsindices.Max() < matchingemb.Files.Count)
						{
							for (int k = 0; k < ddsindices.Count; k++)
							{
								dds = (DDS)matchingemb.Files[ddsindices[k]];

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
							mg.Children.Add(new EmissiveMaterial()
							{
								Brush = new SolidColorBrush()
								{
									Color = System.Windows.Media.Color.FromRgb(255, 0, 0),
									Opacity = 0.5
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

		private void AddOrphanEMGtoPreview(EMG emg)
		{
			matchingemb = new EMB();

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
						if (matchingemb.Files != null && ddsindices.Max() < matchingemb.Files.Count)
						{
							for (int k = 0; k < ddsindices.Count; k++)
							{
								dds = (DDS)matchingemb.Files[ddsindices[k]];

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

		private void cEMGpreviewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EH3D.Visible = true;
			EH3D.BringToFront();

			uc.ClearModels();

			TreeNode node = LastSelectedTreeNodeU;
			//Get the EMO node
			while (node.Tag.GetType() != typeof(EMG))
			{
				if (node.Parent == null) break;
				node = node.Parent;
			}
			if( node.Tag.GetType() == typeof(EMG))
            {
				EMG emg = (EMG)node.Tag;

				if(node.Parent != null)
                {
					EMO emo = (EMO)node.Parent.Tag;
					AddEMGtoPreview(emo, emg);
				}
				else
                {
					AddOrphanEMGtoPreview(emg);
                }
            }
		}

		private void closePreviewWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			uc.ClearModels();
			EH3D.Visible = false;
		}

		private void btnEO_CompressSM_Click(object sender, EventArgs e)
		{
			EMG emg = (EMG)LastSelectedTreeNodeU.Parent.Parent.Tag;
			Model model = (Model)LastSelectedTreeNodeU.Parent.Tag;
			SubModel sm = (SubModel)LastSelectedTreeNodeU.Tag;

			using (CompressDaisyChain Compress = new CompressDaisyChain(sm))
			{
				if (Compress.ShowDialog(this) == DialogResult.OK)
				{
					model.SubModels.RemoveAt(LastSelectedTreeNodeU.Index);
					model.SubModels.Insert(LastSelectedTreeNodeU.Index, Compress.sm);
					emg.Models.RemoveAt(LastSelectedTreeNodeU.Parent.Index);
					emg.Models.Insert(LastSelectedTreeNodeU.Parent.Index, model);
					emg.GenerateBytes();
					if(LastSelectedTreeNodeU.Parent.Parent.Parent != null)
                    {
						EMO emo = (EMO)LastSelectedTreeNodeU.Parent.Parent.Parent.Tag;
						emo.EMGs.RemoveAt(LastSelectedTreeNodeU.Parent.Parent.Index);
						emo.EMGs.Insert(LastSelectedTreeNodeU.Parent.Parent.Index, emg);
						emo.GenerateBytes();
                    }

					TreeDisplaySubModelData(Compress.sm);
					RefreshTree(tvTreeUSF4);
					AddStatus("Daisychain updated.");
				}
				else AddStatus("Compression cancelled.");
			}
		}

		public void btn_Reset_Click(object sender, EventArgs e)
		{
			uc.ClearModels();
			EH3D.Visible = false;
			//btn_ClosePreview.SendToBack();
		}

		private void cmEMOpreviewEMOToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EH3D.Visible = true;
			EH3D.BringToFront();
			uc.ClearModels();

			EMO emo = (EMO)LastSelectedTreeNodeU.Tag;
			foreach (EMG emg in emo.EMGs)
			{
				AddEMGtoPreview(emo, emg);
			}
		}

		private void eMOToLibraryControllerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GeometryIO.EMOtoCollada_Library_Controller((EMO)WorkingEMZ.Files[LastSelectedTreeNodeU.Index]);
		}

		private void cmEMOemoToColladaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)LastSelectedTreeNodeU.Tag;
			EMA ema = (EMA)((EMB)LastSelectedTreeNodeU.Parent.Tag).Files[LastSelectedTreeNodeU.Index-2];

			Grendgine_Collada collada = new Grendgine_Collada()
			{
				Collada_Version = "1.4.1",
				Asset = new Grendgine_Collada_Asset()
				{
					Up_Axis = "Y_UP"
				},
				Library_Controllers = GeometryIO.EMOtoCollada_Library_Controller(emo),
				//Library_Controllers = GeometryIO.LEGACY_EMOtoCollada_Library_Controller(emo),
				Library_Geometries = GeometryIO.LEGACY_EMOtoCollada_Library_Geometries(emo),
				//Library_Visual_Scene = GeometryIO.LEGACY_EMOtoCollada_Library_Visual_Scenes(emo),
				Library_Visual_Scene = GeometryIO.EMOtoCollada_Library_Visual_Scenes(emo),
				//Library_Visual_Scene = GeometryIO.EMOtoCollada_Library_Visual_Scenes_Matrix_NODEPTH(emo),
				//Library_Visual_Scene = GeometryIO.EMOtoCollada_Library_Visual_Scenes_Matrix(emo),
				Library_Animations = GeometryIO.LEGACY_EMAtoCollada_Library_Animations(ema)
				//Library_Animations = GeometryIO.EMAtoCollada_Library_AnimationsWithInterpolation(ema,1)
				//Library_Animations = GeometryIO.EMAtoCollada_Library_Animations_Matrix(ema, 0),
				//Library_Animations = GeometryIO.EMAtoCollada_Library_Animations_AssetExplorer(ema, 0),
			};

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



		private void setColourToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EMO emo = (EMO)WorkingEMZ.Files[LastSelectedTreeNodeU.Parent.Index];
			EMG emg = emo.EMGs[LastSelectedTreeNodeU.Index];

			for (int i = 0; i < emg.Models.Count; i++)
			{
				Model m = emg.Models[i];

				List<Vertex> tempverts = new List<Vertex>();
				for (int j = 0; j < m.VertexData.Count; j++)
				{
					Vertex v = m.VertexData[j];
					v.Colour = Utils.ReadFloat(0, new byte[] { 0xFE, 0xFE, 0xFE, 0xFF });
					v.ntangentX = 1;
					v.ntangentY = 0;
					v.ntangentZ = 0;

					tempverts.Add(v);
				}

				m.VertexData = tempverts;
				emg.Models.RemoveAt(i);
				emg.Models.Insert(i, m);
			}

			emg.GenerateBytes();
			emo.EMGs.RemoveAt(LastSelectedTreeNodeU.Index);
			emo.EMGs.Insert(LastSelectedTreeNodeU.Index, emg);
			emo.GenerateBytes();
		}

		#region keyhandling
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				uc.ClearModels();
				EH3D.Visible = false;
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
			{
				e.IsInputKey = true;
			}
			else if (e.KeyCode == Keys.Escape)
			{
				e.IsInputKey = true;
			}
			else if (e.KeyCode == Keys.Space)
			{
				e.IsInputKey = true;
			}
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
			{
				uc.ShiftPressed = true;
			}
			else if (e.KeyCode == Keys.Space)
			{
				uc.CentreCamera();
				uc.RotateCameraY(0);
			}
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
			{
				uc.ShiftPressed = false;
			}
		}
		#endregion

		private void Form1_Resize(object sender, EventArgs e)
		{
			EH3D.Width = pSelectedTreeNodeData.Width;
			EH3D.Height = pSelectedTreeNodeData.Height;
		}

        private void cmUNIVcloseFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			CloseFile(LastSelectedTreeNodeU);
        }

        private void cmUNIVdeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (LastSelectedTreeNodeU.Parent != null)
			{
				((USF4File)LastSelectedTreeNodeU.Parent.Tag).DeleteSubfile(LastSelectedTreeNodeU.Index);
				RefreshTree(tvTreeUSF4);
			}
        }

        private void cmUNIVsaveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			string filepath;

			saveFileDialog1.InitialDirectory = string.Empty;

			TreeNode n = LastSelectedTreeNodeU;
			while (n.Parent != null)
            {
				n = n.Parent;
            }
			USF4File file = (USF4File)n.Tag;

			saveFileDialog1.Filter = string.Empty;
			saveFileDialog1.FileName = file.Name;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				filepath = saveFileDialog1.FileName;
				File.Delete(filepath); //Not very good but works
				file.SaveFile(filepath);
				AddStatus($"Saved {filepath}");
			}
		}

        private void cmUNIVrawDumpFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			string filepath;

			saveFileDialog1.InitialDirectory = string.Empty;

			USF4File file = (USF4File)LastSelectedTreeNodeU.Tag;

			saveFileDialog1.Filter = string.Empty;
			saveFileDialog1.FileName = file.Name;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				filepath = saveFileDialog1.FileName;
				File.Delete(filepath); //Not very good but works
				file.SaveFile(filepath);
				AddStatus($"Raw dumped {filepath}");
			}
		}

        private void cmUNIVopenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = string.Empty;
			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				if (filepath.Trim() != string.Empty)
				{
					OpenFile(filepath);
				}
			}
		}

		private void OpenFile(string filepath)
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

			int FileType = Utils.ReadInt(true, 0, bytes);

			USF4File file = USF4Methods.CheckFile(FileType);

			file.ReadFile(bytes);
			file.Name = diagOpenOBJ.SafeFileName;
			master_USF4FileList.Add(file);

			RefreshTree(tvTreeUSF4);
		}

		private void CloseFile(TreeNode n)
        {
			while (n.Parent != null) n = n.Parent;
			master_USF4FileList.RemoveAt(n.Index);
			RefreshTree(tvTreeUSF4);
		}

        private void btnEO_SaveEMOEdits_Click(object sender, EventArgs e)
        {
			if (tbEMOmatcount.Text.Trim() != string.Empty)
			{
				int newMatCount = int.Parse(tbEMOmatcount.Text.Trim());
				EMO emo = (EMO)LastSelectedTreeNodeU.Tag;
				emo.NumberEMMMaterials = newMatCount;
				emo.GenerateBytes();

				AddStatus("EMO changes saved.");
				TreeDisplayEMOData(emo);
			}
		}

        private void cmMODopenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			diagOpenOBJ.RestoreDirectory = true;
			diagOpenOBJ.FileName = string.Empty;
			diagOpenOBJ.InitialDirectory = LastOpenFolder;
			diagOpenOBJ.Filter = string.Empty;
			if (diagOpenOBJ.ShowDialog() == DialogResult.OK)
			{
				string filepath = diagOpenOBJ.FileName;
				string extension = diagOpenOBJ.SafeFileName.Split('.').Last();
				if (filepath.Trim() != string.Empty)
				{
					if (extension == "obj")
                    {
						master_ModelFileList.Add(new ObjFile().ReadFile(filepath, diagOpenOBJ.SafeFileName));
						RefreshTree(tvTreeModel);
                    }

					if (extension == "smd")
                    {
						master_ModelFileList.Add(new SMDFile().ReadFile(filepath, diagOpenOBJ.SafeFileName));
						RefreshTree(tvTreeModel);
                    }
				}
			}
		}

        private void cmMODgenerateEMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (LastSelectedTreeNodeM.Tag.GetType() == typeof(ObjFile))
            {
				if (sender.GetType() == typeof(ToolStripMenuItem))
                {
					int bitdepth = Convert.ToInt32(((ToolStripMenuItem)sender).Text, 16);
					master_USF4FileList.Add(((ObjFile)LastSelectedTreeNodeM.Tag).GenerateEMO(bitdepth));
					RefreshTree(tvTreeUSF4);
                }
            }
			if (LastSelectedTreeNodeM.Tag.GetType() == typeof(SMDFile))
			{
				if (sender.GetType() == typeof(ToolStripMenuItem))
				{
					int bitdepth = Convert.ToInt32(((ToolStripMenuItem)sender).Text, 16);
					master_USF4FileList.Add(((SMDFile)LastSelectedTreeNodeM.Tag).GenerateEMO(bitdepth));
					RefreshTree(tvTreeUSF4);
				}
			}
		}

        private void cmMODcloseFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			//Find top level node for current item
			TreeNode n = LastSelectedTreeNodeM;
			while (n.Parent != null) n = n.Parent;
			master_ModelFileList.RemoveAt(n.Index);
			RefreshTree(tvTreeModel);
        }

        private void cmEMOpreviewSkeletonToolStripMenuItem_Click(object sender, EventArgs e)
        {
			AddEMOSkeletontoPreview((EMO)LastSelectedTreeNodeU.Tag);
        }

		int frame = 0;

        private void cmEMAgenerateKeyFramesToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMA ema = (EMA)LastSelectedTreeNodeU.Parent.Tag;
			Animation anim = (Animation)LastSelectedTreeNodeU.Tag;

			EMAProcessor eMAProcessor = new EMAProcessor(ema, LastSelectedTreeNodeU.Index);

			eMAProcessor.AnimateFrame(frame);
			frame += 5;

			AddProcessedSkeletonToPreview(eMAProcessor);

			Anim.InterpolateRelativeKeyFrames(anim, 3, 3, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz);

			GeometryIO.EMAtoCollada_Library_AnimationsWithInterpolation(ema, LastSelectedTreeNodeU.Index);

			Grendgine_Collada collada = new Grendgine_Collada()
			{
				Collada_Version = "1.4.1",
				Asset = new Grendgine_Collada_Asset()
				{
					Up_Axis = "Y_UP"
				},
				Library_Animations = GeometryIO.EMAtoCollada_Library_AnimationsWithInterpolation(ema, LastSelectedTreeNodeU.Index)
			};

			try
			{
				Grendgine_Collada col_scenes = collada;

				XmlSerializer sr = new XmlSerializer(typeof(Grendgine_Collada));
				TextWriter tw = new StreamWriter("anim_out.dae");
				sr.Serialize(tw, col_scenes);

				tw.Close();


			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.ReadLine();
			}

			//ANIMATIONS ARE APPLIED TO MODEL GROUPS, NOT MODELS

			for (int i = 0; i < ema.Skeleton.Nodes.Count; i++)
            {
				Node n = ema.Skeleton.Nodes[i];
				SingleAnimationUsingKeyFrames saufk = Anim.GenerateKeyFrames(anim, i, 0, 0);
            }
        }

        private void cmUNIrenameFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
			USF4File uf = (USF4File)LastSelectedTreeNodeU.Tag;

			IB.StartPosition = FormStartPosition.CenterParent; IB.ShowDialog(this);

			if (IB.EnteredValue.Trim() != string.Empty)
			{
				string newname = IB.EnteredValue.Trim();
				string oldname = uf.Name;

				if (oldname == newname)
				{
					return;
				}

				uf.Name = newname;

				RefreshTree(tvTreeUSF4);
				AddStatus($"File '{oldname}' renamed to '{newname}'.");
			}
			else
			{
				return;
			}
		}

        private void cmEMAexportToColladaToolStripMenuItem_Click(object sender, EventArgs e)
        {
			EMA ema = (EMA)LastSelectedTreeNodeU.Parent.Tag;
			EMO emo = (EMO)((EMB)LastSelectedTreeNodeU.Parent.Parent.Tag).Files[LastSelectedTreeNodeU.Parent.Index + 2];

			Grendgine_Collada collada = new Grendgine_Collada()
			{
				Collada_Version = "1.4.1",
				Asset = new Grendgine_Collada_Asset()
				{
					Up_Axis = "Y_UP"
				},
				Library_Controllers = GeometryIO.EMOtoCollada_Library_Controller(emo),
				//Library_Controllers = GeometryIO.LEGACY_EMOtoCollada_Library_Controller(emo),
				Library_Geometries = GeometryIO.LEGACY_EMOtoCollada_Library_Geometries(emo),
				//Library_Visual_Scene = GeometryIO.LEGACY_EMOtoCollada_Library_Visual_Scenes(emo),
				//Library_Visual_Scene = GeometryIO.EMOtoCollada_Library_Visual_Scenes_Matrix_NODEPTH(emo),
				Library_Visual_Scene = GeometryIO.EMOtoCollada_Library_Visual_Scenes_Matrix(emo),
				//Library_Visual_Scene = GeometryIO.EMOtoCollada_Library_Visual_Scenes(emo),
				//Library_Animations = GeometryIO.LEGACY_EMAtoCollada_Library_Animations(ema)
				//Library_Animations = GeometryIO.EMAtoCollada_Library_AnimationsWithInterpolation(ema,0)
				//Library_Animations = GeometryIO.EMAtoCollada_Library_Animations_Matrix(ema, 0),
				Library_Animations = GeometryIO.EMAtoCollada_Library_Animations_AssetExplorer(ema, LastSelectedTreeNodeU.Index),
			};

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


