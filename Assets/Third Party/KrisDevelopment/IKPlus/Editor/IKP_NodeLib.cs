
#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using IKPn.Blend;
using UnityEngine.Serialization;
using SETUtil.Types;
using SETUtil.Common.Types;

namespace IKPn.Node
{
	//NODE Editor
	[System.Serializable]
	public class NodeField
	{
		public IKPBlendMachine blendMachine;
		private Node[] nodes;
		public Rect controlRect = new Rect(0, 0, 1, 1);
		private Rect displayRect = new Rect(0, 0, 1, 1);
		public bool eventDuplicate = false;

		[FormerlySerializedAs("linkHandler")] public NodeLink nodeLink;

		private Vector2 mouseInitialPos = Vector2.zero;
		private Vector2 mousePos = Vector2.zero;
		private bool dragEventStarted = false;
		private int? selectedNode = null;
		public bool debug = false;
		public int selectedLayer = 0;

		private string
			debugLog = "",
			dynamicDebugLog = "";

		public NodeField(IKPBlendMachine blendMachine)
		{
			SetControlRect(new Rect(0, 0, Screen.width, Screen.height));
			this.blendMachine = blendMachine;
			ClearLinkHandler();
			LoadNodes(blendMachine.blendNodeData);
			CheckForDuplicates();

			//First-time load instructions
			if (!blendMachine.firstEdit) {
				GenerateEventMachines();
				SerializedObject so = new SerializedObject(blendMachine);
				SerializedProperty so_firstEdit = so.FindProperty(nameof(IKPBlendMachine.firstEdit));
				so_firstEdit.boolValue = true;
				so.ApplyModifiedProperties();
			}
		}

		public void SetLayer(int layer)
		{
			selectedLayer = layer;
		}

		void LoadNodes(BlendNodeData[] blendNodeData)
		{
			nodes = new Node[blendNodeData.Length];

			for (int i = 0; i < blendNodeData.Length; i++) {
				nodes[i] = new Node(blendNodeData[i].rect, blendNodeData[i].name, blendNodeData[i].nodeType, this);
				nodes[i].animation = blendNodeData[i].animation;
				nodes[i].propertyMask = blendNodeData[i].propertyMask;
				nodes[i].states = blendNodeData[i].states;
				nodes[i].layer = blendNodeData[i].layer;
				nodes[i].standardOutput = blendNodeData[i].standardOutput;
				nodes[i].stateSwitch = blendNodeData[i].stateSwitch;
			}
		}

		public void SetControlRect(Rect controlRect)
		{
			this.controlRect = controlRect;
		}

		public Node AddNode(Node node)
		{
			return AddNode(node.name, node.rect, node.nodeType);
		}

		public Node AddNode(string name)
		{
			return AddNode(name, IKPNodeType.Default);
		}

		public Node AddNode(string name, IKPNodeType nodeType)
		{
			Vector2 newNodePosition = Vector2.zero;
			if (nodes.Length > 0) {
				Node previousNode = GetNode(nodes.Length - 1);
				if (previousNode != null) {
					newNodePosition = new Vector2(previousNode.rect.x, previousNode.rect.y + previousNode.rect.height + 10);
				}
			}

			return AddNode(name, newNodePosition, nodeType);
		}

		public Node AddNode(string name, Vector2 position, IKPNodeType nodeType)
		{
			Rect newNodeRect = Node.DEFAULT_RECT;
			newNodeRect.x = position.x - newNodeRect.width / 2;
			newNodeRect.y = position.y - newNodeRect.height / 2;
			return AddNode(name, newNodeRect, nodeType);
		}

		public Node AddNode(string name, Rect rect, IKPNodeType nodeType)
		{
			Node newNode = new Node(rect, name, nodeType, this);
			Node[] tempNodes = new Node[nodes.Length];
			for (int i = 0; i < nodes.Length; i++)
				tempNodes[i] = nodes[i];
			nodes = new Node[nodes.Length + 1];
			for (int i = 0; i < tempNodes.Length; i++)
				nodes[i] = tempNodes[i];
			nodes[nodes.Length - 1] = newNode;
			newNode.layer = IKPUtils.moduleSignatures[selectedLayer];

			SerializeData();
			return newNode;
		}

		public void DeleteNode(int i)
		{
			//display dialogue
			if (!EditorUtility.DisplayDialog("Delete Node (" + nodes[i].name + ")?", "Do you really want to delete this node? You can't use Undo to reverse the deletion.", "Delete!", "Cancel"))
				return;

			//clear links
			ClearConnectedLinks(i);

			//resize arrays
			Node[] tempNodes = new Node[nodes.Length];
			int count = 0;
			for (int j = 0; j < nodes.Length; j++)
				if (i != j)
					tempNodes[j] = nodes[j];
			nodes = new Node[nodes.Length - 1];
			for (int j = 0; j < tempNodes.Length; j++)
				if (tempNodes[j] != null) {
					nodes[count] = tempNodes[j];
					count++;
				}

			selectedNode = null;

			ShiftLinksDown(i);
			SerializeData();
		}

		public Node GetNode(int i)
		{
			if (nodes.Length > i) {
				return nodes[i];
			}

			return null;
		}

		void OnDraw()
		{
			Event current = Event.current;
			mousePos = current.mousePosition;
			bool contains = Contains(displayRect, mousePos);
			int? nodeHover = null;

			if (nodes != null)
				for (int i = 0; i < nodes.Length; i++) {
					if (nodes[i].layer == IKPUtils.moduleSignatures[selectedLayer]) {
						Rect nodeRect = IKPUtils.AddRectPosition(nodes[i].rect, controlRect);
						DynamicDebug("Node [" + nodes[i].name + "]: x:" + nodeRect.x + " y:" + nodeRect.y + " w:" + nodeRect.width + " h:" + nodeRect.height);
						if (Contains(nodeRect, mousePos - new Vector2(displayRect.x, displayRect.y))) {
							nodeHover = i;
						}
					}
				}


			if ((contains) ? true : dragEventStarted) {
				DynamicDebug("FIELD CONTAINS MOUSE!");

				if (current.type == EventType.MouseDown) {
					//MOUSE CLICK ONCE
					//node deselect (run before the link handler checks)
					if (nodeHover == null && !dragEventStarted) {
						selectedNode = null;

						for (int i = 0; i < nodes.Length; i++) //clear previous selections
							nodes[i].active = false;
					}

					mouseInitialPos = mousePos;

					if (current.button == (int) MouseButton.Left) //select node
						if (!dragEventStarted && nodeHover != null) {
							for (int i = 0; i < nodes.Length; i++) //clear previous selections
								nodes[i].active = false;

							selectedNode = nodeHover;
							nodes[(int) selectedNode].active = true;
						}
				}

				if (current.type == EventType.MouseDrag) {
					if (current.button == (int) MouseButton.Middle) {
						//drag view
						if (!dragEventStarted) {
							dragEventStarted = true;
						} else {
							controlRect.x -= (mouseInitialPos.x - mousePos.x);
							controlRect.y -= (mouseInitialPos.y - mousePos.y);
							mouseInitialPos = mousePos;
						}
					}

					if (current.button == (int) MouseButton.Left) {
						//drag node
						if (!dragEventStarted && nodeHover != null) {
							dragEventStarted = true;
						} else if (selectedNode != null) {
							nodes[(int) selectedNode].Drag(mousePos - mouseInitialPos);
							mouseInitialPos = mousePos;
						}
					}
				}
			}

			if (current.type == EventType.MouseUp) {
				dragEventStarted = false;

				if (current.button == (int) MouseButton.Right) {
					//right click check
					if (nodeLink.outputNodeId == null) {
						if (nodeHover == null) {
							//show context menu
							GenericMenu menu = new GenericMenu();
							AddFieldContextItem(menu, "Add Blend Animation", IKPNodeType.Animation);
							AddFieldContextItem(menu, "Add Event Machine", IKPNodeType.EventMachine);
							AddFieldContextItem(menu, "Add Event Call", IKPNodeType.EventCall);
							menu.ShowAsContext();
						}
					} else {
						nodeLink.outputNodeId = null;
					}
				}

				SerializeData();
			}

			DynamicDebug("Mouse Pos: x:" + mousePos.x + " y:" + mousePos.y);
		}

		public Rect Draw()
		{
			Vector2 borderOffset = new Vector2(5, 5);

			//DRAW DUMMY LAYOUT GROUP TO GET THE RECT FROM
			GUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(Screen.height));
			GUILayout.Label("");
			GUILayout.EndVertical();
			Rect fieldRect = GUILayoutUtility.GetLastRect();

			//rect update handling (ignore dummy rect at layout event)
			if (Event.current.type != EventType.Repaint)
				fieldRect = this.displayRect;
			else {
				this.displayRect = fieldRect;
			}

			fieldRect.width -= borderOffset.x;
			fieldRect.height -= borderOffset.y;

			Draw(fieldRect, false);
			return fieldRect;
		}

		public void Draw(Rect displayRect, bool setDisplayRect)
		{
			if (setDisplayRect)
				this.displayRect = displayRect;

			OnDraw();

			GUI.color = IKPStyle.COLOR_NODE_BG;
			GUILayout.BeginArea(displayRect, "", "Box");
			DrawBackground();
			GUI.color = Color.white;

			if (nodes != null) {
				for (int i = 0; i < nodes.Length; i++) {
					if (nodes[i].layer == IKPUtils.moduleSignatures[selectedLayer]) {
						nodes[i].Draw(new Vector2(controlRect.x, controlRect.y), i);
						DrawLinks(i);

						if (selectedNode == i) {
							//selected node options
							if (debug)
								if (i < nodes.Length) //do this to check if the index is still inside the length after deletion
									GUI.Label(new Rect(nodes[i].rect.x + controlRect.x, nodes[i].rect.y - 20 + controlRect.y, nodes[i].rect.width, 20),
										string.Format("ID: {0} Layer: {1}", i, nodes[i].layer), EditorStyles.helpBox);
						}
					}
				}

				if (nodeLink != null) //draw a link line during the link process
					if (nodeLink.outputNodeId != null) {
						DrawLine(nodes[(int) nodeLink.outputNodeId].GetPosition()
						         + ((nodes[(int) nodeLink.outputNodeId].nodeType == IKPNodeType.EventMachine) ? nodes[(int) nodeLink.outputNodeId].StateOutputPosition((int) nodeLink.contentId) : nodes[(int) nodeLink.outputNodeId].StandardOutputPosition()),
							mousePos - new Vector2(controlRect.x + displayRect.x, controlRect.y + displayRect.y));
					}
			}

			//debug draw
			if (debug) {
				GUILayout.Label(dynamicDebugLog, EditorStyles.helpBox, GUILayout.Width(displayRect.width / 2));
				GUILayout.Label("LOG:\n" + debugLog, EditorStyles.helpBox, GUILayout.Width(displayRect.width / 2));
			}

			GUILayout.EndArea();

			ClearDynamicDebug();

			GUI.Button(displayRect, "", "label"); //force GUI update

			LinkCheck();
		}

		public void SerializeData(int? id = null)
		{
			//serialize a specific data index
			if (!blendMachine) {
				PrintDebug("Serialization failed due to missing blend machine reference.");
				return;
			}

			blendMachine.SerializeData(nodes as BlendNodeData[], id);
		}

		public void CheckForDuplicates()
		{
			eventDuplicate = blendMachine.CheckForDuplicates(IKPUtils.moduleSignatures[selectedLayer]);
		}

		public void Clear()
		{
			nodes = new Node[0];
		}

		public void ClearLinkHandler()
		{
			nodeLink = new NodeLink();
		}

		public void DrawLine(Vector2 p1, Vector2 p2)
		{
			Vector2 p1b = new Vector2((p1.x + p2.x) / 2f, p1.y);
			Vector2 p2b = new Vector2((p1.x + p2.x) / 2f, p2.y);
			Vector2 posOffset = new Vector2(controlRect.x, controlRect.y);

			Handles.BeginGUI();
			Handles.DrawBezier(p1 + posOffset, p2 + posOffset, p1b + posOffset, p2b + posOffset, Color.grey, null, 6);
			Handles.DrawBezier(p1 + posOffset, p2 + posOffset, p1b + posOffset, p2b + posOffset, IKPStyle.COLOR_NODE_LINE, null, 3);
			Handles.EndGUI();
		}

		void DrawBackground()
		{
			Rect rect = new Rect(controlRect.x, controlRect.y, displayRect.width, displayRect.height);
			float segmentSize = 13;
			int wideLineIndex = 10;
			int loopsHorizontal = (int) Mathf.Ceil(rect.width / segmentSize) + 1;
			int loopsVertical = (int) Mathf.Ceil(rect.height / segmentSize) + 1;
			float addedWidth = 0;

			Handles.BeginGUI();
			for (int i = 0; i < loopsVertical; i++) {
				if (i % wideLineIndex == 0)
					addedWidth = 2;
				else addedWidth = 0;
				float loopHeight = loopsVertical * segmentSize;

				Vector2 p1 = new Vector2(0, rect.y + segmentSize * i);
				p1.y = p1.y % (loopHeight);
				if (p1.y < 0)
					p1.y = loopHeight + p1.y;
				Vector2 p2 = new Vector2(rect.width, rect.y + segmentSize * i);
				p2.y = p2.y % (loopHeight);
				if (p2.y < 0)
					p2.y = loopHeight + p2.y;

				Handles.DrawBezier(p1, p2, p1, p2, IKPStyle.COLOR_NODE_BGLINE, null, 3 + addedWidth);
			}

			for (int i = 0; i < loopsHorizontal; i++) {
				if (i % wideLineIndex == 0)
					addedWidth = 2;
				else addedWidth = 0;
				float loopWidth = loopsHorizontal * segmentSize;

				Vector2 p1 = new Vector2(rect.x + segmentSize * i, 0);
				p1.x = p1.x % (loopWidth);
				if (p1.x < 0)
					p1.x = loopWidth + p1.x;

				Vector2 p2 = new Vector2(rect.x + segmentSize * i, rect.height);
				p2.x = p2.x % (loopWidth);
				if (p2.x < 0)
					p2.x = loopWidth + p2.x;

				Handles.DrawBezier(p1, p2, p1, p2, IKPStyle.COLOR_NODE_BGLINE, null, 3 + addedWidth);
			}

			Handles.EndGUI();
		}

		void DrawLinks(int id)
		{
			if (id >= nodes.Length)
				return;

			if (nodes[id].nodeType == IKPNodeType.Animation) {
				if (nodes[id].standardOutput >= 0)
					DrawLine(nodes[id].GetPosition() + nodes[id].StandardOutputPosition(), nodes[(int) nodes[id].standardOutput].GetInputPosition());
			}

			if (nodes[id].nodeType == IKPNodeType.EventMachine) {
				if (nodes[id].states != null)
					for (int i = 0; i < nodes[id].states.Length; i++)
						if (nodes[id].states[i].output >= 0 && nodes[id].states[i].output <= nodes.Length) {
							DrawLine(nodes[id].GetPosition() + nodes[id].StateOutputPosition(i), nodes[(int) nodes[id].states[i].output].GetInputPosition());
						}
			}
		}

		bool Contains(Rect rect, Vector2 point)
		{
			if (rect.x < point.x && rect.width + rect.x > point.x)
				if (rect.y < point.y && rect.height + rect.y > point.y)
					return true;
			return false;
		}

		void LinkCheck()
		{
			//listen for a link output
			if (nodeLink == null)
				return;

			if (nodeLink.clickedNodeId != null && nodeLink.outputNodeId != null) {
				if (nodes[(int) nodeLink.outputNodeId].nodeType == IKPNodeType.EventMachine) {
					if (nodes[(int) nodeLink.outputNodeId].states != null)
						nodes[(int) nodeLink.outputNodeId].states[(int) nodeLink.contentId].output = (int) nodeLink.clickedNodeId;
					else
						PrintDebug("Node " + (int) nodeLink.outputNodeId + " doesn't have states initialized!");
				} else {
					nodes[(int) nodeLink.outputNodeId].standardOutput = (int) nodeLink.clickedNodeId;
				}

				ClearLinkHandler();
			}
		}

		void ShiftLinksDown(int index)
		{
			ShiftLinks(index, -1);
		}

		void ShiftLinks(int index, int amount)
		{
			//shift output all ids after the index by the specified amount			
			for (int n = 0; n < nodes.Length; n++) {
				if (nodes[n].nodeType == IKPNodeType.EventMachine) {
					if (nodes[n].states != null)
						for (int i = 0; i < nodes[n].states.Length; i++) {
							if (nodes[n].states[i].output >= index)
								nodes[n].states[i].output = nodes[n].states[i].output + amount;
						}
				}

				if (nodes[n].nodeType == IKPNodeType.Animation) {
					if (nodes[n].standardOutput >= index)
						nodes[n].standardOutput = nodes[n].standardOutput + amount;
				}
			}
		}

		void ClearConnectedLinks(int index)
		{
			for (int n = 0; n < nodes.Length; n++) {
				if (nodes[n].states != null)
					for (int i = 0; i < nodes[n].states.Length; i++) {
						if (nodes[n].states[i].output == index)
							nodes[n].states[i].output = -1;
					}

				if (nodes[n].standardOutput == index)
					nodes[n].standardOutput = -1;
			}
		}

		public void PrintDebug(string msg)
		{
			//standard debug messages that don't get cleared
			if (debugLog.Length + msg.Length > 16300)
				debugLog = "Log exceeded 16300 characters. Cleaning...";

			debugLog += msg + "\n";
			if (debug)
				Debug.Log("[Node Field] " + msg);
		}

		public void DynamicDebug(string msg)
		{
			//dynamic debug messages that get cleared every frame
			dynamicDebugLog += msg + "\n";
		}

		void ClearDebug()
		{
			debugLog = string.Empty;
		}

		void ClearDynamicDebug()
		{
			dynamicDebugLog = string.Empty;
		}

		void AddFieldContextItem(GenericMenu menu, string path, IKPNodeType nodeType)
		{
			menu.AddItem(new GUIContent(path), false, AddNodeFromContext, nodeType);
		}

		void AddNodeFromContext(object nodeType)
		{
			IKPNodeType nt = (IKPNodeType) nodeType;
			string newNodeName = "Undefined ";
			float height = Node.DEFAULT_RECT.height;
			Rect newNodeRect = Node.DEFAULT_RECT;
			switch (nt) {
				case IKPNodeType.Default:
					newNodeName = "Default Node";
					break;
				case IKPNodeType.Animation:
					newNodeName = "Blend Animation " + nodes.Length;
					break;
				case IKPNodeType.EventMachine:
					newNodeName = "Event Machine";
					break;
				case
					IKPNodeType.EventCall:
					newNodeName = "Event Call " + nodes.Length;
					break;
			}

			newNodeRect.x = mouseInitialPos.x - displayRect.x - controlRect.x;
			newNodeRect.y = mouseInitialPos.y - displayRect.y - controlRect.y;

			AddNode(newNodeName, newNodeRect, nt);
		}

		public void GenerateEventMachines()
		{
			for (int i = 0; i < ModuleManager.Count; CreateBlendMachine(i), i++) ;
		}

		void CreateBlendMachine(int layer)
		{
			Node newNode = AddNode("(Default) Event Machine", new Vector2(140, 105), IKPNodeType.EventMachine);
			newNode.AddState();
			newNode.layer = IKPUtils.moduleSignatures[selectedLayer];
		}
	}

	[System.Serializable]
	public class Node : BlendNodeData
	{
		public static Rect DEFAULT_RECT = new Rect(5, 5, 220, 30);

		public NodeField nodeField;
		public bool active = false;

		private Color color = Color.white;
		private Vector2 inputPosition = Vector2.zero;
		private Vector2[] statesOutputPositions = new Vector2[0];
		private bool editName = false;

		private static string modulePropertyNamesLayer;
		private static string[] modulePropertyNames = new string[0];

		public Node()
		{
			rect = DEFAULT_RECT;
			name = "New Node";
			nodeType = IKPNodeType.Default;
			if (states != null)
				statesOutputPositions = new Vector2[states.Length];
		}

		public Node(Rect rect, NodeField nodeField)
		{
			this.rect = rect;
			name = "New Node";
			nodeType = IKPNodeType.Default;
			this.nodeField = nodeField;
			if (states != null)
				statesOutputPositions = new Vector2[states.Length];
			PaintNode(nodeType);
		}

		public Node(Rect rect, string name, NodeField nodeField)
		{
			this.rect = rect;
			this.name = name;
			nodeType = IKPNodeType.Default;
			this.nodeField = nodeField;
			if (states != null)
				statesOutputPositions = new Vector2[states.Length];
			PaintNode(nodeType);
		}

		public Node(Rect rect, string name, IKPNodeType nodeType, NodeField nodeField)
		{
			this.rect = rect;
			this.name = name;
			this.nodeType = nodeType;
			this.nodeField = nodeField;
			if (states != null)
				statesOutputPositions = new Vector2[states.Length];
			PaintNode(nodeType);
		}

		public void Draw(Vector2 position, int id)
		{
			Draw(position, active, id);
		}

		public void Draw(Vector2 position, bool active, int id)
		{
			//draw selection box
			this.active = active;
			if (active) {
				float border = 4f;
				GUI.color = IKPStyle.COLOR_ACTIVE;
				GUI.Box(new Rect(rect.x + position.x - border, rect.y + position.y - border, rect.width + border * 2, rect.height + border * 2), "", "Button");
			}

			Event current = Event.current;

			//node tools
			GUI.color = color;
			GUILayout.BeginArea(new Rect(rect.x + position.x, rect.y + position.y, rect.width, rect.height), "", "Button");
			GUILayout.BeginHorizontal();
			{
				if (!editName) {
					GUILayout.Label(name, EditorStyles.boldLabel, GUILayout.Width(rect.width - (IKPStyle.MEDIUM_HEIGHT + 6) * 2 - 6), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT));
				} else {
					SETUtil.EditorUtil.BeginColorPocket(Color.white);
					name = GUILayout.TextField(name, GUILayout.Width(rect.width - (IKPStyle.MEDIUM_HEIGHT + 14)));
					SETUtil.EditorUtil.EndColorPocket();
				}

				using(new ColorPocket(Color.white))
				{
					if (!editName)
						if (GUILayout.Button(new GUIContent(IKPStyle.editIcon, "Rename"), "Label", GUILayout.Width(IKPStyle.MEDIUM_HEIGHT), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT))) {
							if (current.button == (int) MouseButton.Left) editName = true;
						}

					if (GUILayout.Button(new GUIContent(IKPStyle.xIcon, "Delete"), "Label", GUILayout.Width(IKPStyle.MEDIUM_HEIGHT), GUILayout.Height(IKPStyle.MEDIUM_HEIGHT))) {
						if (current.button == (int) MouseButton.Left) nodeField.DeleteNode(id);
					}
				}
			}
			GUILayout.EndHorizontal();

			if (nodeField != null) {
				DrawContent(nodeType, id);
			} else {
				EditorGUILayout.HelpBox("[ERROR!] NODE FIELD NOT SPECIFIED!", MessageType.Error);
			}

			GUILayout.EndArea();
			GUI.color = Color.white;

			if (nodeType != IKPNodeType.EventMachine)
				DrawInpitBox(position, id);

			if (nodeType == IKPNodeType.Animation)
				DrawStandardOutputBox(position, id);


			if (editName) {
				if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.Return))
					editName = false;

				if (!active)
					editName = false;
			}
		}

		public void Drag(Vector2 offset)
		{
			if (editName)
				return;
			rect.x += offset.x;
			rect.y += offset.y;
		}

		public Vector2 GetPosition()
		{
			return new Vector2(rect.x, rect.y);
		}

		public void AddState()
		{
			if (states == null)
				states = new NodeStateContent[0];

			NodeStateContent[] tmp = states;
			states = new NodeStateContent[tmp.Length + 1];
			for (int i = 0; i < tmp.Length; i++)
				states[i] = tmp[i];
			states[states.Length - 1] = new NodeStateContent(IKPBlendMachine.DEFAULT_STATE);

			nodeField.SerializeData();
			nodeField.CheckForDuplicates();
		}

		void RemoveState()
		{
			if (states == null)
				return;
			if (states.Length == 0)
				return;

			NodeStateContent[] tmp = states;
			states = new NodeStateContent[tmp.Length - 1];
			for (int i = 0; i < states.Length; i++)
				states[i] = tmp[i];

			nodeField.SerializeData();
			nodeField.CheckForDuplicates();
		}

		void DrawContent(IKPNodeType nodeType, int id)
		{
			if (nodeField == null)
				return;

			//collect module property names for the selected layer
			if (modulePropertyNamesLayer != layer || modulePropertyNames == null) {
				modulePropertyNames = IKPUtils.GetPropertyNames(layer);
				modulePropertyNamesLayer = layer;
			}

			Event current = Event.current;

			using (new ColorPocket(Color.white))
			{
				if (nodeType == IKPNodeType.Animation)
				{
					GUILayout.Label("Animation Source:");

					EditorGUI.BeginChangeCheck();
					animation = (IKPBlendAnimation)EditorGUILayout.ObjectField(animation, typeof(IKPBlendAnimation), false);

					if (EditorGUI.EndChangeCheck())
					{
						if (animation != null)
							name = "Blend Animation";
						nodeField.SerializeData(id); //serialize the animation node if any changes were made to it's values
					}


					if (animation != null)
					{
						GUILayout.Label("Ignored properties: (click to remove)", EditorStyles.miniBoldLabel);
						if (propertyMask != null)
							for (int i = 0; i < propertyMask.Length && i < modulePropertyNames.Length; i++)
							{
								if (!propertyMask[i] && animation.GetUse(i))
								{
									if (GUILayout.Button(new GUIContent(" " + SETUtil.StringUtil.WordSplit(modulePropertyNames[i]), IKPStyle.xIcon), EditorStyles.miniButton, GUILayout.Height(IKPStyle.SMALL_HEIGHT)))
									{
										if (current.button == (int)MouseButton.Left)
										{
											propertyMask[i] = true;
											nodeField.SerializeData(id);
										}
									}
								}
							}

						if (GUILayout.Button(new GUIContent(" Add Property Exception", IKPStyle.addIcon), EditorStyles.popup, GUILayout.Height(IKPStyle.SMALL_HEIGHT)))
						{
							if (current.button == (int)MouseButton.Left)
							{
								PropertyExceptionMenu();
							}
						}
					}
				}

				if (nodeType == IKPNodeType.EventMachine)
				{
					if (states != null)
					{
						if (states.Length != statesOutputPositions.Length) //resize the statesOutputPositions accordingly to be used by the draw methods
							statesOutputPositions = new Vector2[states.Length];

						for (int i = 0; i < states.Length; i++)
						{
							GUILayout.BeginHorizontal();

							string tempSt = states[i].state;
							tempSt = GUILayout.TextField(tempSt, GUILayout.Height(IKPStyle.SMALL_HEIGHT), GUILayout.Width(rect.width - 15 - IKPStyle.SMALL_HEIGHT));
							if (states[i].state != tempSt)
							{
								states[i].state = tempSt;
								nodeField.CheckForDuplicates();
							}

							Rect contentRect = GUILayoutUtility.GetLastRect();
							//rect update handling (ignore dummy* rect at layout event)
							if (Event.current.type != EventType.Repaint)
							{
								contentRect = states[i].rect;
							}
							else
							{
								states[i].rect = contentRect;
							}

							GUILayout.Label("", GUILayout.Width(IKPStyle.SMALL_HEIGHT)); //buffer - spacing
							GUILayout.EndHorizontal();

							DrawOutputBox(contentRect, id, i);
						}
					}

					GUILayout.BeginHorizontal();
					if (GUILayout.Button(new GUIContent(" Add", IKPStyle.addIcon)))
						if (current.button == (int)MouseButton.Left)
							AddState();

					if (GUILayout.Button(new GUIContent(" Remove", IKPStyle.xIcon)))
						if (current.button == (int)MouseButton.Left)
							RemoveState();
					GUILayout.EndHorizontal();
				}

				if (nodeType == IKPNodeType.EventCall)
				{
					string tempSt = stateSwitch;
					tempSt = GUILayout.TextField(tempSt, GUILayout.Width(rect.width - 12));
					if (tempSt != stateSwitch)
					{
						stateSwitch = tempSt;
						nodeField.SerializeData();
					}
				}
			}

			GUILayout.Label(""); //dummy rect for getting the height of the layout
			Rect layoutRect = GUILayoutUtility.GetLastRect();
			int layoutHeight = (int) layoutRect.y;
			//rect update handling (ignore dummy* rect at layout event)
			if (Event.current.type == EventType.Repaint) {
				if (layoutHeight >= Node.DEFAULT_RECT.height)
					rect.height = layoutHeight + 5;
			}
		}

		void PropertyExceptionMenu()
		{
			GenericMenu _menu = new GenericMenu();
			for (int i = 0; i < modulePropertyNames.Length; i++) {
				if (animation.GetUse(i) && GetPropertyMaskValue(i))
					_menu.AddItem(new GUIContent(SETUtil.StringUtil.WordSplit(modulePropertyNames[i])), false, AddPropertyException, i);
			}

			_menu.ShowAsContext();
		}

		void AddPropertyException(object propertyId)
		{
			int
				_pptMaskLen = (propertyMask == null) ? 0 : propertyMask.Length,
				_propertyId = (int) propertyId;
			if (propertyMask == null || propertyMask.Length <= _propertyId) {
				if (propertyMask == null) propertyMask = new bool[0];
				for (int i = 0; i < (_propertyId + 1) - _pptMaskLen; i++)
					SETUtil.Deprecated.ArrUtil.AddElement(ref propertyMask, true);
			}

			propertyMask[_propertyId] = false;
			nodeField.SerializeData();
		}

		void DrawOutputBox(Rect contentRect, int id, int contentId)
		{
			int size = IKPStyle.SMALL_HEIGHT;
			Event current = Event.current;
			Rect inpRect = new Rect(contentRect.width + contentRect.x + 3, contentRect.y, size, size);
			if (GUI.Button(inpRect, new GUIContent("", "Output")) && current.button == (int) MouseButton.Left) {
				states[contentId].output = -1;
				nodeField.nodeLink.contentId = contentId;
				nodeField.nodeLink.clickedNodeId = null;
				nodeField.nodeLink.outputNodeId = id;
			}

			if (contentId < statesOutputPositions.Length)
				statesOutputPositions[contentId] = new Vector2(inpRect.x + inpRect.width / 2, inpRect.y + inpRect.height / 2);
		}

		void DrawStandardOutputBox(Vector2 position, int id)
		{
			Event current = Event.current;
			if (GUI.Button(new Rect(rect.x + rect.width + position.x, position.y + rect.y + rect.height / 2 - IKPStyle.SMALL_HEIGHT / 2, IKPStyle.SMALL_HEIGHT, IKPStyle.SMALL_HEIGHT), new GUIContent("", "Output"))
			    && current.button == (int) MouseButton.Left) {
				standardOutput = -1; //clear the output
				nodeField.nodeLink.clickedNodeId = null;
				nodeField.nodeLink.outputNodeId = id;
			}
		}

		void DrawInpitBox(Vector2 position, int id)
		{
			int size = IKPStyle.SMALL_HEIGHT;
			Rect inpRect = new Rect(rect.x + position.x - size, rect.y + position.y + rect.height / 2 - size / 2, size, size);
			inputPosition = new Vector2(rect.x - size / 2, rect.y + rect.height / 2);

			if (GUI.Button(inpRect, new GUIContent("", "Input"))) {
				nodeField.nodeLink.clickedNodeId = id;
			}
		}

		public Vector2 StateOutputPosition(int i)
		{
			if (i >= statesOutputPositions.Length)
				return Vector2.zero;
			return statesOutputPositions[i];
		}

		public Vector2 StandardOutputPosition()
		{
			return new Vector2(rect.width + IKPStyle.SMALL_HEIGHT / 2f, rect.height / 2);
		}

		public Vector2 GetInputPosition()
		{
			return inputPosition;
		}

		void PaintNode(IKPNodeType type)
		{
			switch (type) {
				case IKPNodeType.Animation:
					color = IKPStyle.COLOR_NODE_ANIM;
					break;
				case IKPNodeType.EventMachine:
					color = IKPStyle.COLOR_NODE_EVENT;
					break;
				case IKPNodeType.EventCall:
					color = IKPStyle.COLOR_NODE_SWITCH;
					break;
				default:
					color = Color.white;
					break;
			}
		}
	}
}

#endif
