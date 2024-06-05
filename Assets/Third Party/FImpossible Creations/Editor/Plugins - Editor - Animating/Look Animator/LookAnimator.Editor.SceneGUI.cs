using FIMSpace.FEditor;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator_Editor
    {
        protected virtual void OnSceneGUI()
        {
            if (Application.isPlaying) return;

            if (Get.BaseTransform)
                if (Get.LeadBone)
                    if (Get.StartLookPointOffset != Vector3.zero)
                    {
                        Undo.RecordObject(Get, "Changing position of look start position");

                        if (Get.AnchorStartLookPoint)
                        {
                            Vector3 pos = Get.BaseTransform.TransformPoint(Get.BaseTransform.InverseTransformPoint(Get.LeadBone.position)) + Get.BaseTransform.TransformVector(Get.StartLookPointOffset);
                            Vector3 transformed = FEditor_TransformHandles.PositionHandle(pos, Get.BaseTransform.rotation, .3f, true, false);

                            if (Vector3.Distance(transformed, pos) > 0.00001f)
                            {
                                Get.StartLookPointOffset = Get.BaseTransform.InverseTransformPoint(transformed) - Get.BaseTransform.InverseTransformPoint(Get.LeadBone.position);
                                SerializedObject obj = new SerializedObject(Get);
                                if (obj != null) { obj.ApplyModifiedProperties(); obj.Update(); }
                            }
                        }
                        else
                        {
                            Vector3 off = Get.LookBones[0].Transform.TransformDirection(Get.StartLookPointOffset);
                            Vector3 pos = Get.LeadBone.position + off;
                            Vector3 transformed = FEditor_TransformHandles.PositionHandle(pos, Get.BaseTransform.rotation, .3f, true, false);

                            if (Vector3.Distance(transformed, pos) > 0.00001f)
                            {
                                Vector3 diff = transformed - pos;
                                Get.StartLookPointOffset = Get.LookBones[0].Transform.InverseTransformDirection(off + diff);
                                SerializedObject obj = new SerializedObject(Get);
                                if (obj != null) { obj.ApplyModifiedProperties(); obj.Update(); }
                            }
                        }
                    }
        }
    }
}