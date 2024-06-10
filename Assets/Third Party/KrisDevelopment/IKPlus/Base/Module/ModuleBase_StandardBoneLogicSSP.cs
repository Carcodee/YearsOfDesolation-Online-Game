// IKP - by Hristo Ivanov (Kris Development)

using UnityEngine;
using SETUtil.Types;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
    /// <summary>
    /// Setup, Settings, Properties
    /// </summary>
    public abstract class ModuleBase_StandardBoneLogicSSP : ModuleBase_PropertiesProvider
    {
        protected virtual bool requiresBoneRotationOffsetValues => true;
        protected virtual bool bonePrerequisitesAvailable => Validate(new List<ValidationResult>());


        /// <summary>
        /// Util: Checks the object for missing ref, add result to list.
        /// </summary>
        protected static bool ValidateCriticalBodySetupBone(Transform bodySetup, string nameOf, List<ValidationResult> outToList)
        {
            if (bodySetup == null)
            {
                outToList.Add(new ValidationResult()
                {
                    message = $"Missing {SETUtil.StringUtil.WordSplit(nameOf, true)}",
                    outcome = ValidationResult.Outcome.CriticalError,
                });

                return false;
            }
            return true;
        }


        //This is an entirely editor GUI-focused template class. Its contents will not be included in the build
#if UNITY_EDITOR
        [HideInInspector, SerializeField]
        private bool
            showSettings = true,
            showProperties = true,
            expandSetup = true;

        protected SerializedProperty
            m_so_showSettings,
            m_so_showProperties,
            m_so_expandSetup;

        public override void DrawEditorGUI()
        {
            if (!ikp)
            {
                EditorGUILayout.HelpBox("IKP was not set!", MessageType.Error);
                return;
            }

            EditorGUI.BeginDisabledGroup(!active);

            //serialization
            InitSerializedPropertiesIfNeeded();
            serialized.Update();

            // draw some validation
            DrawValidation();

            // draw expandables
            if (ExpandSetup())
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        DrawSetup();
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        ApplyModifiedProperties();
                    }
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(4);
            }

            if (ExpandSettings())
            {
                EditorGUI.indentLevel++;
                DrawSettings();
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(4);
            }

            if (ExpandProperties())
            {
                EditorGUI.indentLevel++;
                if (IKPModule_EditorSimulation.isAnyInstancePlaying)
                {
                    EditorGUILayout.HelpBox("Can't edit properties while Editor simulation is running.", MessageType.Info);
                }
                DrawProperties();
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(4);
            }

            EditorGUI.EndDisabledGroup();

            ApplyModifiedProperties();
        }

        /// <summary>
        /// [WARNING] method relies on previous SPInit call
        /// </summary>
        private bool ExpandSetup()
        {
            return m_so_expandSetup.boolValue = SETUtil.EditorUtil.ExpandButton(m_so_expandSetup.boolValue, "Bones", FontStyle.Bold);
        }

        protected virtual void DrawSetup()
        {
            InitSerializedPropertiesIfNeeded();
        }

        /// <summary>
        /// [WARNING] method relies on previous SPInit call
        /// </summary>
        private bool ExpandSettings()
        {
            m_so_showSettings.boolValue = SETUtil.EditorUtil.ExpandButton(showSettings, "Settings", IKPStyle.MEDIUM_HEIGHT, FontStyle.Bold);
            return m_so_showSettings.boolValue;
        }

        protected virtual void DrawSettings()
        {

        }

        /// <summary>
        /// [WARNING] method relies on previous SPInit call
        /// </summary>
        private bool ExpandProperties()
        {
            m_so_showProperties.boolValue = SETUtil.EditorUtil.ExpandButton(showProperties, $"Properties ({PropertyCount()})", IKPStyle.MEDIUM_HEIGHT, FontStyle.Bold);
            return m_so_showProperties.boolValue;
        }

        protected virtual void DrawProperties()
        {
            InitSerializedPropertiesIfNeeded();
        }

        protected override void InitializeSerializedProperties()
        {
            base.InitializeSerializedProperties();
            serialized.Update();
            m_so_showSettings = serialized.FindProperty(nameof(showSettings));
            m_so_showProperties = serialized.FindProperty(nameof(showProperties));
            m_so_expandSetup = serialized.FindProperty(nameof(expandSetup));
        }
#endif
    }
}
