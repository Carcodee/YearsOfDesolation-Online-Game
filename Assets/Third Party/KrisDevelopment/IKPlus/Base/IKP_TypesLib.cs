// IKP - by Hristo Ivanov (Kris Development)

using System;
using UnityEngine;
using TransformData = SETUtil.Types.TransformData;

namespace IKPn
{
	public enum Relative
	{
		World,
		Object
	}

	public enum BodyParts
	{
		Head,
		Neck,
		LeftShoulder,
		LeftElbow,
		LeftHand,
		RightShoulder,
		RightElbow,
		RightHand,
		Chest,
		Spine,
		Hips,
		LeftThigh,
		LeftKnee,
		LeftFoot,
		RightThigh,
		RightKnee,
		RightFoot
	}

	public enum Side
	{
		Left,
		Right
	}

	public enum IKPTargetMode
	{
		Position,
		Transform
	}

	public enum ExecutionFlag
	{
		Continue,
		Break,
	}


	/// <summary>
	/// Keeps track of a module component, added to the game object
	/// </summary>
	[System.Serializable]
	public class ModuleInstanceData
	{
		public string signature;
		public IKPModule module;

		public bool valid
		{
			get
			{
				return (!string.IsNullOrEmpty(signature)
						&& ModuleManager.Has(signature)
						&& module != null);
			}
		}
	}

	public static class BoneNamesLibrary
	{
		public static readonly string[] alwaysIgnore = {
			"armature",
			"handle",
			".rotat",
			"_rotat",
		};

		public static readonly string[] neck = {
			"neck"
		};

		public static readonly string[] head = {
			"head"
		};

		public static readonly string[] hips = {
			"hips",
			"base",
			"main"
		};

		public static readonly string[] spine = {
			"spine",
			"spn",
			"gut",
			"back",
			"torso",
		};

		public static readonly string[] chest = {
			"chest",
            "ribs",
			"upper_torso",
			"upper.torso",
			"torso_upper",
			"torso.upper",
		};

		public static readonly string[] arm = {
			"@arm",
			"shoulder",
			"arm_upper",
			"arm.upper",
			"upper_arm",
			"upper.arm",
			"upperarm"
		};

		public static readonly string[] elbow = {
			"forearm",
			"elbow",
			"arm_lower",
			"arm.lower",
			"lower.arm",
			"lower_arm",
			"low_arm",
			"lowerarm"
		};

		public static readonly string[] hand = {
			"hand",
			"palm",
			"wrist"
		};

		public static readonly string[] thigh = {
			"thigh",
			"leg",
			"upper_leg",
			"upperleg"
		};

		public static readonly string[] knee = {
			"knee",
			"shin",
			"lower_leg",
			"lowerleg",
			"leg.lower",
		};

		public static readonly string[] foot = {
			"foot",
			"feet",
			"toe"
		};

		public static readonly string[] left = {
			"left",
			".l",
			"lft",
			"_l",
			"@l_",
			"@l.",
		};

		public static readonly string[] right = {
			"right",
			".r",
			"rgt",
			"_r",
			"@r_",
			"@r.",
		};
	}

	/// <summary>
	/// IKP encapsulation for the armature bone transforms. Enables orientation correction.
	/// </summary>
	[System.Serializable]
	public class Bone
	{
		public const string TRANSFORM_PROPERTY_NAME = nameof(m_transform);

		[SerializeField] private Transform m_transform;
		public Transform transform { get { return m_transform; } }
		public bool valid { get { return m_transform != null; } }

		public Vector3 position { get { return m_transform.position; } set { m_transform.position = value; } }
		public Quaternion rotation { get { return m_transform.rotation * Quaternion.Inverse(rotationOffset); } set { m_transform.rotation = value * rotationOffset; } }

		public Vector3 up { get { return rotation * Vector3.up; } }
		public Vector3 down { get { return rotation * Vector3.down; } }
		public Vector3 left { get { return rotation * Vector3.left; } }
		public Vector3 right { get { return rotation * Vector3.right; } }
		public Vector3 forward { get { return rotation * Vector3.forward; } }
		public Vector3 back { get { return rotation * Vector3.back; } }

		// ----------------------------------------

		[SerializeField] private Quaternion rotationOffset;
		public Vector3 initialLocalPosition { get; private set; } = Vector3.zero;
		public Vector3 initialLocalPositionNormalized { get; private set; } = Vector3.zero;


		private Bone() {}

		public Bone(Transform transform) : this()
		{
			this.m_transform = transform;
		}


		public IKPLocalSpace GetLocalSpace()
		{
			return new IKPLocalSpace(right, up, forward);
		}

		public Quaternion LookAt(Vector3 targetPoint, Vector3 upVector)
		{
			rotation = Quaternion.LookRotation(targetPoint - position, upVector);
			return rotation;
		}


		///<summary>
		/// Initialize the rotation offset values for the joints based on the initial orientation of the origin.
		///</summary>
		internal void Setup(/*Can be null*/ IKPLocalSpace localSpaceInfo)
		{
			if (localSpaceInfo == null)
			{
				rotationOffset = IKPUtils.INDIFFERENT_QUATERNION;
			}
			else
			{
				rotationOffset = IKPUtils.GetRotationOffset(localSpaceInfo.ToQuaternion(), m_transform.rotation);
			}

			initialLocalPosition = m_transform.localPosition;
			initialLocalPositionNormalized = initialLocalPosition.normalized;
		}
	}

	[System.Serializable]
	public class HeadSetup
	{
		public Transform head;
		[HideInInspector] public Quaternion headRotationOffset;
		public Transform neck;
		[HideInInspector] public Quaternion neckRotationOffset;
		public Transform chest;
		[HideInInspector] public Quaternion chestRotationOffset;
	}

	[System.Serializable]
	public class LowerBodySetup
	{
		public Transform hips;
		[HideInInspector] public Quaternion hipsRotationOffset;
		public Transform leftFoot;
		[HideInInspector] public Quaternion leftFootRotationOffset;
		public Transform leftKnee;
		[HideInInspector] public Quaternion leftKneeRotationOffset;
		public Transform leftThigh;
		[HideInInspector] public Quaternion leftThighRotationOffset;
		public Transform rightFoot;
		[HideInInspector] public Quaternion rightFootRotationOffset;
		public Transform rightKnee;
		[HideInInspector] public Quaternion rightKneeRotationOffset;
		public Transform rightThigh;
		[HideInInspector] public Quaternion rightThighRotationOffset;

		[HideInInspector] public float leftKneeDistance;
		[HideInInspector] public float leftFootDistance;
		[HideInInspector] public float rightKneeDistance;
		[HideInInspector] public float rightFootDistance;
	}

	/// <summary>
	/// Humanoid body data for use in the editor during the setup stage
	/// </summary>
	[System.Serializable]
	public class BodySetupContext
	{
		public Animator animator;
		public Transform root;
		public Transform[] allBones;
    }

	[System.Serializable]
	public class IKPPose
	{
		public TransformData[] poseData;
		public Transform[] bones; //bone references
		public int poseVersion = 0;

		public IKPPose()
		{
			poseData = new TransformData[0];
			bones = new Transform[0];
		}

		public IKPPose(int sz)
		{
			poseData = new TransformData[sz];
			bones = new Transform[sz];
			for (uint i = 0; i < poseData.Length; i++)
				poseData[i] = new TransformData();
		}

		public IKPPose(Transform[] t) : this(t.Length)
		{
			for (int i = 0; i < t.Length && i < poseData.Length && i < bones.Length; i++)
			{
				poseData[i].position = t[i].localPosition;
				poseData[i].rotation = t[i].localRotation;
				bones[i] = t[i];
			}
		}

		void Set(ref TransformData td, Transform t)
		{
			td.position = t.position;
			td.rotation = t.rotation;
		}
	}

	/// <summary>
	/// Data container that holds reference and preferences related to the target position
	/// </summary>
	[System.Serializable]
	public class IKPTarget
	{
		public const string PROPERTY_NAME_TargetMode = nameof(targetMode);
		public const string PROPERTY_NAME_TargetPos = nameof(targetPos);
		public const string PROPERTY_NAME_RelativeTo = nameof(relativeTo);
		public const string PROPERTY_NAME_TargetRot = nameof(targetRot);
		public const string PROPERTY_NAME_TargetObj = nameof(targetObj);

		[SerializeField] private IKPTargetMode targetMode;

		[Header("Target Position")] [SerializeField] private Vector3 targetPos;

		[SerializeField] private Relative relativeTo = Relative.World;

		[HideInInspector] [SerializeField] private Quaternion targetRot;

		[Header("Target Transform")] [SerializeField] private Transform targetObj; //optional target transform

		public Transform currentTransformTargetIfAny => targetMode == IKPTargetMode.Transform ? targetObj : null;


		public IKPTarget()
		{
			targetMode = IKPTargetMode.Position;
			targetPos = Vector3.zero;
			relativeTo = Relative.World;
			targetRot = Quaternion.identity;
			targetObj = null;
		}

		public IKPTarget(Transform tr)
		{
			this.targetMode = IKPTargetMode.Transform;
			this.targetObj = tr;
		}

		public IKPTarget(Relative relativeTo, Vector3 pos, Quaternion rot)
		{
			this.targetMode = IKPTargetMode.Position;
			this.relativeTo = relativeTo;
			this.targetPos = pos;
			this.targetRot = rot;
			this.targetObj = null;
		}


		public void Copy(IKPTarget t)
		{
			this.targetMode = t.targetMode;
			this.relativeTo = t.relativeTo;
			this.targetPos = t.targetPos;
			this.targetRot = t.targetRot;
			this.targetObj = t.targetObj;
		}

		/// <summary>
		/// Sets a target object
		/// </summary>
		public void Set (Transform t)
		{
			this.targetObj = t;
			this.targetMode = IKPTargetMode.Transform;
		}

		/// <summary>
		/// Sets a target position and rotation
		/// </summary>
		public void Set (Vector3 position, Quaternion rotaiton, Relative relative)
		{
			this.targetPos = position;
			this.targetRot = rotaiton;
			this.relativeTo = relative;
			this.targetMode = IKPTargetMode.Position;
		}

		/// <summary>
		/// Returns target information. Requires origin for the case where the peroperties are in relative space.
		/// </summary>
		public TransformData Get(Transform origin)
		{
			if (targetMode == IKPTargetMode.Transform && targetObj != null)
				return new TransformData(targetObj);
			return new TransformData(GetPosition(origin), GetRotation());
		}

		/// <summary>
		/// Returns target information. Requires origin for the case where the peroperties are in relative space.
		/// </summary>
		public TransformData Get(Bone origin)
		{
			if (targetMode == IKPTargetMode.Transform && targetObj != null)
				return new TransformData(targetObj);
			return new TransformData(GetPosition(origin), GetRotation());
		}

		public Quaternion GetRotation()
		{
			if (targetMode == IKPTargetMode.Transform)
			{
				if (targetObj)
				{
					IKP_Target ikpTrg = targetObj.GetComponent<IKP_Target>();
					if (ikpTrg)
						return ikpTrg.GetRotation();
					else
						return targetObj.rotation;
				}
			}
			 
			return targetRot;
		}

		/// <summary>
		/// Returns target information. Requires origin for the case where the peroperties are in relative space.
		/// </summary>
		public Vector3 GetPosition(Transform origin)
		{
			if (targetMode == IKPTargetMode.Transform && targetObj != null)
				return targetObj.position;

			if (relativeTo == Relative.Object)
				if (origin)
					return origin.right * targetPos.x + origin.up * targetPos.y + origin.forward * targetPos.z;
			return targetPos;
		}

		/// <summary>
		/// Returns target information. Requires origin for the case where the peroperties are in relative space.
		/// </summary>
		public Vector3 GetPosition(Bone origin)
		{
			if (targetMode == IKPTargetMode.Transform && targetObj != null)
				return targetObj.position;

			if (relativeTo == Relative.Object)
				if (origin != null)
					return origin.right * targetPos.x + origin.up * targetPos.y + origin.forward * targetPos.z;
			return targetPos;
		}
	}

	[System.Serializable]
	public class IKPLocalSpace
	{
		public Vector3 forward;
		public Vector3 right;
		public Vector3 up;

		public IKPLocalSpace()
		{
			this.right = Vector3.right;
			this.up = Vector3.up;
			this.forward = Vector3.forward;
		}

		public IKPLocalSpace(Quaternion source)
		{
			this.right = source * Vector3.right;
			this.up = source * Vector3.up;
			this.forward = source * Vector3.forward;
		}

		public IKPLocalSpace(Vector3 x, Vector3 y, Vector3 z)
		{
			this.right = x;
			this.up = y;
			this.forward = z;
		}

		public IKPLocalSpace(IKPLocalSpace lsp)
		{
			this.right = lsp.right;
			this.up = lsp.up;
			this.forward = lsp.forward;
		}

		public Quaternion ToQuaternion()
		{
			return Quaternion.LookRotation(forward, up);
		}

		public void Normalize()
		{
			right = Vector3.Normalize(right);
			up = Vector3.Normalize(up);
			forward = Vector3.Normalize(forward);
		}
	}

	[System.Serializable]
	public class ProjectionPlane
	{
		public Vector3 vector1;
		public Vector3 vector2;

		public ProjectionPlane()
		{
			this.vector1 = Vector3.zero;
			this.vector2 = Vector3.zero;
		}

		public ProjectionPlane(Vector3 vec1, Vector3 vec2)
		{
			this.vector1 = vec1;
			this.vector2 = vec2;
		}
	}
}

namespace IKPn.Blend
{
	public enum ToggleAnimation
	{
		//animated module toggle
		Default,
		On,
		Off
	}

	public enum IKPNodeType
	{
		Default,
		Animation,
		EventMachine,
		EventCall
	}

	[System.Serializable]
	public class BlendNodeData
	{
		public Rect rect;
		public string name;
		public IKPNodeType nodeType = IKPNodeType.Default;
		public string layer = ""; //on which animation layer (module) is the node located

		public IKPBlendAnimation animation;
		public bool[] propertyMask; //ignore some properties
		public int standardOutput = -1; //(-1 = no output) used by the animation nodes
		public NodeStateContent[] states;
		public string stateSwitch = IKPBlendMachine.DEFAULT_STATE;

		//methods
		public bool GetPropertyMaskValue(int i)
		{
			if (propertyMask == null || propertyMask.Length <= i)
				return true; //do not mask the value
			return propertyMask[i];
		}
	}

	[System.Serializable]
	public class NodeStateContent
	{
		public string state;
		public int output = -1; //(-1 = no output)
		public Rect rect;

		public NodeStateContent()
		{
			state = IKPBlendMachine.DEFAULT_STATE;
			output = -1;
			rect = new Rect(0, 0, 1, 1);
		}

		public NodeStateContent(string st)
		{
			state = st;
			output = -1;
			rect = new Rect(0, 0, 1, 1);
		}
	}

	/// <summary>
	/// Reads the current state of the node layer
	/// </summary>
	public class NodePointer
	{
		public int? currentNode = null;
		public IKPNodeType nodeType = IKPNodeType.Default; //record the current node type for easier access

		public float
			animationProgress = 0f,
			animationSpeed = 0f,
			animationSize = 0f;

		public string layer = string.Empty;


		public bool IsIdle()
		{
			return animationProgress >= animationSize;
		}
	}

	public class BlendAnimationValue
	{
		public bool? toggle; //toggle the module
		public float?[] properties;
	}

	[System.Serializable]
	public class NodeLink
	{
		public int contentId;
		public int? outputNodeId; //also used to identify if and when a connection is being made
		public int? clickedNodeId; //null until it has been clicked
	}
}
