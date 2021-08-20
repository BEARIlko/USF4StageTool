using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace USF4_Stage_Tool
{
    public static class Anim
    {
        public static SingleAnimationUsingKeyFrames GenerateKeyFrames(Animation anim, int boneID, int axis, int ttype)
        {
            SingleAnimationUsingKeyFrames a = new SingleAnimationUsingKeyFrames()
            {
                Duration = new TimeSpan(166667 * anim.Duration)
            };

            foreach (CMDTrack c in anim.CMDTracks)
            {
                if (c.BoneID == boneID && (c.BitFlag & axis) == axis && c.TransformType == ttype)
                {
                    for (int i = 0; i < c.StepsList.Count; i++)
                    {
                        a.KeyFrames.Add(new LinearSingleKeyFrame()
                        {
                            KeyTime = new TimeSpan(c.StepsList[i] * 166667),
                            Value = anim.ValuesList[(c.IndicesList[i] & 0b0011111111111111)]
                        });
                    }
                }
            }

            return a;
        }

        public static void InterpolateRelativeKeyFrames(Animation anim, int boneID, int frame, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz)
        {
            //Declare defaults for if a value isn't animated
            tx = ty = tz = 0;
            rx = ry = rz = 0;
            sx = sy = sz = 0;

            foreach (CMDTrack c in anim.CMDTracks)
            {
                if (c.BoneID == boneID)
                {
                    if ((c.BitFlag & 0x10) == 0x10) continue;
                    //throw new ArgumentException($"Bone {boneID}: Absolute animation tracks cannot be parsed by this function.");

                    for (int i = 0; i < c.StepsList.Count; i++)
                    {
                        if (c.StepsList[i] > frame)
                        {
                            int f_end = c.StepsList[i];
                            int f_initial = c.StepsList[i - 1];
                            float v_end = anim.ValuesList[(c.IndicesList[i] & 0b0011111111111111)];
                            float v_initial = anim.ValuesList[(c.IndicesList[i - 1] & 0b0011111111111111)];

                            float v_current = v_initial + frame * ((v_end - v_initial) / ((float)f_end - (float)f_initial));

                            if ((c.BitFlag & 0x03) == 0x00)
                            {
                                if (c.TransformType == 0) tx = v_current;
                                else if (c.TransformType == 1) rx = v_current;
                                else if (c.TransformType == 2) sx = v_current;
                            }
                            else if ((c.BitFlag & 0x03) == 0x01)
                            {
                                if (c.TransformType == 0) ty = v_current;
                                else if (c.TransformType == 1) ry = v_current;
                                else if (c.TransformType == 2) sy = v_current;
                            }
                            else if ((c.BitFlag & 0x03) == 0x02)
                            {
                                if (c.TransformType == 0) tz = v_current;
                                else if (c.TransformType == 1) rz = v_current;
                                else if (c.TransformType == 2) sz = v_current;
                            }

                            break;
                        }
                        else if (c.StepsList[i] == frame)
                        {
                            float v_current = anim.ValuesList[(c.IndicesList[i] & 0b0011111111111111)];

                            if ((c.BitFlag & 0x03) == 0x00)
                            {
                                if (c.TransformType == 0) tx = v_current;
                                else if (c.TransformType == 1) rx = v_current;
                                else if (c.TransformType == 2) sx = v_current;
                            }
                            else if ((c.BitFlag & 0x03) == 0x01)
                            {
                                if (c.TransformType == 0) ty = v_current;
                                else if (c.TransformType == 1) ry = v_current;
                                else if (c.TransformType == 2) sy = v_current;
                            }
                            else if ((c.BitFlag & 0x03) == 0x02)
                            {
                                if (c.TransformType == 0) tz = v_current;
                                else if (c.TransformType == 1) rz = v_current;
                                else if (c.TransformType == 2) sz = v_current;
                            }

                            break;
                        }
                    }
                }
            }
        }

        private static float HermiteInterpolation(float P1, float T1, float P2, float T2, float s)
        {
            float s2 = s * s;
            float s3 = s * s * s;
            float h1 = 2 * s3 - 3 * s2 + 1;          // calculate basis function 1
            float h2 = -2 * s3 + 3 * s2;              // calculate basis function 2
            float h3 = s3 - 2 * s2 + s;          // calculate basis function 3
            float h4 = s3 - s2;              // calculate basis function 4

            return h1 * P1 +                    // multiply and sum all funtions
                    h2 * P2 +                    // together to build the interpolated
                    h3 * T1 +                    // point along the curve.
                    h4 * T2;
        }

        public static Matrix4x4 InterpolateParentKeyFramesRecursive(EMA ema, Animation anim, int bone_index, int frame)
        {
            CMDTrack[] CMDtracks = new CMDTrack[9];
            float[] values = new float[9];

            Node n = ema.Skeleton.Nodes[bone_index];

            Matrix4x4 parenttransform = Matrix4x4.Identity;

            //Think we don't include the current node?
            List<int> q = new List<int>();// { bone_index };

            //Find all the nodes in the chain
            while (n.Parent != -1)
            {
                q.Add(n.Parent);
                n = ema.Skeleton.Nodes[n.Parent];
            }
            while (q.Count > 0)
            {
                parenttransform = InterpolateKeyFramesAsMatrices(ema, anim, q.Last(), frame) * parenttransform;

                q.RemoveAt(q.Count - 1);
            }

            return parenttransform;
        }

        public static Matrix4x4 InterpolateKeyFramesAsMatrices(EMA ema, Animation anim, int bone_index, int frame)
        {
            CMDTrack[] AbsoluteCMDTracks = new CMDTrack[9];
            CMDTrack[] CMDtracks = new CMDTrack[9];
            float[] absolutevalues = new float[9];
            float[] values = new float[9];

            bool absolute_translation = false;
            bool absolute_rotation = false;

            //ABSOLUTE TRANSLATION TESTING
            Matrix4x4 Transform_World = Matrix4x4.CreateTranslation(10, 0, 0);
            Matrix4x4 Transform_Local = Matrix4x4.CreateRotationX((float)(Math.PI / 2d));
            //Matrix4x4 Transform_Local = Matrix4x4.CreateTranslation(1, 0, 0);

            //Bind matrix simulates our target node being parented to a node at (1,0,0) w/ 90deg rotation around Y
            Matrix4x4 BindMatrix = Matrix4x4.CreateRotationY((float)(Math.PI / 2d)) * Matrix4x4.CreateTranslation(1, 0, 2);
            Matrix4x4.Invert(BindMatrix, out Matrix4x4 InverseBindMatrix);

            Matrix4x4 Identity = InverseBindMatrix * BindMatrix;
            Matrix4x4 Identity2 = BindMatrix * InverseBindMatrix;

            //Final transform...
            Matrix4x4 FinalMatrix = Transform_World * InverseBindMatrix * Transform_Local;

            //If all transforms are absolute, it's simple...
            //We just do inverse parent matrix * abs_scale * abs_rot * abs_trans

            //ABSOLUTE ROTATION TESTING
            //Relative scale, absolute rotation, relative translation...
            //scale * (absolute rotation * INVERSE(BindMatrix * Scale)) * translation
            Matrix4x4 ABS_Rotation = Matrix4x4.CreateRotationY((float)(Math.PI / 4d));
            Matrix4x4 REL_Scale = Matrix4x4.CreateScale(1.1f, 1.1f, 1.1f);
            Matrix4x4 PxS_BindMatrix = BindMatrix * REL_Scale;
            Matrix4x4.Invert(PxS_BindMatrix, out Matrix4x4 PxS_Inverse);

            //TRANSFORM THE RELATIVE COMPONENTS INTO THEIR EQUIVALENT WORLD-SPACE COMPONENTS
            //WORK ENTIRELY IN WORLD SPACE, THEN CONVERT THE WHOLE WORLD-TRANSFORM BACK INTO LOCAL SPACE

            //[BINDMATRIX] * REL_Scale * REL_Rot * REL_Tra == [NEW BIND MATRIX]
            //FINAL MATRIX = ABS_Scale * ABS_Rot * ABS_tra * INV[NEW BIND MATRIX]

            Matrix4x4 FinalMatrix2 = BindMatrix * PxS_Inverse * ABS_Rotation * REL_Scale;

            //Relative scale, absolute rotation, absolute translation...
            //scale * ((absolute rotation * INVERSE(BindMatrix * Scale)) 


            Utils.DecomposeMatrixToDegrees(FinalMatrix2, out float stx, out float sty, out float stz, out float srx, out float sry, out float srz, out float ssx, out float ssy, out float ssz);

            //Get the bone's parent transform
            Matrix4x4 AbsoluteMatrix = Matrix4x4.Identity;
            Matrix4x4 AbsoluteInverseMatrix = Matrix4x4.Identity;

            Matrix4x4 AbsoluteTranslation = Matrix4x4.Identity;
            Matrix4x4 AbsoluteRotation = Matrix4x4.Identity;

            //Gather all relevant CMDTracks...
            foreach (CMDTrack c in anim.CMDTracks)
            {
                //Skip abs scale values
                if ((c.BitFlag & 0x10) == 0x10 && c.TransformType == 2) continue;

                int ttype = c.TransformType * 3 + (c.BitFlag & 0x03);

                //Absolute translations...
                if (c.BoneID == bone_index && (c.BitFlag & 0x10) == 0x10)
                {
                    AbsoluteCMDTracks[ttype] = c;
                    absolutevalues[ttype] = InterpolateRelativeKeyFrames(anim, c, frame);

                    if (c.TransformType == 0) absolute_translation = true;
                    if (c.TransformType == 1) absolute_rotation = true;
                }
                else if (c.BoneID == bone_index)
                {
                    CMDtracks[ttype] = c;

                    values[ttype] = InterpolateRelativeKeyFrames(anim, c, frame);
                }
            }

            Utils.DecomposeMatrixToDegrees(ema.Skeleton.Nodes[bone_index].NodeMatrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz);

            sx = 1;
            sy = 1;
            sz = 1;

            if (CMDtracks[0].IndicesList != null) tx += values[0];
            if (CMDtracks[1].IndicesList != null) ty += values[1];
            if (CMDtracks[2].IndicesList != null) tz += values[2];

            if (CMDtracks[3].IndicesList != null) rx = values[3];
            if (CMDtracks[4].IndicesList != null) ry = values[4];
            if (CMDtracks[5].IndicesList != null) rz = values[5];

            if (CMDtracks[6].IndicesList != null) sx = values[6];
            if (CMDtracks[7].IndicesList != null) sy = values[7];
            if (CMDtracks[8].IndicesList != null) sz = values[8];

            if (absolute_translation || absolute_rotation)
            {
                AbsoluteMatrix = InterpolateParentKeyFramesRecursive(ema, anim, bone_index, frame);
                Matrix4x4.Invert(AbsoluteMatrix, out AbsoluteInverseMatrix);
            }
            if (absolute_translation)
            {
                AbsoluteTranslation = AbsoluteInverseMatrix * Matrix4x4.CreateTranslation(absolutevalues[0], absolutevalues[1], absolutevalues[2]);
            }
            if (absolute_rotation)
            {
                AbsoluteRotation = AbsoluteInverseMatrix * (Matrix4x4.CreateRotationX(absolutevalues[3] * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationY(absolutevalues[4] * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationZ(absolutevalues[5] * (float)(Math.PI / 180d)));
            }
            Matrix4x4 translate = Matrix4x4.CreateTranslation(tx, ty, tz);
            Matrix4x4 rotate = Matrix4x4.CreateRotationX(rx * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationY(ry * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationZ(rz * (float)(Math.PI / 180d));
            Matrix4x4 scale = Matrix4x4.CreateScale(sx, sy, sz);

            //This just checks that we can get back our TRS values from the matrix
            //Utils.DecomposeMatrixToDegrees(scale * rotate * translate, out tx, out ty, out tz, out rx, out ry, out rz, out sx, out sy, out sz);

            //Apply the inverse of the parent matrix => point is now at world origin
            //Apply Absolute matrices => 
            //Apply the local rotation and scale
            //Apply the global 

            //return AbsoluteInverseMatrix * AbsoluteRotation * AbsoluteTranslation * scale * rotate * translate;
            return scale * rotate * translate * AbsoluteRotation * AbsoluteTranslation;
        }

        public static Matrix4x4 InterpolateKeyFramesAsMatrices_NODEPTH(EMA ema, Animation anim, int bone_index, int frame)
        {
            CMDTrack[] AbsoluteCMDTracks = new CMDTrack[9];
            CMDTrack[] CMDtracks = new CMDTrack[9];
            float[] absolutevalues = new float[9];
            float[] values = new float[9];

            bool absolute_translation = false;
            bool absolute_rotation = false;
            bool absolute_scale = false;

            bool animated = false;

            //Get the bone's parent transform
            Matrix4x4 AbsoluteMatrix = Matrix4x4.Identity;
            Matrix4x4 AbsoluteInverseMatrix = Matrix4x4.Identity;

            Matrix4x4 AbsoluteTranslation = Matrix4x4.Identity;
            Matrix4x4 AbsoluteRotation = Matrix4x4.Identity;
            Matrix4x4 AbsoluteScale = Matrix4x4.Identity;

            //Gather all relevant CMDTracks...
            foreach (CMDTrack c in anim.CMDTracks)
            {
                //Skip abs scale values
                //if ((c.BitFlag & 0x10) == 0x10 && c.TransformType == 2) continue;

                int ttype = c.TransformType * 3 + (c.BitFlag & 0x03);

                //Absolute translations...
                if (c.BoneID == bone_index && (c.BitFlag & 0x10) == 0x10)
                {
                    animated = true;

                    AbsoluteCMDTracks[ttype] = c;
                    absolutevalues[ttype] = InterpolateRelativeKeyFrames(anim, c, frame);

                    if (c.TransformType == 0) absolute_translation = true;
                    if (c.TransformType == 1) absolute_rotation = true;
                    if (c.TransformType == 2) absolute_scale = true;
                }
                else if (c.BoneID == bone_index)
                {
                    animated = true;
                    CMDtracks[ttype] = c;

                    values[ttype] = InterpolateRelativeKeyFrames(anim, c, frame);
                }
            }

            if (!animated) return ema.Skeleton.Nodes[bone_index].NodeMatrix;

            Utils.DecomposeMatrixToDegrees(ema.Skeleton.Nodes[bone_index].NodeMatrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz);

            if (CMDtracks[0].IndicesList != null) tx += values[0];
            if (CMDtracks[1].IndicesList != null) ty += values[1];
            if (CMDtracks[2].IndicesList != null) tz += values[2];

            if (CMDtracks[3].IndicesList != null) rx = values[3];
            if (CMDtracks[4].IndicesList != null) ry = values[4];
            if (CMDtracks[5].IndicesList != null) rz = values[5];

            if (CMDtracks[6].IndicesList != null) sx = values[6];
            if (CMDtracks[7].IndicesList != null) sy = values[7];
            if (CMDtracks[8].IndicesList != null) sz = values[8];

            AbsoluteMatrix = InterpolateParentKeyFramesRecursive(ema, anim, bone_index, frame);

            if (absolute_translation)
            {
                //AbsoluteTranslation = Matrix4x4.CreateTranslation(tx + absolutevalues[0], ty + absolutevalues[1], tz + absolutevalues[2]);
                AbsoluteTranslation = Matrix4x4.CreateTranslation(absolutevalues[0], absolutevalues[1], absolutevalues[2]);
            }
            if (absolute_rotation)
            {
                AbsoluteRotation = Matrix4x4.CreateRotationX(absolutevalues[3] * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationY(absolutevalues[4] * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationZ(absolutevalues[5] * (float)(Math.PI / 180d));
            }
            if (absolute_scale)
            {
                AbsoluteScale = Matrix4x4.CreateScale(absolutevalues[6], absolutevalues[7], absolutevalues[8]);
            }
            Matrix4x4 translate = Matrix4x4.CreateTranslation(tx, ty, tz);
            Matrix4x4 rotate = Matrix4x4.CreateRotationX(rx * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationY(ry * (float)(Math.PI / 180d)) * Matrix4x4.CreateRotationZ(rz * (float)(Math.PI / 180d));
            //Matrix4x4 scale = Matrix4x4.CreateScale(sx, sy, sz);
            Matrix4x4 scale = Matrix4x4.CreateScale(1, 1, 1);

            Matrix4x4.Invert(AbsoluteMatrix, out AbsoluteInverseMatrix);

            if (absolute_translation) translate = Matrix4x4.Identity;
            if (absolute_rotation) rotate = Matrix4x4.Identity;
            //if (absolute_scale) scale = Matrix4x4.Identity;
            if (absolute_translation || absolute_rotation) AbsoluteMatrix = Matrix4x4.Identity;

            if (!absolute_translation && !absolute_rotation && !absolute_scale) return scale * rotate * translate;

            return scale * rotate * translate * AbsoluteMatrix * AbsoluteRotation * AbsoluteTranslation * AbsoluteInverseMatrix;
            //return AbsoluteScale * AbsoluteRotation * AbsoluteTranslation * AbsoluteInverseMatrix;
        }

        /*Code blatantly stolen from AE starts here*/
        static Vector4 var_xmm0;
        static Vector4 var_xmm1;
        static Vector4 var_xmm2;
        static Vector4 var_xmm3;
        static Vector4 var_xmm4;

        static Matrix4x4 MatrixCreate(Matrix4x4 pM, Vector4 pV1, Vector4 pV2, Vector4 pV3, Vector4 pV4)
        {
            //
            pM.M11 = pV1.X;
            pM.M12 = pV1.Y;
            pM.M13 = pV1.Z;
            pM.M14 = pV1.W;

            //
            pM.M21 = pV2.X;
            pM.M22 = pV2.Y;
            pM.M23 = pV2.Z;
            pM.M24 = pV2.W;

            //
            pM.M31 = pV3.X;
            pM.M32 = pV3.Y;
            pM.M33 = pV3.Z;
            pM.M34 = pV3.W;

            //
            pM.M41 = pV4.X;
            pM.M42 = pV4.Y;
            pM.M43 = pV4.Z;
            pM.M44 = pV4.W;

            return pM;
        }

        //This looks to be our frame entry point
        public static void updateDeviceObjects(AnimatedSkeleton s)
        {

        }

        public static void SetupFrame(AnimatedSkeleton s, Animation anim, float frame)
        {
            for (int i = 0; i < s.AnimatedNodes.Count; i++)
            {
                AnimatedNode node = s.AnimatedNodes[i];

                node.animationProcessingDone = false;
                node.animatedAbsoluteRotationFlag = false;
                node.animatedAbsoluteScaleFlag = false;
                node.animatedAbsoluteTranslationFlag = false;

                node.animatedRotation = node.Rotation;
                node.animatedScale = node.Scale;
                node.animatedTranslation = node.Translation;
                node.animatedRotationQuaternion = node.RotationQuaternion;
                node.animatedMatrix = node.NodeMatrix;


                
            }
        }

        public static Matrix4x4 sub_504330(Matrix4x4 pM_ECX, Vector4 pV1_ESI, Vector4 pV2)
        {
            Vector4 var_60 = new Vector4(0, 0, 0, 0);
            Vector4 var_50 = new Vector4(0, 0, 0, 0);
            Vector4 var_40 = new Vector4(0, 0, 0, 0);
            Vector4 var_30 = new Vector4(0, 0, 0, 0);
            Vector4 var_20 = new Vector4(0, 0, 0, 0);
            Vector4 var_10 = new Vector4(0, 0, 0, 0);

            var_60 = new Vector4(pV1_ESI.Y, pV1_ESI.Z, pV1_ESI.X, 0);

            var_30 = new Vector4(pV1_ESI.Z, pV1_ESI.X, pV1_ESI.Y, 0);

            var_50.Z = 1f - pV2.Y;

            var_10 = new Vector4(pV2.X, pV2.X, pV2.X, pV2.X);

            var_20 = new Vector4(pV2.Y, pV2.Y, pV2.Y, pV2.Y);

            var_50 = new Vector4(var_50.Z, var_50.Z, var_50.Z, var_50.Z);

            var_40 = new Vector4(pV2.X, pV2.X, pV2.X, pV2.X);

            var_xmm0 = Vector4.Multiply(var_50, var_60);

            var_xmm2 = Vector4.Multiply(var_50, pV1_ESI);

            var_xmm0 = Vector4.Multiply(var_xmm0, var_30);

            var_xmm1 = Vector4.Multiply(pV1_ESI, var_xmm2);

            var_xmm1 = Vector4.Add(var_xmm1, var_20);

            var_50 = var_xmm1;
            var_30 = var_xmm0;

            var_xmm1 = Vector4.Multiply(pV1_ESI, var_40);
            var_xmm1 = Vector4.Add(var_xmm1, var_xmm0);

            var_40 = var_xmm1;

            var_xmm0 = Vector4.Multiply(pV1_ESI, var_10);

            var_60 = Vector4.Subtract(var_xmm0, var_30);

            var_60 = -var_60;

            pM_ECX.M11 = var_50.X;
            pM_ECX.M12 = var_40.Z;
            pM_ECX.M13 = var_60.Y;
            pM_ECX.M13 = 0f;

            pM_ECX.M21 = var_60.Z;
            pM_ECX.M22 = var_50.Y;
            pM_ECX.M23 = var_40.X;
            pM_ECX.M24 = 0f;

            pM_ECX.M31 = var_40.Y;
            pM_ECX.M32 = var_60.X;
            pM_ECX.M33 = var_50.Z;
            pM_ECX.M34 = 0f;

            pM_ECX.M41 = 0f;
            pM_ECX.M42 = 0f;
            pM_ECX.M43 = 0f;
            pM_ECX.M44 = 0f;

            return pM_ECX;
        }

        public static void ProcessIKData0x00_00(AnimatedSkeleton s, IKDataBlock ik)
        {

        }

        public static void ProcessIKData0x00_02(AnimatedSkeleton s, IKDataBlock ik)
        {
            AnimatedNode node0 = s.AnimatedNodes[ik.IKShorts[0]];
            AnimatedNode node1 = s.AnimatedNodes[ik.IKShorts[1]];
            AnimatedNode node2 = s.AnimatedNodes[ik.IKShorts[2]];
            AnimatedNode node3 = s.AnimatedNodes[ik.IKShorts[3]];
            AnimatedNode node4 = s.AnimatedNodes[ik.IKShorts[4]];
            AnimatedNode nodeP = s.AnimatedNodes[node0.Parent];

            // sub_524390
            if (1 == 1)
            {
                Vector4 var_F0 = new Vector4(0, 0, 0, 0);
                Vector4 var_D0 = new Vector4(0, 0, 0, 0);
                Vector4 var_C0 = new Vector4(0, 0, 0, 0);
                Vector4 var_B0 = new Vector4(0, 0, 0, 0);
                Vector4 var_A0 = new Vector4(0, 0, 0, 1);

                Vector4 var_140 = new Vector4(0, 0, 0, 0);
                Vector4 var_120 = new Vector4(0, 0, 0, 0);
                Vector4 var_100 = new Vector4(0, 0, 0, 0);
                Vector4 var_90 = new Vector4(0, 0, 0, 0);

                // update bone index unknown0x08
                updateNode(s, node1.ID);
                // update bone index unknown0x0A
                updateNode(s, node2.ID);

                var_100 = new Vector4(node0.animatedTranslation, 1); // position unknown0x06
                var_120 = new Vector4(node3.animatedTranslation, 1); // position unknown0x0C
                var_140 = new Vector4(node4.animatedTranslation, 1); // position unknown0x0E

                var_120 = Vector4.Subtract(var_120, var_100);
                var_140 = Vector4.Subtract(var_140, var_100);

                Vector3 temp_v3_1 = new Vector3(var_140.X, var_140.Y, var_140.Z);
                Vector3 temp_v3_2 = new Vector3(var_120.X, var_120.Y, var_120.Z);

                Vector3 temp_v3_result = Vector3.Cross(temp_v3_1, temp_v3_2);

                var_F0.X = temp_v3_result.X;
                var_F0.Y = temp_v3_result.Y;
                var_F0.Z = temp_v3_result.Z;

                if ((ik.IKShorts[1] & 0x01) == 0x01)
                {
                    var_F0 = -var_F0;
                    var_140 = -var_140;
                }

                var_D0 = Vector4.Normalize(var_120);
                var_C0 = Vector4.Normalize(var_140);
                var_B0 = Vector4.Normalize(var_F0);

                var_A0 = var_100;

                var_xmm4 = new Vector4(var_120.Length(),0,0,0);

                Vector4 scale0x08 = new Vector4(node1.animatedScale, 1);
                Vector4 scale0x0A = new Vector4(node2.animatedScale, 1);

                float xScale0x08 = scale0x08.X;
                float unkScale0x08 = node1.PreMatrixFloat;
                float xScale0x0A = scale0x0A.X;
                float unkScale0x0A = node2.PreMatrixFloat;

                var_xmm1 = new Vector4(xScale0x08 * unkScale0x08, 0, 0, 0);

                var_xmm3 = new Vector4(var_xmm4.X, var_xmm1.X, 0, 0);

                var_xmm2 = new Vector4(xScale0x0A * unkScale0x0A, 0, 0, 0);

                float var_104 = var_xmm1.X;

                var_xmm1 = new Vector4(var_xmm1.X, var_xmm2.X, 0, 0);

                var_xmm2 = new Vector4(var_xmm2.X, var_xmm4.X, 0, 0);

                var_100 = var_xmm1;

                var_xmm4 = var_xmm3;

                var_xmm1 = Vector4.Multiply(var_xmm1, var_xmm1);

                var_xmm4 = Vector4.Multiply(var_xmm4, var_xmm3);

                var_90 = var_xmm3;

                var_xmm2 = Vector4.Multiply(var_xmm2, var_xmm2);

                var_xmm4 = Vector4.Add(var_xmm4, var_xmm1);

                var_xmm1 = new Vector4(0.5f, 0, 0, 0);

                var_xmm3 = new Vector4(var_100.Y, 0, 0, 0);
                var_xmm3.X *= var_90.Y;

                var_xmm4 = Vector4.Subtract(var_xmm4, var_xmm2);

                var_xmm2 = new Vector4(var_100.X, 0, 0, 0);
                var_xmm2.X *= var_90.X;

                var_xmm1 = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);

                var_xmm4 = Vector4.Multiply(var_xmm4, var_xmm1);

                var_120 = var_xmm4;

                var_xmm1 = new Vector4(var_120.X, 0, 0, 0);

                var_xmm4 = new Vector4(1f, 0, 0, 0);

                var_xmm1.X /= var_xmm2.X;

                var_xmm2 = new Vector4(var_120.Y, 0, 0, 0);

                var_xmm2.X /= var_xmm3.X;

                var_xmm3 = new Vector4(-1f, 0, 0, 0);

                var_120.X = var_xmm1.X;

                var_120.Y = var_xmm2.X;

                if(var_xmm3.X > var_xmm1.X)
                {
                    var_120.X = var_xmm3.X;
                }
                else if (var_xmm4.X < var_xmm1.X)
                {
                    var_120.X = var_xmm4.X;
                }

                if (var_xmm3.X > var_xmm2.X)
                {
                    var_120.Y = var_xmm3.X;
                }
                else if (var_xmm4.X < var_xmm2.X)
                {
                    var_120.Y = var_xmm4.X;
                }

                var_xmm1 = var_120;

                var_xmm0 = new Vector4(1f, 1f, 0, 0); // D3DXVECTOR4(var_xmm4.x, var_xmm4.x);

                var_xmm1 = Vector4.Multiply(var_xmm1, var_xmm1);

                var_xmm0 = Vector4.Subtract(var_xmm0, var_xmm1);

                var_140 = new Vector4((float)Math.Sqrt(var_xmm0.X), (float)Math.Sqrt(var_xmm0.Y), 0, 0);

                var_xmm0 = new Vector4(0, 0, 0, 0);

                var_F0 = new Vector4(0, 0, -1f, 0);

                Matrix4x4 var_80 = Matrix4x4.Identity, var_40 = Matrix4x4.Identity;

                //LINE 690
                var_80 = sub_504330(var_80, var_F0, new Vector4(var_140.X, var_120.X, 0f, 0f));
                var_40 = sub_504330(var_40, var_F0, new Vector4(-var_140.Y, -var_120.Y, 0f, 0f));

                Matrix4x4 mat_D0 = Matrix4x4.Identity;

                MatrixCreate(mat_D0, var_D0, var_C0, var_B0, var_A0);
                var_80 = Matrix4x4.Multiply(var_80, mat_D0);

                // bone matrix = bone scale matrix * mat_80
                Matrix4x4 mat_bone0x08 = Matrix4x4.CreateScale(scale0x08.X, scale0x08.Y, scale0x08.Z);
                mat_bone0x08 = Matrix4x4.Multiply(mat_bone0x08, var_80);

                mat_D0 = var_80;

                var_100 = new Vector4(var_104, 0, 0, 1f);

                // mat_D0 position = bone matrix position
                mat_D0.M41 = mat_bone0x08.M41;
                mat_D0.M42 = mat_bone0x08.M42;
                mat_D0.M43 = mat_bone0x08.M43;
                mat_D0.M44 = mat_bone0x08.M44;

                var_100 = Vector4.Transform(var_100, mat_D0);

                mat_D0.M41 = var_100.X;
                mat_D0.M42 = var_100.Y;
                mat_D0.M43 = var_100.Z;
                mat_D0.M44 = var_100.W;

                var_40 = Matrix4x4.Multiply(var_40, mat_D0);

                // bone matrix = bone scale matrix * mat_80
                Matrix4x4 mat_bone0x0A = Matrix4x4.CreateScale(scale0x0A.X, scale0x0A.Y, scale0x0A.Z);
                mat_bone0x08 = Matrix4x4.Multiply(mat_bone0x0A, var_40);

                var_100 = new Vector4(unkScale0x0A, 0, 0, 1f);

                var_100 = Vector4.Transform(var_100, mat_bone0x0A);

                // update position and quaternion information for bone index 0x08
                Quaternion rotation = Quaternion.CreateFromRotationMatrix(var_80);
                Utils.LeftHandToEulerAnglesXYZ(var_80, out node1.animatedRotation.X, out node1.animatedRotation.Y, out node1.animatedRotation.Z);

                node1.animatedTranslation = mat_bone0x08.Translation;
                node1.animatedRotationQuaternion = rotation;
                node1.animatedMatrix = mat_bone0x08;

                // update position and quaternion information for bone index 0x0A
                rotation = Quaternion.CreateFromRotationMatrix(var_40);
                Utils.LeftHandToEulerAnglesXYZ(var_40, out node2.animatedRotation.X, out node2.animatedRotation.Y, out node2.animatedRotation.Z);
                
                node1.animatedTranslation = mat_bone0x0A.Translation;
                node1.animatedRotationQuaternion = rotation;
                node1.animatedMatrix = mat_bone0x0A;

                // set flags for absolute translation and rotation
                node2.animationProcessingDone = true;
                node2.animatedAbsoluteRotationFlag = true;
                node2.animatedAbsoluteTranslationFlag = true;

                // update position for bone index 0x0C
                node3.animatedTranslation = new Vector3(var_100.X, var_100.Y, var_100.Z);
                node3.animatedMatrix.Translation = new Vector3(var_100.X, var_100.Y, var_100.Z);

                // set flags for absolute translation
                node2.animatedAbsoluteTranslationFlag = true;

                // update children of bone index 0x08
                updateNode(s, node1.ID, false, true, false);
                // update children of bone index 0x0A
                updateNode(s, node2.ID, false, true, false);
            }
        }

        public static Vector3 lerpVector3(Vector3 in1, Vector3 in2, float f_scale)
        {
            return in1 + (in2 - in1) * f_scale;
        }
        public static Quaternion lerpQuaternion(Quaternion in1, Quaternion in2, float f_scale)
        {
            return in1 + (in2 - in1) * f_scale;
        }

        public static void ProcessIKData0x01_00(AnimatedSkeleton s, IKDataBlock ik)
        {
            // sub_525270
            {
                AnimatedNode node0 = s.AnimatedNodes[ik.IKShorts[0]];
                AnimatedNode node1 = s.AnimatedNodes[ik.IKShorts[1]];
                AnimatedNode node2 = s.AnimatedNodes[ik.IKShorts[2]];
                AnimatedNode nodeP = new AnimatedNode();
                if(node0.Parent != -1)
                {
                    nodeP = s.AnimatedNodes[node0.Parent];
                }

                if (node0.NodeMatrix == null || node1.NodeMatrix == null || node2.NodeMatrix == null || nodeP.NodeMatrix == null)
                {
                    return;
                }

                updateNode(s, node1.ID);
                updateNode(s, node2.ID);
                updateNode(s, nodeP.ID);

                Vector3 translation;
                // if flags & 0x01
                if ((ik.Flag0x01 & 0x01) == 0x01)
                {
                    // - boneTransform0x06.position = lerp(boneTransform0x08.position, boneTransform0x0A.position, float0x0C);
                    translation = lerpVector3(node1.animatedTranslation, node2.animatedTranslation, ik.IKFloats[0]);
                }
                else
                {
                    translation = Vector3.Transform(node0.Translation, nodeP.animatedMatrix);
                }
                Quaternion rotation;
                // if flags & 0x02
                if ((ik.Flag0x01 & 0x02) == 0x02)
                {
                    rotation = lerpQuaternion(node1.animatedRotationQuaternion, node2.animatedRotationQuaternion, ik.IKFloats[1]);
                }
                else
                {
                    rotation = node0.animatedRotationQuaternion * nodeP.animatedRotationQuaternion;
                }
                Vector3 scale;
                //if flags & 0x04
                if ((ik.Flag0x01 & 0x04) == 0x04)
                {
                    scale = lerpVector3(node1.animatedScale, node2.animatedScale, ik.IKFloats[2]);
                }
                else
                {
                    scale = nodeP.animatedScale;
                }

                Utils.composeMatrixQuat(rotation, scale, translation, out Matrix4x4 matrix);

                Utils.LeftHandToEulerAnglesXYZ(matrix, out node0.animatedRotation.X, out node0.animatedRotation.Y, out node0.animatedRotation.Z);

                node0.animatedScale = scale;
                node0.animatedTranslation = translation;
                node0.animatedRotationQuaternion = rotation;
                node0.animatedMatrix = matrix;

                node0.animationProcessingDone = true;
                node0.animatedAbsoluteRotationFlag = true;
                node0.animatedAbsoluteTranslationFlag = true;
                node0.animatedAbsoluteScaleFlag = true;

                updateNode(s, node0.ID, false, true, false);
            }
        }

        public static bool updateIKData(AnimatedSkeleton s, int ikNumber)
        {
            bool bResult = false;

            IKDataBlock ik = s.IKDataBlocks[ikNumber];

            switch (ik.Method)
            {
                case 0x00:
                    {
                        switch (ik.Flag0x00)
                        {
                            case 0x00:
                                {
                                    ProcessIKData0x00_00(s, ik);
                                } break;
                            case 0x02:
                                {
                                    ProcessIKData0x00_02(s, ik);
                                    bResult = true;
                                } break;
                        }
                    } break;
                case 0x01:
                    {
                        ProcessIKData0x01_00(s, ik);
                        bResult = true;
                    } break;
            }

            return bResult;
        }

        public static bool updateNode(AnimatedSkeleton s, int nodeNumber, bool bForce = false, bool bUpdateChildren = false, bool bUpdateSiblings = false)
        {
            //    EMAData::SkelettonNodePerNumberMap & skelettonNodes = emaData.m_skelettonNodes;
            //    EMAData::SkelettonNodePerNumberMap::iterator nodeIt = skelettonNodes.find(nodeNumber);
            AnimatedNode node = s.AnimatedNodes[nodeNumber];

            int parentNumber = node.Parent;
            //    if (skelettonNodes.end() != nodeIt)
            //    {
            //        EMASkelettonNode & node = nodeIt->second;
            //        unsigned short parentNumber = node.parent;
            if(bForce)
            {
                node.animationProcessingDone = false;
            }
            //        if (bForce)
            //        {
            //            node.animationProcessingDone = false;
            //        }

            if (node.animationProcessingDone)
            {
                return true;
            }

            //        D3DXMATRIX matrix(node.animatedMatrix);
            Matrix4x4 matrix = node.animatedMatrix;
            //        D3DXVECTOR3 translation, scale;
            Vector3 translation = node.animatedTranslation;
            Vector3 scale = node.animatedScale;
            //        D3DXQUATERNION rotation;
            Quaternion rotation = node.animatedRotationQuaternion;
            //        translation = D3DXVECTOR3(node.animatedTranslation);
            //        rotation = D3DXQUATERNION(node.animatedRotationQuaternion);
            //        scale = D3DXVECTOR3(node.animatedScale);

            //        EMAData::SkelettonNodePerNumberMap::const_iterator parentIt = skelettonNodes.find(parentNumber);
            if (parentNumber != 0xFFFF)
            {
                AnimatedNode parent = s.AnimatedNodes[node.Parent];

                if (!parent.animationProcessingDone)
                {
                    if (!updateNode(s, parentNumber))
                        return false;
                }
                //        if (skelettonNodes.end() != nodeIt && 0xFFFF != parentNumber)
                //        {
                //            EMASkelettonNode const&parent = parentIt->second;
                //            if (!parent.animationProcessingDone)
                //            {
                //                if (!updateNode(parentNumber))
                //                    return false;
                //            }
                Matrix4x4 parentMatrix = parent.animatedMatrix;
                //            D3DXMATRIX parentMatrix(parent.animatedMatrix);
            //            D3DXMATRIX tempMatrix;
            //            D3DXMatrixMultiply(&tempMatrix, &matrix, &parentMatrix);
            //            matrix = tempMatrix;
                matrix = Matrix4x4.Multiply(matrix, parentMatrix);

                if (!node.animatedAbsoluteTranslationFlag)
                {
                    translation = Vector3.Transform(translation, parent.animatedMatrix);
                }
                //            if (!node.animatedAbsoluteTranslationFlag)
                //            {
                //                D3DXVec3TransformCoord(&translation, &translation, &D3DXMATRIX(parent.animatedMatrix));
                //            }
                if (!node.animatedAbsoluteRotationFlag)
                {
                    rotation *= parent.animatedRotationQuaternion;
                }
                //            if (!node.animatedAbsoluteRotationFlag)
                //            {
                //                rotation *= D3DXQUATERNION(parent.animatedRotationQuaternion);
                //                //rotation = D3DXQUATERNION(parent.animatedRotationQuaternion) * rotation;
                //            }
                if (!node.animatedAbsoluteScaleFlag)
                {
                    scale.X *= parent.animatedScale.X;
                    scale.Y *= parent.animatedScale.Y;
                    scale.Z *= parent.animatedScale.Z;
                }
                //            if (!node.animatedAbsoluteScaleFlag)
                //            {
                //                scale.x *= parent.animatedScale[0];
                //                scale.y *= parent.animatedScale[1];
                //                scale.z *= parent.animatedScale[2];
                //            }
                Utils.composeMatrixQuat(rotation, scale, translation, out matrix);
                //            MathHelpers::composeMatrixQuat(rotation, scale, translation, matrix);

            }
            //}

            node.animatedScale = scale;
            node.animatedTranslation = translation;
            node.animatedRotationQuaternion = rotation;
            node.animatedMatrix = matrix;
            //        memcpy(node.animatedScale, (float*)scale, 3 * sizeof(float));
            //        memcpy(node.animatedTranslation, (float*)translation, 3 * sizeof(float));
            //        memcpy(node.animatedRotationQuaternion, (float*)rotation, 4 * sizeof(float));
            //        memcpy(node.animatedMatrix, (float*)matrix, 16 * sizeof(float));
            node.animationProcessingDone = true;
            //        node.animationProcessingDone = true;

            if (bUpdateSiblings)
            {
                updateNode(s, node.Sibling, true, bUpdateChildren, true);
            }
            //        if (bUpdateSiblings)
            //        {
            //            updateNode(node.sibling, true, bUpdateChildren, true);
            //        }
            if (bUpdateChildren)
            {
                updateNode(s, node.Child1, true, true, true);
            }
            //        if (bUpdateChildren)
            //        {
            //            updateNode(node.child1, true, true, true);
            //        }

            return true;
            //        return true;
            //    }

            //    return false;
        }

        public static float InterpolateRelativeKeyFrames(Animation anim, CMDTrack c, int frame)
        {
            //Declare default
            float value = 0;

            for (int i = 0; i < c.StepsList.Count; i++)
            {
                if (c.StepsList[i] > frame)
                {
                    int v_index1;
                    int v_index2;
                    int t_index1;
                    int t_index2;

                    if ((c.BitFlag & 0x40) == 0x40) //check long or short value indices
                    {
                        v_index1 = (c.IndicesList[i - 1] & 0b00111111111111111111111111111111);
                        v_index2 = (c.IndicesList[i] & 0b00111111111111111111111111111111);
                        t_index1 = v_index1 + (c.IndicesList[i - 1] >> 30);
                        t_index2 = v_index2 + (c.IndicesList[i] >> 30);

                        if ((c.IndicesList[i - 1] >> 30) == 0) t_index1 = 0;
                        if ((c.IndicesList[i] >> 30) == 0) t_index2 = 0;
                    }
                    else
                    {
                        v_index1 = (c.IndicesList[i - 1] & 0b0011111111111111);
                        v_index2 = (c.IndicesList[i] & 0b0011111111111111);
                        t_index1 = v_index1 + (c.IndicesList[i - 1] >> 14);
                        t_index2 = v_index2 + (c.IndicesList[i] >> 14);
                        
                        if ((c.IndicesList[i - 1] >> 14) == 0) t_index1 = 0;
                        if ((c.IndicesList[i] >> 14) == 0) t_index2 = 0;
                    }

                    float p1 = anim.ValuesList[v_index1];
                    float p2 = anim.ValuesList[v_index2];
                    float t1 = anim.ValuesList[t_index1];
                    float t2 = anim.ValuesList[t_index2];
                    float f_initial = c.StepsList[i - 1];
                    float f_end = c.StepsList[i];

                    float s = (frame - (float)f_initial) / (float)(f_end - f_initial);

                    value = HermiteInterpolation(p1, t1, p2, t2, s);

                    break;
                }
                else if (c.StepsList[i] == frame)
                {
                    value = anim.ValuesList[(c.IndicesList[i] & 0b0011111111111111)];

                    if ((c.BitFlag & 0x40) == 0x40) //check long or short value indices
                    {
                        value = anim.ValuesList[(c.IndicesList[i] & 0b00111111111111111111111111111111)];
                    }
                    else
                    {
                        value = anim.ValuesList[(c.IndicesList[i] & 0b0011111111111111)];
                    }
                    break;
                }
            }

            return value;
        }

        public static void InterpolateAbsoluteKeyFrames(Animation anim, Skeleton skel, CMDTrack c, int frame, out float value)
        {
            value = 0;

            //Grab the value, this is just a copy-paste from InterpolateRelativeKeyFrames
            for (int i = 0; i < c.StepsList.Count; i++)
            {
                if (c.StepsList[i] > frame)
                {
                    int v_index1;
                    int v_index2;
                    int t_index1;
                    int t_index2;

                    if ((c.BitFlag & 0x40) == 0x40) //check long or short value indices
                    {
                        v_index1 = (c.IndicesList[i - 1] & 0b00111111111111111111111111111111);
                        v_index2 = (c.IndicesList[i] & 0b00111111111111111111111111111111);
                        t_index1 = v_index1 + (c.IndicesList[i - 1] >> 30);
                        t_index2 = v_index2 + (c.IndicesList[i] >> 30);

                        if ((c.IndicesList[i - 1] >> 30) == 0) t_index1 = 0;
                        if ((c.IndicesList[i] >> 30) == 0) t_index2 = 0;
                    }
                    else
                    {
                        v_index1 = (c.IndicesList[i - 1] & 0b0011111111111111);
                        v_index2 = (c.IndicesList[i] & 0b0011111111111111);
                        t_index1 = v_index1 + (c.IndicesList[i - 1] >> 14);
                        t_index2 = v_index2 + (c.IndicesList[i] >> 14);

                        if ((c.IndicesList[i - 1] >> 14) == 0) t_index1 = 0;
                        if ((c.IndicesList[i] >> 14) == 0) t_index2 = 0;
                    }

                    float p1 = anim.ValuesList[v_index1];
                    float p2 = anim.ValuesList[v_index2];
                    float t1 = anim.ValuesList[t_index1];
                    float t2 = anim.ValuesList[t_index2];
                    float f_initial = c.StepsList[i - 1];
                    float f_end = c.StepsList[i];

                    float s = (frame - (float)f_initial) / (float)(f_end - f_initial);

                    value = HermiteInterpolation(p1, t1, p2, t2, s);

                    break;
                }
                else if (c.StepsList[i] == frame)
                {
                    value = anim.ValuesList[(c.IndicesList[i] & 0b0011111111111111)];

                    if ((c.BitFlag & 0x40) == 0x40) //check long or short value indices
                    {
                        value = anim.ValuesList[(c.IndicesList[i] & 0b00111111111111111111111111111111)];
                    }
                    else
                    {
                        value = anim.ValuesList[(c.IndicesList[i] & 0b0011111111111111)];
                    }
                    break;
                }
            }

            //Need to get the affine matrix for the parent node...
            //Take the inverse matrix...
            //Multiply by the 
        }

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
                skel.NodeNamePointersList.Add(0x00);
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
                cmd.IndicesList = new List<int> (anim.CMDTracks[i].IndicesList);
                cmd.IndicesListPointer = anim.CMDTracks[i].IndicesListPointer;
                cmd.StepCount = anim.CMDTracks[i].StepCount;
                cmd.StepsList = new List<int>(anim.CMDTracks[i].StepsList);
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
                            anim.ValuesList.Add(anim.ValuesList[cmd.IndicesList[j]] + 2f);
                            anim.ValueCount++;
                            cmd.IndicesList[j] = anim.ValuesList.Count;
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
                    anim.CmdTrackPointersList.Add(0x00);
                }
            }

            return anim;
        }
    }

    
}