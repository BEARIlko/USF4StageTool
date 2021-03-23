using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using grendgine_collada;

namespace USF4_Stage_Tool
{
    public static class GeometryIO
    {
        public static List<int> BruteForceChain(List<int[]> nIndices, int attempts)
        {

            List<int> best = new List<int>();

            Random random = new Random();

            int rnd = random.Next(0, nIndices.Count);

            for (int i = 0; i < attempts; i++)
            {
                List<int[]> test = new List<int[]>();

                test.AddRange(nIndices.GetRange(rnd, nIndices.Count - rnd));
                test.AddRange(nIndices.GetRange(0, rnd));

                List<int> newlist = new List<int>(DaisyChainFromIndices(test));

                if (best.Count == 0) best = new List<int>(newlist);
                else if (best.Count > newlist.Count)
                {
                    best = new List<int>(newlist);
                }
            }
            return best;
        }

        public static List<int> DaisyChainFromIndices(List<int[]> nIndices)
        {
            List<int> Chain = new List<int>();
            bool bForwards = false;
            int count = nIndices.Count;

            int compression = nIndices.Count * 3;
            int compression_zero = compression;

            //Initialise start of chain
            int buffer1 = nIndices[0][2];
            int buffer2 = nIndices[0][1];
            Chain.AddRange(new List<int> { nIndices[0][0], nIndices[0][1], nIndices[0][2] });
            nIndices.RemoveAt(0);

            int[] workingArray;

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
                            //TimeEstimate(TStrings.STR_ReorderingFaces, count, Chain.Count);
                            break;
                        }

                    }
                }
                //No match found - if we've run out of faces, great, if not, re-initialise
                if (nIndices.Count > 0)
                {
                    compression += 2;
                    Random random = new Random();

                    int rnd =  random.Next(0, nIndices.Count);

                    workingArray = nIndices[rnd];

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
                    nIndices.RemoveAt(rnd);
                    bForwards = !bForwards;
                    //progressBar1.Value += 1;
                    //TimeEstimate(TStrings.STR_ReorderingFaces, count, Chain.Count);
                }
            }

            //progressBar1.Value = progressBar1.Maximum;

            //if (Chain.Count > 0xFFFF) AddStatus("Warning - Encoded object has too many faces. Consider splitting into smaller sub-models to ensure correct loading.");

            Console.WriteLine($"Compression {compression}/{compression_zero} = {100 * compression / compression_zero}%");

            return Chain;
        }

        public static List<int[]> FaceIndicesFromDaisyChain(int[] DaisyChain, bool readmode = false)
        {
            List<int[]> FaceIndices = new List<int[]>();

            if (readmode == true && DaisyChain.Length % 3 == 0)
            {
                for (int i = 0; i < DaisyChain.Length/3; i++)
                {
                    FaceIndices.Add(new int[] { DaisyChain[3*i + 2], DaisyChain[3*i + 1], DaisyChain[3*i] });
                }
            }
            else
            {
                Boolean bForwards = true;
                for (int i = 0; i < DaisyChain.Length - 2; i++)
                {
                    if (bForwards) //This seems to be backwards?? But it works.
                    {
                        int[] temp = new int[] { DaisyChain[i + 2], DaisyChain[i + 1], DaisyChain[i] };

                        if (temp[0] != temp[1] && temp[1] != temp[2] && temp[2] != temp[0])
                        {
                            FaceIndices.Add(temp);
                        }
                    }
                    else
                    {
                        int[] temp = new int[] { DaisyChain[i], DaisyChain[i + 1], DaisyChain[i + 2] };

                        if (temp[0] != temp[1] && temp[1] != temp[2] && temp[2] != temp[0])
                        {
                            FaceIndices.Add(temp);
                        }
                    }

                    bForwards = !bForwards;
                }
            }

            return FaceIndices;
        }

        public static List<string> EMGtoOBJ(EMG emg, string name = "??EMO_??EMG_??Model", bool invert_indices = false)
        {
            List<string> lines = new List<string>();
            //Force inverted face indices if there's multiple models
            //Otherwise, respect the initial "invert_indices" setting
            if (emg.ModelCount > 1) invert_indices = true; 
            
            lines.Add($"o {name}");

            for (int i = 0; i < emg.Models.Count; i++)
            {
                lines.AddRange(ModeltoOBJ(emg, i, name, invert_indices));
            }

            return lines;
        }

        public static List<string> ModeltoOBJ(EMG emg, int modelindex, string name = "??EMO_??EMG_??Model", bool invert_indices = false)
        {
            List<string> lines = new List<string>();

            Model model = emg.Models[modelindex];

            lines.Add($"g {name}_m_{modelindex}");
            for (int j = 0; j < model.VertexData.Count; j++)
            {
                Vertex v = model.VertexData[j];
                lines.Add($"v {-v.X} {v.Y} {v.Z}");
            }
            for (int j = 0; j < model.VertexData.Count; j++)
            {
                Vertex v = model.VertexData[j];
                lines.Add($"vt {v.U} {-v.V}");
            }
            for (int j = 0; j < model.VertexData.Count; j++)
            {
                Vertex v = model.VertexData[j];
                lines.Add($"vn {v.nX} {v.nY} {v.nZ}");
            }

            for (int j = 0; j < model.SubModels.Count; j++)
            {
                lines.AddRange(SubmodeltoOBJ(emg, modelindex, j, false, invert_indices));
            }

            return lines;
        }

        public static List<string> SubmodeltoOBJ(EMG emg, int modelindex, int submodelindex, bool writeverts, bool invert_indices = false)
        {
            Model model = emg.Models[modelindex];
            SubModel sm = model.SubModels[submodelindex];
            List<string> lines = new List<string>();
            bool readmode = false;
            if (model.ReadMode == 0) { readmode = true; }

            if (writeverts) //If we're using this as a sub-call of ModeltoOBJ, we don't want verts writing
            {
                for (int i = 0; i < model.VertexData.Count; i++)
                {
                    Vertex v = model.VertexData[i];
                    lines.Add($"v {-v.X} {v.Y} {v.Z}");
                }
                for (int i = 0; i < model.VertexData.Count; i++)
                {
                    Vertex v = model.VertexData[i];
                    lines.Add($"vt {v.U} {-v.V}");
                }
                for (int i = 0; i < model.VertexData.Count; i++)
                {
                    Vertex v = model.VertexData[i];
                    lines.Add($"vn {v.nX} {v.nY} {v.nZ}");
                }
            }

            List<int[]> indices = FaceIndicesFromDaisyChain(sm.DaisyChain, readmode);

            if (invert_indices)
            {
                for (int i = 0; i < indices.Count; i++)
                {
                    for (int j = 0; j < indices[i].Length; j++)
                    {
                        indices[i][j] -= model.VertexData.Count;
                    }
                }
            }
            lines.Add($"usemtl {Encoding.ASCII.GetString(sm.SubModelName).Split('\0')[0]}");
            //Add in material tags - if you want to texture the model in blender makes the process slightly easier

            for (int i = 0; i < indices.Count; i++)
            {
                if (invert_indices) lines.Add($"f {indices[i][0]}/{indices[i][0]}/{indices[i][0]} {indices[i][1]}/{indices[i][1]}/{indices[i][1]} {indices[i][2]}/{indices[i][2]}/{indices[i][2]}");
                else lines.Add($"f {indices[i][0] + 1}/{indices[i][0] + 1}/{indices[i][0] + 1} {indices[i][1] + 1}/{indices[i][1] + 1}/{indices[i][1] + 1} {indices[i][2] + 1}/{indices[i][2] + 1}/{indices[i][2] + 1}");
            }

            return lines;
        }

        public static Animation AnimationFromColladaStruct()
        {
            Grendgine_Collada model = Grendgine_Collada.Grendgine_Load_File("USAMananim.dae");
            Animation animation1 = new Animation();

            Dictionary<string, int> master_bone_dict = new Dictionary<string, int>();

            foreach (Grendgine_Collada_Controller c in model.Library_Controllers.Controller)
            {
                foreach (Grendgine_Collada_Source s in c.Skin.Source)
                { 
                    if (s.ID.Contains("skin-joints"))
                    {
                        foreach (string str in s.Name_Array.Value_Pre_Parse.Trim().Split(' '))
                        {
                            if (!master_bone_dict.TryGetValue(str, out _))
                            {
                                master_bone_dict.Add(str, master_bone_dict.Count);
                            }
                        }
                    }
                }
            }
            
            foreach (Grendgine_Collada_Animation a in model.Library_Animations.Animation)
            {
                Dictionary<float, int> valuedict = new Dictionary<float, int>();
                valuedict.Add(0, 0);

                int d_max = 0;

                animation1 = new Animation()
                {
                    CmdTrackCount = 0,
                    CmdTrackPointersList = new List<int>(),
                    CMDTracks = new List<CMDTrack>(),
                    Name = Encoding.ASCII.GetBytes("ANIMATION_000"),
                    NamePointer = 0,
                    ValuesList = new List<float>(),
                    ValuesListPointer = 0,
                    ValueCount = 0,
                    Duration = 0
                };

                foreach (Grendgine_Collada_Animation an in a.Animation)
                {
                    int boneID = -1;
                    byte bitflag = 0;
                    byte transformtype = 3;
                    List<int> tempsteps = new List<int>() { 0 };
                    List<int> tempindices = new List<int>() { 0 };

                    foreach (string str in an.ID.Trim().Split('_'))
                    {
                        if (master_bone_dict.TryGetValue(str, out boneID) && boneID > 0) break;
                        boneID = -1;
                    }

                    if (an.ID.Contains("location"))
                    {
                        transformtype = 0;

                        if (an.ID.Contains("location_X")) bitflag = 0;
                        else if (an.ID.Contains("location_Y")) bitflag = 1;
                        else bitflag = 2;
                    }
                    else if (an.ID.Contains("rotation_euler"))
                    {
                        transformtype = 1;

                        if (an.ID.Contains("rotation_euler_X")) bitflag = 0;
                        else if (an.ID.Contains("rotation_euler_Y")) bitflag = 1;
                        else bitflag = 2;
                    }
                    else
                    {
                        transformtype = 2;

                        if (an.ID.Contains("scale_X")) bitflag = 0;
                        else if (an.ID.Contains("scale_Y")) bitflag = 1;
                        else bitflag = 2;
                    }

                    foreach (Grendgine_Collada_Source s in an.Source)
                    {
                        if (s.ID.Contains("input"))
                        {
                            foreach (string str in s.Float_Array.Value_As_String.Trim().Split(' '))
                            {
                                tempsteps.Add((int)Math.Round(float.Parse(str) * 24f));
                            }
                        }
                        if (s.ID.Contains("output"))
                        {
                            foreach (string str in s.Float_Array.Value_As_String.Trim().Split(' '))
                            {
                                if (!valuedict.TryGetValue(float.Parse(str), out _))
                                {
                                    valuedict.Add(float.Parse(str), valuedict.Count);
                                }

                                tempindices.Add(valuedict[float.Parse(str)]);
                            }
                        }
                        if (s.ID.Contains("interpolation"))
                        {

                        }
                    }

                    d_max = Math.Max(d_max, tempsteps.Max());

                    CMDTrack cmd = new CMDTrack()
                    {
                        TransformType = transformtype,
                        BitFlag = bitflag,
                        BoneID = boneID,
                        StepCount = tempsteps.Count,
                        StepsList = tempsteps,
                        IndicesListPointer = 0,
                        IndicesList = tempindices
                    };

                    animation1.CMDTracks.Add(cmd);
                }

                animation1.CmdTrackCount = animation1.CMDTracks.Count;
                animation1.CmdTrackPointersList = new int[animation1.CMDTracks.Count].ToList();
                animation1.Duration = d_max;
                animation1.ValueCount = valuedict.Count;
                animation1.ValuesList = valuedict.Keys.ToList();
            }
            
            return animation1;
        }

        public static Grendgine_Collada_Library_Geometries EMOtoCollada_Library_Geometries(EMO emo)
        {
            string emo_name = Encoding.ASCII.GetString(emo.Name);
            List<Vertex> vertices = new List<Vertex>();
            string pos_string = string.Empty;
            string norm_string = string.Empty;
            string map_string = string.Empty;
            string p_string = string.Empty;

            foreach (EMG e in emo.EMGs)
            {
                int v_t = 0;
                foreach (Model m in e.Models)
                {
                    bool readmode = (m.ReadMode == 1) ? true : false;

                    vertices.AddRange(m.VertexData);

                    foreach (SubModel sm in m.SubModels)
                    {
                        List<int[]> temp_indices = GeometryIO.FaceIndicesFromDaisyChain(sm.DaisyChain, readmode);

                        foreach (int[] f in temp_indices)
                        {
                            p_string += $"{f[2] + v_t} {f[2] + v_t} {f[2] + v_t} {f[1] + v_t} {f[1] + v_t} {f[1] + v_t} {f[0] + v_t} {f[0] + v_t} {f[0] + v_t} ";
                        }
                    }

                    v_t = vertices.Count;
                }
            }

            foreach (Vertex v in vertices)
            {
                pos_string += $"{v.X} {v.Y} {v.Z} ";
                norm_string += $"{v.nX} {v.nY} {v.nZ} ";
                map_string += $"{v.U} {v.V} ";
            }
            pos_string.Trim();
            norm_string.Trim();
            map_string.Trim();
            p_string.Trim();

            Grendgine_Collada_Library_Geometries geometries = new Grendgine_Collada_Library_Geometries()
            {

                Geometry = new Grendgine_Collada_Geometry[]
                {
                    new Grendgine_Collada_Geometry()
                    {
                        ID = $"{emo_name}-mesh",
                        Name = emo_name,
                        Mesh = new Grendgine_Collada_Mesh()
                        {
                            Source = new Grendgine_Collada_Source[]
                            {
                                //Position floats
                                new Grendgine_Collada_Source()
                                {
                                    ID = $"{emo_name}-mesh-positions",
                                    Float_Array = new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emo_name}-mesh-positions-array",
                                        Count = vertices.Count * 3,
                                        Value_As_String = pos_string
                                    },
                                    Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                    {
                                        Accessor = new Grendgine_Collada_Accessor()
                                        {
                                            Count = (uint)vertices.Count,
                                            Stride = 3,
                                            Param = new Grendgine_Collada_Param[]
                                            {
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "X",
                                                    Type = "float"
                                                },
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "Y",
                                                    Type = "float"
                                                },
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "Z",
                                                    Type = "float"
                                                },
                                            }
                                        }
                                    }
                                },
                                //Normal floats
                                new Grendgine_Collada_Source()
                                {
                                    ID = $"{emo_name}-mesh-normals",
                                    Float_Array = new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emo_name}-mesh-normals-array",
                                        Count = vertices.Count*3,
                                        Value_As_String = norm_string
                                    },
                                    Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                    {
                                        Accessor = new Grendgine_Collada_Accessor()
                                        {
                                            Count = (uint)vertices.Count,
                                            Stride = 3,
                                            Param = new Grendgine_Collada_Param[]
                                            {
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "X",
                                                    Type = "float"
                                                },
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "Y",
                                                    Type = "float"
                                                },
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "Z",
                                                    Type = "float"
                                                },
                                            }
                                        }
                                    }
                                },
                                //UV Map floats
                                new Grendgine_Collada_Source()
                                {
                                    ID = $"{emo_name}-mesh-map-0",
                                    Float_Array = new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emo_name}-mesh-map-0-array",
                                        Count = vertices.Count*2,
                                        Value_As_String = map_string
                                    },
                                    Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                    {
                                        Accessor = new Grendgine_Collada_Accessor()
                                        {
                                            Count = (uint)vertices.Count,
                                            Stride = 2,
                                            Param = new Grendgine_Collada_Param[]
                                            {
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "S",
                                                    Type = "float"
                                                },
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "T",
                                                    Type = "float"
                                                },
                                            }
                                        }
                                    }
                                }
                            },
                            Vertices = new Grendgine_Collada_Vertices()
                            {
                                ID = $"#{emo_name}-mesh-vertices",
                                Input = new Grendgine_Collada_Input_Unshared[]
                                {
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.POSITION,
                                        source = $"#{emo_name}-mesh-positions"
                                    }
                                }
                            },
                            Triangles = new Grendgine_Collada_Triangles[]
                            {
                                new Grendgine_Collada_Triangles()
                                {
                                   //Material = "Material-material",
                                   P = new Grendgine_Collada_Int_Array_String()
                                   {
                                        Value_As_String = p_string,
                                   },
                                   Count = p_string.Split(' ').Count(),
                                   Input = new Grendgine_Collada_Input_Shared[]
                                   {
                                       new Grendgine_Collada_Input_Shared()
                                       {
                                           Semantic = Grendgine_Collada_Input_Semantic.VERTEX,
                                           source = $"#{emo_name}-mesh-vertices",
                                           Offset = 0
                                       },
                                       new Grendgine_Collada_Input_Shared()
                                       {
                                           Semantic = Grendgine_Collada_Input_Semantic.NORMAL,
                                           source = $"#{emo_name}-mesh-normals",
                                           Offset = 1
                                       },
                                       new Grendgine_Collada_Input_Shared()
                                       {
                                           Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD,
                                           source = $"#{emo_name}-mesh-map-0",
                                           Offset = 2
                                       }
                                   }
                                }
                            }
                        }
                    }
                }
            };

            return geometries;
        }

        public static Grendgine_Collada_Library_Visual_Scenes EMOtoCollada_Library_Visual_Scenes(EMO emo)
        {
            List<Grendgine_Collada_Node> node_list = new List<Grendgine_Collada_Node>();

            Skeleton sk = emo.Skeleton;


            for (int i = 0; i < sk.Nodes.Count; i++)
            {
                Node n = sk.Nodes[i];
                if (n.Parent == -1) //Root nodes
                {
                    Matrix4x4 m = n.NodeMatrix;
                    string name = Encoding.ASCII.GetString(sk.NodeNames[i]);

                    node_list.Add(new Grendgine_Collada_Node()
                    {
                        ID = name,
                        Name = name,
                        sID = name,
                        Type = Grendgine_Collada_Node_Type.JOINT,
                        node = RecursiveNode(sk,i),
                        Matrix = new Grendgine_Collada_Matrix[]
                        {
                            new Grendgine_Collada_Matrix() //Column-major
                            {
                                sID = "transform",
                                Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44}"
                            }
                        }
                    });
                }
            }

            node_list.Add(new Grendgine_Collada_Node()
            {
                ID = "Armature",
                Name = "Armature",
                Type = Grendgine_Collada_Node_Type.NODE,
                Matrix = new Grendgine_Collada_Matrix[] 
                {
                    new Grendgine_Collada_Matrix()
                    {
                        sID = "transform",
                        Value_As_String = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"
                    }
                },
                node = new Grendgine_Collada_Node[] {node_list[0]}
            });

            Grendgine_Collada_Node[] RecursiveNode(Skeleton s, int n)
            {
                List<Grendgine_Collada_Node> child_list = new List<Grendgine_Collada_Node>();
                for (int i = 0; i < s.Nodes.Count; i++)
                {
                    if(s.Nodes[i].Parent == n)
                    {
                        Node node = sk.Nodes[i];
                        Matrix4x4 m = node.NodeMatrix;
                        string name = Encoding.ASCII.GetString(sk.NodeNames[i]);

                        child_list.Add(new Grendgine_Collada_Node() 
                        {
                            ID = name,
                            Name = name,
                            sID = name,
                            Type = Grendgine_Collada_Node_Type.JOINT,
                            node = RecursiveNode(s,i),
                            Matrix = new Grendgine_Collada_Matrix[]
                            {
                                new Grendgine_Collada_Matrix() //Might need to switch columns/rows
                                {
                                    sID = "transform",
                                    Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44}"
                                }
                            }
                        });
                    }
                }
                return child_list.ToArray();
            }

            Grendgine_Collada_Library_Visual_Scenes vscenes = new Grendgine_Collada_Library_Visual_Scenes()
            {
                Visual_Scene = new Grendgine_Collada_Visual_Scene[]
                {
                    new Grendgine_Collada_Visual_Scene()
                    {
                        ID = "Scene",
                        Name = "Scene",
                        Node = new Grendgine_Collada_Node[]
                        {
                            node_list[1],
                            new Grendgine_Collada_Node()
                            {
                                ID = Encoding.ASCII.GetString(emo.Name),
                                Name = Encoding.ASCII.GetString(emo.Name),
                                Type = Grendgine_Collada_Node_Type.NODE,
                                Instance_Controller = new Grendgine_Collada_Instance_Controller[]
                                {
                                    new Grendgine_Collada_Instance_Controller()
                                    {
                                        URL = $"Armature_{Encoding.ASCII.GetString(emo.Name)}-skin",
                                        Skeleton = new Grendgine_Collada_Skeleton[]
                                        {
                                            new Grendgine_Collada_Skeleton()
                                            {
                                                Value = $"#{Encoding.ASCII.GetString(emo.Skeleton.NodeNames[0])}"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return vscenes;
        }

        public static Grendgine_Collada_Library_Controllers EMOtoCollada_Library_Controller(EMO emo)
        {
            string sbp_string = string.Empty;
            Dictionary<float, int> bw_float_dict = new Dictionary<float, int>();
            Dictionary<int, Vertex> v_dict = new Dictionary<int, Vertex>();
            string vcounts_string = string.Empty;
            string v_string = string.Empty;
            string emo_name = Encoding.ASCII.GetString(emo.Name);
            List<string> bone_names = new List<string>();
            List<Vertex> vertices = new List<Vertex>();

            //Building an index to translate Vertex bone IDs into Skeleton bone IDs.
            //Verts can appear in multiple submodels, but when they do their bone ID entries have to "match" in
            //each submodel's BoneIntegersList ie return the same bone
            //...I hope
            foreach (EMG e in emo.EMGs)
            {
                foreach (Model m in e.Models)
                {
                    foreach (SubModel sm in m.SubModels)
                    {
                        for (int i = 0; i < sm.DaisyChain.Length; i++)
                        {
                            Vertex v = m.VertexData[sm.DaisyChain[i]];

                            List<int> temp_boneIDs = new List<int>();

                            foreach (int b in v.BoneIDs)
                            {
                                if(b != 0) temp_boneIDs.Add(sm.BoneIntegersList[b]);
                            }
                            if (temp_boneIDs.Count == 0) temp_boneIDs.Add(sm.BoneIntegersList[0]);

                            v.BoneIDs = temp_boneIDs;

                            if(!v_dict.TryGetValue(sm.DaisyChain[i], out _))
                            {
                                v_dict.Add(sm.DaisyChain[i], v);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < v_dict.Count; i++)
            {
                Vertex v = v_dict[i];

                if (v.BoneWeights.Count < 4) //Calculate the 4th weight from the 3 weights stored in the EMG
                {
                    v.BoneWeights.Add(1 - v.BoneWeights.Sum());
                }

                int boneweightcount = 0;
                for (int j = 0; j < v.BoneIDs.Count; j++)
                {
                    if (v.BoneWeights[j] != 0)
                    {
                        boneweightcount++;
                        if (!bw_float_dict.TryGetValue(v.BoneWeights[j], out _))
                        {
                            bw_float_dict.Add(v.BoneWeights[j], bw_float_dict.Count);
                        }
                        v_string += (v.BoneIDs[j]+1) + " " + bw_float_dict[v.BoneWeights[j]] + " ";
                    }
                }

                vcounts_string += boneweightcount + " ";
            }

            vcounts_string.Trim();
            v_string.Trim();
            
            for (int i = 0; i < emo.Skeleton.NodeNames.Count; i++)
            {
                bone_names.Add(Encoding.ASCII.GetString(emo.Skeleton.NodeNames[i]));
            }

            foreach(Node n in emo.Skeleton.Nodes) //Column-major order
            {
                sbp_string += n.SkinBindPoseMatrix.M11.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M21.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M31.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M41.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M21.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M22.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M32.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M42.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M13.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M23.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M33.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M43.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M14.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M24.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M34.ToString() + " ";
                sbp_string += n.SkinBindPoseMatrix.M44.ToString() + " ";
            }

            Grendgine_Collada_Library_Controllers library_Controller = new Grendgine_Collada_Library_Controllers()
            {
                Controller = new Grendgine_Collada_Controller[]
                {
                    new Grendgine_Collada_Controller()
                    {
                        ID = $"{emo_name}-skin",

                        Name = "Armature",
                        Skin = new Grendgine_Collada_Skin()
                        {
                            Bind_Shape_Matrix = new Grendgine_Collada_Float_Array_String()
                            {
                                Value_As_String = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"
                            },
                            sID = $"#{emo_name}-mesh",
                            Source = new Grendgine_Collada_Source[]
                            {
                                //Skin Joints
                                new Grendgine_Collada_Source()
                                {
                                    ID = $"{emo_name}-skin-joints",
                                    Name_Array = new Grendgine_Collada_Name_Array()
                                    {
                                        ID = $"{emo_name}-skin-joints-array",
                                        Count = bone_names.Count,
                                        Value_Pre_Parse = string.Join(" ", bone_names),
                                    },
                                    Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                    {
                                        Accessor = new Grendgine_Collada_Accessor()
                                        {
                                            Source = $"#{emo_name}-skin-joints-array",
                                            Count = (uint)emo.Skeleton.Nodes.Count,
                                            Stride = 1,
                                            Param = new Grendgine_Collada_Param[]
                                            {
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "JOINT",
                                                    Type = "Name"
                                                }
                                            }
                                        }
                                    }
                                },
                                //Skin Bind Poses
                                new Grendgine_Collada_Source()
                                {
                                    ID = $"{emo_name}-skin-bind_poses",
                                    Float_Array =  new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emo_name}-skin-bind_poses-array",
                                        Count = emo.Skeleton.Nodes.Count * 16,
                                        Value_As_String = sbp_string.Trim()
                                    },
                                    Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                    {
                                        Accessor = new Grendgine_Collada_Accessor()
                                        {
                                            Source = $"#{emo_name}-skin-bind_poses-array",
                                            Count = (uint)emo.Skeleton.Nodes.Count,
                                            Stride = 16,
                                            Param = new Grendgine_Collada_Param[]
                                            {
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "TRANSFORM",
                                                    Type = "float4x4"
                                                }
                                            }
                                        }
                                    }
                                },
                                //Skin Weights
                                new Grendgine_Collada_Source()
                                {
                                    ID = $"{emo_name}-skin-weights",
                                    Float_Array =  new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emo_name}-skin-weights-array",
                                        Count = bw_float_dict.Count,
                                        Value_As_String = string.Join(" ", bw_float_dict.Keys)
                                    },
                                    Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                    {
                                        Accessor = new Grendgine_Collada_Accessor()
                                        {
                                            Source = $"#{emo_name}-skin-weights-array",
                                            Count = (uint)bw_float_dict.Count,
                                            Stride = 1,
                                            Param = new Grendgine_Collada_Param[]
                                            {
                                                new Grendgine_Collada_Param()
                                                {
                                                    Name = "WEIGHT",
                                                    Type = "float"
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            Joints = new Grendgine_Collada_Joints()
                            {
                                Input = new Grendgine_Collada_Input_Unshared[]
                                {
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.JOINT,
                                        source = $"#{emo_name}-skin-joints"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.INV_BIND_MATRIX,
                                        source = $"#{emo_name}-skin-bind_poses"
                                    }
                                }
                            },
                            Vertex_Weights = new Grendgine_Collada_Vertex_Weights()
                            {
                                Count = v_dict.Count,
                                Input = new Grendgine_Collada_Input_Shared[]
                                {
                                    new Grendgine_Collada_Input_Shared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.JOINT,
                                        source = $"#{emo_name}-skin-joints",
                                        Offset = 0
                                    },
                                    new Grendgine_Collada_Input_Shared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.WEIGHT,
                                        source = $"#{emo_name}-skin-weights",
                                        Offset = 1
                                    }
                                },
                                //VCOUNT and V
                                VCount = new Grendgine_Collada_Int_Array_String()
                                {
                                    Value_As_String = vcounts_string.Trim()
                                },
                                V = new Grendgine_Collada_Int_Array_String()
                                {
                                    Value_As_String = v_string.Trim()        
                                }
                            }
                        }
                    }
                }   
            };

            return library_Controller;
        }

        public static Dictionary<double,int> BoneWeightDictionary(List<Vertex> vs)
        {
            Dictionary<double, int> bw = new Dictionary<double, int>();

            foreach(Vertex v in vs)
            {
                foreach(float w in v.BoneWeights)
                {
                    if (!bw.TryGetValue(w, out _)) bw.Add(w, bw.Count);
                }
            }

            return bw;
        }

        public static EMG GrendgineCollada(out List<Node> Skeleton)
        {
            Grendgine_Collada model = Grendgine_Collada.Grendgine_Load_File("USAMananim no rot.dae");

            Skeleton = new List<Node>();

            List<float> position_floats = new List<float>();
            List<float> normal_floats = new List<float>();
            List<float> texture_floats = new List<float>();
            List<int> tri_indices = new List<int>();

            List<Vertex> position_list = new List<Vertex>();
            List<Normal> normal_list = new List<Normal>();
            List<UVMap> texture_list = new List<UVMap>();

            List<Vertex> VertexList = new List<Vertex>();
            List<int[]> FaceIndices = new List<int[]>();

            Dictionary<string, int> master_bone_dict = new Dictionary<string, int>();
            List<string> bone_names = new List<string>();
            List<Matrix4x4> bindpose_matrices = new List<Matrix4x4>();

            List<int> vert_bonecounts = new List<int>();
            List<int> bone_indices = new List<int>();
            List<double> bone_floats = new List<double>();

            foreach (Grendgine_Collada_Geometry g in model.Library_Geometries.Geometry)
            {
                //Triangles...
                foreach (string str in g.Mesh.Triangles[0].P.Value_As_String.Split(' '))
                {
                    tri_indices.Add(int.Parse(str));
                }

                //Vertex positions/normals/UVs
                foreach (Grendgine_Collada_Source s in g.Mesh.Source)
                {
                    if (s.ID.Contains("mesh-positions"))
                    {
                        foreach (string str in s.Float_Array.Value_As_String.Split(' '))
                        {
                            position_floats.Add(float.Parse(str));
                        }
                    }
                    else if (s.ID.Contains("mesh-normals"))
                    {
                        foreach (string str in s.Float_Array.Value_As_String.Split(' '))
                        {
                            normal_floats.Add(float.Parse(str));
                        }
                    }
                    else if (s.ID.Contains("mesh-map"))
                    {
                        foreach (string str in s.Float_Array.Value_As_String.Split(' '))
                        {
                            texture_floats.Add(float.Parse(str));
                        }
                    }
                }
            }

            foreach (Grendgine_Collada_Controller c in model.Library_Controllers.Controller)
            {
                foreach (Grendgine_Collada_Source s in c.Skin.Source)
                {
                    if (s.ID.Contains("skin-bind_poses"))
                    {
                        List<float> bpf = new List<float>();

                        foreach (string str in s.Float_Array.Value_As_String.Trim().Split(' '))
                        {
                            bpf.Add(float.Parse(str));
                        }

                        for (int i = 0; i < bpf.Count/16; i++)
                        {
                            bindpose_matrices.Add(new Matrix4x4(bpf[16 * i + 0],  bpf[16 * i + 4],  bpf[16 * i + 8],  bpf[16 * i + 12],
                                                                bpf[16 * i + 1],  bpf[16 * i + 5],  bpf[16 * i + 9],  bpf[16 * i + 13],
                                                                bpf[16 * i + 2],  bpf[16 * i + 6],  bpf[16 * i + 10], bpf[16 * i + 14],
                                                                bpf[16 * i + 3],  bpf[16 * i + 7],  bpf[16 * i + 11], bpf[16 * i + 15]));
                        }
                    }
                    if (s.ID.Contains("skin-joints"))
                    {
                        foreach (string str in s.Name_Array.Value_Pre_Parse.Trim().Split(' '))
                        {
                            if(!master_bone_dict.TryGetValue(str, out _))
                            {
                                master_bone_dict.Add(str, master_bone_dict.Count);
                            }
                        }
                    }
                    if (s.ID.Contains("skin-weights"))
                    {
                        foreach (string str in s.Float_Array.Value_As_String.Split(' '))
                        {
                            bone_floats.Add(float.Parse(str));
                        }
                    }
                }

                foreach (string str in c.Skin.Vertex_Weights.V.Value_As_String.Split(' '))
                {
                    vert_bonecounts.Add(int.Parse(str));
                }
                foreach (string str in c.Skin.Vertex_Weights.VCount.Value_As_String.Trim().Split(' '))
                {
                    bone_indices.Add(int.Parse(str));
                }
            }

            //Actual bones (nodes) are recursively stored in the visual scenes library

            Grendgine_Collada_Node current_node;

            foreach (Grendgine_Collada_Visual_Scene v in model.Library_Visual_Scene.Visual_Scene)
            {
                
                foreach (Grendgine_Collada_Node n in v.Node)
                {
                    List<Grendgine_Collada_Node> q = n.node.ToList();
                    //Depth-first search for nodes. Importing the master node-name list seems to be broken
                    //Hoping depth-first is always right, if not have to match bones later using names
                    while (q.Count > 0)
                    {
                        current_node = q[0];

                        Matrix4x4 current_matrix = new Matrix4x4();

                        //Compile transform matrix...
                        foreach (Grendgine_Collada_Matrix m in current_node.Matrix)
                        {
                            string[] strings = m.Value_As_String.Trim().Split(' ');
                            float[] mf = new float[16];
                            for (int i = 0; i < 16; i++)
                            {
                                mf[i] = float.Parse(strings[i]);
                            }
                            current_matrix = new Matrix4x4( mf[0], mf[4], mf[8], mf[12],
                                                            mf[1], mf[5], mf[9], mf[13],
                                                            mf[2], mf[6], mf[10],mf[14],
                                                            mf[3], mf[7], mf[11],mf[15]);
                            //current_matrix = new Matrix4x4(mf[0], mf[1], mf[2], mf[3],
                            //                                mf[4], mf[5], mf[6], mf[7],
                            //                                mf[8], mf[9], mf[10], mf[11],
                            //                                mf[12], mf[13], mf[14], mf[15]);
                        }                        

                        List<string> children = new List<string>();

                        if (current_node.node != null)
                        {
                            foreach (Grendgine_Collada_Node nc in current_node.node)
                            {
                                children.Add(nc.Name);
                            }
                        }

                        //Test for "extra" nodes that shouldn't be there, add to skeleton if it's a real node
                        if(master_bone_dict.TryGetValue(current_node.Name, out _))
                        {
                            Skeleton.Add(new Node()
                            {   
                                Name = current_node.Name,
                                NodeMatrix = current_matrix,
                                SkinBindPoseMatrix = bindpose_matrices[Skeleton.Count],
                                child_strings = children
                            });
                        }
                        
                        q.RemoveAt(0);
                        if (current_node.node == null) continue; //No more children? break and start over

                        for (int j = 0; j < current_node.node.Length; j++)
                        {
                            q.Insert(0, current_node.node[current_node.node.Length - (j + 1)]); //Adding them in reverse order
                        }
                    };
                }
            }

            

            //Compile indexes - these are the equivalent of OBJ V/VN/VT indexes
            int pointer = 0;
            for (int i = 0; i < position_floats.Count / 3; i++)
            {
                List<int> tempIDs = new List<int>();
                List<float> tempFloats = new List<float>();

                for (int j = 0; j < bone_indices[i]; j++)
                {
                    tempIDs.Add(vert_bonecounts[pointer * 2]);
                    tempFloats.Add(Convert.ToSingle(bone_floats[vert_bonecounts[pointer * 2 + 1]]));

                    pointer++;
                }

                position_list.Add(new Vertex()
                {
                    X = position_floats[i * 3],
                    Y = position_floats[i * 3 + 1],
                    Z = position_floats[i * 3 + 2],
                    BoneCount = bone_indices[i],
                    BoneIDs = tempIDs,
                    BoneWeights = tempFloats,
                });
            }

            for (int i = 0; i < normal_floats.Count / 3; i++)
            {
                normal_list.Add(new Normal()
                {
                    nX = normal_floats[i * 3],
                    nY = normal_floats[i * 3 + 1],
                    nZ = normal_floats[i * 3 + 2],
                });
            }
            for (int i = 0; i < texture_floats.Count / 2; i++)
            {
                texture_list.Add(new UVMap()
                {
                    U = texture_floats[i * 2],
                    V = texture_floats[i * 2 + 1]
                });
            }


            //Working from the tri indices, construct an SF4-style vertex table
            //Currently no vertex merging
            for (int i = 0; i < tri_indices.Count / 3; i++)
            {
                VertexList.Add(new Vertex()
                {
                    X = position_list[tri_indices[i * 3]].X,
                    Y = position_list[tri_indices[i * 3]].Y,
                    Z = position_list[tri_indices[i * 3]].Z,
                    BoneCount = position_list[tri_indices[i * 3]].BoneCount,
                    BoneIDs = new List<int>(position_list[tri_indices[i * 3]].BoneIDs),
                    BoneWeights = new List<float>(position_list[tri_indices[i * 3]].BoneWeights),
                    nX = normal_list[tri_indices[i * 3 + 1]].nX,
                    nY = normal_list[tri_indices[i * 3 + 1]].nY,
                    nZ = normal_list[tri_indices[i * 3 + 1]].nZ,
                    U = texture_list[tri_indices[i * 3 + 2]].U,
                    V = texture_list[tri_indices[i * 3 + 2]].V,
                });
            }

            for (int i = 0; i < tri_indices.Count / 3; i++)
            {
                FaceIndices.Add(new int[]{
                        i * 3, i * 3 + 1, i * 3 + 2
                    });
            }

            int[] test = new int[tri_indices.Count / 3];

            for (int i = 0; i < tri_indices.Count / 3; i++)
            {
                test[i] = i;
            }

            List<int> Daisy = DaisyChainFromIndices(FaceIndices);

            //Build a dictionary to translate absolute bone ref to submodel bone ref
            Dictionary<int, int> BoneDictionary = new Dictionary<int, int>();
            for (int i = 0; i < VertexList.Count; i++)
            {
                for (int j = 0; j < VertexList[i].BoneIDs.Count; j++)
                {
                    if (!BoneDictionary.TryGetValue(VertexList[i].BoneIDs[j], out _))
                    {
                        BoneDictionary.Add(VertexList[i].BoneIDs[j], BoneDictionary.Count);
                    }
                }
            }

            BoneDictionary.Add(0, BoneDictionary.Count);
            //Update vertex list with submodel bone refs
            for (int i = 0; i < VertexList.Count; i++)
            {
                for (int j = 0; j < VertexList[i].BoneIDs.Count; j++)
                {
                    VertexList[i].BoneIDs[j] = BoneDictionary[VertexList[i].BoneIDs[j]];
                }
            }

            EMG emg = new EMG()
            {
                RootBone = 0x01,
                ModelCount = 1,
                HEXBytes = new byte[0],
                ModelPointersList = new List<int>() { 0x00 },
                Models = new List<Model>()
                {
                    new Model()
                    {
                        HEXBytes = new byte[0],
                        BitFlag = 0x0247,
                        BitDepth = 0x34,
                        TextureCount = 1,
                        TextureListPointer = 0x00,
                        VertexCount = VertexList.Count,
                        VertexData = VertexList,
                        ReadMode = 0,   //triangles
                        //ReadMode = 1, //stripped
                        SubModelsCount = 1,
                        SubModelsListPointer = 1,
                        SubModelPointersList = new List<int>() { 0x00 },
                        SubModels = new List<SubModel>
                        {
                            new SubModel()
                            {
                                DaisyChain = test,
                                DaisyChainLength = test.Length,
                                //DaisyChain = Daisy.ToArray(),
                                //DaisyChainLength = Daisy.Count,
                                SubModelName = Utils.MakeModelName("Polygon"),
                                BoneIntegersCount = BoneDictionary.Count,
                                MaterialIndex = 0,
                                BoneIntegersList = BoneDictionary.Keys.ToList(),
                                MysteryFloats = new byte[] { 0x9E, 0xDF, 0xDD, 0xBC, 0xC5, 0x2A, 0x3B, 0x3E,
                                    0xA7, 0x68, 0x3F, 0x3C, 0x00, 0x00, 0x80, 0x3F },
                                HEXBytes = new byte[0]
                            }
                        },
                        TexturePointersList = new List<int>() { 0x00 },
                        Textures = new List<EMGTexture>()
                        {
                            new EMGTexture
                            {
                                TextureLayers = 1,
                                TextureIndicesList = new List<int> { 0 },
                                Scales_UList = new List<float> { 1f },
                                Scales_VList = new List<float> { 1f }
                            }
                        },
                        CullData = new byte[]
                        {
                            0x00, 0x0A, 0x1B, 0x3C, 0xC3, 0xA4, 0x9E, 0x40,
                            0x80, 0x89, 0x27, 0x3E, 0xF6, 0x79, 0x3E, 0x41,
                            0x50, 0x94, 0xA1, 0xC0, 0xFD, 0x14, 0x9D, 0xBF,
                            0xEE, 0x94, 0x0A, 0xC1, 0x43, 0xB9, 0x0D, 0x43,
                            0x5A, 0x2F, 0xA2, 0x40, 0x63, 0x47, 0x32, 0x41,
                            0x3A, 0xD1, 0x0F, 0x41, 0xF6, 0x79, 0xBE, 0x41
                        },
                    }
                }
            };

            emg.GenerateBytes();

            return emg;
        }
    }
}