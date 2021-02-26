using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collada141;

namespace USF4_Stage_Tool
{
    public static class GeometryIO
    {
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

            while (nIndices.Count > 0)
            {
                for (int i = 0; i < nIndices.Count; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        int[] workingArray = Utils.Rotate3Array(nIndices[i], j);
                        if (bForwards == true && workingArray[1] == buffer1 && workingArray[0] == buffer2)
                        {
                            compression -= 2;
                            buffer2 = buffer1;
                            buffer1 = workingArray[2];
                            Chain.Add(buffer1);
                            nIndices.RemoveAt(i);
                            i = -1;
                            bForwards = !bForwards;
                            //progressBar1.Value += 1;
                            //TimeEstimate(TStrings.STR_ReorderingFaces, count, Chain.Count);
                            break;
                        }
                        if (bForwards == false && workingArray[1] == buffer1 && workingArray[2] == buffer2)
                        {
                            compression -= 2;
                            buffer2 = buffer1;
                            buffer1 = workingArray[0];
                            Chain.Add(buffer1);
                            nIndices.RemoveAt(i);
                            i = -1;
                            bForwards = !bForwards;
                            //progressBar1.Value += 1;
                            //TimeEstimate(TStrings.STR_ReorderingFaces, count, Chain.Count);
                            break;
                        }

                    }
                }
                //No match found - if we've run out of faces, great, if not, re-initialise
                if (nIndices.Count > 0)
                {
                    compression += 2;
                    Chain.Add(buffer1);
                    if (bForwards)
                    {
                        //Create chain break
                        Chain.Add(nIndices[0][0]);
                        Chain.Add(nIndices[0][0]);
                        Chain.Add(nIndices[0][1]);
                        Chain.Add(nIndices[0][2]);
                        //Re-initialise buffer
                        buffer1 = nIndices[0][2];
                        buffer2 = nIndices[0][1];
                    }
                    if (!bForwards)
                    {
                        //Create chain break
                        Chain.Add(nIndices[0][2]);
                        Chain.Add(nIndices[0][2]);
                        Chain.Add(nIndices[0][1]);
                        Chain.Add(nIndices[0][0]);
                        //Re-initialise buffer
                        buffer1 = nIndices[0][0];
                        buffer2 = nIndices[0][1];
                    }
                    //Clear the used face and flip the flag
                    nIndices.RemoveAt(0);
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
                    if (bForwards)
                    {
                        int[] temp = new int[] { DaisyChain[i], DaisyChain[i + 1], DaisyChain[i + 2] };

                        if (temp[0] != temp[1] && temp[1] != temp[2] && temp[2] != temp[0])
                        {
                            FaceIndices.Add(temp);
                        }
                    }
                    else
                    {
                        int[] temp = new int[] { DaisyChain[i + 2], DaisyChain[i + 1], DaisyChain[i] };

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

            if (emg.ModelCount > 1) invert_indices = true; //Force inverted indices if there's multiple vert lists

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

            lines.Add($"o {name}_{modelindex}");
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

            lines.Add($"g {Encoding.ASCII.GetString(sm.SubModelName).Split('\0')[0]}");
            lines.Add($"usemtl {Encoding.ASCII.GetString(sm.SubModelName).Split('\0')[0]}");
            //Add in material tags - if you want to texture the model in blender makes the process slightly easier

            for (int i = 0; i < indices.Count; i++)
            {
                if (invert_indices) lines.Add($"f {indices[i][0]}/{indices[i][0]}/{indices[i][0]} {indices[i][1]}/{indices[i][1]}/{indices[i][1]} {indices[i][2]}/{indices[i][2]}/{indices[i][2]}");
                else lines.Add($"f {indices[i][0] + 1}/{indices[i][0] + 1}/{indices[i][0] + 1} {indices[i][1] + 1}/{indices[i][1] + 1}/{indices[i][1] + 1} {indices[i][2] + 1}/{indices[i][2] + 1}/{indices[i][2] + 1}");
            }

            return lines;
        }

        public static EMZ UpdateLegacyStage(EMZ emz)
        {
            for (int i = 0; i < emz.Files.Count; i++)
            {
                if (emz.Files[i] is EMO)
                {
                    EMO emo = (EMO)emz.Files[i];
                    for (int j = 0; j < emo.EMGList.Count; j++)
                    {
                        EMG emg = emo.EMGList[j];

                        for (int k = 0; k < emg.Models.Count; k++)
                        {
                            Model mod = emg.Models[k];
                            if (mod.ReadMode == 0)
                            {
                                for (int l = 0; l < mod.SubModels.Count; l++)
                                {
                                    SubModel sub = mod.SubModels[l];

                                    sub.DaisyChain = DaisyChainFromIndices(FaceIndicesFromDaisyChain(sub.DaisyChain, true)).ToArray();
                                    sub.DaisyChainLength = sub.DaisyChain.Length;
                                    mod.SubModels.RemoveAt(l);
                                    mod.SubModels.Insert(l, sub);
                                }

                                mod.ReadMode = 1;
                                emg.GenerateBytes();
                                emg.Models.RemoveAt(k);
                                emg.Models.Insert(k, mod);
                            }
                        }

                        emo.GenerateBytes();
                        emo.EMGList.RemoveAt(j);
                        emo.EMGList.Insert(j, emg);
                    }

                    emz.Files.Remove(i);
                    emz.Files.Add(i, emo);
                }
            }

            return emz;
        }

        public static EMG ReadColladaStruct()
        {
            COLLADA model = COLLADA.Load("USA_MAN03_B.dae");

            //coll.Save("USAman2.dae");

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
                            
                            while(q.Count > 0) //While there's still nodes, keep going...
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
                    BoneIDs = position_list[tri_indices[i * 3]].BoneIDs,
                    BoneWeights = position_list[tri_indices[i * 3]].BoneWeights,
                    nX = normal_list[tri_indices[i * 3 + 1]].nX,
                    nY = normal_list[tri_indices[i * 3 + 1]].nY,
                    nZ = normal_list[tri_indices[i * 3 + 1]].nZ,
                    U = texture_list[tri_indices[i * 3 + 2]].U,
                    V = texture_list[tri_indices[i * 3 + 2]].V,
                }) ;
            }

            //for(int i = 0; i < VertexList.Count; i++)
            //{
            //    for(int j = 0; j < VertexList[i].BoneCount; j++)
            //    {
            //        VertexList[i].BoneIDs.Add((i + j) * 2);
            //        VertexList[i].BoneWeights.Add(Convert.ToSingle(bone_floats[(i + j) * 2 + 1]));
            //    }
            //}

            for(int i = 0; i < tri_indices.Count / 3; i++)
            {
                FaceIndices.Add(new int[]{
                    i * 3, i * 3 + 1, i * 3 + 2
                });
            }

            int[] test = new int[tri_indices.Count];

            for(int i = 0; i < tri_indices.Count; i++)
            {
                test[i] = i;
            }

            List<int> Daisy = DaisyChainFromIndices(FaceIndices);

            EMG emg = new EMG()
            {
                RootBone = 0x01,
                ModelCount = 1,
                HEXBytes = new byte[0],
                ModelPointerList = new List<int>() { 0x00 },
                Models = new List<Model>()
                {
                    new Model()
                    {
                        HEXBytes = new byte[0],
                        BitFlag = 0x0247,
                        BitDepth = 0x32,
                        TextureCount = 1,
                        TextureListPointer = 0x00,
                        VertexCount = VertexList.Count,
                        VertexData = VertexList,
                        ReadMode = 0,
                        //ReadMode = 1,
                        SubModelsCount = 1,
                        SubModeListPointer = 1,
                        SubModelList = new List<int>() { 0x00 },
                        SubModels = new List<SubModel>
                        {
                            new SubModel()
                            {
                                //DaisyChain = test,
                                //DaisyChainLength = test.Length,
                                DaisyChain = Daisy.ToArray(),
                                DaisyChainLength = Daisy.Count,
                                SubModelName = Utils.MakeModelName("Polygon"),
                                BoneIntegersCount = 0,
                                MaterialIndex = 0,
                                BoneIntegersList = new List<int>(),
                                MysteryFloats = new byte[] { 0x9E, 0xDF, 0xDD, 0xBC, 0xC5, 0x2A, 0x3B, 0x3E,
                                    0xA7, 0x68, 0x3F, 0x3C, 0x00, 0x00, 0x80, 0x3F },
                                HEXBytes = new byte[0]
                            }
                        },
                        TexturePointer = new List<int>() { 0x00 },
                        TexturesList = new List<EMGTexture>()
                        {
                            new EMGTexture
                            {
                                TextureLayers = 1,
                                TextureIndex = new List<int> { 1 },
                                Scales_U = new List<float> { 1f },
                                Scales_V = new List<float> { 1f }
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