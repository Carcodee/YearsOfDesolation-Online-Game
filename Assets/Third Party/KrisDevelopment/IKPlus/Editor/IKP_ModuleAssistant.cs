
#if UNITY_EDITOR

using System.Collections.Generic;
using SETUtil.Types;
using UnityEngine;
using UnityEditor;
using SETUtil;

namespace IKPn
{
    public class IKP_ModuleAssistant : EditorWindow
    {

        private enum DrawOrderSorting
        {
            InspectorOrder,
            ExecutionOrder,
        }


        private static Vector2 DIMENSION_MIN = new Vector2(250, 250);

        private static int menu = 0;
        private Vector2 scrollView = Vector2.zero;
        private List<string> drawOrder = new List<string>();
        private static string[] menuNames = { "Add/Remove Module", "Linker Assistant" };


        private DrawOrderSorting drawOrderSorting = DrawOrderSorting.InspectorOrder;



        [MenuItem(IKPUtils.IKP_TOOL_MENU + "/Module Assistant")]
        public static void ShowWindow()
        {
            ShowWindow(1);
        }

        public static void ShowWindow(int menuIndex)
        {
            var _win = GetWindow<IKP_ModuleAssistant>("Module Ast.");
            _win.minSize = DIMENSION_MIN;
            IKP_ModuleAssistant.menu = menuIndex;
            _win.UpdateDrawOrder(_win.drawOrderSorting);
        }

        void UpdateDrawOrder(DrawOrderSorting sorting)
        {
			this.drawOrderSorting = sorting;

            drawOrder.Clear();
            drawOrder.AddRange(IKPUtils.moduleSignatures);

            switch (sorting)
            {
                case DrawOrderSorting.InspectorOrder:
                    drawOrder.Sort((a, b) => ModuleManager.GetInspectorOrder(a).CompareTo(ModuleManager.GetInspectorOrder(b)));
                    break;

                case DrawOrderSorting.ExecutionOrder:
                    drawOrder.Sort((a, b) => ModuleManager.GetUpdateOrder(a).CompareTo(ModuleManager.GetUpdateOrder(b)));
                    break;
            }
        }

        void OnGUI()
		{
			GUIStyle centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Box(IKPStyle.logo, centeredStyle, GUILayout.MaxWidth(Screen.width));

            menu = GUILayout.SelectionGrid(menu, menuNames, menuNames.Length);

            if (menu == 0)
            {
                if (drawOrder.Count != ModuleManager.Count){
                    UpdateDrawOrder(drawOrderSorting);
				}

                GameObject _selectedObject = Selection.activeGameObject;
                IKP _selectedIKP = null;

                if (_selectedObject)
                {
                    _selectedIKP = _selectedObject.GetComponent<IKP>();
                }

                if (_selectedIKP == null)
                {
                    GUILayout.Label("No IKP selected!\n[IKP Module Assistant]", centeredStyle);
                    return;
                }

                int modulesCount = ModuleManager.Count;

                if (ModuleManager.Count != modulesCount)
                {
					EditorGUILayout.HelpBox("[ERROR] Module count missmatch", MessageType.Error);
					return;
                }

				string prefabStateError;
				if (!IKPEditorUtils.CanEditGameObjectState(_selectedIKP.gameObject, out prefabStateError))
				{
					EditorGUILayout.HelpBox(prefabStateError, MessageType.Error);
					return;
				}

				GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    GUILayout.Label(_selectedIKP.transform.name + " (IKP) > Available Modules:");
                }
                GUILayout.EndHorizontal();

                using (new ColorPocket())
                {
                    scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.ExpandHeight(false));
                    {
                        foreach (var _signature in drawOrder)
                        {
                            var _linker = ModuleManager.Linker(_signature);

                            if (_linker == null)
                            {
                                continue;
                            }

                            GUI.color = (!_selectedIKP.HasModule(_signature)) || _selectedIKP.GetModule(_signature).IsActive() ? Color.white : IKPStyle.COLOR_GREY;
                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Button(new GUIContent(" " + _linker.displayName + " Module", _linker.icon), GUILayout.Height(IKPStyle.BIG_HEIGHT)))
                                {
                                    _selectedIKP.ToggleModule(_signature);
                                    _selectedIKP.ForceComponentOrder();
                                    IKPEditor.TryRepaint();
                                }

                                if (_selectedIKP.HasModule(_signature))
                                {
                                    if (GUILayout.Button(IKPStyle.xIcon, GUILayout.Height(IKPStyle.BIG_HEIGHT), GUILayout.Width(IKPStyle.BIG_HEIGHT)))
                                    {
                                        _selectedIKP.RemoveModule(_signature);
                                        _selectedIKP.ForceComponentOrder();
                                        IKPEditor.TryRepaint();
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }

                EditorUtil.HorizontalRule();
                GUILayout.Label("Tip: To achieve optimal performance add/toggle only the modules that you need.", EditorStyles.helpBox);

                if (GUILayout.Button("Force Component Order"))
                {
                    _selectedIKP.ForceComponentOrder();
                }
            }
            else if (menu == 1)
            {
                EditorGUILayout.HelpBox("Linker assistant allows you to inspect signature, type, current execution and drawing order of modules.", MessageType.None);

				GUILayout.Label("Module Linkers: " + ModuleManager.Count, EditorStyles.toolbarButton);
                    
                GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                {
                   

					var _sortByInspectorOrderContent = EditorGUIUtility.IconContent("CustomSorting");
					_sortByInspectorOrderContent.tooltip = "Sort by inspector draw order";
					_sortByInspectorOrderContent.text = " By Inspector";

					if (GUILayout.Button(_sortByInspectorOrderContent, EditorStyles.toolbarButton))
                    {
                        UpdateDrawOrder(DrawOrderSorting.InspectorOrder);
                    }

					var _sortByExecutionContent  = EditorGUIUtility.IconContent("CustomSorting");
					_sortByExecutionContent.tooltip = "Sort by execution order";
					_sortByExecutionContent.text = " By Execution";

					if (GUILayout.Button(_sortByExecutionContent, EditorStyles.toolbarButton))
                    {
                        UpdateDrawOrder(DrawOrderSorting.ExecutionOrder);
                    }

					if (GUILayout.Button(new GUIContent(IKPStyle.refreshIcon, "Reload"), EditorStyles.toolbarButton, GUILayout.Width(35)))
                    {
                        ModuleManager.LoadLinkers();
                    }
                }
                GUILayout.EndHorizontal();

                scrollView = GUILayout.BeginScrollView(scrollView);
                int _countDisplay = 0;

                foreach (var _signature in drawOrder)
                {
                    IKPModuleLinker _linker = ModuleManager.Linker(_signature);

                    if (_linker != null)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);
                        {
                            GUILayout.Label(_countDisplay.ToString());
                            GUILayout.BeginVertical();
                            {
                                GUILayout.Label(new GUIContent(" " + _linker.displayName, _linker.icon), EditorStyles.boldLabel, GUILayout.Height(IKPStyle.MEDIUM_HEIGHT));
                                GUILayout.Label("Signature: " + _linker.signature, EditorStyles.label);
                                GUILayout.Label("Type: " + _linker.type.Name, EditorStyles.label);
                                GUILayout.Label("Inspector Order: " + _linker.inspectorOrder, EditorStyles.label);
                                GUILayout.Label("Update/Execution Order: " + _linker.updateOrder, EditorStyles.label);
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        using (new ColorPocket(Color.red))
                        {
                            GUILayout.Box("[!] Missing module linker", GUILayout.MaxWidth(Screen.width));
                        }
                    }

                    _countDisplay++;
                }

                GUILayout.EndScrollView();
            }

            centeredStyle.alignment = TextAnchor.UpperLeft;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}

#endif
