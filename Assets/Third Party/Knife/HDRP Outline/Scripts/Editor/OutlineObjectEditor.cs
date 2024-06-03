using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Knife.HDRPOutline.Core
{
    [CustomEditor(typeof(OutlineObject))]
    [CanEditMultipleObjects]
    public class OutlineObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty material;
        private SerializedProperty color;
        private SerializedProperty mask;
        private SerializedProperty alphaThreshold;
        private SerializedProperty fresnelScale;
        private SerializedProperty fresnelPower;

        private MaterialEditor materialEditor;

        private void OnEnable()
        {
            material = serializedObject.FindProperty("material");
            color = serializedObject.FindProperty("color");
            mask = serializedObject.FindProperty("mask");
            alphaThreshold = serializedObject.FindProperty("alphaThreshold");
            fresnelScale = serializedObject.FindProperty("fresnelScale");
            fresnelPower = serializedObject.FindProperty("fresnelPower");

            CreateMaterialEditor();

            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            DestroyImmediate(materialEditor);
        }

        private void UndoRedoPerformed()
        {
            CreateMaterialEditor();
        }

        private void CreateMaterialEditor()
        {
            if (materialEditor != null)
                DestroyImmediate(materialEditor, true);

            Material[] materials = targets.ToList().ConvertAll(t => (t as OutlineObject).Material).ToArray();
            materialEditor = CreateEditor(materials, typeof(MaterialEditor)) as MaterialEditor;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(HDRPOutlineEditor.GetLogo());

            // additional check to recreate material editor
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(material);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                CreateMaterialEditor();
            }

            EditorGUI.BeginChangeCheck();
            if (material.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("You must assign material", MessageType.Error);
            }
            else
            {
                EditorGUILayout.PropertyField(color);

                bool isMaskEnabled = false;
                bool isFresnelEnabled = false;
                Material mat = material.objectReferenceValue as Material;

                isMaskEnabled = mat.IsKeywordEnabled("_BASECOLOR_ALPHA") || mat.IsKeywordEnabled("_BASECOLOR_COLOR");
                if (mat.HasProperty("_Fresnel"))
                {
                    isFresnelEnabled = mat.GetFloat("_Fresnel") == 1;
                }

                if (!isFresnelEnabled)
                {
                    EditorGUILayout.HelpBox("You can use Fresnel Scale and Power properties only if Fresnel property will be enabled in material properties", MessageType.Info);
                }

                EditorGUI.BeginDisabledGroup(!isFresnelEnabled);
                EditorGUILayout.PropertyField(fresnelScale);
                EditorGUILayout.PropertyField(fresnelPower);
                EditorGUI.EndDisabledGroup();

                if (!isMaskEnabled)
                {
                    EditorGUILayout.HelpBox("You can use mask property only if Base Color = Alpha (_BASECOLOR_ALPHA) or Base Color = Color (_BASECOLOR_COLOR) keyword enabled in material properties", MessageType.Info);
                }

                EditorGUI.BeginDisabledGroup(!isMaskEnabled);
                EditorGUILayout.PropertyField(mask);
                EditorGUILayout.PropertyField(alphaThreshold);
                EditorGUI.EndDisabledGroup();

                if (materialEditor != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    materialEditor.DrawHeader();
                    materialEditor.OnInspectorGUI();
                    EditorGUILayout.EndHorizontal();
                }
            }

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach(var t in targets)
                {
                    var outlineObject = t as OutlineObject;
                    outlineObject.PushParameters();
                }
            }
        }
    }
}