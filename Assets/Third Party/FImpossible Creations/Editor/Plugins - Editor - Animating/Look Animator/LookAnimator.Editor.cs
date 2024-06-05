using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator_Editor
    {
        [MenuItem("CONTEXT/FLookAnimator/Switch displaying header bar")]
        private static void HideFImpossibleHeader(MenuCommand menuCommand)
        {
            int current = PlayerPrefs.GetInt("FLookHeader", 1);
            if (current == 1) current = 0; else current = 1;
            PlayerPrefs.SetInt("FLookHeader", current);
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Look Animator Inspector");

            serializedObject.Update();

            FLookAnimator Get = (FLookAnimator)target;
            string title = drawDefaultInspector ? " Default Inspector" : " " + Get._editor_displayName;
            if (!drawNewInspector) title = " Old GUI Version";

            if (PlayerPrefs.GetInt("FLookHeader", 1) == 1)
            {
                HeaderBoxMain(title, ref Get.drawGizmos, ref drawDefaultInspector, _TexLookAnimIcon, Get, 27);
                GUILayout.Space(4f);
            }
            else
            {
                GUILayout.Space(4f);
            }

            #region Default Inspector
            if (drawDefaultInspector)
            {
                #region Exluding from view not needed properties

                List<string> excludedVars = new List<string>();

                if (Get.BackBonesCount < 1)
                {
                    excludedVars.Add("BackBonesTransforms");
                    excludedVars.Add("BackBonesFalloff");
                    excludedVars.Add("m_script");
                }

                #endregion

                if (Get.BackBonesCount < 0) Get.BackBonesCount = 0;

                // Draw default inspector without not needed properties
                DrawPropertiesExcluding(serializedObject, excludedVars.ToArray());
            }
            else
            #endregion

            {
                if (drawNewInspector)
                {
                    DrawNewGUI();
                }
                else
                {
                    DrawOldGUI();
                }
            }

            // Apply changed parameters variables
            serializedObject.ApplyModifiedProperties();
        }



        void DrawCategoryButton(FLookAnimator.EEditorLookCategory target, Texture icon, string lang)
        {
            if (Get._Editor_Category == target) GUI.backgroundColor = new Color(0.1f, 1f, 0.2f, 1f);

            int height = 28;
            int lim = 390;
            if (choosedLang == ELangs.русский) lim = 440;

            if (EditorGUIUtility.currentViewWidth > lim)
            {
                if (GUILayout.Button(new GUIContent("  " + Lang(lang), icon), FGUI_Resources.ButtonStyle, GUILayout.Height(height))) Get._Editor_Category = target;
            }
            else
                if (GUILayout.Button(new GUIContent(icon, Lang(lang)), FGUI_Resources.ButtonStyle, GUILayout.Height(height))) Get._Editor_Category = target;
           
            GUI.backgroundColor = bc;
        }


        private void DrawNewGUI()
        {
            #region Preparations for unity versions and skin

            c = Color.Lerp(GUI.color * new Color(0.8f, 0.8f, 0.8f, 0.7f), GUI.color, Mathf.InverseLerp(0f, 0.15f, Get.LookAnimatorAmount));
            bc = GUI.backgroundColor;

            RectOffset zeroOff = new RectOffset(0, 0, 0, 0);
            float bgAlpha = 0.05f; if (EditorGUIUtility.isProSkin) bgAlpha = 0.1f;

#if UNITY_2019_3_OR_NEWER
        int headerHeight = 22;
#else
            int headerHeight = 25;
#endif

            #endregion


            //GUILayout.BeginVertical(FGUI_Resources.BGBoxStyle); GUILayout.Space(1f);

            EditorGUILayout.BeginHorizontal();
            DrawCategoryButton(FLookAnimator.EEditorLookCategory.Setup, FGUI_Resources.Tex_GearSetup, "Setup");
            DrawCategoryButton(FLookAnimator.EEditorLookCategory.Tweak, FGUI_Resources.Tex_Sliders, "Tweak");
            DrawCategoryButton(FLookAnimator.EEditorLookCategory.Limit, FGUI_Resources.Tex_Knob, "Limit");
            DrawCategoryButton(FLookAnimator.EEditorLookCategory.Features, FGUI_Resources.Tex_Module, "Modules");
            DrawCategoryButton(FLookAnimator.EEditorLookCategory.Corrections, FGUI_Resources.Tex_Repair, "Correct");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(1);


            switch (Get._Editor_Category)
            {
                case FLookAnimator.EEditorLookCategory.Setup:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.7f, .7f, 0.7f, bgAlpha), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Character Setup"), true, FGUI_Resources.Tex_GearSetup, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawSetup();
                    GUILayout.EndVertical();
                    break;

                case FLookAnimator.EEditorLookCategory.Tweak:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.3f, .4f, 1f, bgAlpha), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Tweak Animation"), true, FGUI_Resources.Tex_Sliders, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawTweaking();
                    GUILayout.EndVertical();
                    break;

                case FLookAnimator.EEditorLookCategory.Limit:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.675f, .675f, 0.275f, bgAlpha), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Limit Animation Behaviour"), true, FGUI_Resources.Tex_Knob, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawLimiting();
                    GUILayout.EndVertical();
                    break;

                case FLookAnimator.EEditorLookCategory.Features:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.3f, 1f, .7f, bgAlpha), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Additional Modules"), true, FGUI_Resources.Tex_Module, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawAdditionalFeatures();
                    GUILayout.EndVertical();
                    break;

                case FLookAnimator.EEditorLookCategory.Corrections:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(1f, .4f, .4f, bgAlpha * 0.5f), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Corrections"), true, FGUI_Resources.Tex_Repair, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawCorrections();
                    GUILayout.EndVertical();
                    break;
            }

            GUILayout.Space(2f);
            //GUILayout.EndVertical();

        }


    }
}