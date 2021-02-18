using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USF4_Stage_Tool
{
    public static class Anim
    {
        public static Skeleton DuplicateSkeleton(Skeleton skel, int first, int last)
        {
            //Skip the root node, copy everything else
            int adjnodecount = (last - first)+1;

            //Fix polymsh1 sibling
            Node polymsh1 = skel.Nodes[21];
            polymsh1.Sibling = 22;
            skel.Nodes.RemoveAt(21);
            skel.Nodes.Add(polymsh1);
            //Loop over nodes, ignoring the root node
            for (int i = first; i <= last; i++)
            {
                Node nNode = skel.Nodes[i];

                if (nNode.Parent > 0) { nNode.Parent += (adjnodecount + 1); }
                if (nNode.Sibling != -1 && nNode.Sibling <= last) { nNode.Sibling += adjnodecount; }
                if (nNode.Child1 != -1 && nNode.Child1 <= last) { nNode.Child1 += adjnodecount; }
                if (nNode.Child3 != -1 && nNode.Child3 <= last) { nNode.Child3 += adjnodecount; }
                if (nNode.Child4 != -1 && nNode.Child4 <= last) { nNode.Child4 += adjnodecount; }
                if (nNode.Child1 == 41) { nNode.Child1 = -1; }
                    
                //Node updated, add it to the list.
                skel.Nodes.Add(nNode);
                skel.NodeCount++;
                skel.NodeNameIndex.Add(0x00);
                skel.NodeNames.Add(skel.NodeNames[i]);
                skel.FFList.Add(skel.FFList[i]);
                
            }

            return skel;
        }
        
        public static Animation DuplicateAnimation(Animation anim, int first, int last)
        {
            int initial_cmdcount = anim.CmdTrackCount;
            int adjnodecount = (last - first) + 2;

            for (int i = 0; i < initial_cmdcount; i++)
            {
                CMDTrack cmd = new CMDTrack();

                cmd.BitFlag = anim.CMDTracks[i].BitFlag;
                cmd.BoneID = anim.CMDTracks[i].BoneID;
                cmd.IndiceList = new List<int> (anim.CMDTracks[i].IndiceList);
                cmd.IndiceListPointer = anim.CMDTracks[i].IndiceListPointer;
                cmd.StepCount = anim.CMDTracks[i].StepCount;
                cmd.StepList = new List<int>(anim.CMDTracks[i].StepList);
                cmd.TransformType = anim.CMDTracks[i].TransformType;

                if (cmd.BoneID >= first)
                {
                    cmd.BoneID += adjnodecount;
                    if(cmd.BoneID == 22 && cmd.TransformType == 0 && (cmd.BitFlag & 0x03) == 0)
                    {
                        cmd.BitFlag = Convert.ToByte((cmd.BitFlag | 0x40));
                        int Steps = cmd.StepCount;
                        for(int j = 0; j < Steps; j++)
                        {
                            anim.ValueList.Add(anim.ValueList[cmd.IndiceList[j]] + 2f);
                            anim.ValueCount++;
                            cmd.IndiceList[j] = anim.ValueList.Count;
                        }
                    }
                    //else if (cmd.TransformType == 0 && (cmd.BitFlag & 0x03) == 0 && (cmd.BitFlag & 0x10) == 1)
                    //{
                    //    cmd.BitFlag = Convert.ToByte((cmd.BitFlag | 0x20));
                    //    int Steps = cmd.StepCount;
                    //    for (int j = 0; j < Steps; j++)
                    //    {
                    //        anim.ValueList.Add(anim.ValueList[cmd.StepList[j]] + 1);
                    //        anim.ValueCount++;
                    //        cmd.StepList[j] = anim.ValueList.Count;
                    //    }
                    //}
                    anim.CMDTracks.Add(cmd);
                    anim.CmdTrackCount++;
                    anim.CmdTrackPointerList.Add(0x00);
                }
            }

            return anim;
        }
    }

    
}