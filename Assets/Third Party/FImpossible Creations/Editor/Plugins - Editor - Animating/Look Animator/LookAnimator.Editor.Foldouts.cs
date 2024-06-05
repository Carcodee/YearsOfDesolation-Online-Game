using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator_Editor
    {
        bool drawAdditionalSetup = true;
        private void Fold_DrawAdditionalSetup()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(drawAdditionalSetup, 10, "►") + "  Optimization & More", FGUI_Resources.TexAddIcon), FGUI_Resources.FoldStyle)) drawAdditionalSetup = !drawAdditionalSetup;

            if (drawAdditionalSetup)
            {
                if (animatorAnimPhys && Get.AnimatePhysics == false)
                {
                    FGUI_Inspector.DrawWarning(" Unity's Animator is using 'Animate Physics'!");
                    GUI.color = new Color(.9f, .9f, 0.6f, 1f);
                }

                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_animphys);
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(sp_starttpose);
                EditorGUILayout.EndHorizontal();

                GUI.color = Color.white;
                GUILayout.Space(3);
                El_DrawOptimizeWithMesh();
                //GUILayout.Space(2);
                //if (Get.DetectZeroKeyframes == 0) GUI.color = unchangedC;
                //EditorGUILayout.PropertyField(sp_DetectZeroKeyframes);
                //GUI.color = c;
                GUILayout.Space(4f);
            }


            GUILayout.EndVertical();
            GUILayout.Space(-5);
        }


        static bool drawBackBones = true;
        private void Fold_DrawBackBones()
        {

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            GUILayout.BeginHorizontal();
            int preCount = Get.BackBonesCount;

            EditorGUIUtility.labelWidth = 130f;
            //EditorGUILayout.LabelField(new GUIContent(""Additional Modules" Spine Bones: (" + Get.BackBonesCount + ")"), EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(drawBackBones, 10, "►") + "  Additional Spine Bones: (" + Get.BackBonesCount + ")", _TexBackbonesIcon, "Adding more neck/spine bones to look animation chain, using this feature can give you much more natural looking effects!\n\nBeware of neck bones for stoop pose, adjust weight of neck bone if needed or go to 'Backing Offset' in 'Corrections' Tab."), FGUI_Resources.FoldStyle)) drawBackBones = !drawBackBones;
            EditorGUIUtility.labelWidth = 0f;


            if (GUILayout.Button("+", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(18) }))
            {
                Get.BackBonesCount++;
                serializedObject.ApplyModifiedProperties();
                UpdateCustomInspector(Get, true);// Debug.Log("[LOOK ANIMATOR] Don't change backbones count in playmode");
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("-", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(18) }))
            {
                if (Get.BackBonesCount > 0)
                {
                    Get.BackBonesCount--;
                    serializedObject.ApplyModifiedProperties();
                    UpdateCustomInspector(Get, true);// Debug.Log("[LOOK ANIMATOR] Don't change backbones count in playmode");
                    EditorUtility.SetDirty(target);
                }
            }

            GUILayout.EndHorizontal();
            int spc = 4;
            if (drawBackBones)
            {
                GUILayout.Space(6f);

                if (Get.BackBonesCount < 0) Get.BackBonesCount = 0;
                if (preCount != Get.BackBonesCount) UpdateCustomInspector(Get);


                #region Backbones array


                if (Get.BigAngleAutomation)
                {
                    if (drawBigAngleWeightsSettings) EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); else EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);
                    GUILayout.Space(3);
                    GUI.color = new Color(1f, 1f, 1f, 0.75f);
                    EditorGUILayout.LabelField(drawBigAngleWeightsSettings ? Lang("Weight when character looking far back") : Lang("Default look angle bone weights"), new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
                    GUI.color = c;
                    GUILayout.Space(5);
                }
                else drawBigAngleWeightsSettings = false;



                EditorGUILayout.BeginVertical();
                // Draw leading bone weight settings
                EditorGUILayout.BeginHorizontal();
                GUIStyle rightAlignText = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };

                if (Get.LookBones != null)
                    if (Get.LookBones.Count > 0)
                    {
                        GUILayout.Label("Lead Bone", GUILayout.Width(70));
                        GUI.enabled = false; EditorGUILayout.ObjectField(Get.LookBones[0].Transform, typeof(Transform), true); GUI.enabled = true;

                        if (drawBigAngleWeightsSettings)
                        {
                            Get.LookBones[0].lookWeightB = GUILayout.HorizontalSlider(Get.LookBones[0].lookWeightB, 0f, 1f, GUILayout.Width(50f));
                            if (Application.isPlaying) GUILayout.Label("(" + System.Math.Round(Get.LookBones[0].motionWeight, 1) + ")", rightAlignText, GUILayout.Width(31));
                            GUILayout.Label(" " + System.Math.Round(Get.LookBones[0].lookWeightB * 100f) + "%", rightAlignText, GUILayout.Width(38));
                        }
                        else
                        {
                            Get.LookBones[0].lookWeight = GUILayout.HorizontalSlider(Get.LookBones[0].lookWeight, 0f, 1f, GUILayout.Width(50f));
                            if (Application.isPlaying) GUILayout.Label("(" + System.Math.Round(Get.LookBones[0].motionWeight, 1) + ")", rightAlignText, GUILayout.Width(31));
                            GUILayout.Label(" " + System.Math.Round(Get.LookBones[0].lookWeight * 100f) + "%", rightAlignText, GUILayout.Width(38));
                        }
                    }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                GUILayout.Space(3);

                if (Get.BackBonesCount > 0)
                {
                    //EditorGUILayout.EndVertical();
                    GUI.color = new Color(1f, 0.94f, 0.94f, 0.5f);
                    //EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
                    EditorGUILayout.BeginVertical();
                    GUI.color = c;

                    EditorGUIUtility.labelWidth = 138f; EditorGUIUtility.fieldWidth = 33f;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sp_AutoBackbonesWeights, new GUIContent("Auto Weights Motion", sp_AutoBackbonesWeights.tooltip), GUILayout.Width(159));
                    if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); /*if (Get.AutoBackbonesWeights) Get.CurveSpread = false;*/ }
                    EditorGUIUtility.labelWidth = 0f; EditorGUIUtility.fieldWidth = 0f;

                    // Drawing Falloff Slider
                    if (!Get.BigAngleAutomation)
                    {
                        if (!Get.CurveSpread)
                            if (Get.AutoBackbonesWeights)
                            {
                                GUILayout.Label(new GUIContent("Falloff", sp_fallvall.tooltip), GUILayout.Width(42));
                                EditorGUI.BeginChangeCheck();
                                Get.FaloffValue = GUILayout.HorizontalSlider(Get.FaloffValue, 0f, 1f);
                                if (EditorGUI.EndChangeCheck()) { Get.SetAutoWeightsDefault(); serializedObject.ApplyModifiedProperties(); }
                                GUILayout.Label(new GUIContent(" ", sp_fallvall.tooltip), GUILayout.Width(8));
                            }
                    }
                    else // Falloff sliders for and b values
                    {
                        if (Get.AutoBackbonesWeights)
                        {
                            if (drawBigAngleWeightsSettings)
                            {
                                GUILayout.Label(new GUIContent("Falloff", sp_fallvall.tooltip), GUILayout.Width(42));
                                EditorGUI.BeginChangeCheck();
                                Get.FaloffValueB = GUILayout.HorizontalSlider(Get.FaloffValueB, 0f, 1.85f);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    float[] tgt = Get.CalculateRotationWeights(Get.FaloffValueB);
                                    for (int i = 1; i < Get.LookBones.Count; i++) Get.LookBones[i].lookWeightB = tgt[i];
                                    serializedObject.ApplyModifiedProperties();
                                }

                                GUILayout.Label(new GUIContent(" ", sp_fallvall.tooltip), GUILayout.Width(8));
                            }
                            else
                            {
                                GUILayout.Label(new GUIContent("Falloff", sp_fallvall.tooltip), GUILayout.Width(42));
                                EditorGUI.BeginChangeCheck();
                                Get.FaloffValue = GUILayout.HorizontalSlider(Get.FaloffValue, 0f, 1f);
                                if (EditorGUI.EndChangeCheck()) { Get.SetAutoWeightsDefault(); serializedObject.ApplyModifiedProperties(); }
                                GUILayout.Label(new GUIContent(" ", sp_fallvall.tooltip), GUILayout.Width(8));
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    // Draw backbones list
                    for (int i = 1; i < Get.LookBones.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();

                        if (animator)
                            if (animator.isHuman)
                                if (animator.GetBoneTransform(HumanBodyBones.Hips) == Get.LookBones[i].Transform) GUI.color = new Color(1f, 1f, 0.7f, 1f);


                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false; GUILayout.Label("[" + i + "]", GUILayout.Width(20)); GUI.enabled = true;
                        Get.LookBones[i].Transform = (Transform)EditorGUILayout.ObjectField(Get.LookBones[i].Transform, typeof(Transform), true);
                        if (Get.AutoBackbonesWeights == false) GUI.enabled = true; else GUI.enabled = false;

                        if (drawBigAngleWeightsSettings)
                        {
                            Get.LookBones[i].lookWeightB = GUILayout.HorizontalSlider(Get.LookBones[i].lookWeightB, 0f, 1f, GUILayout.Width(50f));
                            if (Application.isPlaying) GUILayout.Label("(" + System.Math.Round(Get.LookBones[i].motionWeight, 1) + ")", rightAlignText, GUILayout.Width(31));
                            GUILayout.Label(" " + System.Math.Round(Get.LookBones[i].lookWeightB * 100f) + "%", rightAlignText, GUILayout.Width(38));
                        }
                        else
                        {
                            Get.LookBones[i].lookWeight = GUILayout.HorizontalSlider(Get.LookBones[i].lookWeight, 0f, 1f, GUILayout.Width(50f));
                            if (Get.BigAngleAutomation) if (Application.isPlaying) GUILayout.Label("(" + System.Math.Round(Get.LookBones[i].motionWeight, 1) + ")", rightAlignText, GUILayout.Width(31));
                            GUILayout.Label(" " + System.Math.Round(Get.LookBones[i].lookWeight * 100f) + "%", rightAlignText, GUILayout.Width(38));
                        }

                        EditorGUILayout.EndHorizontal();
                        GUI.enabled = true;

                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(Get);
                        }
                    }


                    GUI.color = c;
                    EditorGUIUtility.labelWidth = 0f;


                    #endregion

                    if (Get.BigAngleAutomation) EditorGUILayout.EndVertical();

                    GUILayout.Space(9f);

                    if (Get.BigAngleAutomation)
                        EditorGUIUtility.labelWidth = 145f;
                    else
                        EditorGUIUtility.labelWidth = 150f;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sp_BigAngleAutomation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(Get);
                        serializedObject.ApplyModifiedProperties();
                        float[] tgt = Get.CalculateRotationWeights(Get.FaloffValueB); Debug.Log("diupa");
                        for (int i = 1; i < Get.LookBones.Count; i++) Get.LookBones[i].lookWeightB = tgt[i];
                        if (Get.BigAngleAutomation) Get.CurveSpread = false; Get.UpdateAutomationWeights();
                    }

                    if (Get.BigAngleAutomation)
                    {
                        string bigAngleSwitchTitle = drawBigAngleWeightsSettings ? "Show Default Weights" : "Show Big Angle Weights";
                        if (GUILayout.Button(bigAngleSwitchTitle)) drawBigAngleWeightsSettings = !drawBigAngleWeightsSettings;
                    }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5f);
                    EditorGUIUtility.labelWidth = 0f;
                    Fold_DrawAdvancedBackBones();
                    EditorGUILayout.EndVertical();
                    spc = 0;
                }
                else
                {

                    GUILayout.Space(4f);
                    EditorGUILayout.HelpBox("Hit '+' to assign spine bones for additional look motion.", MessageType.None);
                    GUILayout.Space(4f);

                }
            }

            GUILayout.Space(spc);

            EditorGUILayout.EndVertical();
        }

        public bool drawBigAngleWeightsSettings = false;
        public bool drawBigAngleCompensationSettings = false;

        private void Fold_DrawAdvancedBackBones()
        {
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            if (Get.CurveSpread)
            {
                EditorGUIUtility.labelWidth = 100f;
                EditorGUILayout.PropertyField(sp_usecurve);
            }
            else
            {
                EditorGUIUtility.labelWidth = 182f;
                EditorGUILayout.PropertyField(sp_usecurve, new GUIContent("Curve Spread (Deprecated)", sp_usecurve.tooltip));
            }
            if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); if (Get.CurveSpread) { Get.AutoBackbonesWeights = true; Get.BigAngleAutomation = false; } }

            EditorGUIUtility.labelWidth = 0f;

            if (Get.CurveSpread)
            {
                if (!Get.AutoBackbonesWeights) GUI.enabled = false;
                EditorGUILayout.PropertyField(sp_falloff, new GUIContent(""), new GUILayoutOption[1] { GUILayout.Height(18f) });
                if (!Get.AutoBackbonesWeights) GUI.enabled = true;

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

            GUILayout.EndHorizontal();
        }


        static bool drawCompens = false;
        private void Fold_DrawCompensation()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

            if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(drawCompens, 10, "►") + "  Bones Compensation Settings (" + Get.CompensationBones.Count + ")", _TexCompensationIcon, "If you don't want arms to be rotated when character is looking up/down with big angle"), FGUI_Resources.FoldStyle)) drawCompens = !drawCompens;

            if (drawCompens)
            {
                if (compensationBonesCount <= 0)
                    GUILayout.Space(6f);
                else
                    GUILayout.Space(3f);

                #region Compensation bones list

                if (Get.CompensationBones != null)
                    compensationBonesCount = Get.CompensationBones.Count;
                else
                    compensationBonesCount = 0;


                if (compensationBonesCount <= 0)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.7f);
                    EditorGUILayout.LabelField(new GUIContent("Not affected bones when spine rotates"), FGUI_Resources.HeaderStyle);
                    GUILayout.Space(3f);
                    GUI.color = c;
                }

                GUILayout.BeginHorizontal();
                //compensationBonesCount = EditorGUILayout.IntField("Compensation Bones", compensationBonesCount);
                if (GUILayout.Button(new GUIContent("Auto Find", "By pressing this button, algorithm will try to find bones with names 'Shoulder', 'Upper Arm' be aware, because you can have other objects inside including this names"), GUILayout.MaxHeight(15))) FindCompensationBones(Get);
                if (GUILayout.Button("+", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(15) }))
                {
                    serializedObject.ApplyModifiedProperties();
                    Get.SetAutoWeightsDefault();
                    compensationBonesCount++;
                    Get.CompensationBones.Add(new FLookAnimator.CompensationBone(null));
                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("-", new GUILayoutOption[2] { GUILayout.MaxWidth(28), GUILayout.MaxHeight(15) }))
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
                GUILayout.Space(7f);


                if (compensationBonesCount <= 0)
                {
                    compensationBonesCount = 0;

                    GUI.color = new Color(1f, 1f, 1f, 0.7f);
                    EditorGUILayout.LabelField("Compensation List Is Empty", FGUI_Resources.HeaderStyle); GUI.color = c;
                    GUILayout.Space(7f);
                }
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
                        //EditorGUI.indentLevel++;

                        for (int i = 0; i < Get.CompensationBones.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUI.enabled = false; GUILayout.Label("[" + (i + 1) + "]", GUILayout.Width(20)); GUI.enabled = true;
                            Get.CompensationBones[i].Transform = (Transform)EditorGUILayout.ObjectField(Get.CompensationBones[i].Transform, typeof(Transform), true);

                            if (GUILayout.Button("X", new GUILayoutOption[2] { GUILayout.MaxWidth(24), GUILayout.MaxHeight(16) }))
                            {
                                Get.CompensationBones.RemoveAt(i);
                                serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(target);
                                return;
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        //EditorGUI.indentLevel--;

                        GUILayout.Space(7f);


                    }
                }

                if (compensationBonesCount > 0)
                {

                    // Compensation sliders
                    if (Get.BigAngleAutomationCompensation)
                    {
                        if (drawBigAngleCompensationSettings) EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); else EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

                        GUILayout.Space(3);
                        GUI.color = new Color(1f, 1f, 1f, 0.75f);
                        EditorGUILayout.LabelField(drawBigAngleCompensationSettings ? Lang("Weight when character looking far back") : Lang("Default look angle bone weights"), new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
                        GUI.color = c;
                        GUILayout.Space(5);
                    }

                    if (!Get.BigAngleAutomationCompensation || !drawBigAngleCompensationSettings)
                    {
                        EditorGUIUtility.labelWidth = 165;
                        EditorGUILayout.PropertyField(sp_compensblend);
                        EditorGUIUtility.labelWidth = 165;
                        EditorGUILayout.PropertyField(sp_poscompens);
                    }
                    else
                    {
                        EditorGUIUtility.labelWidth = 165;
                        EditorGUILayout.PropertyField(sp_compensblendB);
                        EditorGUIUtility.labelWidth = 165;
                        EditorGUILayout.PropertyField(sp_poscompensB);
                    }

                    if (Get.BigAngleAutomationCompensation) GUILayout.EndVertical();


                    #endregion


                    GUILayout.Space(5f);

                    EditorGUILayout.BeginHorizontal();

                    if (Get.BigAngleAutomationCompensation) EditorGUIUtility.labelWidth = 147f; else EditorGUIUtility.labelWidth = 154f;

                    EditorGUILayout.PropertyField(sp_BigAngleAutomationCompensation, new GUIContent("Big Angle Automation", sp_BigAngleAutomationCompensation.tooltip)); ;

                    if (Get.BigAngleAutomationCompensation)
                    {
                        string bigAngleSwitchTitle = drawBigAngleCompensationSettings ? "Show Default Weights" : "Show Big Angle Weights";
                        if (GUILayout.Button(bigAngleSwitchTitle)) drawBigAngleCompensationSettings = !drawBigAngleCompensationSettings;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(5f);

            }

            GUILayout.EndVertical();

        }


        private bool drawTargeting = false;
        private void Fold_DrawTargeting()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            if (GUILayout.Button(new GUIContent(" " + FGUI_Resources.GetFoldSimbol(drawTargeting, 10, "►") + "  " + Lang("Targeting"), FGUI_Resources.TexTargetingIcon, ""), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle)) drawTargeting = !drawTargeting;

            if (drawTargeting)
            {
                GUILayout.Space(4f);
                EditorGUIUtility.labelWidth = 146;
                EditorGUILayout.PropertyField(sp_eyespos);
                GUILayout.Space(2f);
                EditorGUIUtility.labelWidth = 168;
                EditorGUILayout.PropertyField(sp_ancheyes);

                if (Application.isPlaying)
                {
                    if (Get.AnchorStartLookPoint)
                    {
                        GUILayout.Space(2f);
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(sp_anchrefr); GUILayout.Space(2);
                        EditorGUI.indentLevel--;
                    }
                }

                GUILayout.Space(9f);

                EditorGUIUtility.labelWidth = 172;
                if (Get.LookAtPositionSmoother <= 0f) GUI.color = unchangedC;
                EditorGUILayout.PropertyField(sp_LookAtPositionSmoother);
                GUI.color = c; EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(5);
                EditorGUIUtility.labelWidth = 172;
                if (Get.ChangeTargetSmoothing == 0f) GUI.color = unchangedC;
                EditorGUILayout.PropertyField(sp_chretspeed);
                GUI.color = c;
                EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(6);
            }

            GUILayout.EndVertical();
        }


        private bool drawMotionSettings = false;
        private void Fold_DrawAddMotionSettings()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

            if (GUILayout.Button(new GUIContent(" " + FGUI_Resources.GetFoldSimbol(drawMotionSettings, 10, "►") + "  " + Lang("Additional Motion Settings"), FGUI_Resources.TexMotionIcon, ""), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle)) drawMotionSettings = !drawMotionSettings;

            EditorGUIUtility.labelWidth = 140;


            if (drawMotionSettings)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_AnimationStyle);

                if (Get.AnimationStyle != FLookAnimator.EFAnimationStyle.Linear)
                {
                    GUILayout.Space(5f);
                    if (Get.MaxRotationSpeed >= 2.5f) GUI.color = unchangedC;
                    EditorGUILayout.PropertyField(sp_MaxRotationSpeed);
                    GUI.color = c;
                }

                //GUILayout.Space(3f);
                //El_DrawQuickerRotAbove();

                if (Get.ModelForwardAxis == Vector3.forward && Get.ModelUpAxis == Vector3.up)
                {
                    GUILayout.Space(5f);
                    if (Get.BaseRotationCompensation <= 0f) GUI.color = unchangedC;
                    EditorGUIUtility.labelWidth = 178;
                    EditorGUIUtility.fieldWidth = 25;
                    EditorGUILayout.PropertyField(sp_BaseRotationCompensation);
                    EditorGUIUtility.fieldWidth = 0;
                    EditorGUIUtility.labelWidth = 0;
                    GUI.color = c;
                }
                else
                {
                    EditorGUILayout.HelpBox("'Base Rotation Compensation' is not yet avilable when using axis correction!", MessageType.None);
                    GUI.enabled = false;
                    EditorGUIUtility.labelWidth = 178;
                    EditorGUIUtility.fieldWidth = 25;
                    EditorGUILayout.PropertyField(sp_BaseRotationCompensation);
                    EditorGUIUtility.fieldWidth = 0;
                    EditorGUIUtility.labelWidth = 0;
                    GUI.enabled = true;
                }

                GUILayout.Space(5f);
                if (Get.WeightsMultiplier == 1f) GUI.color = unchangedC;
                EditorGUIUtility.labelWidth = 142;
                EditorGUILayout.PropertyField(sp_weighmul);
                EditorGUIUtility.labelWidth = 0;
                GUI.color = c;
                GUILayout.Space(5f);
            }

            EditorGUIUtility.labelWidth = 0;

            GUILayout.EndVertical();
            GUILayout.Space(-5);
        }


        private bool drawBehaviourSettings = true;
        private void Fold_DrawBehaviourSettings()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

            if (GUILayout.Button(new GUIContent(" " + FGUI_Resources.GetFoldSimbol(drawBehaviourSettings, 10, "►") + "  " + Lang("Animation Behaviour"), FGUI_Resources.TexBehaviourIcon, ""), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle)) drawBehaviourSettings = !drawBehaviourSettings;

            if (drawBehaviourSettings)
            {
                GUILayout.Space(6f);
                El_DrawMaxDist();
                GUILayout.Space(6f);

                bool wrongLimit = false;
                if (Mathf.Abs(Get.XRotationLimits.x) > Get.StopLookingAbove) wrongLimit = true;
                if (Mathf.Abs(Get.XRotationLimits.y) > Get.StopLookingAbove) wrongLimit = true;
                El_DrawLimitMaxAngle(wrongLimit);
                GUI.color = c;

                EditorGUIUtility.labelWidth = 148;
                GUILayout.Space(-2);
                El_DrawLookWhenAbove();
                GUILayout.Space(5);
                EditorGUIUtility.labelWidth = 180;
                if (Get.HoldRotateToOppositeUntil <= 0f) GUI.color = unchangedC;
                EditorGUILayout.PropertyField(sp_HoldRotateToOppositeUntil);
                GUI.color = c;
                GUILayout.Space(5);
                EditorGUIUtility.labelWidth = 0;
            }

            GUILayout.EndVertical();
        }


        static bool drawBirdMode = true;
        private void Fold_DrawBirdMode()
        {

            GUILayout.BeginHorizontal();

            if (Get.BirdMode)
            {
                if (GUILayout.Button(new GUIContent(" " + FGUI_Resources.GetFoldSimbol(drawBirdMode, 10, "►") + "  " + Lang("Bird Mode"), _TexBirdIcon, sp_bird.tooltip), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle)) drawBirdMode = !drawBirdMode;
                Get.BirdMode = EditorGUILayout.Toggle(Get.BirdMode, GUILayout.Width(16));
            }
            else
            {
                GUILayout.Button(new GUIContent("  " + Lang("Bird Mode"), _TexBirdIcon, sp_bird.tooltip), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle);
                Get.BirdMode = EditorGUILayout.Toggle(Get.BirdMode, GUILayout.Width(16));
            }

            GUILayout.EndHorizontal();

            if (drawBirdMode && Get.BirdMode)
            {
                //FGUI_Inspector.DrawUILine(0.1f, 0.8f, 1, 4, 1f);
                if (Get.RotationSpeed < 1.85f)
                {
                    GUILayout.Space(3f);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("To make bird mode look better increase 'Rotation Speed' Parameter under Tweaking Tab", MessageType.Warning);
                    if (GUILayout.Button("Adjust", new GUILayoutOption[] { GUILayout.Width(48), GUILayout.Height(38) })) { Get.MaxRotationSpeed = 2.5f; Get.RotationSpeed = 2.35f; }
                    GUILayout.EndHorizontal();
                }
                else
                    if (Get.MaxRotationSpeed < 1.8f)
                {
                    GUILayout.Space(3f);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("To make bird mode look better increase 'Max Rotation Speed' Parameter under 'Additional Motion Settings' Tab", MessageType.Info);
                    if (GUILayout.Button("Adjust", new GUILayoutOption[] { GUILayout.Width(48), GUILayout.Height(38) })) { Get.MaxRotationSpeed = 2.5f; }
                    GUILayout.EndHorizontal();
                }


                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_birdlag);
                EditorGUILayout.PropertyField(sp_birdfreq);
                FGUI_Inspector.DrawUILine(0.1f, 0.05f, 1, 12, 0.95f);
                EditorGUILayout.PropertyField(sp_birddel);

                if (Get.DelayPosition > 0f)
                {
                    EditorGUILayout.PropertyField(sp_birmaxdist);
                    EditorGUILayout.PropertyField(sp_birddelgospeed);
                }

                GUILayout.Space(5f);
            }
        }


        static bool drawEyesSettings = true;
        private void Fold_DrawEyesModule()
        {

            GUILayout.BeginHorizontal();

            if (Get.UseEyes)
            {
                if (GUILayout.Button(new GUIContent(" " + FGUI_Resources.GetFoldSimbol(drawEyesSettings, 10, "►") + "  " + Lang("Eyes Module"), _TexEyesIcon, "Enabling possibility for using eye bones with look animator, check my other package 'Eyes Animator' for more advanced motion for eyes"), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle)) drawEyesSettings = !drawEyesSettings;
                Get.UseEyes = EditorGUILayout.Toggle(Get.UseEyes, GUILayout.Width(16));
            }
            else
            {
                GUILayout.Button(new GUIContent("  " + Lang("Eyes Module"), _TexEyesIcon, "Enabling possibility for using eye bones with look animator, check my other package 'Eyes Animator' for more advanced motion for eyes"), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle);
                Get.UseEyes = EditorGUILayout.Toggle(Get.UseEyes, GUILayout.Width(16));
            }

            GUILayout.EndHorizontal();

            if (drawEyesSettings && Get.UseEyes)
            {
                GUILayout.Space(5f);
                Fold_Eyes_DrawSetup();

                Fold_Eyes_DrawMotion();

                GUILayout.Space(1f);
                EditorGUILayout.HelpBox("Eyes clamp ranges limit settings are inside '" + Lang("Character Setup") + "' tab", MessageType.None);
                GUILayout.Space(4f);
            }

            GUILayout.EndVertical();
        }


        private bool drawEyesSetup = false;
        private bool drawEyesIndiv = false;
        private void Fold_Eyes_DrawSetup()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawEyesSetup, 10, "►") + "  " + Lang("Character Setup"), FGUI_Resources.Tex_MiniGear, ""), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle)) drawEyesSetup = !drawEyesSetup;

            EditorGUIUtility.labelWidth = 140;

            if (drawEyesSetup)
            {
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 55;
                EditorGUILayout.PropertyField(sp_eyeL);
                EditorGUILayout.LabelField(" ", GUILayout.Width(4));
                EditorGUILayout.PropertyField(sp_eyeR);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 98;
                EditorGUILayout.PropertyField(sp_eyeLInv);
                EditorGUILayout.LabelField(" ", GUILayout.Width(4));
                EditorGUILayout.PropertyField(sp_eyeRInv);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("►", EditorStyles.label, GUILayout.Width(18))) drawEyesIndiv = !drawEyesIndiv;
                EditorGUILayout.PropertyField(sp_EyesOffsetRotation);
                EditorGUILayout.EndHorizontal();

                if (drawEyesIndiv)
                {
                    var sp = sp_EyesOffsetRotation.Copy();
                    EditorGUI.indentLevel++;
                    sp.Next(false); EditorGUILayout.PropertyField(sp);
                    sp.Next(false); EditorGUILayout.PropertyField(sp);
                    EditorGUI.indentLevel--;
                    GUILayout.Space(3);
                }

                GUILayout.Space(3);



                EditorGUILayout.BeginHorizontal();

                if (!Get.HeadReference)
                {
                    GUI.color = new Color(1f, 1f, .65f, 1f);
                    EditorGUILayout.PropertyField(sp_head);
                    GUI.color = c;

                    GUI.enabled = false;
                    EditorGUILayout.LabelField("(", GUILayout.Width(8));
                    EditorGUILayout.ObjectField(Get.LeadBone, typeof(Transform), true, GUILayout.Width(80));
                    EditorGUILayout.LabelField(")", GUILayout.Width(8));
                    GUI.enabled = true;
                }
                else
                    EditorGUILayout.PropertyField(sp_head);


                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
            }

            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndVertical();
        }



        private bool drawEyesMotion = true;
        private void Fold_Eyes_DrawMotion()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawEyesMotion, 10, "►") + "  " + Lang("Additional Motion Settings"), FGUI_Resources.Tex_MiniMotion, ""), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle)) drawEyesMotion = !drawEyesMotion;

            EditorGUIUtility.labelWidth = 100;

            if (drawEyesMotion)
            {
                GUILayout.Space(7f);


                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(sp_eyesTarget);
                if (!Get.EyesTarget)
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("(", GUILayout.Width(8));
                    EditorGUILayout.ObjectField(Get.ObjectToFollow, typeof(Transform), true, GUILayout.Width(80));
                    EditorGUILayout.LabelField(")", GUILayout.Width(8));
                    GUI.enabled = true;
                }

                EditorGUILayout.EndHorizontal();


                EditorGUILayout.PropertyField(sp_eyesSpeed);
                EditorGUILayout.PropertyField(sp_eyesBlend);

                GUILayout.Space(5);

                EditorGUIUtility.labelWidth = 144;
                EditorGUILayout.PropertyField(sp_EyesNoKeyframes);
                EditorGUIUtility.labelWidth = 0;
            }


            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndVertical();
        }



        static bool drawMomentTargets = false;
        private void Fold_DrawMomentTargets()
        {


            if (GUILayout.Button(new GUIContent(" " + FGUI_Resources.GetFoldSimbol(drawMomentTargets, 10, "►") + "  " + "Moment Targets", FGUI_Resources.TexWaitIcon, sp_bird.tooltip), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle, GUILayout.Height(26))) drawMomentTargets = !drawMomentTargets;

            if (drawMomentTargets)
            {
                if (!Application.isPlaying)
                    EditorGUILayout.HelpBox("You can use momoent targets through custom coding or events using methods like 'SetMomentTarget' etc.", MessageType.Info);
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("Moment Target", Get.MomentLookTransform, typeof(Transform), true);
                    GUI.enabled = true;


                }

                GUILayout.Space(5f);
                EditorGUIUtility.labelWidth = 278;
                EditorGUILayout.PropertyField(sp_DestroyMomentTransformOnMaxDistance);
                EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(5f);
            }
        }


        bool drawHidden = false;
        private void Fold_DrawHidden()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(drawHidden, 10, "►") + "  " + Lang("Hidden Rare Settings"), FGUI_Resources.Tex_HiddenIcon), FGUI_Resources.FoldStyle)) drawHidden = !drawHidden;

            if (drawHidden)
            {
                GUILayout.Space(5);

                EditorGUILayout.PropertyField(sp_ModelForwardAxis);
                EditorGUILayout.PropertyField(sp_ModelUpAxis);


                GUILayout.Space(5f);
                EditorGUIUtility.labelWidth = 278;
                EditorGUILayout.PropertyField(sp_DestroyMomentTransformOnMaxDistance);
                EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(6f);
                //EditorGUILayout.PropertyField(sp_AnimationStyle);
                EditorGUILayout.PropertyField(sp_DeltaType);
                EditorGUILayout.PropertyField(sp_SimulationSpeed);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OverrideRotations"));
                EditorGUIUtility.labelWidth = 262;
                var sp = serializedObject.FindProperty("OverrideHeadForPerfectLookDirection");
                EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
                sp.Next(false);
                EditorGUILayout.PropertyField(sp);

                GUILayout.Space(4f);
            }

            GUILayout.Space(2f);

            GUILayout.EndVertical();
        }


        bool drawCorrRots = false;
        private void Fold_DrawBoneCorrectionRotations()
        {
            //GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(drawCorrRots, 10, "►") + "  " + Lang("Bones Rotation Corrections"), FGUI_Resources.Tex_GearMain), FGUI_Resources.FoldStyle, new GUILayoutOption[] { GUILayout.Height(24) })) drawCorrRots = !drawCorrRots;

            if (drawCorrRots)
            {
                GUILayout.Space(3);

                for (int i = 0; i < Get.LookBones.Count; i++)
                {
                    //string name = Get.LookBones[i].Transform.name.Substring(0, Mathf.Min(16, Get.LookBones[i].Transform.name.Length));
                    //Get.LookBones[i].correctionOffset = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent(name, Get.LookBones[i].Transform.name), Get.LookBones[i].correctionOffset.eulerAngles));

                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(Get.LookBones[i].Transform, typeof(Transform), true);
                    GUI.enabled = true;
                    Get.LookBones[i].correctionOffset = EditorGUILayout.Vector3Field("", Get.LookBones[i].correctionOffset);
                    //Get.LookBones[i].correctionOffset = Quaternion.Euler(EditorGUILayout.Vector3Field("", Get.LookBones[i].correctionOffset.eulerAngles));
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(1);
                }

                GUILayout.Space(2f);
            }

            GUILayout.Space(2f);

            //GUILayout.EndVertical();
        }

    }

}