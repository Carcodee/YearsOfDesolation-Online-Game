// IKP - by Hristo Ivanov (Kris Development)

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using IKPn.Node;
using SETUtil.Types;
using KrisDevelopment.DistributedInternalUtilities;

namespace IKPn
{
	public class IKP_BlendMachineEditor : EditorWindow //editor window
	{
		public const int MODULE_SELECTOR_WIDTH = 215;
		private static Vector2 dimensions_min = new Vector2(560, 320);

		private bool init = false;
		private int selectedMenu = 0;
		private int selectedModule = 0;

		[NonSerialized] private string[] m_moduleSignatures;
		private string[] moduleSignatures
		{
			get
			{
				return m_moduleSignatures ?? (m_moduleSignatures = ModuleManager.GetSignatures().ToArray());
			}
		}
		
		private Vector2 menuScrollView = Vector2.zero;
		private string[] menuLabels = { "Layers", "Settings", "Remap Nodes" };
		private NodeField nodeField;
		public IKPBlendMachine blendMachine;

		private string
			remapFrom = "*", // "*" - any layer
			remapTo = "",
			remapLog = "";

		[MenuItem(IKPUtils.IKP_TOOL_MENU + "/Blend Machine Editor")]
		public static IKP_BlendMachineEditor ShowWindow()
		{
			var window = GetWindow<IKP_BlendMachineEditor>();
			window.minSize = dimensions_min;
			var windowTitle = new GUIContent("IKP Blender", IKPStyle.blendMachineIcon);
			window.titleContent = windowTitle;
			return window;
		}

		public static void ShowWindow(IKPBlendMachine bm)
		{
			if (!bm)
			{
				Debug.Log("[WARNING!] Blend Machine is NULL. Aborting. ( IKPBlendMachineEditor.Show(IKPBlendMachine) )");
				return;
			}

			IKP_BlendMachineEditor window = ShowWindow();
			window.blendMachine = bm;
			window.Init();
		}

		void OnGUI()
		{
			DrawGUI();
		}

		void OnSelectionChange()
		{
			CheckForBlendMachine();
			Init();
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}

		void CheckForBlendMachine()
		{
			IKPBlendMachine _blendMachine = Selection.activeObject as IKPBlendMachine;
			if (_blendMachine)
			{
				blendMachine = _blendMachine;
			}
		}

		void Init()
		{
			init = false; //reset in case the operation fails 
			if (blendMachine == null)
			{
				return;
			}

			nodeField = new NodeField(blendMachine);
			nodeField.SetLayer(selectedModule);

			init = true;
		}

		void DrawGUI()
		{
			if (!init)
			{
				Init();
				return;
			}

			//serialized properties
			SerializedObject _bms = new SerializedObject(blendMachine);
			SerializedProperty _b_defaultState = _bms.FindProperty(nameof(IKPBlendMachine.defaultState));
			SerializedProperty _b_transitionTime = _bms.FindProperty(nameof(IKPBlendMachine.transitionTime));

			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				string _addedName = " (" + blendMachine.name + ")";
				GUILayout.Label("IKP Blend Machine Editor" + _addedName);
				if (nodeField != null) {
					if (GUILayout.Button(new GUIContent(IKPStyle.moveIcon, "Center view"), EditorStyles.toolbarButton, GUILayout.Width(26)))
						nodeField.SetControlRect(new Rect(0, 0, nodeField.controlRect.width, nodeField.controlRect.height));
				}

				if (GUILayout.Button(new GUIContent(IKPStyle.refreshIcon, "Refresh view"), EditorStyles.toolbarButton, GUILayout.Width(26)))
				{
					ShowWindow(blendMachine);
				}

				if (GUILayout.Button(new GUIContent(IKPStyle.recycleIcon, "Clear unused data"), EditorStyles.toolbarButton, GUILayout.Width(26))) {
					blendMachine.ClearUnusedData();
					ShowWindow(blendMachine);
				}

				BugReporting.ToolbarBugReportButton();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(EditorStyles.helpBox);
			{
				selectedMenu = GUILayout.SelectionGrid(selectedMenu, menuLabels, menuLabels.Length, GUILayout.MaxWidth(menuLabels.Length * 150));
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			//MENU
			GUILayout.BeginVertical(GUILayout.Width(MODULE_SELECTOR_WIDTH));
			menuScrollView = GUILayout.BeginScrollView(menuScrollView);
			
			if (selectedMenu == 0)
			{ //layers
				GUILayout.BeginVertical(EditorStyles.helpBox);
				//modules
				GUILayout.Box("Modules", GUILayout.MaxWidth(Screen.width), GUILayout.Height(19));
				for(int i = 0; i < moduleSignatures.Length; i++) {
					var _signature = moduleSignatures[i];
					var _linker = ModuleManager.Linker(_signature);

					using (new ColorPocket((selectedModule == i) ? IKPStyle.COLOR_ACTIVE : Color.white))
					{
						if (GUILayout.Button(new GUIContent(_linker.displayName, _linker.icon), GUILayout.Height(IKPStyle.BIG_HEIGHT)))
						{
							selectedModule = i;
							if (nodeField != null)
								nodeField.SetLayer(selectedModule);
						}
					}
				}
				GUILayout.EndVertical();
			}
			if (selectedMenu == 1)
			{ //settings
				GUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.Label("Default State:", EditorStyles.boldLabel);
				GUILayout.Box(blendMachine.defaultState, "TextField");

				GUILayout.BeginHorizontal();
				if (GUILayout.Button(IKPBlendMachine.DEFAULT_STATE, GUILayout.Height(20)))
				{
					_b_defaultState.stringValue = IKPBlendMachine.DEFAULT_STATE;
				}
				if (GUILayout.Button(new GUIContent("▼", "Select from list of states. The selected state will be called on start once."), GUILayout.Width(20), GUILayout.Height(20)))
				{
					StateSelectMenu();
				}
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();

				/*
				GUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.Label("Transition:", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(b_transitionTime, GUILayout.Width(MODULE_SELECTOR_WIDTH - 16));
				GUILayout.EndVertical();*/

				nodeField.debug = GUILayout.Toggle(nodeField.debug, "NodeField.debug (" + nodeField.debug + ")");
			}
			if (selectedMenu == 2)
			{ //node remap
				GUILayout.BeginVertical(EditorStyles.helpBox);
				{
					GUILayout.Label("Remap Nodes");
					EditorGUILayout.HelpBox("Remap nodes from one layer to another", MessageType.None);
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label("From");
						remapFrom = EditorGUILayout.TextField(remapFrom);
						GUILayout.Label("To");
						remapTo = EditorGUILayout.TextField(remapTo);
					}
					GUILayout.EndHorizontal();

					if (GUILayout.Button("Remap Now!"))
					{
						RemapNodes();
					}
				}
				GUILayout.EndVertical();

				if (remapLog != "")
				{
					GUILayout.BeginVertical(EditorStyles.helpBox);
					{
						GUILayout.Label(remapLog);
						if (GUILayout.Button("Clear"))
						{
							remapLog = "";
						}
					}
					GUILayout.EndVertical();
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			//NODE FIELD
			Rect _nodeFieldRect = new Rect(); //use it to record the node field rect and display overlay interface on top if needed
			if (nodeField != null)
			{
				_nodeFieldRect = nodeField.Draw();
			}
			else
			{
				GUILayout.Label("[ERROR] Node Field null", EditorStyles.boldLabel);
			}

			//OVERLAY INTERFACE
			if (nodeField != null)
			{
				GUILayout.BeginArea(_nodeFieldRect);
				if (nodeField.eventDuplicate)
				{
					GUI.color = IKPStyle.COLOR_RESET;
					GUILayout.Label("Two or more states share the same name. Only the first of them is going to be called.", EditorStyles.helpBox);
					GUI.color = Color.white;
				}
				GUILayout.EndArea();
			}
			GUILayout.EndHorizontal();

			_bms.ApplyModifiedProperties();
		}

		void StateSelectMenu()
		{
			GenericMenu _menu = new GenericMenu();
			string[] _collectedStates = blendMachine.CollectStates();
			AddStateSelectItem(_menu, IKPBlendMachine.DEFAULT_STATE, IKPBlendMachine.DEFAULT_STATE == blendMachine.defaultState);
			
			for (uint i = 0; i < _collectedStates.Length; i++)
			{
				AddStateSelectItem(_menu, _collectedStates[i], _collectedStates[i] == blendMachine.defaultState);
			}

			_menu.ShowAsContext();
		}

		void AddStateSelectItem(GenericMenu menu, string state, bool selected)
		{
			menu.AddItem(new GUIContent(state), selected, SetDefaultState, state);
		}

		void SetDefaultState(object state)
		{
			SerializedObject _bms = new SerializedObject(blendMachine);
			SerializedProperty _b_defaultState = _bms.FindProperty(nameof(IKPBlendMachine.defaultState));
			_b_defaultState.stringValue = (string)state;
			_bms.ApplyModifiedProperties();
		}

		void RemapNodes()
		{
			int _moved = blendMachine.RemapNodes(remapFrom, remapTo);
			remapLog = string.Format("Moved {0} nodes!", _moved);
			ShowWindow(blendMachine);
		}
	}

	[CustomEditor(typeof(IKPBlendMachine))]
	public class IKPBlendMachineEditor : Editor
	{ //inspector editor

		public override void OnInspectorGUI()
		{
			IKPBlendMachine blendMachine = (IKPBlendMachine)target;

			if (GUILayout.Button(new GUIContent(" Edit", IKPStyle.blendMachineIcon), GUILayout.Height(IKPStyle.BIG_HEIGHT)))
				Edit(blendMachine);
		}

		public void Edit(IKPBlendMachine blendMachine)
		{
			IKP_BlendMachineEditor.ShowWindow(blendMachine);
		}
	}
}
#endif
