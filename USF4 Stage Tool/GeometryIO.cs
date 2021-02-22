using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USF4_Stage_Tool
{
    public static class GeometryIO
    {
        public static List<int> DaisyChainFromIndices(List<int[]> nIndices)
        {
            List<int> Chain = new List<int>();
            bool bForwards = false;
            int count = nIndices.Count;

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
                //TODO -X?
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
            if(model.ReadMode == 0) { readmode = true; }

            if(writeverts) //If we're using this as a sub-call of ModeltoOBJ, we don't want verts writing
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

            if(invert_indices)
            {
                for(int i = 0; i < indices.Count; i++)
                {
                    for(int j = 0; j < indices[i].Length; j++)
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
                if(invert_indices) lines.Add($"f {indices[i][0]}/{indices[i][0]}/{indices[i][0]} {indices[i][1]}/{indices[i][1]}/{indices[i][1]} {indices[i][2]}/{indices[i][2]}/{indices[i][2]}");
                else lines.Add($"f {indices[i][0] + 1}/{indices[i][0] + 1}/{indices[i][0] + 1} {indices[i][1] + 1}/{indices[i][1] + 1}/{indices[i][1] + 1} {indices[i][2] + 1}/{indices[i][2] + 1}/{indices[i][2] + 1}");
            }

            return lines;
        }

        public static EMZ UpdateLegacyStage(EMZ emz)
        {
            for (int i = 0; i < emz.Files.Count; i++)
            {
                if(emz.Files[i] is EMO)
                {
                    EMO emo = (EMO)emz.Files[i];
                    for(int j = 0; j < emo.EMGList.Count; j++)
                    {
                        EMG emg = emo.EMGList[j];

                        for(int k = 0; k < emg.Models.Count; k++)
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

    }
}