using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IKPn;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu(IKPUtils.IKP_COMPONENT_MENU + "/Weapon Setup")]
public class IKP_Weapon : MonoBehaviour
{
	public Transform
		rightHandTarget,
		leftHandTarget;

	public WeaponDimensions weaponDimensions = new WeaponDimensions();

	[HideInInspector]
	[SerializeField]
	[Tooltip("The preferred forward offset of the stock realted to the shoulder")]
	private float forwardOffset = 0.05f;

	private Transform origin;

	//cache
	private IKP_Target
		targScriptL,
		targScriptR;


	void OnDrawGizmosSelected()
	{
		Vector3
			_stockPos = transform.position + transform.forward * weaponDimensions.stock,
			_tipPose = transform.position + transform.forward * (weaponDimensions.length + weaponDimensions.stock);

		IKPEditorUtils.PaintBone(_stockPos, _tipPose, GizmoPalette.Green, true);
	}

	public Transform GetOrigin()
	{
		if (!origin)
			origin = this.transform;
		return origin;
	}

	public Vector3 GetHandPosition(Side side)
	{
		Transform _origin = GetOrigin();

		if (side == Side.Left)
		{
			if (leftHandTarget)
			{
				return _origin.position + GetHandOffset(side);
			}
		}

		if (side == Side.Right)
		{
			if (rightHandTarget)
			{
				return _origin.position + GetHandOffset(side);
			}
		}

		return _origin.position;
	}

	public Quaternion GetHandRotation(Side side)
	{
		if (side == Side.Left)
		{
			if (targScriptL == null)
				targScriptL = leftHandTarget.GetComponent<IKP_Target>();
			if (targScriptL != null)
				return targScriptL.GetRotation();

			return leftHandTarget.rotation;
		}

		if (side == Side.Right)
		{
			if (targScriptR == null)
				targScriptR = rightHandTarget.GetComponent<IKP_Target>();
			if (targScriptR != null)
				return targScriptR.GetRotation();

			return rightHandTarget.rotation;
		}

		// this will probably never get called but it is here to make the compiler happy
		return Quaternion.identity;
	}

	public Vector3 GetHandOffset(Side side)
	{
		if (side == Side.Left && leftHandTarget)
			return leftHandTarget.position - GetOrigin().position;
		else if (side == Side.Right && rightHandTarget)
			return rightHandTarget.position - GetOrigin().position;

		return Vector3.zero;
	}

	public float GetForwardOffset()
	{
		return forwardOffset;
	}

	public void SetForwardOffset(float f)
	{
		bool editor = false;

#if UNITY_EDITOR
		editor = (!Application.isPlaying);
		if (editor)
		{
			SerializedObject so = new SerializedObject(this);
			so.Update();
			SerializedProperty so_forwardOffset = so.FindProperty(nameof(forwardOffset));
			so_forwardOffset.floatValue = f;
			so.ApplyModifiedProperties();
		}
#endif
		if (!editor)
		{
			forwardOffset = f;
		}
	}

	public float GetStockOffset()
	{
		return weaponDimensions.stock;
	}

	public float GetLength()
	{
		return weaponDimensions.length;
	}

	public float GetWidth()
	{
		return weaponDimensions.width;
	}

	[System.Serializable]
	public class WeaponDimensions
	{
		public float stock = 0f;
		public float length = 0.5f;
		public float width = 0.1f;

		public WeaponDimensions()
		{
			stock = 0f;
			length = 0.5f;
			width = 0.1f;
		}

		public WeaponDimensions(float stock, float length, float width)
		{
			this.stock = stock;
			this.length = length;
			this.width = width;
		}
	}
}
