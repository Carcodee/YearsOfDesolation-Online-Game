using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IKPn.Blend;
using IKPn;

[CreateAssetMenu(fileName = "New Blend Animation", menuName = "IK Plus/Blend Animation"), System.Serializable]
public class IKPBlendAnimation : ScriptableObject
{

	public const string PROPERTY_NAME_Use = nameof(use);
	public const string PROPERTY_NAME_Properties = nameof(properties);

	public float speed;
	public ToggleAnimation toggle; //toggle the module

	[SerializeField] private bool[] use;
	[SerializeField] private AnimationCurve[] properties;
	
	public IKPBlendAnimation () {
		speed = 1f;
		toggle = ToggleAnimation.Default;
		properties = new AnimationCurve[0];
		use = new bool[0];
	}
	
	public float GetMaxSize(){ //get longest animation curve
		float size = 0f;
		for(uint i = 0; i < properties.Length; i++){ //find the longest animation
			int kindex = properties[i].length - 1;
			if(kindex >= 0){
				float animSz = properties[i].keys[kindex].time;
				if(animSz > size)
					size = animSz;
			}
		}
		return size;
	}
	
	public int GetPropertiesLength(){
		if(properties != null)
			return properties.Length;
		Debug.Log("[ERROR IKPBlendAnimation.properties is null!");
		return 0;
	}
	
	public AnimationCurve GetProperty(int i){ //get properties animation at index
		if(SETUtil.Deprecated.ArrUtil.AutoResize(ref properties, i))
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
		return properties[i];
	}
	
	public bool GetUse (int i){ //get
		if(SETUtil.Deprecated.ArrUtil.AutoResize(ref use, i))
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		if(use != null)
			if(use.Length > i)
				return use[i];
		return false;
	}
}
