// IKP - by Hristo Ivanov (Kris Development)

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SETUtil.Types;
using KrisDevelopment.DistributedInternalUtilities;

namespace IKPn
{
    [CustomEditor(typeof(IKP))]
    internal class IKPEditor : Editor
    {
        const string ASSET_STORE_URL = "https://assetstore.unity.com/packages/tools/animation/ik-plus-alpha-97606";

        internal static IKPEditor ikpEditor;
        private IKP ikp;
        private List<string> drawOrder = new List<string>();


        internal static void TryRepaint()
        {
            if (ikpEditor)
                ikpEditor.Repaint();
        }

        internal void OnEnable()
        {
            ikpEditor = this;

            if (!ikp)
                ikp = (IKP)target;

            UpdateDrawOrder();
        }

        void UpdateDrawOrder()
        {
            drawOrder.Clear();
            drawOrder.AddRange(IKPUtils.moduleSignatures);
            drawOrder.Sort((a, b) => ModuleManager.GetInspectorOrder(a).CompareTo(ModuleManager.GetInspectorOrder(b)));
        }

        public override void OnInspectorGUI()
        {
            GUIStyle _centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            _centeredStyle.alignment = TextAnchor.UpperCenter;

            if (!ikp)
            {
                ikp = (IKP)target;
            }

            if (drawOrder == null || drawOrder.Count != ikp.GetModulesCount())
            {
                UpdateDrawOrder();
            }

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button(new GUIContent(IKPStyle.logo, "Rate on the asset store!"), _centeredStyle, GUILayout.MaxWidth(Screen.width)))
            {
                //load AS link
                Application.OpenURL(ASSET_STORE_URL);
            }

            GUILayout.BeginHorizontal();
            {
                Texture2D _shouldShowWarning = ikp.IsConfigured() ? null : IKPStyle.warningIcon;

                if (GUILayout.Button(new GUIContent("Quick Setup", _shouldShowWarning)))
                {
                    string error;
                    if (IKPEditorUtils.CanEditGameObjectState(ikp.gameObject, out error))
                    {
                        if (EditorUtility.DisplayDialog("[1/2] Auto Assign Bones?", "This action will try to automatically find and assign missing bones. Additional manual tweaks may be needed.", "Yes", "No"))
                        {
                            ikp.EditorAutoSetupModules();
                        }
                    }
                    else
                    {
                        //Open prefab mode first
                        EditorUtility.DisplayDialog("One more step!", error, "Ok");
                    }
                }

                BugReporting.StandardBugReportButton();
            }
            GUILayout.EndHorizontal();

            SETUtil.EditorUtil.HorizontalRule();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent("► Collapse All", "Collapse all opened menus"), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT), GUILayout.Width(95)))
                {
                    ikp.ExpandAll(false);
                }


                using (new ColorPocket(IKPStyle.COLOR_ACTIVE))
                {
                    if (GUILayout.Button(new GUIContent(" Add / Edit Modules", IKPStyle.addIcon), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT)))
                    {
                        OpenModuleEditor();
                    }
                }
            }
            GUILayout.EndHorizontal();
            SETUtil.EditorUtil.HorizontalRule();

            bool _hasOneModule = false;
            using (new ColorPocket(IKPStyle.COLOR_ACTIVE))
            {
                foreach (var _signature in drawOrder.ToArray())
                {
                    if (ikp.HasModule(_signature))
                    {
                        //try to find the module
                        var _module = ikp.GetModule(_signature);

                        _hasOneModule = true;

                        GUILayout.BeginHorizontal();
                        {
                            GUI.color = _module.IsActive() ? IKPStyle.COLOR_ACTIVE : IKPStyle.COLOR_DISABLED;

                            string expandLabel = _module.GetExpand() ? "▼" : "►";
                            if (GUILayout.Button(expandLabel, GUILayout.Height(IKPStyle.BIG_HEIGHT), GUILayout.Width(IKPStyle.BIG_HEIGHT)))
                            {
                                _module.SetExpand(!_module.GetExpand());
                            }

                            string _stateText = _module.IsActive() ? "Active" : "Disabled";
                            var _errors = new List<ValidationResult>();
                            bool _checkConfig = ikp.IsConfigured(_errors, _signature);

                            string _warningMessages = "";

                            if (!_checkConfig)
                            {
                                foreach (var _error in _errors)
                                {
                                    if (_error.outcome == ValidationResult.Outcome.Valid)
                                    {
                                        continue;
                                    }

                                    _warningMessages += $"\n[{_error.outcome}] {_error.message}";
                                }

                                using (new ColorPocket(Color.white)) {
                                    if (GUILayout.Button(IKPStyle.warningIcon, GUILayout.Height(IKPStyle.BIG_HEIGHT), GUILayout.Width(IKPStyle.BIG_HEIGHT)))
                                    {
                                        EditorUtility.DisplayDialog("Warnings:", _warningMessages, "Close");
                                    }
                                }
                            }

                            using (new ColorPocket(_module.IsActive() ? IKPStyle.COLOR_ACTIVE : IKPStyle.COLOR_DISABLED)) {
                                var _icon = ModuleManager.Linker(_signature).icon;
                                if (GUILayout.Button(new GUIContent($" {ModuleManager.GetName(_signature)} Module: {_stateText}", _icon), GUILayout.Height(IKPStyle.BIG_HEIGHT)))
                                {
                                    ikp.ToggleModule(_signature);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        using (new ColorPocket(_module.IsActive() ? Color.white : IKPStyle.COLOR_GREY)) //paint the underlying custom GUI elements according to the state of the module
                        {
                            if (_module.GetExpand())
                            {
                                GUILayout.BeginVertical(EditorStyles.helpBox);
                                {
                                    _module.DrawEditorGUI();
                                }
                                GUILayout.EndVertical();
                            }
                        }
                    }
                }
            }

            if (!_hasOneModule)
            {
                GUILayout.Label("No modules to display! Add modules from the menu (+ Add Modules)", EditorStyles.helpBox);
            }

            GUILayout.Label("Other: ", EditorStyles.boldLabel);

            using (SerializedObject _so = new SerializedObject(ikp))
            {
                SerializedProperty _so_raycastingMask = _so.FindProperty(nameof(ikp.raycastingMask));
                SerializedProperty _so_ikChildren = _so.FindProperty(nameof(ikp.ikChildren));
                SerializedProperty _so_skipValidationInPlayMode = _so.FindProperty(nameof(ikp.skipValidationInPlayMode));

                EditorGUILayout.PropertyField(_so_raycastingMask);
                EditorGUILayout.PropertyField(_so_ikChildren);
                EditorGUILayout.PropertyField(_so_skipValidationInPlayMode);

                if (EditorGUI.EndChangeCheck())
                    _so.ApplyModifiedProperties();
            }
        }

        private void OpenModuleEditor()
        {
#if UNITY_EDITOR && UNITY_2018_1_OR_NEWER
            //Open prefab mode first
            string error;
            if (!IKPEditorUtils.CanEditGameObjectState(ikp.gameObject, out error))
            {
                EditorUtility.DisplayDialog("One more step!", error, "Ok");
                return;
            }
#endif
            IKP_ModuleAssistant.ShowWindow(0);
        }
    }
}
#endif
