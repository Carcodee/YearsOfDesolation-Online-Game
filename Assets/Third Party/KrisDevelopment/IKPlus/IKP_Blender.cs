using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IKPn.Blend;


namespace IKPn
{
	[AddComponentMenu(IKPUtils.IKP_COMPONENT_MENU + "/Animation Blender (IKP_Blender)")]
	public class IKP_Blender : MonoBehaviour
	{
		private string currentState = string.Empty;
		[SerializeField] private IKPBlendMachine blendMachine;
		private Dictionary<string, NodePointer> nodePointers = new Dictionary<string, NodePointer>(); //one node pointer per blend group
		private IKP ikp;


		void Init()
		{
			if (!ikp)
				ikp = GetComponent<IKP>();

			nodePointers.Clear();
			foreach (var _moduleSignature in ModuleManager.GetSignatures()) {
				nodePointers.Add(_moduleSignature, new NodePointer() {
					layer = _moduleSignature
				});
			}
		}

		void Start()
		{
			if (!HasBlendMachine()) return;

			Init();

			if (string.IsNullOrEmpty(currentState)) //call the default state if it has not been called yet
				CallEvent(blendMachine.defaultState);
		}

		void Update()
		{
			if (!ikp || !HasBlendMachine() || nodePointers == null)
				return;

			foreach (var _ptrPair in nodePointers) {
				var _nodePointer = _ptrPair.Value;

				//if there's a function node at all
				if (_nodePointer.currentNode == null) {
					continue;
				}

				int _currentNode = (int) _nodePointer.currentNode;

				if (_nodePointer.nodeType == IKPNodeType.EventCall || _nodePointer.IsIdle()) {
					//if the animation is complete or a state switch
					if (_currentNode >= 0 && _currentNode < blendMachine.blendNodeData.Length) {
						switch (_nodePointer.nodeType) {
							//check if currentNode is pointing to an existing node
							//operations by Node Type:
							case IKPNodeType.Animation: {
								//animation node output
								if (blendMachine.blendNodeData[_currentNode].standardOutput >= 0) {
									JumpToNode(blendMachine.blendNodeData[_currentNode].standardOutput, _ptrPair.Key);
								}

								break;
							}
							case IKPNodeType.EventCall:
								//state switch
								CallEvent(blendMachine.blendNodeData[_currentNode].stateSwitch);
								break;
						}
					}
				} else if (_nodePointer.animationProgress < _nodePointer.animationSize) {
					if (blendMachine.blendNodeData[_currentNode].layer != _ptrPair.Key) {
						continue;
					}

					_nodePointer.animationProgress += Time.deltaTime * _nodePointer.animationSpeed; //continue the animation
					BlendAnimationValue val = blendMachine.Evaluate(_nodePointer); //evaluate the node state
					ikp.SetProperties(val, _nodePointer.layer); //send property values to the IKP
				}
			}
		}

		public void CallEvent(string eventName)
		{
			//the entrance point of the blend system (call this through an external script)
			if (!HasBlendMachine()) {
				return;
			}

			currentState = eventName;

			if (nodePointers == null) {
				Init();
			}

			//look for a matching event name in each module/layer and jump to the node it points to
			foreach (var _ptrPair in nodePointers) {
				//for each module there is a node pointer
				string _layerSignature = _ptrPair.Key;

				for (uint n = 0; n < blendMachine.blendNodeData.Length; n++) {
					//go through all nodes
					if (blendMachine.blendNodeData[n].layer == _layerSignature) {
						//found node on that layer
						if (blendMachine.blendNodeData[n].states != null
						    && blendMachine.blendNodeData[n].nodeType == IKPNodeType.EventMachine) {
							//if the node is an event machine (and has states)
							for (uint j = 0; j < blendMachine.blendNodeData[n].states.Length; j++) {
								if (blendMachine.blendNodeData[n].states[j].state == eventName) {
									JumpToNode(blendMachine.blendNodeData[n].states[j].output, _layerSignature);
									break;
								}
							}
						}
					}
				}
			}
		}

		public void CallEventOnce(string eventName)
		{
			if (currentState != eventName)
				CallEvent(eventName);
		}

		public bool HasBlendMachine()
		{
			return (blendMachine);
		}

		public IKPBlendMachine GetBlendMachine()
		{
			if (HasBlendMachine())
				return blendMachine;

			Debug.Log("[WARNING!] No blend machine assigned!");
			return null;
		}

		public string GetCurrentState()
		{
			return currentState;
		}

		void JumpToNode(int id, string layer)
		{
			if (id < 0)
				return;

			nodePointers[layer].currentNode = id;
			nodePointers[layer].nodeType = blendMachine.blendNodeData[id].nodeType;
			nodePointers[layer].animationProgress = 0f;
			if (blendMachine.blendNodeData[id].animation != null) {
				nodePointers[layer].animationSpeed = blendMachine.blendNodeData[id].animation.speed;
				nodePointers[layer].animationSize = blendMachine.blendNodeData[id].animation.GetMaxSize();

				if (blendMachine.EvaluateToggle(nodePointers[layer]) != ToggleAnimation.Default) {
					bool _toggle = blendMachine.EvaluateToggle(nodePointers[layer]) == ToggleAnimation.On;
					if (blendMachine.EvaluateToggle(nodePointers[layer]) == ToggleAnimation.Off)
						_toggle = false;
					ikp.ToggleModule(nodePointers[layer].layer, _toggle);
				}
			}
		}
	}
}
