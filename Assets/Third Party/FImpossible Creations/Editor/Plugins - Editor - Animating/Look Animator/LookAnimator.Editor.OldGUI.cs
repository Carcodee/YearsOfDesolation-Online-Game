using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FLook
{
    [CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FLookAnimator))]
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    public partial class FLookAnimator_Editor : UnityEditor.Editor
    {

        private void DrawOldGUI()
        {

            EditorGUILayout.BeginVertical(FEditor_Styles.GrayBackground);
            EditorGUILayout.BeginHorizontal();
            drawDefaultInspector = GUILayout.Toggle(drawDefaultInspector, "Default inspector");

            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 80;
            Get.drawGizmos = GUILayout.Toggle(Get.drawGizmos, "Draw Gizmos");

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);

            EditorGUILayout.BeginVertical(FEditor_Styles.GrayBackground);
            EditorGUI.indentLevel++;

            drawMain = EditorGUILayout.Foldout(drawMain, "Main Animation Parameters", true);

            #region Main tab

            if (drawMain)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (!Get.ObjectToFollow)
                    GUILayout.BeginHorizontal(FEditor_Styles.YellowBackground);
                else
                {
                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal();
                }

                EditorGUIUtility.labelWidth = 133f;

                EditorGUILayout.PropertyField(sp_tofollow);

                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();

                if (!Get.LeadBone)
                    GUILayout.BeginHorizontal(FEditor_Styles.RedBackground);
                else
                    GUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(sp_leadbone);


                if (Get.LeadBone != previousHead)
                {
                    previousHead = Get.LeadBone;

                    Get.UpdateForCustomInspector();
                }

                if (GUILayout.Button(new GUIContent("Auto Find", "By pressing this button, algorithm will go trough hierarchy and try to find object which name includes 'head' or 'neck', be aware, this bone can not be correct but sure it will help you find right one quicker"), new GUILayoutOption[2] { GUILayout.MaxWidth(90), GUILayout.MaxHeight(15) }))
                {
                    FindHeadBone(Get);
                    EditorUtility.SetDirty(target);
                }

                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (Get.BaseTransform != Get.transform || !Get.BaseTransform)
                {
                    if (!Get.BaseTransform)
                        GUILayout.BeginHorizontal(FEditor_Styles.YellowBackground);
                    else
                        GUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(sp_basetr);

                    if (GUILayout.Button("Try Find", new GUILayoutOption[2] { GUILayout.MaxWidth(90), GUILayout.MaxHeight(15) }))
                    {
                        Get.FindBaseTransform();
                        EditorUtility.SetDirty(target);
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndHorizontal();
                Color preColor = GUI.color;

                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(sp_rotspd);
                GUILayout.Space(3f);
                if (Get.BirdMode) GUI.color = new Color(1f, 0.88f, 0.88f, 0.95f);
                EditorGUILayout.PropertyField(sp_usmooth);
                GUI.color = preColor;
                EditorGUILayout.PropertyField(sp_starttpose);


                //if (!Get.Fix180) if (Get.UltraSmoother > 0f) GUI.color = new Color(1f, 1f, 0.35f, 0.8f);

                //GUILayout.Space(2f);
                //EditorGUILayout.PropertyField(sp_180prev);
                GUI.color = preColor;

                EditorGUIUtility.labelWidth = 0f;

                GUILayout.Space(5f);

                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal(FEditor_Styles.BlueBackground);
                EditorGUILayout.HelpBox("Using more bones (back bones) to help rotate head (head-neck-spine)", MessageType.None);
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                int preCount = Get.BackBonesCount;

                EditorGUIUtility.labelWidth = 130f;
                EditorGUILayout.LabelField(new GUIContent("Backbones: (" + Get.BackBonesCount + ")"));
                EditorGUIUtility.labelWidth = 0f;

                if (GUILayout.Button("+", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(14) }))
                {
                    Get.BackBonesCount++;
                    serializedObject.ApplyModifiedProperties();
                    if (!UpdateCustomInspector(Get, false)) Debug.Log("[LOOK ANIMATOR] Don't change backbones count in playmode");
                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("-", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(14) }))
                {
                    Get.BackBonesCount--;
                    serializedObject.ApplyModifiedProperties();
                    if (!UpdateCustomInspector(Get, false)) Debug.Log("[LOOK ANIMATOR] Don't change backbones count in playmode");
                    EditorUtility.SetDirty(target);
                }

                GUILayout.EndHorizontal();

                if (Get.BackBonesCount < 0) Get.BackBonesCount = 0;

                if (preCount != Get.BackBonesCount) UpdateCustomInspector(Get);

                if (Get.BackBonesCount > 0)
                {
                    drawBackBones = EditorGUILayout.Foldout(drawBackBones, "View back bones", true);
                    if (drawBackBones)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUIUtility.labelWidth = 150f;
                        for (int i = 1; i < Get.LookBones.Count; i++)
                        {
                            string weightString = " " + Mathf.Round(Get.LookBones[i].lookWeight * 100f * Get.WeightsMultiplier) + "%";

                            Transform preTr = Get.LookBones[i].Transform;
                            Get.LookBones[i].Transform = (Transform)EditorGUILayout.ObjectField("Back bone [" + i + "]" + weightString, Get.LookBones[i].Transform, typeof(Transform), true);

                            // If we assigned own bone
                            if (preTr != Get.LookBones[i].Transform)
                            {
                                EditorUtility.SetDirty(target);
                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                        EditorGUIUtility.labelWidth = 0f;

                        EditorGUI.indentLevel--;
                    }

                    GUILayout.Space(5f);

                    EditorGUILayout.BeginHorizontal();


                    EditorGUIUtility.labelWidth = 105f;
                    EditorGUILayout.PropertyField(sp_usecurve);
                    EditorGUIUtility.labelWidth = 0f;

                    if (Get.CurveSpread)
                    {
                        EditorGUILayout.PropertyField(sp_falloff, new GUIContent(""), new GUILayoutOption[2] { GUILayout.MaxHeight(40f), GUILayout.MinHeight(30f) });

                        bool curveChanged = false;
                        if (preKeyframes.Length != Get.BackBonesFalloff.keys.Length) curveChanged = true;

                        if (!curveChanged)
                        {
                            for (int i = 0; i < preKeyframes.Length; i++)
                            {
                                Keyframe pre = preKeyframes[i];
                                Keyframe curr = Get.BackBonesFalloff.keys[i];

                                if (pre.value != curr.value || pre.time != curr.time || pre.inTangent != curr.inTangent || pre.outTangent != curr.outTangent)
                                {
                                    curveChanged = true;
                                    break;
                                }
                            }
                        }

                        if (curveChanged)
                            if (preKeyframes != Get.BackBonesFalloff.keys)
                            {
                                preKeyframes = Get.BackBonesFalloff.keys;
                                UpdateCustomInspector(Get);
                                EditorUtility.SetDirty(target);
                            }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(sp_fallvall, new GUIContent(""));
                    }

                    GUILayout.Space(8f);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(8f);
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);

                #region Limiting angles

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                //if (drawLimiting)
                {
                    #region Clamping angles

                    bool wrongLimit = false;
                    if (Mathf.Abs(Get.XRotationLimits.x) > Get.StopLookingAbove) wrongLimit = true;
                    if (Mathf.Abs(Get.XRotationLimits.y) > Get.StopLookingAbove) wrongLimit = true;

                    Color preCol = GUI.color;

                    GUILayout.Space(4f);
                    GUILayout.BeginVertical(FEditor_Styles.LBlueBackground);

                    if (Get.MaximumDistance > 0f)
                        EditorGUILayout.PropertyField(sp_maxdist);
                    else
                    {
                        GUILayout.BeginHorizontal();
                        //GUILayout.FlexibleSpace();

                        //EditorGUIUtility.labelWidth = 80;
                        EditorGUILayout.PropertyField(sp_maxdist);
                        //EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.LabelField("(Infinity)", new GUILayoutOption[] { GUILayout.Width(70f) });

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(4f);
                    EditorGUILayout.PropertyField(sp_LookWhenAbove);

                    FEditor_Styles.DrawUILine(Color.white * 0.5f, 1, 4);
                    EditorGUILayout.PropertyField(sp_chretspeed);
                    EditorGUILayout.PropertyField(sp_NoddingTransitions);

                    if (Get.NoddingTransitions != 0f)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(sp_NodAxis);
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(sp_BackBonesNod);
                        EditorGUI.indentLevel--;
                    }

                    if (Get.MaximumDistance < 0f)
                    {
                        Get.MaximumDistance = 0f;
                        EditorUtility.SetDirty(target);
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(4f);

                    if (!wrongLimit)
                        GUI.color = new Color(0.55f, 0.9f, 0.75f, 0.8f);
                    else
                        GUI.color = new Color(0.9f, 0.55f, 0.55f, 0.8f);

                    // X
                    GUILayout.BeginVertical(FEditor_Styles.Emerald);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("  Clamp Angle Horizontal (X)", GUILayout.MaxWidth(170f));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(Mathf.Round(Get.XRotationLimits.x) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
                    //FEditor_CustomInspectorHelpers.DrawMinMaxSphere(Get.XRotationLimits.x, Get.XRotationLimits.y, 14, Get.XElasticRange);
                    FEditor_CustomInspectorHelpers.DrawMinMaxSphere(Get.XRotationLimits.x, Get.XRotationLimits.y, 14, Get.XElasticRange);
                    GUILayout.Label(Mathf.Round(Get.XRotationLimits.y) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.MinMaxSlider(ref Get.XRotationLimits.x, ref Get.XRotationLimits.y, -180f, 180f);

                    //if (Mathf.Abs(Get.XRotationLimits.x) > Get.MaxRotationDiffrence) Get.MaxRotationDiffrence = Mathf.Abs(Get.XRotationLimits.x);
                    //if (Mathf.Abs(Get.XRotationLimits.y) > Get.MaxRotationDiffrence) Get.MaxRotationDiffrence = Mathf.Abs(Get.XRotationLimits.y);

                    bothX = EditorGUILayout.Slider("Adjust symmetrical", bothX, 1f, 180f);
                    EditorGUILayout.PropertyField(sp_elasticX);

                    if (lastBothX != bothX)
                    {
                        Get.XRotationLimits.x = -bothX;
                        Get.XRotationLimits.y = bothX;
                        lastBothX = bothX;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }

                    GUILayout.EndVertical();

                    GUILayout.Space(7f);

                    GUI.color = new Color(0.6f, 0.75f, 0.9f, 0.8f);

                    // Y
                    GUILayout.BeginVertical(FEditor_Styles.LBlueBackground);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("  Clamp Angle Vertical (Y)", GUILayout.MaxWidth(170f));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(Mathf.Round(Get.YRotationLimits.x) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
                    FEditor_CustomInspectorHelpers.DrawMinMaxVertSphere(-Get.YRotationLimits.y, -Get.YRotationLimits.x, 14, Get.YElasticRange);
                    GUILayout.Label(Mathf.Round(Get.YRotationLimits.y) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.MinMaxSlider(ref Get.YRotationLimits.x, ref Get.YRotationLimits.y, -90f, 90f);
                    bothY = EditorGUILayout.Slider("Adjust symmetrical", bothY, 1f, 90f);
                    EditorGUILayout.PropertyField(sp_elasticY);

                    if (lastBothY != bothY)
                    {
                        Get.YRotationLimits.x = -bothY;
                        Get.YRotationLimits.y = bothY;
                        lastBothY = bothY;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }

                    GUILayout.EndVertical();

                    #endregion

                    GUI.color = preCol;

                    GUILayout.Space(15f);

                    GUILayout.BeginHorizontal(FEditor_Styles.BlueBackground);
                    EditorGUILayout.HelpBox("If this angle is exceeded character will look forward", MessageType.None);
                    GUILayout.EndHorizontal();

                    if (!wrongLimit)
                        GUI.color = new Color(0.55f, 0.9f, 0.75f, 0.8f);
                    else
                        GUI.color = new Color(0.9f, 0.55f, 0.55f, 0.8f);

                    EditorGUILayout.BeginVertical(FEditor_Styles.Emerald);

                    GUILayout.BeginHorizontal();

                    Get.StopLookingAbove = EditorGUILayout.Slider("Max Angle Diff", Get.StopLookingAbove, 25f, 180f);
                    FEditor_CustomInspectorHelpers.DrawMinMaxSphere(-Get.StopLookingAbove, Get.StopLookingAbove, 14);
                    GUILayout.EndHorizontal();

                    //Get.QuickerRotateAbove = EditorGUILayout.Slider(new GUIContent("Delta Accelerate", "If head have to rotate more than this value it's animation speed for rotating increases, small touch to make animation more realistic"), Get.QuickerRotateAbove, 25f, 70f);
                    // EditorGUILayout.PropertyField(sp_bigsmooth);

                    GUILayout.Space(8f);

                    EditorGUILayout.EndVertical();
                    GUI.color = preCol;
                    serializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndVertical();
            }

            #endregion

            #endregion


            #region Correcting bones orientations


            EditorGUI.indentLevel--;
            EditorGUILayout.BeginVertical(FEditor_Styles.GrayBackground);
            EditorGUI.indentLevel++;

            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Space(3f);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_fixpres);

                EditorGUILayout.BeginVertical(new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(20) });

                if (GUILayout.Button("▲", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(10) }))
                {
                    if ((int)Get.FixingPreset == 0)
                    {
                        Get.FixingPreset = (FLookAnimator.EFAxisFixOrder)(Enum.GetValues(typeof(FLookAnimator.EFAxisFixOrder)).Length - 1);
                    }
                    else
                        Get.FixingPreset--;

                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("▼", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(10) }))
                {
                    if ((int)Get.FixingPreset + 1 >= Enum.GetValues(typeof(FLookAnimator.EFAxisFixOrder)).Length)
                    {
                        Get.FixingPreset = 0;
                    }
                    else
                        Get.FixingPreset++;

                    EditorUtility.SetDirty(target);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(3f);

                if (Get.FixingPreset == FLookAnimator.EFAxisFixOrder.FullManual)
                {
                    EditorGUILayout.PropertyField(sp_axesmul);
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(sp_manfromax);
                    EditorGUILayout.PropertyField(sp_mantoax);
                    GUILayout.Space(5f);
                }

                if (Get.FixingPreset == FLookAnimator.EFAxisFixOrder.FromBased)
                {
                    EditorGUILayout.LabelField("Auto Offset: " + RoundVector(Get.OffsetAuto));
                    EditorGUILayout.LabelField("Auto From Axis: " + RoundVector(Get.FromAuto));
                }

                EditorGUILayout.PropertyField(sp_angoff);
                EditorGUILayout.PropertyField(sp_backoff);
                GUILayout.Space(5f);

                EditorGUILayout.PropertyField(sp_dray);
                GUILayout.Space(5f);

                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
            }

            EditorGUILayout.EndVertical();


            #endregion


            #region Additional parameters


            EditorGUI.indentLevel--;
            EditorGUILayout.BeginVertical(FEditor_Styles.GrayBackground);
            EditorGUI.indentLevel++;

            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Space(5f);
                EditorGUIUtility.labelWidth = 140;

                //Get.BlendToOriginal = EditorGUILayout.Slider(new GUIContent("Animation Blend", "Main blend value for new head look rotations"), Get.BlendToOriginal, 0f, 1f);
                EditorGUILayout.PropertyField(sp_blend);

                EditorGUIUtility.labelWidth = 180;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(sp_eyespos);

                EditorGUILayout.PropertyField(sp_ancheyes);

                if (Get.AnchorStartLookPoint)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(sp_anchrefr);
                    EditorGUI.indentLevel--;
                }

                //GUILayout.Space(3f);
                //EditorGUIUtility.labelWidth = 140;
                //EditorGUILayout.PropertyField(sp_leadblend);

                GUILayout.Space(3f);

                //bool wrong = false;
                //if (!animatorDetected) if (Get.SyncWithAnimator) wrong = true;

                EditorGUILayout.BeginVertical(FEditor_Styles.GreenBackground);
                EditorGUIUtility.labelWidth = 160;

                Color preCol = GUI.color;
                //if (wrong)
                //{
                //    EditorGUILayout.HelpBox("Component can't find animator attached to your character, you should untoggle this variable if there isn't any animator working on character's animation", MessageType.Warning);
                //    GUI.color = new Color(1f, 1f, 0.35f, 0.8f);
                //}

                //EditorGUILayout.PropertyField(sp_animwithsource);

                //if (Get.SyncWithAnimator)
                //{
                //    EditorGUI.indentLevel++;
                //    EditorGUILayout.PropertyField(sp_monitor);
                //    EditorGUI.indentLevel--;
                //}

                GUI.color = preCol;


                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.EndVertical();

                GUILayout.Space(5f);


                // v1.0.7 - Bird Mode
                EditorGUILayout.BeginVertical(FEditor_Styles.LBlueBackground);
                EditorGUIUtility.labelWidth = 160;

                EditorGUILayout.PropertyField(sp_bird);

                if (Get.BirdMode)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(sp_birdlag);
                    //EditorGUILayout.PropertyField(sp_birdspd);
                    //EditorGUILayout.PropertyField(sp_birdlagprog);
                    EditorGUILayout.PropertyField(sp_birdfreq);
                    EditorGUILayout.PropertyField(sp_birddel);
                    EditorGUILayout.PropertyField(sp_birmaxdist);
                    EditorGUILayout.PropertyField(sp_birddelgospeed);
                    EditorGUI.indentLevel--;
                }

                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);


                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal(FEditor_Styles.BlueBackground);
                EditorGUILayout.HelpBox("If you don't want arms to be rotated when spine, bone is rotated by script", MessageType.None);
                GUILayout.EndHorizontal();

                #region Supporting list in custom editor

                if (Get.CompensationBones != null)
                    compensationBonesCount = Get.CompensationBones.Count;
                else
                    compensationBonesCount = 0;

                GUILayout.BeginHorizontal();
                compensationBonesCount = EditorGUILayout.IntField("Compensation Bones", compensationBonesCount);
                if (GUILayout.Button(new GUIContent("Find", "By pressing this button, algorithm will try to find bones with names 'Shoulder', 'Upper Arm' be aware, because you can have other objects inside including this names"), new GUILayoutOption[2] { GUILayout.MaxWidth(58), GUILayout.MaxHeight(14) })) FindCompensationBones(Get);
                if (GUILayout.Button("+", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(14) }))
                {
                    serializedObject.ApplyModifiedProperties();
                    compensationBonesCount++;
                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("-", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(14) }))
                {
                    if (Get.CompensationBones.Count == 1)
                    {
                        Get.CompensationBones = new List<FLookAnimator.CompensationBone>();
                        compensationBonesCount = 0;
                        //Get.BackBonesCount = 0;
                    }
                    else
                        compensationBonesCount--;

                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }

                GUILayout.EndHorizontal();

                if (compensationBonesCount <= 0) compensationBonesCount = 0;
                else
                {
                    if (compensationBonesCount > Get.CompensationBones.Count)
                    {
                        for (int i = Get.CompensationBones.Count; i < compensationBonesCount; i++) Get.CompensationBones.Add(null);
                    }
                    else if (compensationBonesCount < Get.CompensationBones.Count)
                    {
                        for (int i = Get.CompensationBones.Count - 1; i >= compensationBonesCount; i--) Get.CompensationBones.RemoveAt(i);
                    }

                    if (Get.CompensationBones.Count > 0)
                    {
                        EditorGUI.indentLevel++;

                        for (int i = 0; i < Get.CompensationBones.Count; i++)
                        {
                            Get.CompensationBones[i].Transform = (Transform)EditorGUILayout.ObjectField("Bone [" + i + "]", Get.CompensationBones[i].Transform, typeof(Transform), true);
                        }

                        EditorGUI.indentLevel--;

                        GUILayout.Space(5f);

                        EditorGUILayout.PropertyField(sp_compensblend);
                        EditorGUILayout.PropertyField(sp_poscompens);
                    }
                }

                #endregion

                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
            }

            EditorGUILayout.EndVertical();


            #endregion


            #region Optional parameters


            EditorGUI.indentLevel--;
            EditorGUILayout.BeginVertical(FEditor_Styles.GrayBackground);
            EditorGUI.indentLevel++;

            drawOptional = EditorGUILayout.Foldout(drawOptional, "Optional parameters", true);

            if (drawOptional)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Space(10f);

                EditorGUILayout.PropertyField(sp_weighmul);

                GUILayout.Space(3f);

                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("If animation is not animating some head bones (very rare case)", MessageType.None);
                GUILayout.EndHorizontal();

                //EditorGUILayout.PropertyField(sp_DetectZeroKeyframes);
                EditorGUILayout.PropertyField(sp_animphys);

                GUILayout.Space(3f);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_OptimizeWithMesh);
                if (GUILayout.Button("Find", new GUILayoutOption[1] { GUILayout.Width(44) }))
                {
                    if (Get.OptimizeWithMesh == null)
                    {
                        Get.OptimizeWithMesh = Get.transform.GetComponent<Renderer>();
                        if (!Get.OptimizeWithMesh) Get.OptimizeWithMesh = Get.transform.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) Get.OptimizeWithMesh = Get.transform.parent.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) if (Get.transform.parent.parent != null) Get.OptimizeWithMesh = Get.transform.parent.parent.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) if (Get.transform.parent.parent != null) if (Get.transform.parent.parent.parent != null) Get.OptimizeWithMesh = Get.transform.parent.parent.parent.GetComponentInChildren<Renderer>();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);

                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
            }

            EditorGUILayout.EndVertical();

            #endregion

            EditorGUILayout.EndVertical();
        }




    }

}