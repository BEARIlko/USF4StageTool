using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace USF4_Stage_Tool
{
    class EMAProcessor
    {
        static int currentAnimation;
        public static int currentFrame;

        static EMA ema;
        static AnimatedSkeleton aSkeleton;
        public static AnimatedNode[] aNodes_array;

        public EMAProcessor (EMA target_ema, int anim_index = -1)
        {
            currentFrame = 0;
            ema = target_ema;
            aSkeleton = new AnimatedSkeleton().SetupSkeleton(ema.Skeleton);
            aNodes_array = aSkeleton.AnimatedNodes.ToArray();

            if (anim_index != -1)
            {
                currentAnimation = anim_index;
                SetupAnimation(anim_index);
            }
        }

        public AnimatedNode[] ReturnAnimatedNodes()
        {
            return aNodes_array;
        }

        public bool SetupAnimation (int animation_index)
        {
            currentFrame = 0;
            currentAnimation = animation_index;

            //Clear the skeleton and nodes and replace with the originals
            aSkeleton = new AnimatedSkeleton().SetupSkeleton(ema.Skeleton);
            aNodes_array = aSkeleton.AnimatedNodes.ToArray();

            return true;
        }

        public void AnimateFrame(int frame)
        {
            //Lock the frame within the duration
            currentFrame = frame % ema.Animations[currentAnimation].Duration;
            SetupFrame(currentFrame);

            for (int i = 0; i < aNodes_array.Length; i++)
            {
                updateNode(i);
            }

            for (int i = 0; i < ema.Skeleton.IKDataBlocks.Count; i++)
            {
                //updateIKData(i);
            }

        }

        static void SetupFrame(int frame)
        {
            //Reset all the nodes, and calculate their transforms
            for (int i = 0; i < aNodes_array.Length; i++)
            {
                //Reset flags
                aNodes_array[i].animationProcessingDone = false;
                aNodes_array[i].animatedAbsoluteRotationFlag = false;
                aNodes_array[i].animatedAbsoluteScaleFlag = false;
                aNodes_array[i].animatedAbsoluteTranslationFlag = false;
                //Reset transformation
                aNodes_array[i].animatedRotation = aNodes_array[i].Rotation;
                aNodes_array[i].animatedScale = aNodes_array[i].Scale;
                aNodes_array[i].animatedTranslation = aNodes_array[i].Translation;
                aNodes_array[i].animatedRotationQuaternion = aNodes_array[i].RotationQuaternion;
                aNodes_array[i].animatedMatrix = aNodes_array[i].NodeMatrix;

                //Calculate interpolated values, if the node is animated
                if (getTransform(currentAnimation, currentFrame, aNodes_array[i].ID, 0, out Vector3 translation, out aNodes_array[i].animatedAbsoluteTranslationFlag))
                    aNodes_array[i].animatedTranslation = translation;
                if (getTransform(currentAnimation, currentFrame, aNodes_array[i].ID, 1, out Vector3 rotation_d, out aNodes_array[i].animatedAbsoluteRotationFlag))
                {
                    //aNodes_array[i].animatedRotation = new Vector3((float)(rotation_d.X * Math.PI / 180d), (float)(rotation_d.Y * Math.PI / 180d), (float)(rotation_d.Z * Math.PI / 180d));
                    aNodes_array[i].animatedRotation = new Vector3(rotation_d.X, rotation_d.Y, rotation_d.Z);
                    Utils.EulerToQuaternionXYZ((float)(rotation_d.Y * Math.PI / 180d), (float)(rotation_d.Z * Math.PI / 180d), (float)(rotation_d.X * Math.PI / 180d), out aNodes_array[i].animatedRotationQuaternion);
                }
                if (getTransform(currentAnimation, currentFrame, aNodes_array[i].ID, 2, out Vector3 scale, out aNodes_array[i].animatedAbsoluteScaleFlag))
                    aNodes_array[i].animatedScale = scale;

                aNodes_array[i].animatedMatrix = Matrix4x4.CreateScale(aNodes_array[i].animatedScale) * Matrix4x4.CreateFromQuaternion(aNodes_array[i].animatedRotationQuaternion) * Matrix4x4.CreateTranslation(aNodes_array[i].animatedTranslation);
            }
        }

        static bool getTransform(int anim_index, int frame, int bone_index, int transform_type, out Vector3 values, out bool absolute)
        {
            bool animated = false;
            absolute = false;

            values = new Vector3(0, 0, 0);

            bool xget = false;
            bool yget = false;
            bool zget = false;

            foreach (CMDTrack c in ema.Animations[anim_index].CMDTracks)
            {
                int axis = c.BitFlag & 0x03;


                if (xget && yget && zget) break;

                if (c.BoneID == bone_index && c.TransformType == transform_type)
                {
                    if (c.BoneID == bone_index)
                    {
                        animated = true;
                        if ((c.BitFlag & 0x10) == 0x10) absolute = true;
                        else absolute = false;

                        float val = Anim.InterpolateRelativeKeyFrames(ema.Animations[anim_index], c, frame);
                        switch (axis)
                        {
                            case 0:
                                values.X = val;
                                xget = true;
                                break;
                            case 1:
                                values.Y = val;
                                yget = true;
                                break;
                            case 2:
                                values.Z = val;
                                zget = true;
                                break;
                        }
                    }
                }
            }

            return animated;
        }

        static bool updateNode(int nodeNumber, bool bForce = false, bool bUpdateChildren = false, bool bUpdateSiblings = false)
        {
            int parentNumber = aNodes_array[nodeNumber].Parent;

            if (bForce)
            {
                aNodes_array[nodeNumber].animationProcessingDone = false;
            }

            if (aNodes_array[nodeNumber].animationProcessingDone)
            {
                return true;
            }

            {
                Matrix4x4 matrix = aNodes_array[nodeNumber].animatedMatrix;
                Vector3 translation = aNodes_array[nodeNumber].animatedTranslation;
                Vector3 scale = aNodes_array[nodeNumber].animatedScale;
                Quaternion rotation = aNodes_array[nodeNumber].animatedRotationQuaternion;

                if (parentNumber != -1)
                {
                    AnimatedNode parent = aNodes_array[aNodes_array[nodeNumber].Parent];

                    if (!parent.animationProcessingDone)
                    {
                        if (!updateNode(parentNumber))
                            return false;
                    }

                    Matrix4x4 parentMatrix = parent.animatedMatrix;

                    //TODO CHECK THIS, seems like it gets overwritten later
                    matrix = Matrix4x4.Multiply(matrix, parentMatrix);

                    if (!aNodes_array[nodeNumber].animatedAbsoluteTranslationFlag)
                    {
                        translation = Vector3.Transform(translation, parent.animatedMatrix);
                    }

                    if (!aNodes_array[nodeNumber].animatedAbsoluteRotationFlag)
                    {
                        rotation = parent.animatedRotationQuaternion * rotation;
                    }

                    if (!aNodes_array[nodeNumber].animatedAbsoluteScaleFlag)
                    {
                        scale.X *= parent.animatedScale.X;
                        scale.Y *= parent.animatedScale.Y;
                        scale.Z *= parent.animatedScale.Z;
                    }

                    Utils.composeMatrixQuat(rotation, scale, translation, out matrix);
                }

                aNodes_array[nodeNumber].animatedScale = scale;
                aNodes_array[nodeNumber].animatedTranslation = translation;
                aNodes_array[nodeNumber].animatedRotationQuaternion = rotation;
                aNodes_array[nodeNumber].animatedMatrix = matrix;

                if(parentNumber != -1)
                {
                    Matrix4x4.Invert(aNodes_array[parentNumber].animatedMatrix, out Matrix4x4 parentInverseMatrix);
                    aNodes_array[nodeNumber].animatedLocalMatrix = matrix * parentInverseMatrix;
                }

                aNodes_array[nodeNumber].animationProcessingDone = true;
            }

            //TODO don't think these even work properly...?
            if (bUpdateSiblings)// && aNodes_array[nodeNumber].Sibling != -1)
            {
                updateNode(aNodes_array[nodeNumber].Sibling, true, bUpdateChildren, true);
            }

            if (bUpdateChildren)// && aNodes_array[nodeNumber].Child1 != -1)
            {
                updateNode(aNodes_array[nodeNumber].Child1, true, true, true);
            }

            return true;
        }

        static Vector3 lerpVector3(Vector3 in1, Vector3 in2, float f_scale)
        {
            return in1 + (in2 - in1) * f_scale;
        }
        static Quaternion lerpQuaternion(Quaternion in1, Quaternion in2, float f_scale)
        {
            return in1 + ((in2 - in1) * f_scale);
        }

        static Vector4 var_xmm0;
        static Vector4 var_xmm1;
        static Vector4 var_xmm2;
        static Vector4 var_xmm3;
        static Vector4 var_xmm4;

        static Matrix4x4 MatrixCreate(Vector4 pV1, Vector4 pV2, Vector4 pV3, Vector4 pV4)
        {
            Matrix4x4 pM = Matrix4x4.Identity;
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

        static Matrix4x4 sub_504330(Matrix4x4 pM_ECX, Vector4 pV1_ESI, Vector4 pV2)
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
            pM_ECX.M44 = 1f;

            return pM_ECX;
        }

        static void ProcessIKData0x00_00(IKDataBlock ik)
        {

        }

        static float MatrixSum(AnimatedNode a)
        {
            return a.animatedMatrix.M11 + a.animatedMatrix.M12 + a.animatedMatrix.M13 + a.animatedMatrix.M14 +
                     a.animatedMatrix.M21 + a.animatedMatrix.M22 + a.animatedMatrix.M23 + a.animatedMatrix.M24 +
                     a.animatedMatrix.M31 + a.animatedMatrix.M32 + a.animatedMatrix.M33 + a.animatedMatrix.M34 +
                     a.animatedMatrix.M41 + a.animatedMatrix.M42 + a.animatedMatrix.M43 + a.animatedMatrix.M44;
        }

        static void ProcessIKData0x00_02(IKDataBlock ik)
        {

            AnimatedNode node0 = aNodes_array[ik.IKShorts[0]];
            AnimatedNode node1 = aNodes_array[ik.IKShorts[1]];
            AnimatedNode node2 = aNodes_array[ik.IKShorts[2]];
            AnimatedNode node3 = aNodes_array[ik.IKShorts[3]];
            AnimatedNode node4 = aNodes_array[ik.IKShorts[4]];
            AnimatedNode nodeP = aNodes_array[node0.Parent];

            node1.IKanimatedNode = true;
            node2.IKanimatedNode = true;
            node3.IKanimatedNode = true;

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
                updateNode(node1.ID);
                // update bone index unknown0x0A
                updateNode(node2.ID);

                var_100 = new Vector4(node0.animatedTranslation, 1); // position unknown0x06
                var_120 = new Vector4(node3.animatedTranslation, 1); // position unknown0x0C
                var_140 = new Vector4(node4.animatedTranslation, 1); // position unknown0x0E

                var_120 = Vector4.Subtract(var_120, var_100);
                var_140 = Vector4.Subtract(var_140, var_100);

                Vector3 v3_140 = new Vector3(var_140.X, var_140.Y, var_140.Z);
                Vector3 v3_120 = new Vector3(var_120.X, var_120.Y, var_120.Z);

                Vector3 v3_F0 = Vector3.Cross(v3_140, v3_120);
                v3_140 = Vector3.Cross(v3_F0, v3_120);

                var_F0.X = v3_F0.X;
                var_F0.Y = v3_F0.Y;
                var_F0.Z = v3_F0.Z;

                var_140.X = v3_140.X;
                var_140.Y = v3_140.Y;
                var_140.Z = v3_140.Z;

                if ((ik.Flag0x01 & 0x01) == 0x01)
                {
                    var_F0 = -var_F0;
                    var_140 = -var_140;
                }

                var_D0 = Vector4.Normalize(var_120);
                var_C0 = Vector4.Normalize(var_140);
                var_B0 = Vector4.Normalize(var_F0);

                var_A0 = var_100;

                var_xmm4 = new Vector4(var_120.Length(), 0, 0, 0);

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

                if (var_xmm3.X > var_xmm1.X)
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

                Matrix4x4 mat_D0 = MatrixCreate(var_D0, var_C0, var_B0, var_A0);

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
                mat_bone0x0A = Matrix4x4.Multiply(mat_bone0x0A, var_40);

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

                node2.animatedTranslation = mat_bone0x0A.Translation;
                node2.animatedRotationQuaternion = rotation;
                node2.animatedMatrix = mat_bone0x0A;

                // set flags for absolute translation and rotation
                node2.animationProcessingDone = true;
                node2.animatedAbsoluteRotationFlag = true;
                node2.animatedAbsoluteTranslationFlag = true;

                // update position for bone index 0x0C
                node3.animatedTranslation = new Vector3(var_100.X, var_100.Y, var_100.Z);
                node3.animatedMatrix.Translation = new Vector3(var_100.X, var_100.Y, var_100.Z);

                // set flags for absolute translation
                node2.animatedAbsoluteTranslationFlag = true;

                //return nodes to array
                aNodes_array[node0.ID] = node0;
                aNodes_array[node1.ID] = node1;
                aNodes_array[node2.ID] = node2;
                aNodes_array[node3.ID] = node3;
                aNodes_array[node4.ID] = node4;
                aNodes_array[nodeP.ID] = nodeP;

                // update children of bone index 0x08
                updateNode(node1.ID, false, true, false);
                // update children of bone index 0x0A
                updateNode(node2.ID, false, true, false);
            }
        }

        static void ProcessIKData0x01_00(IKDataBlock ik)
        {
            // sub_525270
            {
                AnimatedNode node0 = aNodes_array[ik.IKShorts[0]];
                AnimatedNode node1 = aNodes_array[ik.IKShorts[1]];
                AnimatedNode node2 = aNodes_array[ik.IKShorts[2]];

                //node0.IKanimatedNode = true;

                AnimatedNode nodeP = new AnimatedNode();
                if (node0.Parent != -1)
                {
                    nodeP = aNodes_array[node0.Parent];
                }

                if (node0.NodeMatrix == null || node1.NodeMatrix == null || node2.NodeMatrix == null || nodeP.NodeMatrix == null)
                {
                    return;
                }

                updateNode(node1.ID);
                updateNode(node2.ID);
                updateNode(nodeP.ID);

                Vector3 translation;
                // if flags & 0x01
                if ((ik.Flag0x01 & 0x01) == 0x01)
                {
                    // - boneTransform0x06.position = lerp(boneTransform0x08.position, boneTransform0x0A.position, float0x0C);
                    translation = lerpVector3(node1.animatedTranslation, node2.animatedTranslation, ik.IKFloats[0]);
                }
                else
                {
                    //translation = Vector3.Transform(node0.Translation, nodeP.animatedMatrix);
                    translation = node0.animatedTranslation;
                }
                Quaternion rotation;
                // if flags & 0x02
                if ((ik.Flag0x01 & 0x02) == 0x02)
                {
                    rotation = lerpQuaternion(node1.animatedRotationQuaternion, node2.animatedRotationQuaternion, ik.IKFloats[1]);
                }
                else
                {
                    //rotation = node0.animatedRotationQuaternion * nodeP.animatedRotationQuaternion;
                    rotation = node0.animatedRotationQuaternion;
                }
                Vector3 scale;
                //if flags & 0x04
                if ((ik.Flag0x01 & 0x04) == 0x04)
                {
                    scale = lerpVector3(node1.animatedScale, node2.animatedScale, ik.IKFloats[2]);
                }
                else
                {
                    scale = node0.animatedScale;
                }

                Utils.composeMatrixQuat(rotation, scale, translation, out Matrix4x4 matrix);

                Utils.LeftHandToEulerAnglesXYZ(matrix, out aNodes_array[node0.ID].animatedRotation.X, out aNodes_array[node0.ID].animatedRotation.Y, out aNodes_array[node0.ID].animatedRotation.Z);

                aNodes_array[node0.ID].animatedScale = scale;
                aNodes_array[node0.ID].animatedTranslation = translation;
                aNodes_array[node0.ID].animatedRotationQuaternion = rotation;
                aNodes_array[node0.ID].animatedMatrix = matrix;

                aNodes_array[node0.ID].animationProcessingDone = true;
                aNodes_array[node0.ID].animatedAbsoluteRotationFlag = true;
                aNodes_array[node0.ID].animatedAbsoluteTranslationFlag = true;
                aNodes_array[node0.ID].animatedAbsoluteScaleFlag = true;

                if (node0.Parent != -1)
                {
                    Matrix4x4.Invert(aNodes_array[node0.Parent].animatedMatrix, out Matrix4x4 parentInverseMatrix);
                    aNodes_array[node0.ID].animatedLocalMatrix = matrix * parentInverseMatrix;
                }

                //updateNode(node0.ID, false, true, false);
            }
        }

        static bool updateIKData(int ikNumber)
        {
            bool bResult = false;

            AnimatedSkeleton s = aSkeleton;

            IKDataBlock ik = s.IKDataBlocks[ikNumber];

            switch (ik.Method)
            {
                case 0x00:
                    {
                        switch (ik.Flag0x00)
                        {
                            case 0x00:
                                {
                                    ProcessIKData0x00_00(ik);
                                }
                                break;
                            case 0x02:
                                {
                                    ProcessIKData0x00_02(ik);
                                    bResult = true;
                                }
                                break;
                        }
                    }
                    break;
                case 0x01:
                    {
                        ProcessIKData0x01_00(ik);
                        bResult = true;
                    }
                    break;
            }

            return bResult;
        }
    }
}
