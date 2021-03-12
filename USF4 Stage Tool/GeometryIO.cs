using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collada141;

namespace USF4_Stage_Tool
{
    public static class GeometryIO
    {
        public static List<int> BruteForceChain(List<int[]> nIndices, int attempts)
        {
            List<int> best = new List<int>();
            List<int> newlist = new List<int>();

            Random random = new Random();

            int rnd = random.Next(0, nIndices.Count);

            int bestat = 0;

            for (int i = 0; i < attempts; i++)
            {
                List<int[]> test = new List<int[]>();

                test.AddRange(nIndices.GetRange(rnd, nIndices.Count - rnd));
                test.AddRange(nIndices.GetRange(0, rnd));

                newlist = new List<int>(DaisyChainFromIndices(test));

                if (best.Count == 0) best = new List<int>(newlist);
                else if (best.Count > newlist.Count)
                {
                    best = new List<int>(newlist);
                    bestat = i;
                }
            }

            Console.WriteLine($"Final result {best.Count}/{nIndices.Count*3} = {100 * best.Count / (nIndices.Count * 3)}%");
            Console.WriteLine($"Best result at i = {bestat}.");

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
                for (int i = 0; i < DaisyChain.Length - 2; i++)
                {
                    FaceIndices.Add(new int[] { DaisyChain[i], DaisyChain[i + 1], DaisyChain[i + 2] });
                    i += 2;
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
            COLLADA model = COLLADA.Load("USAMananim.dae");
            Animation animation1 = new Animation();

            foreach (var lib in model.Items)
            {
                //All the mesh data apart from bone weights are in the geometries library
                var animations = lib as library_animations;
                if (animations == null)
                    continue;
                foreach (var anim in animations.animation)
                {
                    if (anim == null)
                        continue;
                    Dictionary<float, int> valuedict = new Dictionary<float, int>();
                    valuedict.Add(0, 0);

                    int d_max = 0;

                    animation1 = new Animation()
                    {
                        CmdTrackCount = 4,
                        CmdTrackPointersList = new List<int>() { 0, 0, 0, 0, 0 },
                        CMDTracks = new List<CMDTrack>(),
                        Name = Encoding.ASCII.GetBytes("ANIMATION_000"),
                        NamePointer = 0,
                        ValuesList = new List<float>(),
                        ValuesListPointer = 0,
                        ValueCount = 0,
                        Duration = 0
                    };

                    for (int i = 0; i < anim.Items.Length; i++)
                    {
                        var item = anim.Items[i];
                        var animation = item as animation;

                        var inputarray = animation.Items[0] as source;
                        var outputarray = animation.Items[1] as source;

                        var input_floatarray = inputarray.Item as float_array;
                        var output_floatarray = outputarray.Item as float_array;

                        int boneID = 0;
                        byte bitflag = 9;
                        byte transformtype = 3;
                        List<int> tempsteps = new List<int>() { 0 };
                        List<int> tempindices = new List<int>() { 0 };


                        if (animation.id.Split('_').Last() == "X") bitflag = 0;
                        if (animation.id.Split('_').Last() == "Y") bitflag = 1;
                        if (animation.id.Split('_').Last() == "Z") bitflag = 2;

                        if (animation.id.Contains("location")) transformtype = 0;
                        if (animation.id.Contains("rotation")) transformtype = 1;
                        if (animation.id.Contains("scale")) transformtype = 2;

                        if (animation.name.Split('_').Last() == "LArm1") boneID = 9;
                        if (animation.name.Split('_').Last() == "LArm2") boneID = 10;
                        if (animation.name.Split('_').Last() == "RArm1") boneID = 13;
                        if (animation.name.Split('_').Last() == "RArm2") boneID = 14;
                        if (animation.name.Split('_').Last() == "LLeg2") boneID = 20;
                        
                        foreach(float f in input_floatarray.Values)
                        {
                            tempsteps.Add((int)Math.Round(f * 24f));
                        }

                        foreach (float f in output_floatarray.Values)
                        {
                            if (!valuedict.TryGetValue(f, out _))
                            {
                                valuedict.Add(f, valuedict.Count);
                            }
                        }
                        
                        foreach (float f in output_floatarray.Values)
                        {
                            tempindices.Add(valuedict[f]);
                        }

                        d_max = Math.Max(d_max, (int)Math.Round(input_floatarray.Values.Max() * 24f));

                        CMDTrack cmd = new CMDTrack()
                        {
                            TransformType = transformtype, //ROTATION
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
                    animation1.Duration = 80;
                    animation1.ValueCount = valuedict.Count;
                    animation1.ValuesList = valuedict.Keys.ToList();
                }
            }

            return animation1;
        }

        public static EMG ReadColladaStruct()
        {
            COLLADA model = COLLADA.Load("USAMananim.dae");

            //model.Save("USAman2.dae");

            List<float> position_floats = new List<float>();
            List<float> normal_floats = new List<float>();
            List<float> texture_floats = new List<float>();
            List<int> tri_indices = new List<int>();

            List<Vertex> position_list = new List<Vertex>();
            List<Normal> normal_list = new List<Normal>();
            List<UVMap> texture_list = new List<UVMap>();

            List<Vertex> VertexList = new List<Vertex>();
            List<int[]> FaceIndices = new List<int[]>();

            string[] s_vert_bonecounts = new string[0];
            string[] s_bone_indices = new string[0];
            List<int> vert_bonecounts = new List<int>();
            List<int> bone_indices = new List<int>();
            List<double> bone_floats = new List<double>();

            foreach (var item in model.Items)
            {
                //All the mesh data apart from bone weights are in the geometries library
                var geometries = item as library_geometries;
                if (geometries == null)
                    continue;
                foreach (var geom in geometries.geometry)
                {
                    var mesh = geom.Item as mesh;
                    if (mesh == null)
                        continue;

                    foreach (var tri in mesh.Items)
                    {
                        var triangles = tri as triangles;
                        string[] string_indices = triangles.p.Split(' ');
                        foreach (string s in string_indices)
                        {
                            tri_indices.Add(int.Parse(s));
                        }
                    }

                    foreach (var source in mesh.source)
                    {
                        var float_array = source.Item as float_array;
                        if (float_array == null)
                            continue;
                        foreach (var mesh_source_value in float_array.Values)
                        {
                            string[] id_split = float_array.id.Split('-');

                            if (id_split[id_split.Length - 2] == "positions")
                            {
                                position_floats.Add((float)mesh_source_value);
                            }
                            if (id_split[id_split.Length - 2] == "normals")
                            {
                                normal_floats.Add((float)mesh_source_value);
                            }
                            if (id_split[id_split.Length - 3] == "map")
                            {
                                texture_floats.Add((float)mesh_source_value);
                            }
                        }
                    }
                }
            }
            //Bone weights are in controllers library...
            foreach (var item in model.Items)
            {
                var controllerlib = item as library_controllers;
                if (controllerlib == null)
                    continue;
                foreach (var controller in controllerlib.controller)
                {
                    var skin = controller.Item as skin;

                    foreach (var source in skin.source)
                    {
                        string[] id_split = source.id.Split('-');
                        if (id_split.Last() == "weights")
                        {
                            var w_item = source.Item as float_array;
                            bone_floats = w_item.Values.ToList();
                        }
                    }

                    var v_weights = skin.vertex_weights;
                    s_vert_bonecounts = v_weights.v.Trim().Split(' ');
                    s_bone_indices = v_weights.vcount.Trim().Split(' ');
                }
            }
            //Actual bones (nodes) are recursively stored in the visual scenes library
            List<Node> Skeleton = new List<Node>();
            node current_node = new node();
            foreach (var item in model.Items)
            {
                var visuallib = item as library_visual_scenes;
                if (visuallib == null)
                    continue;
                foreach(var scene in visuallib.visual_scene)
                {
                    foreach(var node in scene.node)
                    {
                        if(node.node1 != null)
                        {
                            List<node> q = node.node1.ToList();

                            //Depth-first search for nodes. Importing the master node-name list seems to be broken
                            //Hoping depth-first is always right, if not have to match bones later using names
                            while (q.Count > 0) 
                            {                   
                                current_node = q[0];

                                var c_matrix = current_node.Items[0] as matrix;
                                double[] c = c_matrix.Values;

                                List<string> children = new List<string>();

                                if (current_node.node1 != null)
                                {
                                    for (int j = 0; j < current_node.node1.Length; j++)
                                    {
                                        children.Add(current_node.node1[j].id);
                                    }
                                }

                                Skeleton.Add(new Node()
                                {
                                    Name = current_node.name,
                                    NodeMatrix = new System.Numerics.Matrix4x4(
                                        Convert.ToSingle(c[0]), Convert.ToSingle(c[1]), Convert.ToSingle(c[2]), Convert.ToSingle(c[3]),
                                        Convert.ToSingle(c[4]), Convert.ToSingle(c[5]), Convert.ToSingle(c[6]), Convert.ToSingle(c[7]),
                                        Convert.ToSingle(c[8]), Convert.ToSingle(c[9]), Convert.ToSingle(c[10]), Convert.ToSingle(c[11]),
                                        Convert.ToSingle(c[12]), Convert.ToSingle(c[13]), Convert.ToSingle(c[14]), Convert.ToSingle(c[15])),
                                    child_strings = children
                                });

                                q.RemoveAt(0);
                                if (current_node.node1 == null) continue; //No more children? break and start over
                                for(int j = 0; j < current_node.node1.Length; j++)
                                {
                                    q.Insert(0, current_node.node1[current_node.node1.Length - (j+1)]); //Adding them in reverse order
                                }
                            };
                        }
                    }
                }
            }

            //Bone weight pre-processing
            for (int i = 0; i < s_vert_bonecounts.Length; i++)
            {
                vert_bonecounts.Add(int.Parse(s_vert_bonecounts[i]));
            }
            for (int i = 0; i < s_bone_indices.Length; i++)
            {
                bone_indices.Add(int.Parse(s_bone_indices[i])); //Count is == vertex count
            }

            //Compile indexes - these are the equivalent of OBJ V/VN/VT indexes
            int pointer = 0;
            for (int i = 0; i < position_floats.Count/3; i++)
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
            for(int i = 0; i < tri_indices.Count / 3; i++)
            {
                VertexList.Add(new Vertex()
                {
                    X = position_list[tri_indices[i * 3]].X,
                    Y = position_list[tri_indices[i * 3]].Y,
                    Z = position_list[tri_indices[i * 3]].Z,
                    BoneCount = position_list[tri_indices[i * 3]].BoneCount,
                    BoneIDs = new List<int> (position_list[tri_indices[i * 3]].BoneIDs),
                    BoneWeights = new List<float> (position_list[tri_indices[i * 3]].BoneWeights),
                    nX = normal_list[tri_indices[i * 3 + 1]].nX,
                    nY = normal_list[tri_indices[i * 3 + 1]].nY,
                    nZ = normal_list[tri_indices[i * 3 + 1]].nZ,
                    U = texture_list[tri_indices[i * 3 + 2]].U,
                    V = texture_list[tri_indices[i * 3 + 2]].V,
                }) ;
            }

            for(int i = 0; i < tri_indices.Count / 3; i++)
            {
                FaceIndices.Add(new int[]{
                    i * 3, i * 3 + 1, i * 3 + 2
                });
            }

            int[] test = new int[tri_indices.Count/3];

            for(int i = 0; i < tri_indices.Count/3; i++)
            {
                test[i] = i;
            }

            List<int> Daisy = DaisyChainFromIndices(FaceIndices);

            //Build a dictionary to translate absolute bone ref to submodel bone ref
            Dictionary<int, int> BoneDictionary = new Dictionary<int, int>();
            for(int i = 0; i < VertexList.Count; i++)
            {
                for(int j = 0; j < VertexList[i].BoneIDs.Count; j++)
                {
                    if(!BoneDictionary.TryGetValue(VertexList[i].BoneIDs[j], out _))
                    {
                        BoneDictionary.Add(VertexList[i].BoneIDs[j], BoneDictionary.Count);
                    }
                }
            }

            BoneDictionary.Add(0, BoneDictionary.Count);
            //Update vertex list with submodel bone refs
            for(int i = 0; i < VertexList.Count; i++)
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

        public static COLLADA newCollada(List<Vertex> vlist)
        {
            library_controllers clib = new library_controllers()
            {
                controller = new controller[]
                {
                    new controller()
                    {
                        Item = new object[]
                        {
                            new skin()
                            {
                                bind_shape_matrix = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1",
                                joints = new skinJoints
                                {
                                    input = new InputLocal[]
                                    {
                                        new InputLocal()
                                        {
                                            source = "",
                                            semantic = "JOINT"
                                        },
                                        new InputLocal()
                                        {
                                            source = "",
                                            semantic = "INV_BIND_MATRIX"
                                        }
                                    }
                                },
                                source = new source[]
                                {
                                    new source
                                    {
                                        id = "",
                                        Item = new object[]
                                        {
                                            new Name_array()
                                            {
                                                count = 0,
                                                id = "",
                                                name = "",
                                                _Text_ = "",
                                                Values = new string[]
                                                {
                                                    //INSERT bone names?
                                                }
                                            }
                                        }
                                    },
                                    new source
                                    {
                                        id = "",
                                        Item = new object[]
                                        {
                                            new float_array()
                                            {
                                                count = 0,
                                                digits = 6,
                                                id = "",
                                                name = "",
                                                Values = new double[]   //INSERT skin bind poses array
                                                {
                                                    
                                                }
                                            }
                                        }

                                    },
                                    new source
                                    {
                                        id = "", //INSERT skin ID
                                        
                                        Item = new object[]
                                        {
                                            new float_array()
                                            {
                                                count = 0,
                                                digits = 6,
                                                id = "",
                                                name = "",
                                                Values = BoneWeightDictionary(vlist).Keys.ToArray() //INSERT bone floats
                                            }
                                        },
                                        
                                    }
                                },

                                vertex_weights = new skinVertex_weights()
                                {
                                    v = "",     //INSERT bone weights and counts string
                                    vcount = "",
                                    count = 0, //INSERT bone count
                                }
                            }
                        }
                    }
                }
            };

            library_visual_scenes vlib = new library_visual_scenes()
            {
                visual_scene = new visual_scene[]
                {
                    new visual_scene()
                    {
                        node = new node[]
                        {
                            new node()
                            {
                                node1 = new node[]
                                {
                                    new node(){} //INSERT pre-made node list
                                }
                            }
                        }
                    }
                }
            };

            library_geometries glib = new library_geometries()
            {
                geometry = new geometry[]
                {
                    new geometry()
                    {
                        Item = new object[]
                        {
                            new mesh()
                            {
                                Items = new object[]
                                {
                                    new triangles()
                                    {
                                        p = "" //INSERT p-string
                                    }
                                },
                                source = new source[]
                                {
                                    new source()
                                    {
                                        id = "", //INSERT vert id
                                        Item = new object[]
                                        {
                                            new float_array()
                                            {
                                                Values = new double[]
                                                {
                                                    1d  //INSERT vert doubles
                                                }
                                            }
                                        }
                                    },
                                    new source()
                                    {
                                        id = "", //INSERT normal id
                                        Item = new object[]
                                        {
                                            new float_array()
                                            {
                                                Values = new double[]
                                                {
                                                    1d  //INSERT normal doubles
                                                }
                                            }
                                        }
                                    },
                                    new source()
                                    {
                                        id = "", //INSERT texture id
                                        Item = new object[]
                                        {
                                            new float_array()
                                            {
                                                Values = new double[]
                                                {
                                                    1d  //INSERT texture doubles
                                                }
                                            }
                                        }
                                    },
                                }
                            } 
                        }
                    }
                },
            };

            COLLADA col = new COLLADA()
            {   
                asset = new asset()
                {
                    up_axis = UpAxisType.Z_UP,
                    unit = new assetUnit()
                    {
                        meter = 1,
                        name = "meter"
                    }
                },
                Items = new object[] 
                {
                    glib,
                    clib,
                    vlib
                },
            };

            return col;
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
    }
}