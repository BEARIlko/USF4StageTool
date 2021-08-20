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

                    int rnd = random.Next(0, nIndices.Count);

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

            Console.WriteLine($"Compression {compression}/{compression_zero} = {100 * compression / compression_zero}%");

            return Chain;
        }

        public static List<int[]> FaceIndicesFromDaisyChain(int[] DaisyChain, bool readmode = false)
        {
            List<int[]> FaceIndices = new List<int[]>();

            if (readmode == true && DaisyChain.Length % 3 == 0)
            {
                for (int i = 0; i < DaisyChain.Length / 3; i++)
                {
                    FaceIndices.Add(new int[] { DaisyChain[3 * i + 2], DaisyChain[3 * i + 1], DaisyChain[3 * i] });
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

        public static Animation AnimationFromColladaStruct(Grendgine_Collada model)
        {
            Animation newAnim = new Animation();

            float framerate = 24f;

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

            Dictionary<float, int> valuedict = new Dictionary<float, int>();
            valuedict.Add(0, 0);

            int d_max = 0;

            newAnim = new Animation()
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

            for (int i = 0; i < model.Library_Animations.Animation.Length; i++)
            {
                Grendgine_Collada_Animation an = model.Library_Animations.Animation[i];

                string inputID = string.Empty;
                string outputID = string.Empty;
                string interpID = string.Empty;
                string intangentID = string.Empty;
                string outtangentID = string.Empty;

                string target_name = string.Empty;
                string target_var = string.Empty;

                List<int> tempsteps = new List<int>();
                List<int> tempindices = new List<int>();

                byte bitflag = 0;
                byte transformtype = 0;

                int boneID = 0;

                /*  Only correct way to determine transform type/bone target is to go and compare to the actual
                 *  bone declarations to see what value we're targetting.
                 *  Otherwise we're at the mercy of naming conventions.
                 */

                for (int j = 0; j < an.Channel.Length; j++)
                {
                    target_name = an.Channel[j].Target.Split('/')[0];
                    target_var = an.Channel[j].Target.Split('/')[1];

                    master_bone_dict.TryGetValue(target_name, out boneID);
                }

                //Use "target_name" to search the library_visual_scenes for the correct bone
                //Will still blow up if the .dae uses matrix transforms, but meh
                for (int j = 0; j < model.Library_Visual_Scene.Visual_Scene.Length; j++)
                {
                    Grendgine_Collada_Visual_Scene v = model.Library_Visual_Scene.Visual_Scene[j];

                    if (v.Node != null)
                    {
                        List<Grendgine_Collada_Node> n_list = new List<Grendgine_Collada_Node>(v.Node);

                        Grendgine_Collada_Node n = n_list[0];
                        while (n_list.Count > 0) //Loop until we find or run out of nodes
                        {
                            if (n.ID == target_name) break; //Found a match, cancel the search
                            if (n.node != null) n_list.AddRange(n.node);

                            n_list.RemoveAt(0);
                            if (n_list.Count == 0) break;
                            n = n_list[0];
                        }

                        if (n.Translate != null) foreach (Grendgine_Collada_Translate t in n.Translate) if (t.sID == target_var.Split('.')[0]) transformtype = 0;
                        if (n.Scale != null) foreach (Grendgine_Collada_Scale s in n.Scale) if (s.sID == target_var.Split('.')[0]) transformtype = 2;
                        if (n.Rotate != null)
                        {
                            foreach (Grendgine_Collada_Rotate r in n.Rotate)
                            {
                                if (r.sID == target_var)
                                {
                                    transformtype = 1;

                                    //Check the unit vectors to see what axis we're dealing with and set the bitflag
                                    if (r.Value_As_String.StartsWith("1 0 0")) bitflag += 0;
                                    if (r.Value_As_String.StartsWith("0 1 0")) bitflag += 1;
                                    if (r.Value_As_String.StartsWith("0 0 1")) bitflag += 2;
                                }
                            }
                        }
                    }
                }


                for (int j = 0; j < an.Sampler.Length; j++)
                {
                    for (int k = 0; k < an.Sampler[j].Input.Length; k++)
                    {
                        if (an.Sampler[j].Input[k].Semantic == Grendgine_Collada_Input_Semantic.INPUT)
                        {
                            inputID = an.Sampler[j].Input[k].source.Split('#')[1];
                        }
                        else if (an.Sampler[j].Input[k].Semantic == Grendgine_Collada_Input_Semantic.OUTPUT)
                        {
                            outputID = an.Sampler[j].Input[k].source.Split('#')[1];
                        }
                        else if (an.Sampler[j].Input[k].Semantic == Grendgine_Collada_Input_Semantic.INTERPOLATION)
                        {
                            interpID = an.Sampler[j].Input[k].source.Split('#')[1];
                        }
                        else if (an.Sampler[j].Input[k].Semantic == Grendgine_Collada_Input_Semantic.IN_TANGENT)
                        {
                            intangentID = an.Sampler[j].Input[k].source.Split('#')[1];
                        }
                        else if (an.Sampler[j].Input[k].Semantic == Grendgine_Collada_Input_Semantic.OUT_TANGENT)
                        {
                            outtangentID = an.Sampler[j].Input[k].source.Split('#')[1];
                        }
                    }
                }

                for (int j = 0; j < an.Source.Length; j++)
                {
                    if (an.Source[j].ID == inputID)
                    {
                        foreach (string str in an.Source[j].Float_Array.Value_As_String.Trim().Split(' '))
                        {
                            tempsteps.Add((int)Math.Round(float.Parse(str) * framerate));
                        }
                    }
                    if (an.Source[j].ID == outputID)
                    {
                        foreach (string str in an.Source[j].Float_Array.Value_As_String.Trim().Split(' '))
                        {
                            if (!valuedict.TryGetValue(float.Parse(str), out _))
                            {
                                valuedict.Add(float.Parse(str), valuedict.Count);
                            }

                            tempindices.Add(valuedict[float.Parse(str)]);
                        }

                        if (an.Source[j].Technique_Common.Accessor.Param[0].Name == "X") bitflag += 0x00;
                        else if (an.Source[j].Technique_Common.Accessor.Param[0].Name == "Y") bitflag += 0x01;
                        else if (an.Source[j].Technique_Common.Accessor.Param[0].Name == "Z") bitflag += 0x02;
                    }
                    if (an.Source[j].ID == interpID)
                    {

                    }
                    if (an.Source[j].ID == intangentID)
                    {

                    }
                    if (an.Source[j].ID == outtangentID)
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

                newAnim.CMDTracks.Add(cmd);
            }

            //foreach (Grendgine_Collada_Animation an in model.Library_Animations.Animation)
            //{
            //    int boneID = -1;
            //    byte bitflag = 0;
            //    byte transformtype = 3;
            //    List<int> tempsteps = new List<int>() { 0 };
            //    List<int> tempindices = new List<int>() { 0 };

            //    foreach (string str in an.ID.Trim().Split('_'))
            //    {
            //        if (master_bone_dict.TryGetValue(str, out boneID) && boneID > 0) break;
            //        boneID = -1;
            //    }

            //    if (an.ID.Contains("location"))
            //    {
            //        transformtype = 0;

            //        if (an.ID.Contains("location_X")) bitflag = 0;
            //        else if (an.ID.Contains("location_Y")) bitflag = 1;
            //        else bitflag = 2;
            //    }
            //    else if (an.ID.Contains("rotation_euler"))
            //    {
            //        transformtype = 1;

            //        if (an.ID.Contains("rotation_euler_X")) bitflag = 0;
            //        else if (an.ID.Contains("rotation_euler_Y")) bitflag = 1;
            //        else bitflag = 2;
            //    }
            //    else
            //    {
            //        transformtype = 2;

            //        if (an.ID.Contains("scale_X")) bitflag = 0;
            //        else if (an.ID.Contains("scale_Y")) bitflag = 1;
            //        else bitflag = 2;
            //    }

            //    foreach (Grendgine_Collada_Source s in an.Source)
            //    {
            //        if (s.ID.Contains("input"))
            //        {
            //            foreach (string str in s.Float_Array.Value_As_String.Trim().Split(' '))
            //            {
            //                tempsteps.Add((int)Math.Round(float.Parse(str) * 24f));
            //            }
            //        }
            //        if (s.ID.Contains("output"))
            //        {
            //            foreach (string str in s.Float_Array.Value_As_String.Trim().Split(' '))
            //            {
            //                if (!valuedict.TryGetValue(float.Parse(str), out _))
            //                {
            //                    valuedict.Add(float.Parse(str), valuedict.Count);
            //                }

            //                tempindices.Add(valuedict[float.Parse(str)]);
            //            }
            //        }
            //        if (s.ID.Contains("interpolation"))
            //        {

            //        }
            //    }

            //    d_max = Math.Max(d_max, tempsteps.Max());

            //    CMDTrack cmd = new CMDTrack()
            //    {
            //        TransformType = transformtype,
            //        BitFlag = bitflag,
            //        BoneID = boneID,
            //        StepCount = tempsteps.Count,
            //        StepsList = tempsteps,
            //        IndicesListPointer = 0,
            //        IndicesList = tempindices
            //    };

            //    newAnim.CMDTracks.Add(cmd);
            //}

            newAnim.CmdTrackCount = newAnim.CMDTracks.Count;
            newAnim.CmdTrackPointersList = new int[newAnim.CMDTracks.Count].ToList();
            newAnim.Duration = d_max;
            newAnim.ValueCount = valuedict.Count;
            newAnim.ValuesList = valuedict.Keys.ToList();

            return newAnim;
        }

        public static Grendgine_Collada_Library_Geometries EMOtoCollada_Library_Geometries2(EMO emo)
        {
            Grendgine_Collada_Library_Geometries library_geometries = new Grendgine_Collada_Library_Geometries();

            string emo_name = emo.Name;

            List<Grendgine_Collada_Geometry> temp_g = new List<Grendgine_Collada_Geometry>();

            //DO WE NEED TO UPDATE LIBRARY CONTROLLERS && VISUAL SCENES?

            foreach (EMG e in emo.EMGs)
            {
                for (int i = 0; i < e.Models.Count; i++)
                {
                    Model m = e.Models[i];

                    string pos_string = string.Empty;
                    string norm_string = string.Empty;
                    string map_string = string.Empty;
                    string p_string = string.Empty;

                    string emg_name = $"{emo_name}-EMG{i}";

                    bool readmode = (m.ReadMode == 1) ? true : false;

                    List<Vertex> vertices = new List<Vertex>(m.VertexData);

                    foreach (SubModel sm in m.SubModels)
                    {
                        List<int[]> temp_indices = GeometryIO.FaceIndicesFromDaisyChain(sm.DaisyChain, readmode);

                        foreach (int[] f in temp_indices)
                        {
                            p_string += $"{f[2]} {f[2]} {f[2]} {f[1]} {f[1]} {f[1]} {f[0]} {f[0]} {f[0]} ";
                        }
                    }

                    foreach (Vertex v in vertices)
                    {
                        pos_string += $"{v.X} {v.Y} {v.Z} ";
                        norm_string += $"{v.nX} {v.nY} {v.nZ} ";
                        map_string += $"{v.U} {v.V} ";
                    }

                    temp_g.Add(new Grendgine_Collada_Geometry()
                    {
                        ID = $"{emg_name}-mesh",
                        Name = emg_name,
                        Mesh = new Grendgine_Collada_Mesh()
                        {
                            Source = new Grendgine_Collada_Source[]
                            {
                                //Position floats
                                new Grendgine_Collada_Source()
                                {
                                    ID = $"{emg_name}-mesh-positions",
                                    Float_Array = new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emg_name}-mesh-positions-array",
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
                                    ID = $"{emg_name}-mesh-normals",
                                    Float_Array = new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emg_name}-mesh-normals-array",
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
                                    ID = $"{emg_name}-mesh-map-0",
                                    Float_Array = new Grendgine_Collada_Float_Array()
                                    {
                                        ID = $"{emg_name}-mesh-map-0-array",
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
                                ID = $"#{emg_name}-mesh-vertices",
                                Input = new Grendgine_Collada_Input_Unshared[]
                                {
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.POSITION,
                                        source = $"#{emg_name}-mesh-positions"
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
                                           source = $"#{emg_name}-mesh-vertices",
                                           Offset = 0
                                       },
                                       new Grendgine_Collada_Input_Shared()
                                       {
                                           Semantic = Grendgine_Collada_Input_Semantic.NORMAL,
                                           source = $"#{emg_name}-mesh-normals",
                                           Offset = 1
                                       },
                                       new Grendgine_Collada_Input_Shared()
                                       {
                                           Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD,
                                           source = $"#{emg_name}-mesh-map-0",
                                           Offset = 2
                                       }
                                   }
                                }
                            }
                        }
                    });
                }
            }

            Grendgine_Collada_Library_Geometries geometries = new Grendgine_Collada_Library_Geometries()
            {
                Geometry = temp_g.ToArray(),
            };

            return library_geometries;
        }

        public static Grendgine_Collada_Library_Geometries EMOtoCollada_Library_Geometries(EMO emo)
        {
            string emo_name = emo.Name;
            List<Vertex> vertices = new List<Vertex>();
            string pos_string = string.Empty;
            string norm_string = string.Empty;
            string map_string = string.Empty;
            string p_string = string.Empty;

            int v_t = 0;

            foreach (EMG e in emo.EMGs)
            {
                foreach (Model m in e.Models)
                {
                    bool readmode = (m.ReadMode == 1) ? false : true;

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
                    string name = sk.NodeNames[i];

                    Utils.DecomposeMatrixXYZ(m, out float tx, out float ty, out float tz,
                                                out float rx, out float ry, out float rz,
                                                out float sx, out float sy, out float sz);

                    rx = 0;
                    ry = 0;
                    rz = 0;

                    node_list.Add(new Grendgine_Collada_Node()
                    {
                        ID = name,
                        Name = name,
                        sID = name,
                        Type = Grendgine_Collada_Node_Type.JOINT,
                        node = RecursiveNode(sk, i),
                        Translate = new Grendgine_Collada_Translate[]
                        {
                            new Grendgine_Collada_Translate()
                            {
                                sID = "location",
                                Value_As_String = $"{tx} {ty} {tz}"
                            }
                        },
                        Rotate = new Grendgine_Collada_Rotate[]
                        {
                            new Grendgine_Collada_Rotate()
                            {
                                sID = "rotationX",
                                Value_As_String = $"1 0 0 {rx}"
                            },
                            new Grendgine_Collada_Rotate()
                            {
                                sID = "rotationY",
                                Value_As_String = $"0 1 0 {ry}"
                            },
                            new Grendgine_Collada_Rotate()
                            {
                                sID = "rotationZ",
                                Value_As_String = $"0 0 1 {rz}"
                            }
                        },
                        Scale = new Grendgine_Collada_Scale[]
                        {
                            new Grendgine_Collada_Scale()
                            {
                                sID = "scale",
                                Value_As_String = $"{sx} {sy} {sz}"
                            }
                        },
                        //Matrix = new Grendgine_Collada_Matrix[]
                        //{
                        //    new Grendgine_Collada_Matrix() //Might need to switch columns/rows
                        //    {
                        //        sID = "transform",
                        //        Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} "
                        //    }
                        //}
                    });
                }
            }

            Grendgine_Collada_Node[] RecursiveNode(Skeleton s, int n)
            {
                List<Grendgine_Collada_Node> child_list = new List<Grendgine_Collada_Node>();
                for (int i = 0; i < s.Nodes.Count; i++)
                {
                    if (s.Nodes[i].Parent == n)
                    {
                        Node node = sk.Nodes[i];
                        Matrix4x4 m = node.NodeMatrix;
                        string name = sk.NodeNames[i];

                        Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz,
                                                    out float rx, out float ry, out float rz,
                                                    out float sx, out float sy, out float sz);

                        child_list.Add(new Grendgine_Collada_Node()
                        {
                            ID = name,
                            Name = name,
                            sID = name,
                            Type = Grendgine_Collada_Node_Type.JOINT,
                            node = RecursiveNode(s, i),
                            Translate = new Grendgine_Collada_Translate[]
                            {
                                new Grendgine_Collada_Translate()
                                {
                                    sID = "location",
                                    Value_As_String = $"{tx} {ty} {tz}"
                                }
                            },
                            Rotate = new Grendgine_Collada_Rotate[]
                            {
                                new Grendgine_Collada_Rotate()
                                {
                                    sID = "rotationX",
                                    Value_As_String = $"1 0 0 {rx}"
                                },
                                new Grendgine_Collada_Rotate()
                                {
                                    sID = "rotationY",
                                    Value_As_String = $"0 1 0 {ry}"
                                },
                                new Grendgine_Collada_Rotate()
                                {
                                    sID = "rotationZ",
                                    Value_As_String = $"0 0 1 {rz}"
                                }
                            },
                            Scale = new Grendgine_Collada_Scale[]
                            {
                                new Grendgine_Collada_Scale()
                                {
                                    sID = "scale",
                                    Value_As_String = $"{sx} {sy} {sz}"
                                }
                            },
                            //Matrix = new Grendgine_Collada_Matrix[]
                            //{
                            //    new Grendgine_Collada_Matrix() //Might need to switch columns/rows
                            //    {
                            //        sID = "transform",
                            //        Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} "
                            //    }
                            //}
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
                            node_list[0],
                            new Grendgine_Collada_Node()
                            {
                                ID = emo.Name,
                                Name = emo.Name,
                                Type = Grendgine_Collada_Node_Type.NODE,
                                Matrix = new Grendgine_Collada_Matrix[]
                                {
                                    new Grendgine_Collada_Matrix()
                                    {
                                        sID = "transform",
                                        Value_As_String = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"
                                    }
                                },
                                Instance_Controller = new Grendgine_Collada_Instance_Controller[]
                                {
                                    new Grendgine_Collada_Instance_Controller()
                                    {
                                        URL = $"#{emo.Name}-skin",
                                        Skeleton = new Grendgine_Collada_Skeleton[]
                                        {
                                            new Grendgine_Collada_Skeleton()
                                            {
                                                Value = $"#{emo.Skeleton.NodeNames[0]}"
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

            string vcounts_string = string.Empty;
            string v_string = string.Empty;
            string emo_name = emo.Name;
            List<string> bone_names = new List<string>();

            int verts_total = 0;

            //Building an index to translate Vertex bone IDs into Skeleton bone IDs.
            //Verts can appear in multiple submodels, but when they do their bone ID entries have to "match" in
            //each submodel's BoneIntegersList ie return the same bone
            //...I hope
            foreach (EMG e in emo.EMGs)
            {
                foreach (Model m in e.Models)
                {
                    Dictionary<int, Vertex> v_dict = new Dictionary<int, Vertex>();

                    verts_total += m.VertexData.Count;

                    foreach (SubModel sm in m.SubModels)
                    {
                        for (int i = 0; i < sm.DaisyChain.Length; i++)
                        {
                            Vertex v = m.VertexData[sm.DaisyChain[i]];

                            List<int> temp_boneIDs = new List<int>();

                            if ((m.BitFlag & 0x0200) == 0x200) //If we've got bone weight data, use it
                            {
                                foreach (int b in v.BoneIDs)
                                {
                                    if (sm.BoneIntegersList[b] == 133 || sm.BoneIntegersList[b] == 134)
                                    {
                                        int f = 1;
                                    }

                                    if (temp_boneIDs.Contains(sm.BoneIntegersList[0]) && b == 0) continue;
                                    temp_boneIDs.Add(sm.BoneIntegersList[b]);
                                }
                                if (temp_boneIDs.Count == 0) temp_boneIDs.Add(sm.BoneIntegersList[0]);
                            }
                            else //Otherwise use the EMG root bone with weight 1
                            {
                                v.BoneCount = 1;
                                temp_boneIDs = new List<int>() { e.RootBone, 0, 0, 0 };
                                v.BoneWeights = new List<float>() { 1, 0, 0 };
                            }
                            v.BoneIDs = temp_boneIDs;

                            if (!v_dict.TryGetValue(sm.DaisyChain[i], out _))
                            {
                                v_dict.Add(sm.DaisyChain[i], v);
                            }
                        }
                    }

                    for (int i = 0; i < v_dict.Count; i++)
                    {
                        Vertex v = v_dict[i];

                        if (v.BoneWeights.Sum() > 1f)
                        {
                            float f = v.BoneWeights.Sum();
                        }

                        if (v.BoneWeights.Count < v.BoneIDs.Count) //Calculate the 4th weight from the 3 weights stored in the EMG
                        {
                            v.BoneWeights.Add(Math.Max(1 - v.BoneWeights.Sum(),0f));
                        }

                        int boneweightcount = 0;
                        for (int j = 0; j < v.BoneIDs.Count; j++)
                        {
                            if (v.BoneWeights[j] > 0)
                            {
                                boneweightcount++;
                                if (!bw_float_dict.TryGetValue(v.BoneWeights[j], out _))
                                {
                                    bw_float_dict.Add(v.BoneWeights[j], bw_float_dict.Count);
                                }

                                v_string += (v.BoneIDs[j]) + " " + bw_float_dict[v.BoneWeights[j]] + " ";
                            }
                        }

                        vcounts_string += boneweightcount + " ";
                    }
                }
            }

            for (int i = 0; i < emo.Skeleton.NodeNames.Count; i++)
            {
                bone_names.Add(emo.Skeleton.NodeNames[i]);

                Node n = emo.Skeleton.Nodes[i];

                Matrix4x4 m = n.SkinBindPoseMatrix;

                //Check for empty SBP matrices
                if (m.M11 == 0 && m.M12 == 0 && m.M13 == 0 && m.M14 == 0 && m.M21 == 0 && m.M22 == 0 && m.M23 == 0 && m.M24 == 0 && m.M31 == 0 && m.M32 == 0 && m.M33 == 0 && m.M34 == 0 && m.M41 == 0 && m.M42 == 0 && m.M43 == 0 && m.M44 == 0)
                {
                    m = n.NodeMatrix;
                    while (n.Parent != -1)
                    {
                        n = emo.Skeleton.Nodes[n.Parent];
                        m = n.NodeMatrix * m;
                    }

                    Matrix4x4.Invert(m, out m);
                }

                //Matrix4x4 final_SBP = new Matrix4x4(m.M11, -m.M13, m.M12, m.M14,
                //                                    -m.M31, m.M22, m.M23, m.M24,
                //                                     m.M21, m.M32, m.M33, m.M34,
                //                                     m.M41, -m.M43, m.M42, m.M44);
                Matrix4x4 final_SBP = m;

                sbp_string += final_SBP.M11.ToString() + " ";
                sbp_string += final_SBP.M21.ToString() + " ";
                sbp_string += final_SBP.M31.ToString() + " ";
                sbp_string += final_SBP.M41.ToString() + " ";
                sbp_string += final_SBP.M12.ToString() + " ";
                sbp_string += final_SBP.M22.ToString() + " ";
                sbp_string += final_SBP.M32.ToString() + " ";
                sbp_string += final_SBP.M42.ToString() + " ";
                sbp_string += final_SBP.M13.ToString() + " ";
                sbp_string += final_SBP.M23.ToString() + " ";
                sbp_string += final_SBP.M33.ToString() + " ";
                sbp_string += final_SBP.M43.ToString() + " ";
                sbp_string += final_SBP.M14.ToString() + " ";
                sbp_string += final_SBP.M24.ToString() + " ";
                sbp_string += final_SBP.M34.ToString() + " ";
                sbp_string += final_SBP.M44.ToString() + " ";
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
                            source = $"#{emo_name}-mesh",
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
                                Count = verts_total,
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

        public static Grendgine_Collada_Library_Controllers old_EMOtoCollada_Library_Controller(EMO emo)
        {
            string sbp_string = string.Empty;
            Dictionary<float, int> bw_float_dict = new Dictionary<float, int>();

            string vcounts_string = string.Empty;
            string v_string = string.Empty;
            string emo_name = emo.Name;
            List<string> bone_names = new List<string>();
            List<Vertex> vertices = new List<Vertex>();

            int verts_total = 0;

            //Building an index to translate Vertex bone IDs into Skeleton bone IDs.
            //Verts can appear in multiple submodels, but when they do their bone ID entries have to "match" in
            //each submodel's BoneIntegersList ie return the same bone
            //...I hope
            foreach (EMG e in emo.EMGs)
            {
                foreach (Model m in e.Models)
                {
                    Dictionary<int, Vertex> v_dict = new Dictionary<int, Vertex>();

                    verts_total += m.VertexData.Count;

                    foreach (SubModel sm in m.SubModels)
                    {
                        for (int i = 0; i < sm.DaisyChain.Length; i++)
                        {
                            Vertex v = m.VertexData[sm.DaisyChain[i]];

                            List<int> temp_boneIDs = new List<int>();

                            if ((m.BitFlag & 0x0200) == 0x200) //If we've got bone weight data, use it
                            {
                                foreach (int b in v.BoneIDs)
                                {
                                    if (b != 0) temp_boneIDs.Add(sm.BoneIntegersList[b]);
                                }
                                if (temp_boneIDs.Count == 0) temp_boneIDs.Add(sm.BoneIntegersList[0]);
                            }
                            else //Otherwise use the EMG root bone with weight 1
                            {
                                v.BoneCount = 1;
                                temp_boneIDs = new List<int>() { e.RootBone, 0, 0, 0 };
                                v.BoneWeights = new List<float>() { 1, 0, 0 };
                            }
                            v.BoneIDs = temp_boneIDs;

                            if (!v_dict.TryGetValue(sm.DaisyChain[i], out _))
                            {
                                v_dict.Add(sm.DaisyChain[i], v);
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
                                v_string += (v.BoneIDs[j]) + " " + bw_float_dict[v.BoneWeights[j]] + " ";
                            }
                        }

                        vcounts_string += boneweightcount + " ";
                    }
                }
            }

            for (int i = 0; i < emo.Skeleton.NodeNames.Count; i++)
            {
                bone_names.Add(emo.Skeleton.NodeNames[i]);
            }

            foreach (Node n in emo.Skeleton.Nodes) //Column-major order
            {
                Matrix4x4.Invert(n.SkinBindPoseMatrix, out Matrix4x4 inverse);

                //Decompose the node matrix...
                Utils.DecomposeMatrixToDegrees(n.NodeMatrix, out float tx, out float ty, out float tz,
                                                               out float rx, out float ry, out float rz,
                                                               out float sx, out float sy, out float sz);
                //Recompose...
                Matrix4x4 rotX = Matrix4x4.CreateFromYawPitchRoll(rx, 0, 0);
                Matrix4x4 rotY = Matrix4x4.CreateFromYawPitchRoll(0, ry, 0);
                Matrix4x4 rotZ = Matrix4x4.CreateFromYawPitchRoll(0, 0, rz);
                Matrix4x4 tra = Matrix4x4.CreateTranslation(tx, ty, tz);
                Matrix4x4 sca = Matrix4x4.CreateScale(sx, sy, sz);

                //Set up our initial node matrix
                Matrix4x4 world_transform = sca * rotX * rotY * rotZ * tra;
                Node w_node = n;

                while (w_node.Parent != -1)
                {
                    w_node = emo.Skeleton.Nodes[w_node.Parent];

                    //Decompose parent node matrix...
                    Utils.DecomposeMatrixToDegrees(w_node.NodeMatrix, out tx, out ty, out tz,
                                                               out rx, out ry, out rz,
                                                               out sx, out sy, out sz);

                    rotX = Matrix4x4.CreateFromYawPitchRoll(rx, 0, 0);
                    rotY = Matrix4x4.CreateFromYawPitchRoll(0, ry, 0);
                    rotZ = Matrix4x4.CreateFromYawPitchRoll(0, 0, rz);
                    tra = Matrix4x4.CreateTranslation(tx, ty, tz);
                    sca = Matrix4x4.CreateScale(sx, sy, sz);
                    //Multiply by parent matrix
                    world_transform = world_transform * (sca * rotX * rotY * rotZ * tra);
                }

                Console.WriteLine($"");

                Utils.DecomposeMatrixToDegrees(inverse, out float Stx, out float Sty, out float Stz,
                                                               out float Srx, out float Sry, out float Srz,
                                                               out float Ssx, out float Ssy, out float Ssz);

                Matrix4x4.Invert(world_transform, out Matrix4x4 SBP);

                string local_string = SBP.M11.ToString() + " ";
                local_string += SBP.M21.ToString() + " ";
                local_string += SBP.M31.ToString() + " ";
                local_string += SBP.M41.ToString() + " ";
                local_string += SBP.M21.ToString() + " ";
                local_string += SBP.M22.ToString() + " ";
                local_string += SBP.M32.ToString() + " ";
                local_string += SBP.M42.ToString() + " ";
                local_string += SBP.M13.ToString() + " ";
                local_string += SBP.M23.ToString() + " ";
                local_string += SBP.M33.ToString() + " ";
                local_string += SBP.M43.ToString() + " ";
                local_string += SBP.M14.ToString() + " ";
                local_string += SBP.M24.ToString() + " ";
                local_string += SBP.M34.ToString() + " ";
                local_string += SBP.M44.ToString() + " ";


                sbp_string += SBP.M11.ToString() + " ";
                sbp_string += SBP.M21.ToString() + " ";
                sbp_string += SBP.M31.ToString() + " ";
                sbp_string += SBP.M41.ToString() + " ";
                sbp_string += SBP.M21.ToString() + " ";
                sbp_string += SBP.M22.ToString() + " ";
                sbp_string += SBP.M32.ToString() + " ";
                sbp_string += SBP.M42.ToString() + " ";
                sbp_string += SBP.M13.ToString() + " ";
                sbp_string += SBP.M23.ToString() + " ";
                sbp_string += SBP.M33.ToString() + " ";
                sbp_string += SBP.M43.ToString() + " ";
                sbp_string += SBP.M14.ToString() + " ";
                sbp_string += SBP.M24.ToString() + " ";
                sbp_string += SBP.M34.ToString() + " ";
                sbp_string += SBP.M44.ToString() + " ";

                //sbp_string += n.SkinBindPoseMatrix.M11.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M21.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M31.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M41.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M21.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M22.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M32.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M42.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M13.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M23.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M33.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M43.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M14.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M24.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M34.ToString() + " ";
                //sbp_string += n.SkinBindPoseMatrix.M44.ToString() + " ";
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
                            source = $"#{emo_name}-mesh",
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
                                Count = verts_total,
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

        public static Grendgine_Collada_Library_Animations EMAtoCollada_Library_Animations(EMA ema, int anim_index)
        {
            List<Grendgine_Collada_Animation> temp_anims = new List<Grendgine_Collada_Animation>();

            Animation a = ema.Animations[anim_index];

            //foreach (Animation a in ema.Animations)
            {
                foreach (CMDTrack c in a.CMDTracks)
                {

                    string bonename = ema.Skeleton.NodeNames[c.BoneID];
                    string type;
                    string TYPE;
                    string axis;
                    string target;
                    string time_vals = string.Empty;
                    string trans_vals = string.Empty;
                    string interp_vals = string.Empty;
                    string tangent_vals = string.Empty; //In SF4 anims, intangent == outtangent so we only need one list of tangent vals
                    string bezier_in_vals = string.Empty; //...but Blender doesn't support Hermite import, so we have to convert to Bezier
                    string bezier_out_vals = string.Empty; //equivalent Bezier in- and out- control handles will differ even if intangent == outtangent

                    if ((c.BitFlag & 0x10) == 0x10)
                    {
                        Console.WriteLine($"Zettai! bID = {c.BoneID}, {bonename}");
                    }

                    if (ema.Skeleton.Nodes[c.BoneID].Sibling != -1)
                    {
                        Console.WriteLine($"Sibling! This = {bonename}, sibling = {ema.Skeleton.Nodes[c.BoneID].Sibling}, {ema.Skeleton.NodeNames[ema.Skeleton.Nodes[c.BoneID].Sibling]}");
                    }

                    if ((c.BitFlag & 0x03) == 0) axis = "X";
                    else if ((c.BitFlag & 0x03) == 1) axis = "Y";
                    else axis = "Z";

                    if (c.TransformType == 0)
                    {
                        type = "translation";
                        TYPE = axis;
                        target = $"{bonename}/location.{axis}";
                    }
                    else if (c.TransformType == 1)
                    {
                        type = "rotation";
                        TYPE = "ANGLE";
                        target = $"{bonename}/rotation{axis}.ANGLE";
                    }
                    else
                    {
                        type = "scale";
                        TYPE = axis;
                        target = $"{bonename}/scale.{axis}";
                    }

                    string name = $"Armature_{ema.Skeleton.NodeNames[c.BoneID]}_{type}-{axis}";

                    foreach (int t in c.StepsList)
                    {
                        time_vals += $"{Convert.ToSingle(t) / 60f} ";
                        interp_vals += "BEZIER ";
                    }

                    for (int i = 0; i < c.StepsList.Count; i++)
                    {
                        int masked_index;
                        int masked_tangent;
                        float bezier_in;
                        float bezier_out;

                        if ((c.BitFlag & 0x40) == 0)
                        {
                            masked_index = c.IndicesList[i] & 0b0011111111111111;
                            masked_tangent = (c.IndicesList[i] >> 14);

                            if (masked_tangent == -2) masked_tangent = 2;
                        }
                        else
                        {
                            masked_index = c.IndicesList[i] & 0b00111111111111111111111111111111;
                            masked_tangent = c.IndicesList[i] >> 30;

                            if (masked_tangent == -2) masked_tangent = 2;
                        }

                        trans_vals += $"{a.ValuesList[masked_index]} ";


                        if (masked_tangent != 0)
                        {
                            bezier_in = a.ValuesList[masked_index] - a.ValuesList[masked_index + masked_tangent] / 3f;
                            bezier_out = a.ValuesList[masked_index] + a.ValuesList[masked_index + masked_tangent] / 3f;
                            //tangent_vals += $"0 {a.ValuesList[masked_index + masked_tangent]} ";
                            bezier_in_vals += $"0 {bezier_in} ";
                            bezier_out_vals += $"0 {bezier_out} ";
                        }
                        else
                        {
                            bezier_in_vals += "0 0 ";
                            bezier_out_vals += "0 0 ";

                            //tangent_vals += "0 0 ";
                        }
                    }

                    temp_anims.Add(new Grendgine_Collada_Animation()
                    {
                        ID = name,
                        Source = new Grendgine_Collada_Source[]
                        {
                            //INPUT ARRAY (TIME IN SECONDS)
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-input",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-input-array",
                                    Count = c.StepsList.Count,
                                    Value_As_String = time_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-input-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "TIME",
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            //OUTPUT ARRAY (TRANSFORM VALUE)
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-output",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-output-array",
                                    Count = c.StepsList.Count,
                                    Value_As_String = trans_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-output-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = TYPE, //NEEDS TO BE THE TRANSFORM TYPE PROBABLY
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-intangents",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-intangents-array",
                                    Count = 2 * c.StepsList.Count,
                                    Value_As_String = bezier_in_vals,
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                //<source id="Armature_Chest_ArmatureAction___Chest___rotation_euler_X-intangent">
                                //  <float_array id="Armature_Chest_ArmatureAction___Chest___rotation_euler_X-intangent-array" count="8">0.1388888 -0.08885568 0.3472222 -0.006967127 0.5555555 -0.06276804 0.7638889 -0.003921687</float_array>
                                //  <technique_common>
                                //    <accessor source="#Armature_Chest_ArmatureAction___Chest___rotation_euler_X-intangent-array" count="4" stride="2">
                                //      <param name="X" type="float"/>
                                //      <param name="Y" type="float"/>
                                //    </accessor>
                                //  </technique_common>
                                //</source>

                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-intangents-array",
                                        Count = (uint)(2 * c.StepsList.Count),
                                        Stride = 2,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "IN_TANGENT", //NEEDS TO BE THE TRANSFORM TYPE PROBABLY
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-outtangents",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-outtangents-array",
                                    Count = c.StepsList.Count,
                                    Value_As_String = bezier_out_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-outtangents-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "OUT_TANGENT", //NEEDS TO BE THE TRANSFORM TYPE PROBABLY
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            //INTERPOLATION METHOD (BEZIER)
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-interpolation",
                                Name_Array = new Grendgine_Collada_Name_Array()
                                {
                                    ID = $"{name}-interpolation-array",
                                    Count = c.StepsList.Count,
                                    Value_Pre_Parse = interp_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-interpolation-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "INTERPOLATION",
                                                Type = "name"
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Sampler = new Grendgine_Collada_Sampler[]
                        {
                            new Grendgine_Collada_Sampler()
                            {
                                ID = $"{name}-sampler",
                                Input = new Grendgine_Collada_Input_Unshared[]
                                {
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.INPUT,
                                        source = $"#{name}-input"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.OUTPUT,
                                        source = $"#{name}-output"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.IN_TANGENT,
                                        source = $"#{name}-intangents"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.OUT_TANGENT,
                                        source = $"#{name}-outtangents"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.INTERPOLATION,
                                        source = $"#{name}-interpolation"
                                    }
                                }
                            }
                        },
                        Channel = new Grendgine_Collada_Channel[]
                        {
                            new Grendgine_Collada_Channel()
                            {
                                Source = $"#{name}-sampler",
                                Target = target
                            }
                        }
                    });
                }
            }

            Grendgine_Collada_Library_Animations library_animations = new Grendgine_Collada_Library_Animations()
            {
                Animation = temp_anims.ToArray()
            };

            return library_animations;
        }

        private struct CMDKeyframe
        {
            public int BoneID;
            public byte TransformType;
            public byte BitFlag;
            public List<int> StepsList;
            public List<int> IndicesList;
            public List<float> ValueList;
        }

        public static Grendgine_Collada_Library_Animations EMAtoCollada_Library_Animations_AssetExplorer(EMA ema, int anim_index)
        {
            List<Grendgine_Collada_Animation> temp_anims = new List<Grendgine_Collada_Animation>();

            EMAProcessor eMAProcessor = new EMAProcessor(ema, anim_index);

            string[] animated_node_strings = new string[ema.Skeleton.Nodes.Count];

            Animation a = ema.Animations[anim_index];

            for (int i = 0; i < a.Duration; i++)
            {
                eMAProcessor.AnimateFrame(i);
                AnimatedNode[] an = eMAProcessor.ReturnAnimatedNodes();

                for (int j = 0; j < ema.Skeleton.Nodes.Count; j++)
                {
                    if (ema.Skeleton.NodeNames[j] == "RArmRoot")
                    {
                        int test = 0;
                    }

                    Matrix4x4 parent_inv = Matrix4x4.Identity;
                    if (an[j].Parent != -1)
                        Matrix4x4.Invert(an[an[j].Parent].animatedMatrix, out parent_inv);

                    Matrix4x4 local = an[j].animatedMatrix * parent_inv;

                    if (!an[j].IKanimatedNode) local = an[j].animatedLocalMatrix;

                    animated_node_strings[j] += $"{local.M11} ";
                    animated_node_strings[j] += $"{local.M21} ";
                    animated_node_strings[j] += $"{local.M31} ";
                    animated_node_strings[j] += $"{local.M41} ";
                    animated_node_strings[j] += $"{local.M12} ";
                    animated_node_strings[j] += $"{local.M22} ";
                    animated_node_strings[j] += $"{local.M32} ";
                    animated_node_strings[j] += $"{local.M42} ";
                    animated_node_strings[j] += $"{local.M13} ";
                    animated_node_strings[j] += $"{local.M23} ";
                    animated_node_strings[j] += $"{local.M33} ";
                    animated_node_strings[j] += $"{local.M43} ";
                    animated_node_strings[j] += $"{local.M14} ";
                    animated_node_strings[j] += $"{local.M24} ";
                    animated_node_strings[j] += $"{local.M34} ";
                    animated_node_strings[j] += $"{local.M44} ";
                }
            }

            for (int i = 0; i < ema.Skeleton.Nodes.Count; i++)
            {
                bool animated = false;
                for (int j = 0; j < a.CmdTrackCount; j++)
                {
                    if (a.CMDTracks[j].BoneID == i)
                    {
                        animated = true;
                        break;
                    }
                }

                if (!animated) continue;

                string bonename = ema.Skeleton.NodeNames[i];
                string time_vals = string.Empty;
                string trans_vals = string.Empty;
                string interp_vals = string.Empty;

                string name = $"Armature_{ema.Skeleton.NodeNames[i]}_matrix";

                trans_vals = animated_node_strings[i];

                for (int j = 0; j < a.Duration; j++)
                {
                    time_vals += $"{Convert.ToSingle(j) / 60f} ";
                    interp_vals += "LINEAR ";

                    Matrix4x4 m = Anim.InterpolateKeyFramesAsMatrices_NODEPTH(ema, a, i, j);
                    //Matrix4x4 m = Anim.InterpolateKeyFramesAsMatrices(ema, a, i, j);
                }

                temp_anims.Add(new Grendgine_Collada_Animation()
                {
                    ID = name,
                    Source = new Grendgine_Collada_Source[]
                    {
                        //INPUT ARRAY (TIME IN SECONDS)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-input",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-input-array",
                                Count = a.Duration,
                                Value_As_String = time_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-input-array",
                                    Count = (uint)a.Duration,
                                    Stride = 1,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Name = "TIME",
                                            Type = "float"
                                        }
                                    }
                                }
                            }
                        },
                        //OUTPUT ARRAY (TRANSFORM VALUE)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-output",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-output-array",
                                Count = a.Duration * 16,
                                Value_As_String = trans_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-output-array",
                                    Count = (uint)a.Duration,
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
                        //INTERPOLATION METHOD (BEZIER)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-interpolation",
                            Name_Array = new Grendgine_Collada_Name_Array()
                            {
                                ID = $"{name}-interpolation-array",
                                Count = a.Duration,
                                Value_Pre_Parse = interp_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-interpolation-array",
                                    Count = (uint)a.Duration,
                                    Stride = 1,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Name = "INTERPOLATION",
                                            Type = "name"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Sampler = new Grendgine_Collada_Sampler[]
                    {
                        new Grendgine_Collada_Sampler()
                        {
                            ID = $"{name}-sampler",
                            Input = new Grendgine_Collada_Input_Unshared[]
                            {
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.INPUT,
                                    source = $"#{name}-input"
                                },
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.OUTPUT,
                                    source = $"#{name}-output"
                                },
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.INTERPOLATION,
                                    source = $"#{name}-interpolation"
                                }
                            }
                        }
                    },
                    Channel = new Grendgine_Collada_Channel[]
                    {
                        new Grendgine_Collada_Channel()
                        {
                            Source = $"#{name}-sampler",
                            Target = $"{bonename}/matrix"
                        }
                    }
                });
            }

            Grendgine_Collada_Library_Animations library_animations = new Grendgine_Collada_Library_Animations()
            {
                Animation = temp_anims.ToArray()
            };

            return library_animations;
        }

        public static Grendgine_Collada_Library_Animations EMAtoCollada_Library_Animations_Matrix(EMA ema, int anim_index)
        {
            List<Grendgine_Collada_Animation> temp_anims = new List<Grendgine_Collada_Animation>();

            Animation a = ema.Animations[anim_index];

            for (int i = 0; i < ema.Skeleton.Nodes.Count; i++)
            {
                bool animated = false;
                for (int j = 0; j < a.CmdTrackCount; j++)
                {
                    if (a.CMDTracks[j].BoneID == i)
                    {
                        animated = true;
                        break;
                    }
                }

                if (!animated) continue;

                string bonename = ema.Skeleton.NodeNames[i];      
                string time_vals = string.Empty;
                string trans_vals = string.Empty;
                string interp_vals = string.Empty;

                string name = $"Armature_{ema.Skeleton.NodeNames[i]}_matrix";

                for (int j = 0; j < a.Duration; j++)
                {
                    time_vals += $"{Convert.ToSingle(j) / 60f} ";
                    interp_vals += "LINEAR ";

                    Matrix4x4 m = Anim.InterpolateKeyFramesAsMatrices_NODEPTH(ema, a, i, j);
                    //Matrix4x4 m = Anim.InterpolateKeyFramesAsMatrices(ema, a, i, j);

                    trans_vals += $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} ";
                }

                temp_anims.Add(new Grendgine_Collada_Animation()
                {
                    ID = name,
                    Source = new Grendgine_Collada_Source[]
                    {
                        //INPUT ARRAY (TIME IN SECONDS)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-input",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-input-array",
                                Count = a.Duration,
                                Value_As_String = time_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-input-array",
                                    Count = (uint)a.Duration,
                                    Stride = 1,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Name = "TIME",
                                            Type = "float"
                                        }
                                    }
                                }
                            }
                        },
                        //OUTPUT ARRAY (TRANSFORM VALUE)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-output",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-output-array",
                                Count = a.Duration * 16,
                                Value_As_String = trans_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-output-array",
                                    Count = (uint)a.Duration,
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
                        //INTERPOLATION METHOD (BEZIER)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-interpolation",
                            Name_Array = new Grendgine_Collada_Name_Array()
                            {
                                ID = $"{name}-interpolation-array",
                                Count = a.Duration,
                                Value_Pre_Parse = interp_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-interpolation-array",
                                    Count = (uint)a.Duration,
                                    Stride = 1,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Name = "INTERPOLATION",
                                            Type = "name"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Sampler = new Grendgine_Collada_Sampler[]
                    {
                        new Grendgine_Collada_Sampler()
                        {
                            ID = $"{name}-sampler",
                            Input = new Grendgine_Collada_Input_Unshared[]
                            {
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.INPUT,
                                    source = $"#{name}-input"
                                },
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.OUTPUT,
                                    source = $"#{name}-output"
                                },
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.INTERPOLATION,
                                    source = $"#{name}-interpolation"
                                }
                            }
                        }
                    },
                    Channel = new Grendgine_Collada_Channel[]
                    {
                        new Grendgine_Collada_Channel()
                        {
                            Source = $"#{name}-sampler",
                            Target = $"{bonename}/matrix"
                        }
                    }
                });
            }

            Grendgine_Collada_Library_Animations library_animations = new Grendgine_Collada_Library_Animations()
            {
                Animation = temp_anims.ToArray()
            };

            return library_animations;
        }

        public static Grendgine_Collada_Library_Animations EMAtoCollada_Library_AnimationsWithInterpolation(EMA ema, int anim_index)
        {

            List<Grendgine_Collada_Animation> temp_anims = new List<Grendgine_Collada_Animation>();

            Animation a = ema.Animations[anim_index];

            List<CMDKeyframe> temp_cmds = new List<CMDKeyframe>();

            foreach (CMDTrack c in a.CMDTracks)
            {
                if ((c.BitFlag & 0x10) == 0x10) continue;

                CMDKeyframe CMDinterpolated = new CMDKeyframe()
                {
                    BitFlag = c.BitFlag,
                    BoneID = c.BoneID,
                    StepsList = new List<int>(),
                    IndicesList = c.IndicesList,
                    ValueList = new List<float>(),
                    TransformType = c.TransformType
                };

                for (int i = 0; i < a.Duration; i++)
                {
                    CMDinterpolated.StepsList.Add(i);

                    CMDinterpolated.ValueList.Add(Anim.InterpolateRelativeKeyFrames(a, c, i));
                }

                temp_cmds.Add(CMDinterpolated);
            }

            foreach (CMDKeyframe c in temp_cmds)
            {
                string bonename = ema.Skeleton.NodeNames[c.BoneID];
                string type;
                string TYPE;
                string axis;
                string target;
                string time_vals = string.Empty;
                string trans_vals = string.Empty;
                string interp_vals = string.Empty;               

                if ((c.BitFlag & 0x03) == 0) axis = "X";
                else if ((c.BitFlag & 0x03) == 1) axis = "Y";
                else axis = "Z";

                if (c.TransformType == 0)
                {
                    type = "translation";
                    TYPE = axis;
                    target = $"{bonename}/location.{axis}";
                }
                else if (c.TransformType == 1)
                {
                    type = "rotation";
                    TYPE = "ANGLE";
                    target = $"{bonename}/rotation{axis}.ANGLE";
                }
                else
                {
                    type = "scale";
                    TYPE = axis;
                    target = $"{bonename}/scale.{axis}";
                }

                string name = $"Armature_{ema.Skeleton.NodeNames[c.BoneID]}_{type}-{axis}";

                foreach (int t in c.StepsList)
                {
                    time_vals += $"{Convert.ToSingle(t) / 60f} ";
                    interp_vals += "LINEAR ";
                }

                for (int i = 0; i < c.StepsList.Count; i++)
                {
                    //if(axis == "Y") trans_vals += $"{-c.ValueList[i]} ";
                    //else trans_vals += $"{c.ValueList[i]} ";
                    trans_vals += $"{c.ValueList[i]} ";
                }

                temp_anims.Add(new Grendgine_Collada_Animation()
                {
                    ID = name,
                    Source = new Grendgine_Collada_Source[]
                    {
                        //INPUT ARRAY (TIME IN SECONDS)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-input",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-input-array",
                                Count = c.StepsList.Count,
                                Value_As_String = time_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-input-array",
                                    Count = (uint)c.StepsList.Count,
                                    Stride = 1,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Name = "TIME",
                                            Type = "float"
                                        }
                                    }
                                }
                            }
                        },
                        //OUTPUT ARRAY (TRANSFORM VALUE)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-output",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-output-array",
                                Count = c.StepsList.Count,
                                Value_As_String = trans_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-output-array",
                                    Count = (uint)c.StepsList.Count,
                                    Stride = 1,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Name = TYPE, //NEEDS TO BE THE TRANSFORM TYPE PROBABLY
                                            Type = "float"
                                        }
                                    }
                                }
                            }
                        },
                        //INTERPOLATION METHOD (BEZIER)
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-interpolation",
                            Name_Array = new Grendgine_Collada_Name_Array()
                            {
                                ID = $"{name}-interpolation-array",
                                Count = c.StepsList.Count,
                                Value_Pre_Parse = interp_vals
                            },
                            Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                            {
                                Accessor = new Grendgine_Collada_Accessor()
                                {
                                    Source = $"#{name}-interpolation-array",
                                    Count = (uint)c.StepsList.Count,
                                    Stride = 1,
                                    Param = new Grendgine_Collada_Param[]
                                    {
                                        new Grendgine_Collada_Param()
                                        {
                                            Name = "INTERPOLATION",
                                            Type = "name"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Sampler = new Grendgine_Collada_Sampler[]
                    {
                        new Grendgine_Collada_Sampler()
                        {
                            ID = $"{name}-sampler",
                            Input = new Grendgine_Collada_Input_Unshared[]
                            {
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.INPUT,
                                    source = $"#{name}-input"
                                },
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.OUTPUT,
                                    source = $"#{name}-output"
                                },
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.INTERPOLATION,
                                    source = $"#{name}-interpolation"
                                }
                            }
                        }
                    },
                    Channel = new Grendgine_Collada_Channel[]
                    {
                        new Grendgine_Collada_Channel()
                        {
                            Source = $"#{name}-sampler",
                            Target = target
                        }
                    }
                });

            }

            Grendgine_Collada_Library_Animations library_animations = new Grendgine_Collada_Library_Animations()
            {
                Animation = temp_anims.ToArray()
            };

            return library_animations;
        }

        public static Dictionary<double, int> BoneWeightDictionary(List<Vertex> vs)
        {
            Dictionary<double, int> bw = new Dictionary<double, int>();

            foreach (Vertex v in vs)
            {
                foreach (float w in v.BoneWeights)
                {
                    if (!bw.TryGetValue(w, out _)) bw.Add(w, bw.Count);
                }
            }

            return bw;
        }

        public static EMG GrendgineCollada(out List<Node> Skeleton)
        {
            Grendgine_Collada model = Grendgine_Collada.Grendgine_Load_File("clothes.dae");

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

                        for (int i = 0; i < bpf.Count / 16; i++)
                        {
                            bindpose_matrices.Add(new Matrix4x4(bpf[16 * i + 0], bpf[16 * i + 4], bpf[16 * i + 8], bpf[16 * i + 12],
                                                                bpf[16 * i + 1], bpf[16 * i + 5], bpf[16 * i + 9], bpf[16 * i + 13],
                                                                bpf[16 * i + 2], bpf[16 * i + 6], bpf[16 * i + 10], bpf[16 * i + 14],
                                                                bpf[16 * i + 3], bpf[16 * i + 7], bpf[16 * i + 11], bpf[16 * i + 15]));
                        }
                    }
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
                            current_matrix = new Matrix4x4(mf[0], mf[4], mf[8], mf[12],
                                                            mf[1], mf[5], mf[9], mf[13],
                                                            mf[2], mf[6], mf[10], mf[14],
                                                            mf[3], mf[7], mf[11], mf[15]);
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
                        if (master_bone_dict.TryGetValue(current_node.Name, out _))
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

            Utils.WriteDataToStream("skin.emg", emg.HEXBytes);

            return emg;
        }

        public static EMG GrendgineCollada2(Skeleton skel, out List<Node> Skeleton)
        {
            Grendgine_Collada model = Grendgine_Collada.Grendgine_Load_File("clothes.dae");

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

                        for (int i = 0; i < bpf.Count / 16; i++)
                        {
                            bindpose_matrices.Add(new Matrix4x4(bpf[16 * i + 0], bpf[16 * i + 4], bpf[16 * i + 8], bpf[16 * i + 12],
                                                                bpf[16 * i + 1], bpf[16 * i + 5], bpf[16 * i + 9], bpf[16 * i + 13],
                                                                bpf[16 * i + 2], bpf[16 * i + 6], bpf[16 * i + 10], bpf[16 * i + 14],
                                                                bpf[16 * i + 3], bpf[16 * i + 7], bpf[16 * i + 11], bpf[16 * i + 15]));
                        }
                    }
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
                            current_matrix = new Matrix4x4(mf[0], mf[4], mf[8], mf[12],
                                                            mf[1], mf[5], mf[9], mf[13],
                                                            mf[2], mf[6], mf[10], mf[14],
                                                            mf[3], mf[7], mf[11], mf[15]);
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
                        if (master_bone_dict.TryGetValue(current_node.Name, out _))
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
                    BoneWeights = tempFloats
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
                float nx = normal_list[tri_indices[i * 3 + 1]].nX;
                float ny = normal_list[tri_indices[i * 3 + 1]].nY;

                VertexList.Add(new Vertex()
                {
                    X = position_list[tri_indices[i * 3]].X,
                    Y = position_list[tri_indices[i * 3]].Y,
                    Z = position_list[tri_indices[i * 3]].Z,
                    ntangentX = 1,
                    ntangentY = 0,
                    ntangentZ = 0,
                    //Experiment with rotating 90 deg and see what happens
                    //ntangentX = (float)(nx * Math.Cos(Math.PI / 2) - ny * Math.Sin(Math.PI / 2)),
                    //ntangentY = (float)(nx * Math.Sin(Math.PI / 2) + ny * Math.Cos(Math.PI / 2)),
                    //ntangentZ = normal_list[tri_indices[i * 3 + 1]].nZ,
                    BoneCount = position_list[tri_indices[i * 3]].BoneCount,
                    BoneIDs = new List<int>(position_list[tri_indices[i * 3]].BoneIDs),
                    BoneWeights = new List<float>(position_list[tri_indices[i * 3]].BoneWeights),
                    //BoneCount = 1,
                    //BoneIDs = new List<int>() { 8 },
                    //BoneWeights = new List<float>() { 1f },
                    //nX = normal_list[tri_indices[i * 3 + 1]].nX,
                    //nY = normal_list[tri_indices[i * 3 + 1]].nY,
                    //nZ = normal_list[tri_indices[i * 3 + 1]].nZ,
                    nX = -normal_list[tri_indices[i * 3 + 1]].nX,
                    nY = -normal_list[tri_indices[i * 3 + 1]].nY,
                    nZ = -normal_list[tri_indices[i * 3 + 1]].nZ,
                    U = texture_list[tri_indices[i * 3 + 2]].U,
                    V = texture_list[tri_indices[i * 3 + 2]].V,
                    Colour = Utils.ReadFloat(0, new byte[] { 0xFE, 0xFE, 0xFE, 0xFF }),
                });
            }

            for (int i = 0; i < tri_indices.Count / 9; i++)
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

            //Replace DAE bone dictionary with EMO bone dictionary
            Dictionary<int, int> bone_translation_dict = new Dictionary<int, int>();
            for (int i = 0; i < skel.NodeNames.Count; i++)
            {
                bone_translation_dict.Add(master_bone_dict[skel.NodeNames[i]], i);
            }

            //Build a dictionary to translate absolute bone ref to submodel bone ref
            Dictionary<int, int> BoneDictionary = new Dictionary<int, int>();

            for (int i = 0; i < VertexList.Count; i++)
            {
                for (int j = 0; j < VertexList[i].BoneIDs.Count; j++)
                {
                    if (!BoneDictionary.TryGetValue(bone_translation_dict[VertexList[i].BoneIDs[j]], out _))
                    {
                        BoneDictionary.Add(bone_translation_dict[VertexList[i].BoneIDs[j]], BoneDictionary.Count);
                    }
                }
            }

            //BoneDictionary.Add(0, BoneDictionary.Count);
            //Update vertex list with submodel bone refs
            for (int i = 0; i < VertexList.Count; i++)
            {
                for (int j = 0; j < VertexList[i].BoneIDs.Count; j++)
                {
                    VertexList[i].BoneIDs[j] = BoneDictionary[bone_translation_dict[VertexList[i].BoneIDs[j]]];
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
                        BitFlag = 0x02C7,
                        BitDepth = 0x40,
                        TextureCount = 1,
                        TextureListPointer = 0x00,
                        VertexCount = VertexList.Count,
                        VertexData = VertexList,
                        //ReadMode = 0,   //triangles
                        ReadMode = 1, //stripped
                        SubModelsCount = 1,
                        SubModelsListPointer = 1,
                        SubModelPointersList = new List<int>() { 0x00 },
                        SubModels = new List<SubModel>
                        {
                            new SubModel()
                            {
                                //DaisyChain = test,
                                //DaisyChainLength = test.Length,
                                DaisyChain = Daisy.ToArray(),
                                DaisyChainLength = Daisy.Count,
                                SubModelName = Utils.MakeModelName("Polygon"),
                                BoneIntegersCount = BoneDictionary.Count,
                                //BoneIntegersCount = 1,
                                MaterialIndex = 0,
                                BoneIntegersList = BoneDictionary.Keys.ToList(),
                                //BoneIntegersList = new List<int>(){8},
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
                                TextureLayers = 2,
                                TextureIndicesList = new List<int> { 0 , 1 },
                                Scales_UList = new List<float> { 1f, 1f },
                                Scales_VList = new List<float> { 1f, 1f }
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

            Utils.WriteDataToStream("skin.emg", emg.HEXBytes);

            return emg;
        }

        public static Grendgine_Collada_Library_Geometries LEGACY_EMOtoCollada_Library_Geometries(EMO emo)
        {
            string emo_name = emo.Name;
            List<Vertex> vertices = new List<Vertex>();
            string pos_string = string.Empty;
            string norm_string = string.Empty;
            string map_string = string.Empty;
            string p_string = string.Empty;

            int v_t = 0;

            foreach (EMG e in emo.EMGs)
            {
                foreach (Model m in e.Models)
                {
                    bool readmode = (m.ReadMode == 1) ? false : true;

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
            //foreach (Vertex v in vertices)
            //{
            //    pos_string += $"{v.X} {-v.Z} {v.Y} ";
            //    norm_string += $"{v.nX} {-v.nZ} {v.nY} ";
            //    map_string += $"{v.U} {-v.V} ";
            //}
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

        public static Grendgine_Collada_Geometry EMGtoCollada_Geometry(EMO emo, int index)
        {
            EMG emg = emo.EMGs[index];
            string name = emo.Skeleton.NodeNames[emg.RootBone];
            List<Vertex> vertices = new List<Vertex>();
            string pos_string = string.Empty;
            string norm_string = string.Empty;
            string map_string = string.Empty;
            string p_string = string.Empty;

            int v_t = 0;



            foreach (Model m in emg.Models)
            {
                bool readmode = (m.ReadMode == 1) ? false : true;

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

            return new Grendgine_Collada_Geometry()
            {
                ID = $"{name}-mesh",
                Name = name,
                Mesh = new Grendgine_Collada_Mesh()
                {
                    Source = new Grendgine_Collada_Source[]
                    {
                        //Position floats
                        new Grendgine_Collada_Source()
                        {
                            ID = $"{name}-mesh-positions",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-mesh-positions-array",
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
                            ID = $"{name}-mesh-normals",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-mesh-normals-array",
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
                            ID = $"{name}-mesh-map-0",
                            Float_Array = new Grendgine_Collada_Float_Array()
                            {
                                ID = $"{name}-mesh-map-0-array",
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
                        ID = $"#{name}-mesh-vertices",
                        Input = new Grendgine_Collada_Input_Unshared[]
                            {
                                new Grendgine_Collada_Input_Unshared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.POSITION,
                                    source = $"#{name}-mesh-positions"
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
                                    source = $"#{name}-mesh-vertices",
                                    Offset = 0
                                },
                                new Grendgine_Collada_Input_Shared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.NORMAL,
                                    source = $"#{name}-mesh-normals",
                                    Offset = 1
                                },
                                new Grendgine_Collada_Input_Shared()
                                {
                                    Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD,
                                    source = $"#{name}-mesh-map-0",
                                    Offset = 2
                                }
                            }
                        }
                    }
                }
            };
        }

        public static Grendgine_Collada_Library_Geometries MODELSPLIT_EMOtoCollada_Library_Geometries(EMO emo)
        {
            Grendgine_Collada_Geometry[] geometries = new Grendgine_Collada_Geometry[emo.EMGs.Count];

            for (int i = 0; i < emo.EMGs.Count; i++)
            {
                geometries[i] = EMGtoCollada_Geometry(emo, i);
            }

            Grendgine_Collada_Library_Geometries library_geometries = new Grendgine_Collada_Library_Geometries()
            {
                Geometry = geometries
            };

            return library_geometries;
        }

        public static Grendgine_Collada_Library_Controllers MODELSPLIT_EMOtoCollada_Library_Controller(EMO emo)
        {
            Grendgine_Collada_Controller[] controllers = new Grendgine_Collada_Controller[emo.EMGs.Count];

            for (int i = 0; i < emo.EMGs.Count; i++)
            {
                EMGtoCollada_Controller(emo, i);
            }

            return new Grendgine_Collada_Library_Controllers();
        }

        public static Grendgine_Collada_Controller EMGtoCollada_Controller(EMO emo, int index)
        {
            EMG emg = emo.EMGs[index];

            string sbp_string = string.Empty;
            Dictionary<float, int> bw_float_dict = new Dictionary<float, int>();

            string vcounts_string = string.Empty;
            string v_string = string.Empty;
            string emo_name = emo.Name;
            List<string> bone_names = new List<string>();
            List<Vertex> vertices = new List<Vertex>();

            int verts_total = 0;

            foreach (Model m in emg.Models)
            {
                Dictionary<int, Vertex> v_dict = new Dictionary<int, Vertex>();

                verts_total += m.VertexData.Count;

                foreach (SubModel sm in m.SubModels)
                {
                    for (int i = 0; i < sm.DaisyChain.Length; i++)
                    {
                        Vertex v = m.VertexData[sm.DaisyChain[i]];

                        List<int> temp_boneIDs = new List<int>();

                        if ((m.BitFlag & 0x0200) == 0x200) //If we've got bone weight data, use it
                        {
                            foreach (int b in v.BoneIDs)
                            {
                                if (b != 0) temp_boneIDs.Add(sm.BoneIntegersList[b]);
                            }
                            if (temp_boneIDs.Count == 0) temp_boneIDs.Add(sm.BoneIntegersList[0]);
                        }
                        else //Otherwise use the EMG root bone with weight 1
                        {
                            v.BoneCount = 1;
                            temp_boneIDs = new List<int>() { emg.RootBone, 0, 0, 0 };
                            v.BoneWeights = new List<float>() { 1, 0, 0 };
                        }
                        v.BoneIDs = temp_boneIDs;

                        if (!v_dict.TryGetValue(sm.DaisyChain[i], out _))
                        {
                            v_dict.Add(sm.DaisyChain[i], v);
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
                            v_string += (v.BoneIDs[j]) + " " + bw_float_dict[v.BoneWeights[j]] + " ";
                        }
                    }

                    vcounts_string += boneweightcount + " ";
                }
            }

            return new Grendgine_Collada_Controller();
        }

        public static Grendgine_Collada_Library_Controllers LEGACY_EMOtoCollada_Library_Controller(EMO emo)
        {
            string sbp_string = string.Empty;
            Dictionary<float, int> bw_float_dict = new Dictionary<float, int>();

            string vcounts_string = string.Empty;
            string v_string = string.Empty;
            string emo_name = emo.Name;
            List<string> bone_names = new List<string>();
            List<Vertex> vertices = new List<Vertex>();

            int verts_total = 0;

            //Building an index to translate Vertex bone IDs into Skeleton bone IDs.
            //Verts can appear in multiple submodels, but when they do their bone ID entries have to "match" in
            //each submodel's BoneIntegersList ie return the same bone
            //...I hope
            foreach (EMG e in emo.EMGs)
            {
                foreach (Model m in e.Models)
                {
                    Dictionary<int, Vertex> v_dict = new Dictionary<int, Vertex>();

                    verts_total += m.VertexData.Count;

                    foreach (SubModel sm in m.SubModels)
                    {
                        for (int i = 0; i < sm.DaisyChain.Length; i++)
                        {
                            Vertex v = m.VertexData[sm.DaisyChain[i]];

                            List<int> temp_boneIDs = new List<int>();

                            if ((m.BitFlag & 0x0200) == 0x200) //If we've got bone weight data, use it
                            {
                                foreach (int b in v.BoneIDs)
                                {
                                    if (b != 0) temp_boneIDs.Add(sm.BoneIntegersList[b]);
                                }
                                if (temp_boneIDs.Count == 0) temp_boneIDs.Add(sm.BoneIntegersList[0]);
                            }
                            else //Otherwise use the EMG root bone with weight 1
                            {
                                v.BoneCount = 1;
                                temp_boneIDs = new List<int>() { e.RootBone, 0, 0, 0 };
                                v.BoneWeights = new List<float>() { 1, 0, 0 };
                            }
                            v.BoneIDs = temp_boneIDs;

                            if (!v_dict.TryGetValue(sm.DaisyChain[i], out _))
                            {
                                v_dict.Add(sm.DaisyChain[i], v);
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
                                v_string += (v.BoneIDs[j]) + " " + bw_float_dict[v.BoneWeights[j]] + " ";
                            }
                        }

                        vcounts_string += boneweightcount + " ";
                    }
                }
            }

            for (int i = 0; i < emo.Skeleton.NodeNames.Count; i++)
            {
                bone_names.Add(emo.Skeleton.NodeNames[i]);
            }

            foreach (Node n in emo.Skeleton.Nodes) //Column-major order
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
                            source = $"#{emo_name}-mesh",
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
                                Count = verts_total,
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

        public static Grendgine_Collada_Library_Visual_Scenes EMOtoCollada_Library_Visual_Scenes_Matrix(EMO emo)
        {
            List<Grendgine_Collada_Node> node_list = new List<Grendgine_Collada_Node>();

            Skeleton sk = emo.Skeleton;

            for (int i = 0; i < sk.Nodes.Count; i++)
            {
                Node n = sk.Nodes[i];
                if (n.Parent == -1) //Root nodes
                {
                    Matrix4x4 m = n.NodeMatrix;
                    string name = sk.NodeNames[i];

                    Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz,
                                                out float rx, out float ry, out float rz,
                                                out float sx, out float sy, out float sz);

                    node_list.Add(new Grendgine_Collada_Node()
                    {
                        ID = name,
                        Name = name,
                        sID = name,
                        Type = Grendgine_Collada_Node_Type.JOINT,
                        node = RecursiveNode(sk, i),
                        Matrix = new Grendgine_Collada_Matrix[] 
                        { 
                            new Grendgine_Collada_Matrix()
                            {
                                sID = "matrix",
                                Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} "
                            }
                        },
                    });
                }
            }



            Grendgine_Collada_Node[] RecursiveNode(Skeleton s, int n)
            {
                List<Grendgine_Collada_Node> child_list = new List<Grendgine_Collada_Node>();
                for (int i = 0; i < s.Nodes.Count; i++)
                {
                    if (s.Nodes[i].Parent == n)
                    {
                        Node node = sk.Nodes[i];
                        Matrix4x4 m = node.NodeMatrix;
                        string name = sk.NodeNames[i];

                        Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz,
                                                    out float rx, out float ry, out float rz,
                                                    out float sx, out float sy, out float sz);

                        child_list.Add(new Grendgine_Collada_Node()
                        {
                            ID = name,
                            Name = name,
                            sID = name,
                            Type = Grendgine_Collada_Node_Type.JOINT,
                            node = RecursiveNode(sk, i),
                            Matrix = new Grendgine_Collada_Matrix[]
                            {
                                new Grendgine_Collada_Matrix()
                                {
                                    sID = "matrix",
                                    Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} "
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
                            node_list[0],
                            new Grendgine_Collada_Node()
                            {
                                ID = emo.Name,
                                Name = emo.Name,
                                Type = Grendgine_Collada_Node_Type.NODE,
                                Matrix = new Grendgine_Collada_Matrix[]
                                {
                                    new Grendgine_Collada_Matrix()
                                    {
                                        sID = "transform",
                                        Value_As_String = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"
                                    }
                                },
                                Instance_Controller = new Grendgine_Collada_Instance_Controller[]
                                {
                                    new Grendgine_Collada_Instance_Controller()
                                    {
                                        URL = $"#{emo.Name}-skin",
                                        Skeleton = new Grendgine_Collada_Skeleton[]
                                        {
                                            new Grendgine_Collada_Skeleton()
                                            {
                                                Value = $"#{emo.Skeleton.NodeNames[0]}"
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

        public static Grendgine_Collada_Library_Visual_Scenes EMOtoCollada_Library_Visual_Scenes_Matrix_NODEPTH(EMO emo)
        {
            List<Grendgine_Collada_Node> node_list = new List<Grendgine_Collada_Node>();

            Skeleton sk = emo.Skeleton;
           
            for (int i = 0; i < sk.Nodes.Count; i++)
            {
                Node n = sk.Nodes[i];
                if (n.Parent == -1) //Root nodes
                {
                    Matrix4x4 m = n.NodeMatrix;
                    string name = sk.NodeNames[i];

                    Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz,
                                                out float rx, out float ry, out float rz,
                                                out float sx, out float sy, out float sz);

                    node_list.Add(new Grendgine_Collada_Node()
                    {
                        ID = name,
                        Name = name,
                        sID = name,
                        Type = Grendgine_Collada_Node_Type.JOINT,
                        node = RecursiveNode(sk, i),
                        Matrix = new Grendgine_Collada_Matrix[]
                        {
                            new Grendgine_Collada_Matrix()
                            {
                                sID = "matrix",
                                Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} "
                            }
                        },
                    });
                }
            }

            Grendgine_Collada_Node[] RecursiveNode(Skeleton s, int n)
            {
                List<Grendgine_Collada_Node> child_list = new List<Grendgine_Collada_Node>();

                List<Node> temp_nodes = new List<Node>();

                for (int i = 0; i < sk.Nodes.Count; i++)
                {
                    Node temp_node = sk.Nodes[i];

                    if (temp_node.Parent != -1) temp_node.Parent = 0;

                    temp_nodes.Add(temp_node);
                }


                for (int i = 0; i < s.Nodes.Count; i++)
                {
                    if (temp_nodes[i].Parent == n)
                    {
                        Node node = temp_nodes[i];
                        Matrix4x4 m = node.NodeMatrix;
                        string name = sk.NodeNames[i];

                        Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz,
                                                    out float rx, out float ry, out float rz,
                                                    out float sx, out float sy, out float sz);

                        Matrix4x4.Invert(node.SkinBindPoseMatrix, out m);

                        child_list.Add(new Grendgine_Collada_Node()
                        {
                            ID = name,
                            Name = name,
                            sID = name,
                            Type = Grendgine_Collada_Node_Type.JOINT,
                            node = RecursiveNode(sk, i),
                            Matrix = new Grendgine_Collada_Matrix[]
                            {
                                new Grendgine_Collada_Matrix()
                                {
                                    sID = "matrix",
                                    Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} "
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
                            node_list[0],
                            new Grendgine_Collada_Node()
                            {
                                ID = emo.Name,
                                Name = emo.Name,
                                Type = Grendgine_Collada_Node_Type.NODE,
                                Matrix = new Grendgine_Collada_Matrix[]
                                {
                                    new Grendgine_Collada_Matrix()
                                    {
                                        sID = "transform",
                                        Value_As_String = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"
                                    }
                                },
                                Instance_Controller = new Grendgine_Collada_Instance_Controller[]
                                {
                                    new Grendgine_Collada_Instance_Controller()
                                    {
                                        URL = $"#{emo.Name}-skin",
                                        Skeleton = new Grendgine_Collada_Skeleton[]
                                        {
                                            new Grendgine_Collada_Skeleton()
                                            {
                                                Value = $"#{emo.Skeleton.NodeNames[0]}"
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


        public static Grendgine_Collada_Library_Visual_Scenes LEGACY_EMOtoCollada_Library_Visual_Scenes(EMO emo)
        {
            List<Grendgine_Collada_Node> node_list = new List<Grendgine_Collada_Node>();

            Skeleton sk = emo.Skeleton;

            for (int i = 0; i < sk.Nodes.Count; i++)
            {
                Node n = sk.Nodes[i];
                if (n.Parent == -1) //Root nodes
                {
                    Matrix4x4 m = n.NodeMatrix;
                    string name = sk.NodeNames[i];

                    Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz,
                                                out float rx, out float ry, out float rz,
                                                out float sx, out float sy, out float sz);

                    node_list.Add(new Grendgine_Collada_Node()
                    {
                        ID = name,
                        Name = name,
                        sID = name,
                        Type = Grendgine_Collada_Node_Type.JOINT,
                        node = RecursiveNode(sk, i),
                        Translate = new Grendgine_Collada_Translate[]
                        {
                            new Grendgine_Collada_Translate()
                            {
                                sID = "location",
                                Value_As_String = $"{tx.ToString("0.000000")} {(ty).ToString("0.000000")} {tz.ToString("0.000000")}"
                            }
                        },
                        Rotate = new Grendgine_Collada_Rotate[]
                        {
                            new Grendgine_Collada_Rotate()
                            {
                                sID = "rotationZ",
                                Value_As_String = $"0 0 1 {rz.ToString("0.000000")}"
                            },
                            new Grendgine_Collada_Rotate()
                            {
                                sID = "rotationY",
                                Value_As_String = $"0 1 0 {(ry).ToString("0.000000")}"
                            },
                            new Grendgine_Collada_Rotate()
                            {
                                sID = "rotationX",
                                Value_As_String = $"1 0 0 {rx.ToString("0.000000")}"
                            },
                        },
                        Scale = new Grendgine_Collada_Scale[]
                        {
                            new Grendgine_Collada_Scale()
                            {
                                sID = "scale",
                                Value_As_String = $"{sx.ToString("0.000000")} {(sy).ToString("0.000000")} {sz.ToString("0.000000")}"
                            }
                        },
                    });
                }
            }

            Grendgine_Collada_Node[] RecursiveNode(Skeleton s, int n)
            {
                List<Grendgine_Collada_Node> child_list = new List<Grendgine_Collada_Node>();
                for (int i = 0; i < s.Nodes.Count; i++)
                {
                    if (s.Nodes[i].Parent == n)
                    {
                        Node node = sk.Nodes[i];
                        Matrix4x4 m = node.NodeMatrix;
                        string name = sk.NodeNames[i];

                        Utils.DecomposeMatrixToDegrees(m, out float tx, out float ty, out float tz,
                                                    out float rx, out float ry, out float rz,
                                                    out float sx, out float sy, out float sz);

                        child_list.Add(new Grendgine_Collada_Node()
                        {
                            ID = name,
                            Name = name,
                            sID = name,
                            Type = Grendgine_Collada_Node_Type.JOINT,
                            node = RecursiveNode(s, i),
                            Translate = new Grendgine_Collada_Translate[]
                            {
                                new Grendgine_Collada_Translate()
                                {
                                    sID = "location",
                                    Value_As_String = $"{tx.ToString("0.000000")} {(ty).ToString("0.000000")} {tz.ToString("0.000000")}"
                                }
                            },
                                Rotate = new Grendgine_Collada_Rotate[]
                            {
                                new Grendgine_Collada_Rotate()
                                {
                                    sID = "rotationZ",
                                    Value_As_String = $"0 0 1 {rz.ToString("0.000000")}"
                                },
                                new Grendgine_Collada_Rotate()
                                {
                                    sID = "rotationY",
                                    Value_As_String = $"0 1 0 {ry.ToString("0.000000")}"
                                },
                                new Grendgine_Collada_Rotate()
                                {
                                    sID = "rotationX",
                                    Value_As_String = $"1 0 0 {rx.ToString("0.000000")}"
                                },
                            },
                                Scale = new Grendgine_Collada_Scale[]
                            {
                                new Grendgine_Collada_Scale()
                                {
                                    sID = "scale",
                                    Value_As_String = $"{sx.ToString("0.000000")} {(sy).ToString("0.000000")} {sz.ToString("0.000000")}"
                                }
                            },
                            //Matrix = new Grendgine_Collada_Matrix[]
                            //{
                            //    new Grendgine_Collada_Matrix() //Might need to switch columns/rows
                            //    {
                            //        sID = "transform",
                            //        Value_As_String = $"{m.M11} {m.M21} {m.M31} {m.M41} {m.M12} {m.M22} {m.M32} {m.M42} {m.M13} {m.M23} {m.M33} {m.M43} {m.M14} {m.M24} {m.M34} {m.M44} "
                            //    }
                            //}
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
                            node_list[0],
                            new Grendgine_Collada_Node()
                            {
                                ID = emo.Name,
                                Name = emo.Name,
                                Type = Grendgine_Collada_Node_Type.NODE,
                                Matrix = new Grendgine_Collada_Matrix[]
                                {
                                    new Grendgine_Collada_Matrix()
                                    {
                                        sID = "transform",
                                        Value_As_String = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"
                                    }
                                },
                                Instance_Controller = new Grendgine_Collada_Instance_Controller[]
                                {
                                    new Grendgine_Collada_Instance_Controller()
                                    {
                                        URL = $"#{emo.Name}-skin",
                                        Skeleton = new Grendgine_Collada_Skeleton[]
                                        {
                                            new Grendgine_Collada_Skeleton()
                                            {
                                                Value = $"#{emo.Skeleton.NodeNames[0]}"
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

        public static Grendgine_Collada_Library_Animations LEGACY_EMAtoCollada_Library_Animations(EMA ema)
        {
            List<Grendgine_Collada_Animation> temp_anims = new List<Grendgine_Collada_Animation>();

            Animation a = ema.Animations[1];

            //foreach (Animation a in ema.Animations)
            {
                foreach (CMDTrack c in a.CMDTracks)
                {

                    string bonename = ema.Skeleton.NodeNames[c.BoneID];
                    string type;
                    string TYPE;
                    string axis;
                    string target;
                    string time_vals = string.Empty;
                    string trans_vals = string.Empty;
                    string tangent_vals = string.Empty;
                    string bezier_in_vals = string.Empty;
                    string bezier_out_vals = string.Empty;
                    string interp_vals = string.Empty; //In SF4 anims, intangent == outtangent so we only need one list of tangent vals

                    if ((c.BitFlag & 0x10) == 0x10)
                    {
                        Console.WriteLine($"Zettai! bID = {c.BoneID}, {bonename}");
                    }

                    if (ema.Skeleton.Nodes[c.BoneID].Sibling != -1)
                    {
                        Console.WriteLine($"Sibling! This = {bonename}, sibling = {ema.Skeleton.Nodes[c.BoneID].Sibling}, {ema.Skeleton.NodeNames[ema.Skeleton.Nodes[c.BoneID].Sibling]}");
                    }

                    if ((c.BitFlag & 0x03) == 0) axis = "X";
                    else if ((c.BitFlag & 0x03) == 1) axis = "Y";
                    else axis = "Z";

                    if (c.TransformType == 0)
                    {
                        type = "translation";
                        TYPE = axis;
                        target = $"{bonename}/location.{axis}";
                    }
                    else if (c.TransformType == 1)
                    {
                        type = "rotation";
                        TYPE = "ANGLE";
                        target = $"{bonename}/rotation{axis}.ANGLE";
                    }
                    else
                    {
                        type = "scale";
                        TYPE = axis;
                        target = $"{bonename}/scale.{axis}";
                    }

                    string name = $"Armature_{ema.Skeleton.NodeNames[c.BoneID]}_{type}-{axis}";

                    foreach (int t in c.StepsList)
                    {
                        time_vals += $"{Convert.ToSingle(t) / 60f} ";
                        interp_vals += "BEZIER ";
                    }

                    for (int i = 0; i < c.StepsList.Count; i++)
                    {
                        int masked_index;
                        int masked_tangent;
                        float bezier_in;
                        float bezier_out;

                        if ((c.BitFlag & 0x40) == 0)
                        {
                            masked_index = c.IndicesList[i] & 0b0011111111111111;
                            masked_tangent = (c.IndicesList[i] >> 14);

                            if (masked_tangent == -2) masked_tangent = 2;
                        }
                        else
                        {
                            masked_index = c.IndicesList[i] & 0b00111111111111111111111111111111;
                            masked_tangent = c.IndicesList[i] >> 30;

                            if (masked_tangent == -2) masked_tangent = 2;
                        }

                        trans_vals += $"{a.ValuesList[masked_index]} ";


                        if (masked_tangent != 0)
                        {
                            bezier_in = a.ValuesList[masked_index] - a.ValuesList[masked_index + masked_tangent] / 3f;
                            bezier_out = a.ValuesList[masked_index] + a.ValuesList[masked_index + masked_tangent] / 3f;
                            //tangent_vals += $"0 {a.ValuesList[masked_index + masked_tangent]} ";
                            bezier_in_vals += $"0 {bezier_in} ";
                            bezier_out_vals += $"0 {bezier_out} ";
                        }
                        else
                        {
                            bezier_in_vals += "0 0 ";
                            bezier_out_vals += "0 0 ";

                            //tangent_vals += "0 0 ";
                        }
                    }

                    temp_anims.Add(new Grendgine_Collada_Animation()
                    {
                        ID = name,
                        Source = new Grendgine_Collada_Source[]
                        {
                            //INPUT ARRAY (TIME IN SECONDS)
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-input",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-input-array",
                                    Count = c.StepsList.Count,
                                    Value_As_String = time_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-input-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "TIME",
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            //OUTPUT ARRAY (TRANSFORM VALUE)
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-output",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-output-array",
                                    Count = c.StepsList.Count,
                                    Value_As_String = trans_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-output-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = TYPE, //NEEDS TO BE THE TRANSFORM TYPE PROBABLY
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-intangents",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-intangents-array",
                                    Count = 2 * c.StepsList.Count,
                                    Value_As_String = bezier_in_vals,
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                //<source id="Armature_Chest_ArmatureAction___Chest___rotation_euler_X-intangent">
                                //  <float_array id="Armature_Chest_ArmatureAction___Chest___rotation_euler_X-intangent-array" count="8">0.1388888 -0.08885568 0.3472222 -0.006967127 0.5555555 -0.06276804 0.7638889 -0.003921687</float_array>
                                //  <technique_common>
                                //    <accessor source="#Armature_Chest_ArmatureAction___Chest___rotation_euler_X-intangent-array" count="4" stride="2">
                                //      <param name="X" type="float"/>
                                //      <param name="Y" type="float"/>
                                //    </accessor>
                                //  </technique_common>
                                //</source>

                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-intangents-array",
                                        Count = (uint)(2 * c.StepsList.Count),
                                        Stride = 2,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "IN_TANGENT", //NEEDS TO BE THE TRANSFORM TYPE PROBABLY
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-outtangents",
                                Float_Array = new Grendgine_Collada_Float_Array()
                                {
                                    ID = $"{name}-outtangents-array",
                                    Count = c.StepsList.Count,
                                    Value_As_String = bezier_out_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-outtangents-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "OUT_TANGENT", //NEEDS TO BE THE TRANSFORM TYPE PROBABLY
                                                Type = "float"
                                            }
                                        }
                                    }
                                }
                            },
                            //INTERPOLATION METHOD (BEZIER)
                            new Grendgine_Collada_Source()
                            {
                                ID = $"{name}-interpolation",
                                Name_Array = new Grendgine_Collada_Name_Array()
                                {
                                    ID = $"{name}-interpolation-array",
                                    Count = c.StepsList.Count,
                                    Value_Pre_Parse = interp_vals
                                },
                                Technique_Common = new Grendgine_Collada_Technique_Common_Source()
                                {
                                    Accessor = new Grendgine_Collada_Accessor()
                                    {
                                        Source = $"#{name}-interpolation-array",
                                        Count = (uint)c.StepsList.Count,
                                        Stride = 1,
                                        Param = new Grendgine_Collada_Param[]
                                        {
                                            new Grendgine_Collada_Param()
                                            {
                                                Name = "INTERPOLATION",
                                                Type = "name"
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Sampler = new Grendgine_Collada_Sampler[]
                        {
                            new Grendgine_Collada_Sampler()
                            {
                                ID = $"{name}-sampler",
                                Input = new Grendgine_Collada_Input_Unshared[]
                                {
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.INPUT,
                                        source = $"#{name}-input"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.OUTPUT,
                                        source = $"#{name}-output"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.IN_TANGENT,
                                        source = $"#{name}-intangents"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.OUT_TANGENT,
                                        source = $"#{name}-outtangents"
                                    },
                                    new Grendgine_Collada_Input_Unshared()
                                    {
                                        Semantic = Grendgine_Collada_Input_Semantic.INTERPOLATION,
                                        source = $"#{name}-interpolation"
                                    }
                                }
                            }
                        },
                        Channel = new Grendgine_Collada_Channel[]
                        {
                            new Grendgine_Collada_Channel()
                            {
                                Source = $"#{name}-sampler",
                                Target = target
                            }
                        }
                    });
                }
            }

            Grendgine_Collada_Library_Animations library_animations = new Grendgine_Collada_Library_Animations()
            {
                Animation = temp_anims.ToArray()
            };

            return library_animations;
        }
    }
}