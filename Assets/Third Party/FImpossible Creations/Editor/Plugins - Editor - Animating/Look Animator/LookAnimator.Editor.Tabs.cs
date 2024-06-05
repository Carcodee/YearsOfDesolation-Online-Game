using FIMSpace.FEditor;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator_Editor
    {

        private void Tab_DrawSetup()
        {
            FGUI_Inspector.VSpace(-3,-4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);

            El_DrawBaseReferences();
            //El_DrawAnimateWithSource();
            Fold_DrawBackBones();
            Fold_DrawCompensation();

            Fold_DrawAdditionalSetup();

            GUILayout.EndVertical();
        }


        private void Tab_DrawTweaking()
        {
            FGUI_Inspector.VSpace(-3,-4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
            El_DrawToFollow(); 
            
            GUILayout.Space(8);

            EditorGUILayout.PropertyField(sp_rotspd);
            GUILayout.Space(2f);
            if (Get.BirdMode && Get.UltraSmoother > 0.7f) GUI.color = new Color(1f, 0.88f, 0.88f, 0.95f);
            else
            if (Get.UltraSmoother <= 0f) GUI.color = unchangedC;
            EL_DrawUltraSmooth180();
            GUI.color = c;

            EditorGUIUtility.labelWidth = 160f;
            EL_DrawBlendToOriginal();
            EditorGUIUtility.labelWidth = 0f;

            GUILayout.EndVertical();

            Fold_DrawBehaviourSettings();
            Fold_DrawTargeting();
            Fold_DrawAddMotionSettings();

            GUILayout.EndVertical();
        }


        private void EL_DrawBlendToOriginal()
        {
            GUILayout.Space(2);
            EditorGUILayout.PropertyField(sp_blend);
            GUILayout.Space(2);
        }


        private void Tab_DrawLimiting()
        {
            FGUI_Inspector.VSpace(-3,-4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);

            if ( Application.isPlaying)
            {
                GUILayout.Space(6);
                GUI.enabled = false;
                EditorGUILayout.EnumPopup("Current Look State: ", Get.LookState);
                GUI.enabled = true;
                GUILayout.Space(5);
            }

            bool wrongLimit = false;
            if (Mathf.Abs(Get.XRotationLimits.x) > Get.StopLookingAbove) wrongLimit = true;
            if (Mathf.Abs(Get.XRotationLimits.y) > Get.StopLookingAbove) wrongLimit = true;

            El_DrawLimitXAngle(wrongLimit);
            El_DrawLimitYAngle();

            if ( Get.UseEyes ) El_DrawEyesLimitAngles();
            GUILayout.Space(-7f);
            GUILayout.EndVertical();
        }


        private void Tab_DrawAdditionalFeatures()
        {
            FGUI_Inspector.VSpace(-3,-4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); GUILayout.Space(5f);

            El_DrawNodding();

            GUILayout.Space(3f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);
            GUILayout.Space(2f);

            Fold_DrawBirdMode();

            GUILayout.Space(2f);
            GUILayout.EndVertical();


            if (!Get._editor_hideEyes)
            {
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                GUILayout.Space(2f);

                Fold_DrawEyesModule();

                //GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);
                //GUILayout.Space(2f);

                //Fold_DrawMomentTargets();

                //GUILayout.Space(2f);
                //GUILayout.EndVertical();
            }

            GUILayout.Space(-6f);
            GUILayout.EndVertical();
        }


        private void Tab_DrawCorrections()
        {
            FGUI_Inspector.VSpace(-3,-4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); GUILayout.Space(5f);

            El_DrawFixingPresetsSwitchesAndSettings();

            EditorGUILayout.PropertyField(sp_angoff);
            EditorGUILayout.PropertyField(sp_backoff, new GUIContent("Backing Offset", sp_backoff.tooltip)); GUILayout.Space(3f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            GUILayout.Space(3f);
            Fold_DrawBoneCorrectionRotations();
            GUILayout.Space(5f);

            EditorGUILayout.PropertyField(sp_dray);
            GUILayout.Space(5f);

            GUILayout.EndVertical();

            Fold_DrawHidden();
            GUILayout.Space(-5f);

            GUILayout.EndVertical();
        }

    }
}