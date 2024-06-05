using FIMSpace.FEditor;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator_Editor
    {
        private bool drawHeaderFoldout = false;
        private void HeaderBoxMain(string title, ref bool drawGizmos, ref bool defaultInspector, Texture2D scrIcon, MonoBehaviour target, int height = 22)
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(scrIcon), EditorStyles.label, new GUILayoutOption[2] { GUILayout.Width(height - 2), GUILayout.Height(height - 2) }))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(target);
                if (script) EditorGUIUtility.PingObject(script);
                drawHeaderFoldout = !drawHeaderFoldout;
            }

            if (GUILayout.Button(title, FGUI_Resources.GetTextStyle(14, true, TextAnchor.MiddleLeft), GUILayout.Height(height)))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(target);
                if (script) EditorGUIUtility.PingObject(script);
                drawHeaderFoldout = !drawHeaderFoldout;
            }

            if (EditorGUIUtility.currentViewWidth > 326)
                // Youtube channel button
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Tutorials, "Open FImpossible Creations Channel with tutorial videos in your web browser"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    Application.OpenURL("https://www.youtube.com/c/FImpossibleCreations");
                }

            if (EditorGUIUtility.currentViewWidth > 292)
                // Store site button
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Website, "Open FImpossible Creations Asset Store Page inside your web browser"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    Application.OpenURL("https://assetstore.unity.com/publishers/37262");
                }

            // Manual file button
            if (_manualFile == null) _manualFile = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(target))) + "/Look Animator User Manual.pdf");
            if (_manualFile)
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Manual, "Open .PDF user manual file for Look Animator"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    EditorGUIUtility.PingObject(_manualFile);
                    Application.OpenURL(Application.dataPath + "/" + AssetDatabase.GetAssetPath(_manualFile).Replace("Assets/", ""));
                }

            FGUI_Inspector.DrawSwitchButton(ref drawGizmos, FGUI_Resources.Tex_GizmosOff, FGUI_Resources.Tex_Gizmos, "Toggle drawing gizmos on character in scene window", height, height, true);
            FGUI_Inspector.DrawSwitchButton(ref drawHeaderFoldout, FGUI_Resources.Tex_LeftFold, FGUI_Resources.Tex_DownFold, "Toggle to view additional options for foldouts", height, height);

            EditorGUILayout.EndHorizontal();

            if (drawHeaderFoldout)
            {
                FGUI_Inspector.DrawUILine(0.07f, 0.1f, 1, 4, 0.99f);

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                choosedLang = (ELangs)EditorGUILayout.EnumPopup(choosedLang, new GUIStyle(EditorStyles.layerMaskField) { fixedHeight = 0 }, new GUILayoutOption[2] { GUILayout.Width(80), GUILayout.Height(22) });
                if (EditorGUI.EndChangeCheck())
                {
                    PlayerPrefs.SetInt("FLookAnimLang", (int)choosedLang);
                    SetupLangs();
                }

                GUILayout.FlexibleSpace();

                bool hierSwitchOn = PlayerPrefs.GetInt("AnimsH", 1) == 1;
                FGUI_Inspector.DrawSwitchButton(ref hierSwitchOn, FGUI_Resources.Tex_HierSwitch, null, "Switch drawing small icons in hierarchy", height, height, true);
                PlayerPrefs.SetInt("AnimsH", hierSwitchOn ? 1 : 0);

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Change component title to yours (current: '" + Get._editor_displayName + "'"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    string filename = EditorUtility.SaveFilePanelInProject("Type your title (no file will be created)", Get._editor_displayName, "", "Type your title (no file will be created)");
                    if (!string.IsNullOrEmpty(filename))
                    {
                        filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                        if (!string.IsNullOrEmpty(filename))
                        { Get._editor_displayName = filename; serializedObject.ApplyModifiedProperties(); }
                    }
                }

                // Default inspector switch
                FGUI_Inspector.DrawSwitchButton(ref drawNewInspector, FGUI_Resources.Tex_AB, null, "Switch GUI Style to old / new", height, height, true);
                if (!drawNewInspector && drawDefaultInspector) drawDefaultInspector = false;

                // Old new UI Button
                FGUI_Inspector.DrawSwitchButton(ref defaultInspector, FGUI_Resources.Tex_Default, null, "Toggle inspector view to default inspector.\n\nIf you ever edit source code of Look Animator and add custom variables, you can see them by entering this mode, also sometimes there can be additional/experimental variables to play with.", height, height);
                if (!drawNewInspector && drawDefaultInspector) drawNewInspector = false;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }


        private void El_DrawBaseReferences()
        {
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
            GUILayout.Space(5);
            El_DrawBaseTransform();
            El_DrawLeadBone();
            GUILayout.Space(2);
            GUILayout.EndVertical();
        }


        private void El_DrawOptimizeWithMesh()
        {
            if (Get.OptimizeWithMesh)
            {
                if (Application.isPlaying)
                {
                    GUI.color = new Color(1f, 1f, 1f, .5f);
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    if (Get.OptimizeWithMesh.isVisible)
                        EditorGUILayout.LabelField("Look Animator Is Active", FGUI_Resources.HeaderStyle);
                    else
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("Look Animator Is Inactive", FGUI_Resources.HeaderStyle);
                        GUI.enabled = true;
                    }

                    EditorGUILayout.EndHorizontal();
                    GUI.color = c;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp_OptimizeWithMesh);
            if (GUILayout.Button("Find", new GUILayoutOption[1] { GUILayout.Width(44) }))
            {
                if (Get.OptimizeWithMesh == null)
                {
                    if (largestSkin != null) Get.OptimizeWithMesh = largestSkin;
                    else
                    {
                        Get.OptimizeWithMesh = Get.transform.GetComponent<Renderer>();
                        if (!Get.OptimizeWithMesh) Get.OptimizeWithMesh = Get.transform.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) Get.OptimizeWithMesh = Get.transform.parent.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) if (Get.transform.parent.parent != null) Get.OptimizeWithMesh = Get.transform.parent.parent.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) if (Get.transform.parent.parent != null) if (Get.transform.parent.parent.parent != null) Get.OptimizeWithMesh = Get.transform.parent.parent.parent.GetComponentInChildren<Renderer>();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }



        private void EL_DrawUltraSmooth180()
        {
            //GUILayout.BeginHorizontal();
            EditorGUIUtility.fieldWidth = 34;
            EditorGUILayout.PropertyField(sp_usmooth);
            EditorGUIUtility.fieldWidth = 0;
            GUI.color = c;
            //if (!Get.Fix180) if (Get.UltraSmoother > 0f) GUI.color = new Color(1f, 1f, 0.35f, 0.8f);
            //EditorGUILayout.LabelField("", GUILayout.Width(4));
            //EditorGUIUtility.labelWidth = 48;
            //EditorGUILayout.PropertyField(sp_180prev, new GUIContent("Fix180", sp_180prev.tooltip), GUILayout.Width(64));
            //GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;
            GUI.color = c;
        }


        private void El_DrawToFollow()
        {
            GUILayout.Space(4f);
            EditorGUIUtility.labelWidth = 154;

            //if (!Get.ObjectToFollow) GUI.color = new Color(1f, 1f, 0.5f, 1f);
            //EditorGUILayout.PropertyField(sp_tofollow);
            //GUI.color = c;

            if (Get.MomentLookTransform)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("(Using Moment Target)", Get.MomentLookTransform, typeof(Transform), true);
                GUI.enabled = true;
            }

            if (Get.FollowMode == FLookAnimator.EFFollowMode.FollowObject)
            {
                if (!Get.ObjectToFollow) GUI.color = new Color(1f, 1f, 0.5f, 1f);
                EditorGUIUtility.labelWidth = 181;
                EditorGUILayout.PropertyField(sp_tofollow, new GUIContent("  Object To Follow (Target)", FGUI_Resources.TexTargetingIcon, sp_tofollow.tooltip), GUILayout.Height(20));
                GUILayout.Space(4f);
                GUI.color = c;
                EditorGUIUtility.labelWidth = 154;
                EditorGUILayout.PropertyField(sp_FollowMode, new GUIContent(" Offset Mode (optional)", sp_FollowMode.tooltip));
                GUI.color = c;
            }
            else
            if (Get.FollowMode == FLookAnimator.EFFollowMode.FollowJustPosition)
            {
                EditorGUILayout.PropertyField(sp_FollowMode);
                EditorGUILayout.PropertyField(sp_FollowOffset, new GUIContent("Follow Position", sp_FollowOffset.tooltip + "\n\nVariable name: 'FollowOffset'"));
            }
            else
            {
                if (!Get.ObjectToFollow) GUI.color = new Color(1f, 1f, 0.5f, 1f);
                EditorGUILayout.PropertyField(sp_tofollow);
                GUILayout.Space(4f);
                GUI.color = c;
                EditorGUILayout.PropertyField(sp_FollowMode, new GUIContent("Offset Mode (optional)", sp_FollowMode.tooltip));
                if (Get.FollowOffset == Vector3.zero) GUI.color = unchangedC;
                EditorGUILayout.PropertyField(sp_FollowOffset);
                GUI.color = c;
            }

            EditorGUIUtility.labelWidth = 0;
            GUI.color = Color.white;
        }



        private void El_DrawQuickerRotAbove()
        {
            //GUILayout.BeginHorizontal();
            //EditorGUIUtility.fieldWidth = 24;
            //if (Get.QuickerRotateAbove >= 90f) GUI.color = unchangedC;
            //EditorGUIUtility.labelWidth = 154;
            //EditorGUILayout.PropertyField(sp_DeltaAcc, new GUIContent("Quicker Rotate Above", sp_DeltaAcc.tooltip));
            //EditorGUIUtility.labelWidth = 0;
            //Get.QuickerRotateAbove = Mathf.Round(Get.QuickerRotateAbove);
            //EditorGUIUtility.labelWidth = 0; EditorGUIUtility.fieldWidth = 0;
            //EditorGUILayout.LabelField("°", GUILayout.Width(13));
            //GUILayout.EndHorizontal();
            //GUI.color = c;
        }


        private void El_DrawBaseTransform()
        {
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
        }


        void El_DrawBoneSelectionButton()
        {
            if (largestSkin == null) return;

            if (GUILayout.Button(new GUIContent(FGUI_Resources.TexTargetingIcon, "Display bones from your renderer to choose"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(16) }))
            {
                GenericMenu bonesMenu = new GenericMenu();

                for (int i = largestSkin.bones.Length - 1; i >= 0; i--)
                {
                    // Ignoring leg and arm limbs
                    if (largestSkin.bones[i].name.ToLower().Contains("thum")) continue;
                    if (largestSkin.bones[i].name.ToLower().Contains("hand")) continue;
                    if (largestSkin.bones[i].name.ToLower().Contains("inde")) continue;
                    if (largestSkin.bones[i].name.ToLower().Contains("pink")) continue;
                    if (largestSkin.bones[i].name.ToLower().Contains("middle")) continue;
                    if (largestSkin.bones[i].name.ToLower().Contains("ring")) continue;
                    if (largestSkin.bones[i].name.ToLower().Contains("leg")) continue;
                    if (largestSkin.bones[i].name.ToLower().Contains("arm")) continue;

                    GUIContent title = new GUIContent(largestSkin.bones[i].name); Transform tgt = largestSkin.bones[i];

                    bool current = false;
                    if (tgt == Get.LeadBone) current = true;

                    bonesMenu.AddItem(title, current, () => { EditorGUIUtility.PingObject(tgt); Get.LeadBone = tgt; });
                }

                bonesMenu.ShowAsContext();
            }
        }


        private void El_DrawLeadBone()
        {
            if (!Get.LeadBone)
                GUILayout.BeginHorizontal(FGUI_Inspector.Style(new Color(0.9f, 0.2f, 0.2f, 0.2f), 0));
            else
                GUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(sp_leadbone, new GUIContent(" Lead bone / Head", _TexLookAnimIcon, sp_leadbone.tooltip), GUILayout.Height(20));

            El_DrawBoneSelectionButton();

            if (Get.LeadBone != previousHead)
            {
                previousHead = Get.LeadBone;
                Get.UpdateForCustomInspector();
            }

            if (GUILayout.Button(new GUIContent("Auto Find", "By pressing this button, algorithm will go trough hierarchy and try to find object which name includes 'head' or 'neck', be aware, this bone can not be correct but sure it will help you find right one quicker"), new GUILayoutOption[2] { GUILayout.MaxWidth(90), GUILayout.MaxHeight(18) }))
            {
                FindHeadBone(Get);
                EditorUtility.SetDirty(target);
            }

            GUILayout.EndHorizontal();
        }


        private bool drawLookWhenAbove = false;

        private void El_DrawLookWhenAbove()
        {
            GUILayout.Space(4);

            if (Get.LookWhenAbove > 0f)
            {

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(FGUI_Resources.GetFoldSimbol(drawLookWhenAbove, 10, "►"), "Click on this arrow to draw more advanced settings"), FGUI_Resources.FoldStyle, GUILayout.Width(16))) drawLookWhenAbove = !drawLookWhenAbove;
                EditorGUILayout.PropertyField(sp_LookWhenAbove, new GUIContent("Look When Above", sp_LookWhenAbove.tooltip));
                GUILayout.EndHorizontal();

                if (drawLookWhenAbove)
                {

                    GUILayout.Space(2);
                    if (Get.LookWhenAboveVertical <= 0)
                    {
                        GUI.color = unchangedC;
                        GUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 100;
                        EditorGUILayout.PropertyField(sp_LookWhenAboveVertical, new GUIContent("     Vertical", sp_LookWhenAboveVertical.tooltip));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.LabelField("(" + Mathf.Round(Get.LookWhenAbove) + ")", GUILayout.Width(36));
                        GUILayout.EndHorizontal();
                        GUI.color = c;
                    }
                    else
                    {
                        EditorGUIUtility.labelWidth = 100;
                        EditorGUILayout.PropertyField(sp_LookWhenAboveVertical, new GUIContent("     Vertical", sp_LookWhenAboveVertical.tooltip));
                        EditorGUIUtility.labelWidth = 0;
                    }

                    GUILayout.Space(2);
                    EditorGUIUtility.labelWidth = 160;
                    if (Get.WhenAboveGoBackAfter <= 0f) GUI.color = unchangedC;
                    EditorGUILayout.PropertyField(sp_WhenAboveEraseAfter, new GUIContent("     Go Back After", sp_WhenAboveEraseAfter.tooltip));
                    EditorGUIUtility.labelWidth = 0;
                    GUI.color = c;

                    if (Get.WhenAboveGoBackAfter > 0)
                    {
                        //if (Get.WhenAboveGoBackAfterVertical <= 0)
                        //{
                        //    GUI.color = unchangedC;
                        //    GUILayout.BeginHorizontal();
                        //    EditorGUIUtility.labelWidth = 100;
                        //    EditorGUIUtility.fieldWidth = 10;
                        //    EditorGUILayout.PropertyField(sp_WhenAboveEraseAfterVertical, new GUIContent("     Vertical", sp_WhenAboveEraseAfterVertical.tooltip));
                        //    EditorGUIUtility.fieldWidth = 0;
                        //    EditorGUIUtility.labelWidth = 0;
                        //    EditorGUILayout.LabelField("" + System.Math.Round(Get.WhenAboveGoBackAfter, 2) + "", GUILayout.Width(36));
                        //    GUILayout.EndHorizontal();
                        //    GUI.color = c;
                        //}
                        //else
                        //{
                        //    EditorGUIUtility.labelWidth = 100;
                        //    EditorGUILayout.PropertyField(sp_WhenAboveEraseAfterVertical, new GUIContent("     Vertical", sp_WhenAboveEraseAfterVertical.tooltip));
                        //    EditorGUIUtility.labelWidth = 0;
                        //}

                        EditorGUILayout.PropertyField(sp_WhenAboveGoBackDuration, new GUIContent("     Go Back Duration", sp_WhenAboveGoBackDuration.tooltip));
                    }

                    GUI.color = c;

                    GUILayout.Space(4);
                }

            }
            else
            {
                GUI.color = unchangedC;
                EditorGUILayout.PropertyField(sp_LookWhenAbove, new GUIContent("Look When Above", sp_LookWhenAbove.tooltip));
                GUI.color = c;
            }

        }



        private void El_DrawMaxDist()
        {
            if (Get.MaximumDistance > 0f)
            {
                GUILayout.BeginHorizontal();

                EditorGUIUtility.labelWidth = 134;
                EditorGUILayout.PropertyField(sp_maxdist);
                EditorGUIUtility.labelWidth = 176;

                if (!Get._gizmosDrawMaxDist || !Get.drawGizmos)
                {
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_GizmosOff, "Check scene view for sphere gizmos showing max distance and other ranges in world space"), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(20f), GUILayout.Height(16f) })) { Get._gizmosDrawMaxDist = !Get._gizmosDrawMaxDist; }
                }
                else
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Gizmos, "Check scene view for sphere gizmos showing max distance and other ranges in world space"), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(20f), GUILayout.Height(16f) })) { Get._gizmosDrawMaxDist = !Get._gizmosDrawMaxDist; }

                //EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Check scene view for sphere gizmos showing max distance and other ranges in world space"), new GUILayoutOption[] { GUILayout.Width(24f) });
                GUILayout.EndHorizontal();

                if (Get.MaxOutDistanceFactor >= 1f) GUI.color = unchangedC;
                EditorGUILayout.PropertyField(sp_DetectionFactor);
                GUI.color = c;

                EditorGUIUtility.labelWidth = 162;
                if (Get.DistanceMeasurePoint == Vector3.zero) GUI.color = unchangedC;
                EditorGUILayout.PropertyField(sp_distMeasureOffset);
                GUI.color = c;

                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.PropertyField(sp_dist2D);
            }
            else
            {
                GUILayout.BeginHorizontal();

                GUI.color = unchangedC;
                EditorGUIUtility.labelWidth = 134;
                EditorGUILayout.PropertyField(sp_maxdist);
                EditorGUILayout.LabelField("(Not Used)", new GUILayoutOption[] { GUILayout.Width(70f) });
                EditorGUIUtility.labelWidth = 0;
                GUI.color = c;

                GUILayout.EndHorizontal();
            }

            if (Get.MaximumDistance < 0f)
            {
                Get.MaximumDistance = 0f;
                EditorUtility.SetDirty(target);
            }

            GUI.color = c;
        }


        private void El_DrawLimitMaxAngle(bool wrongLimit)
        {
            if (wrongLimit)
            {
                GUI.color = new Color(0.9f, 0.55f, 0.55f, 0.8f);
            }
            else
                if (Get.StopLookingAbove >= 180) GUI.color = unchangedC;

            GUILayout.BeginHorizontal();

            EditorGUIUtility.fieldWidth = 28;
            EditorGUIUtility.labelWidth = 130;
            Get.StopLookingAbove = EditorGUILayout.Slider(new GUIContent("Stop Looking Above", "Stop looking at target after angle to look at exceeds this value.\n\n(Variable name: 'MaxRotationDiffrence')"), Get.StopLookingAbove, 25f, 180f);
            EditorGUIUtility.labelWidth = 0; EditorGUIUtility.fieldWidth = 0;
            EditorGUILayout.LabelField("°", GUILayout.Width(13));

            FEditor_CustomInspectorHelpers.DrawMinMaxSphere(-Get.StopLookingAbove, Get.StopLookingAbove, 14);
            GUILayout.EndHorizontal();

            GUILayout.Space(3f);
            GUI.color = c;
        }

        private void El_DrawLimitXAngle(bool wrongLimit)
        {
            // X
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle); GUILayout.Space(4f);

            if (wrongLimit) GUI.color = new Color(0.9f, 0.55f, 0.55f, 0.8f);
            else
                GUI.color = limitsC;

            GUILayout.BeginHorizontal();
            GUILayout.Label("  Clamp Angle Horizontal (X)", EditorStyles.boldLabel, GUILayout.MaxWidth(194f));
            GUILayout.FlexibleSpace();

            //GUILayout.Label(Mathf.Round(Get.XRotationLimits.x) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.XRotationLimits.x) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();

            FEditor_CustomInspectorHelpers.DrawMinMaxSphere(Get.XRotationLimits.x, Get.XRotationLimits.y, 14, Get.XElasticRange);

            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.XRotationLimits.y) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();
            //GUILayout.Label(Mathf.Round(Get.XRotationLimits.y) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.MinMaxSlider(ref Get.XRotationLimits.x, ref Get.XRotationLimits.y, -180f, 180f);

            bothX = EditorGUILayout.Slider("Adjust symmetrical", bothX, 1f, 180f);
            EditorGUILayout.PropertyField(sp_elasticX);
            EditorGUIUtility.labelWidth = 177;
            EditorGUILayout.PropertyField(sp_StartLookElasticRangeX);
            EditorGUIUtility.labelWidth = 0;

            if (lastBothX != bothX)
            {
                Get.XRotationLimits.x = -bothX;
                Get.XRotationLimits.y = bothX;
                lastBothX = bothX;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            GUILayout.Space(7f);
            GUILayout.EndVertical();
            GUI.color = c;
        }

        private void El_DrawLimitYAngle()
        {
            GUI.color = limitsC;

            // Y
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("  Clamp Angle Vertical (Y)", EditorStyles.boldLabel, GUILayout.MaxWidth(186f));
            GUILayout.FlexibleSpace();
            //GUILayout.Label(Mathf.Round(Get.YRotationLimits.x) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.YRotationLimits.x) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();
            FEditor_CustomInspectorHelpers.DrawMinMaxVertSphere(Get.YRotationLimits.x, Get.YRotationLimits.y, 14, Get.YElasticRange);
            //GUILayout.Label(Mathf.Round(Get.YRotationLimits.y) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.YRotationLimits.y) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.color = limitsC;
            EditorGUILayout.MinMaxSlider(ref Get.YRotationLimits.x, ref Get.YRotationLimits.y, -90f, 90f);
            bothY = EditorGUILayout.Slider("Adjust symmetrical", bothY, 1f, 90f);
            EditorGUILayout.PropertyField(sp_elasticY);
            EditorGUIUtility.labelWidth = 177;
            EditorGUILayout.PropertyField(sp_StartLookElasticRangeY);
            EditorGUIUtility.labelWidth = 0;

            if (lastBothY != bothY)
            {
                Get.YRotationLimits.x = -bothY;
                Get.YRotationLimits.y = bothY;
                lastBothY = bothY;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            GUILayout.Space(7f);
            GUILayout.EndVertical();
            GUILayout.Space(3f);
            GUI.color = c;
        }


        private void El_DrawEyesLimitAngles()
        {
            FGUI_Inspector.HeaderBox(Lang("Eyes Module") + " Limits", true, _TexEyesIcon, 22, 22 - 1, LangBig());

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            GUILayout.Space(3f);

            // X

            GUILayout.BeginHorizontal();
            GUILayout.Label("  Clamp Angle Horizontal (X)", GUILayout.MaxWidth(170f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.EyesXRange.x) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();
            FEditor_CustomInspectorHelpers.DrawMinMaxSphere(Get.EyesXRange.x, Get.EyesXRange.y, 14);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.EyesXRange.y) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.MinMaxSlider(ref Get.EyesXRange.x, ref Get.EyesXRange.y, -90f, 90f);

            eyesbothX = EditorGUILayout.Slider("Adjust symmetrical", eyesbothX, 1f, 90f);

            if (eyeslastBothX != eyesbothX)
            {
                Get.EyesXRange.x = -eyesbothX;
                Get.EyesXRange.y = eyesbothX;
                eyeslastBothX = eyesbothX;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            GUILayout.Space(7f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

            // Y

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("  Clamp Angle Vertical (Y)", GUILayout.MaxWidth(170f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.EyesYRange.x) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();
            //GUILayout.Label(Mathf.Round(Get.EyesYRange.x) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
            FEditor_CustomInspectorHelpers.DrawMinMaxVertSphere(-Get.EyesYRange.y, -Get.EyesYRange.x, 14);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.FrameBoxStyle); GUILayout.Label(Mathf.Round(Get.EyesYRange.y) + "°", GUILayout.MaxWidth(30f)); EditorGUILayout.EndHorizontal();
            //GUILayout.Label(Mathf.Round(Get.EyesYRange.y) + "°", FEditor_Styles.GrayBackground, GUILayout.MaxWidth(40f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.MinMaxSlider(ref Get.EyesYRange.x, ref Get.EyesYRange.y, -90f, 90f);
            eyesbothY = EditorGUILayout.Slider("Adjust symmetrical", eyesbothY, 1f, 90f);

            if (eyeslastBothY != eyesbothY)
            {
                Get.EyesYRange.x = -eyesbothY;
                Get.EyesYRange.y = eyesbothY;
                eyeslastBothY = eyesbothY;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            GUILayout.Space(9f);
            GUILayout.EndVertical();
            GUILayout.Space(3f);

        }


        private void El_DrawNodding()
        {
            if (Get.NoddingTransitions == 0f) GUI.color = unchangedC;
            EditorGUILayout.PropertyField(sp_NoddingTransitions);
            GUI.color = c;

            if (Get.NoddingTransitions != 0f)
            {
                GUILayout.Space(6f);
                EditorGUILayout.PropertyField(sp_NodAxis);
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_BackBonesNod);
                GUILayout.Space(4f);
            }
        }

        GUIContent[] parOffVer;
        int[] parOffVerVals;
        private void El_DrawFixingPresetsSwitchesAndSettings()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp_fixpres);

            if (GUILayout.Button("◄", EditorStyles.miniButton, new GUILayoutOption[2] { GUILayout.MaxWidth(19), GUILayout.MaxHeight(16) }))
            {
                if ((int)Get.FixingPreset == 0)
                    Get.FixingPreset = (FLookAnimator.EFAxisFixOrder)(Enum.GetValues(typeof(FLookAnimator.EFAxisFixOrder)).Length - 1);
                else
                    Get.FixingPreset--;

                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("►", EditorStyles.miniButton, new GUILayoutOption[2] { GUILayout.MaxWidth(19), GUILayout.MaxHeight(16) }))
            {
                if ((int)Get.FixingPreset + 1 >= Enum.GetValues(typeof(FLookAnimator.EFAxisFixOrder)).Length)
                {
                    Get.FixingPreset = 0;
                }
                else
                    Get.FixingPreset++;

                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3f);

            if (Get.FixingPreset == FLookAnimator.EFAxisFixOrder.Parental)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ParentalReferenceBone"));
                GUILayout.Space(5f);
                EditorGUIUtility.labelWidth = 76;
                EditorGUILayout.PropertyField(sp_ConstantParentalAxisUpdate, new GUIContent("Refreshing", sp_ConstantParentalAxisUpdate.tooltip), GUILayout.Width(100));
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();

                if (parOffVer == null)
                {
                    parOffVer = new GUIContent[3];
                    parOffVer[0] = new GUIContent("Version 1");
                    parOffVer[1] = new GUIContent("Version 2");
                    parOffVer[2] = new GUIContent("Version 3");

                    parOffVerVals = new int[3];
                    parOffVerVals[0] = 2;
                    parOffVerVals[1] = 1;
                    parOffVerVals[2] = 0;
                }

                GUILayout.Space(5f);

                Get.ParentalOffsetsV = EditorGUILayout.IntPopup(new GUIContent("Offsets Mode", "Choose in what way should be calculated offset rotations for manual corrections (and nodding)"), Get.ParentalOffsetsV, parOffVer, parOffVerVals);
                GUILayout.Space(1f);
            }
            else
            if (Get.FixingPreset == FLookAnimator.EFAxisFixOrder.FullManual)
            {
                EditorGUILayout.PropertyField(sp_axesmul);
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_manfromax);
                EditorGUILayout.PropertyField(sp_mantoax);
                GUILayout.Space(5f);
            }
            else
            if (Get.FixingPreset == FLookAnimator.EFAxisFixOrder.FromBased)
            {
                EditorGUILayout.LabelField("Auto Offset: " + RoundVector(Get.OffsetAuto));
                EditorGUILayout.LabelField("Auto From Axis: " + RoundVector(Get.FromAuto));
            }
        }

    }
}