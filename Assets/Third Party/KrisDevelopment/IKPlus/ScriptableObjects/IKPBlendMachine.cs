using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IKPn;
using IKPn.Blend;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewBlendMachine", menuName = "IK Plus/Blend Machine"), System.Serializable]
public class IKPBlendMachine : ScriptableObject
{
	public static string DEFAULT_STATE = "Default";
	public string defaultState = IKPBlendMachine.DEFAULT_STATE;
	public float transitionTime = 0.5f;
	public BlendNodeData[] blendNodeData = new BlendNodeData[0];
	public bool firstEdit = false;
	
	
	public void ClearUnusedData ()
	{
		#if UNITY_EDITOR
		foreach(BlendNodeData bndItem in blendNodeData){
			//clear property mask
			int
				_lastUsedMaskItem = -1,
				_propertyCount = IKPUtils.GetPropertyNames(bndItem.layer).Length;
				
			for(int i = 0; i < _propertyCount; i++){
				if(!bndItem.GetPropertyMaskValue(i) && bndItem.animation.GetUse(i)) //note: false flags are the ignored items
					_lastUsedMaskItem = i;
			}
			
			int _resize = (_lastUsedMaskItem + 1) - bndItem.propertyMask.Length;
			SETUtil.Deprecated.ArrUtil.Resize(ref bndItem.propertyMask, _resize);
			
			//clear states
			if(bndItem.states != null && bndItem.nodeType != IKPNodeType.EventMachine)
				bndItem.states = null;
		}
		
		SerializeData(blendNodeData);
		#endif
	}
	
	public void SerializeData (BlendNodeData[] nodes, int? id = null)
	{
		#if UNITY_EDITOR
			SerializedObject so = new SerializedObject (this);
			SerializedProperty so_blendNodeData = so.FindProperty(nameof(blendNodeData));
			
			if(id == null) //if there has been specified a node to serialize, then don't resize the array to prevent loosing data
				so_blendNodeData.arraySize = nodes.Length;
			
			for(int i = 0; i < nodes.Length; i++){
				if(id != null)//if a node has been specified for serialization then set the index (then break the operation)
					i = (int)id;
				
				SerializedProperty bli = so_blendNodeData.GetArrayElementAtIndex(i);
			
				SerializedProperty
					bli_rect = bli.FindPropertyRelative(nameof(BlendNodeData.rect)),
					bli_nodeType = bli.FindPropertyRelative(nameof(BlendNodeData.nodeType)),
					bli_animation = bli.FindPropertyRelative(nameof(BlendNodeData.animation)),
					bli_propertyMask = bli.FindPropertyRelative(nameof(BlendNodeData.propertyMask)),
					bli_standardOutput = bli.FindPropertyRelative(nameof(BlendNodeData.standardOutput)),
					bli_states = bli.FindPropertyRelative(nameof(BlendNodeData.states)),
					bli_name = bli.FindPropertyRelative(nameof(BlendNodeData.name)),
					bli_layer = bli.FindPropertyRelative(nameof(BlendNodeData.layer)),
					bli_stateSwitch = bli.FindPropertyRelative(nameof(BlendNodeData.stateSwitch));
					
				bli_rect.rectValue = nodes[i].rect;
				bli_nodeType.enumValueIndex = (int)nodes[i].nodeType;
				bli_animation.objectReferenceValue = nodes[i].animation;
				bli_standardOutput.intValue = nodes[i].standardOutput;
				bli_name.stringValue = nodes[i].name;
				bli_layer.stringValue = nodes[i].layer;
				bli_stateSwitch.stringValue = nodes[i].stateSwitch;
				
				//property mask array
				if(nodes[i].propertyMask != null && nodes[i].propertyMask.Length > 0){
					bli_propertyMask.arraySize = nodes[i].propertyMask.Length;
					
					for(int j = 0; j < nodes[i].propertyMask.Length; j++){
						SerializedProperty _p = bli_propertyMask.GetArrayElementAtIndex(j);
						_p.boolValue = nodes[i].propertyMask[j];
					}
				}else
					bli_propertyMask.arraySize = 0;
					
				//states array
				if(nodes[i].states != null && nodes[i].states.Length > 0){
					bli_states.arraySize = nodes[i].states.Length;
					
					for(int j = 0; j < nodes[i].states.Length; j++){
						SerializedProperty sti = bli_states.GetArrayElementAtIndex(j);
						
						SerializedProperty
							sti_state = sti.FindPropertyRelative(nameof(NodeStateContent.state)),
							sti_output = sti.FindPropertyRelative(nameof(NodeStateContent.output)),
							sti_rect = sti.FindPropertyRelative(nameof(BlendNodeData.rect));
						
						sti_state.stringValue = nodes[i].states[j].state;
						sti_rect.rectValue = nodes[i].states[j].rect;
						sti_output.intValue = nodes[i].states[j].output;
					}
				}else
					bli_states.arraySize = 0;
				
				if(id != null) //break the operation because only one specific node was serialized
					break;
			}
			
			so.ApplyModifiedProperties();
		#endif
	}
	
	public int RemapNodes (string from, string to)
	{
		int _moved = 0; //returns amount of moved nodes
		if(blendNodeData != null)
			foreach(BlendNodeData bndItem in blendNodeData){
				if(bndItem.layer == from || from == "*"){
					bndItem.layer = to;
					_moved++;
				}
			}
		
		SerializeData(blendNodeData);
		return _moved;
	}
	
	public BlendAnimationValue Evaluate (NodePointer nodePointer)
	{
		BlendAnimationValue val = new BlendAnimationValue();
		
		if(nodePointer.currentNode != null){
			IKPBlendAnimation anim = blendNodeData[(int)nodePointer.currentNode].animation;
			if(anim != null){
				val.toggle = (anim.toggle == ToggleAnimation.Default) ? (bool?)null :
				(anim.toggle == ToggleAnimation.On);
				
				val.properties = new float?[anim.GetPropertiesLength()];
				for(int i = 0; i < val.properties.Length; i++){
					if(anim.GetUse(i) && blendNodeData[(int)nodePointer.currentNode].GetPropertyMaskValue(i))
						val.properties[i] = anim.GetProperty(i).Evaluate(nodePointer.animationProgress);
					else
						val.properties[i] = null;
				}
			}
		}
		
		return val;
	}
	
	public ToggleAnimation EvaluateToggle (NodePointer nodePointer){
		if(nodePointer.currentNode != null)
			return blendNodeData[(int)nodePointer.currentNode].animation.toggle;
		
		return ToggleAnimation.Default;
	}
	
	public int GetOutputNode (int id, string layer){ //get output from current animation
		for(int i = 0; i < blendNodeData.Length; i++){
			if(blendNodeData[i].layer == layer)
				return blendNodeData[i].standardOutput;
		}
		return -1; //search failed
	} 
	
	public int GetOutputNode (string state, string layer){ //get output from state
		for(uint i = 0; i < blendNodeData.Length; i++){
			if(blendNodeData[i].layer == layer)
				if(blendNodeData[i].states != null && blendNodeData[i].nodeType == IKPNodeType.EventMachine)
					for(uint j = 0; j < blendNodeData[i].states.Length; j++)
						if(state == blendNodeData[i].states[j].state)
							return blendNodeData[i].states[j].output;
		}
		
		return -1; //search failed
	} 
	
	public BlendNodeData[] ExtractNodeData(string layer) {
		List<BlendNodeData> targetNodes = new List<BlendNodeData>();
		
		for(uint i = 0; i < blendNodeData.Length; i++){
			if(blendNodeData[i].layer == layer)
				targetNodes.Add(blendNodeData[i]);
		}
		
		return targetNodes.ToArray();
	}
	
	public string[] CollectStates () { //go through all event machines and record all mentioned states
		List<string> statesList = new List<string>();
		
		for(uint i = 0; i < blendNodeData.Length; i++){
			if(blendNodeData[i].nodeType == IKPNodeType.EventMachine)
				for(uint j = 0; j < blendNodeData[i].states.Length; j++){
					if(!CheckRecord(statesList, blendNodeData[i].states[j].state))
						statesList.Add(blendNodeData[i].states[j].state);
				}
		}
		
		return statesList.ToArray();
	}
	
	public bool CheckForDuplicates () { return CheckForDuplicates(null); }
	public bool CheckForDuplicates (string layer) {
		List<string> statesList = new List<string>();
		
		if(blendNodeData == null) return false;
		
		for(uint i = 0; i < blendNodeData.Length; i++){
			if(blendNodeData[i].nodeType == IKPNodeType.EventMachine){
				
				if(layer != null && layer == blendNodeData[i].layer)
					goto Skip;
				
				for(uint j = 0; j < blendNodeData[i].states.Length; j++){
					if(CheckRecord(statesList, blendNodeData[i].states[j].state)){
						return true;
					}
					
					statesList.Add(blendNodeData[i].states[j].state);
				}
				
				Skip:{}
			}
		}
		return false;
	}
	
	//check if the given string is already listed
	bool CheckRecord (List<string> strList, string str) {
		for(int i = 0; i < strList.Count; i ++){
			if(strList[i] == str)
				return true;
		}
		
		return false;
	}
	
	void OnEnable () {
		#if UNITY_EDITOR
		if(blendNodeData == null){
			Debug.Log("[WARNING] blendNodeData is null");
			SerializedObject so = new SerializedObject(this);
			SerializedProperty sp = so.FindProperty(nameof(blendNodeData));
			sp.arraySize = 0;
			so.ApplyModifiedProperties();
		}
		#endif
	}
}
