using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
	[AddComponentMenu(IKPUtils.IKP_COMPONENT_MENU + "/Target")]
	public class IKP_Target : MonoBehaviour
	{
		[HideInInspector] [SerializeField] private Quaternion initialRot = Quaternion.identity;
		[HideInInspector] [SerializeField] private Quaternion offset = Quaternion.identity;
		[HideInInspector] [SerializeField] private bool editMode = false;
		internal Quaternion currentOffset => offset;
		internal bool isInEditMode => editMode;

		private static readonly Rect rectGizmoSize = new Rect(-16, -32, 32, 32);

		SETUtil.SceneUI.GUIImage
			m_gizmoTargetImage,
			m_gizmoTargetImageEdit;

		SETUtil.SceneUI.GUIImage gizmoTargetImage {
			get {
				return m_gizmoTargetImage == null ? 
					(m_gizmoTargetImage = new SETUtil.SceneUI.GUIImage(rectGizmoSize,
						SETUtil.ResourceLoader.EditorTextureResource.Get("ikp_gizmo_target"))) 
						: m_gizmoTargetImage;
			}

		}

		SETUtil.SceneUI.GUIImage gizmoTargetImageEdit {
			get {
				return m_gizmoTargetImageEdit == null ? 
					(m_gizmoTargetImageEdit = new SETUtil.SceneUI.GUIImage(rectGizmoSize,
						SETUtil.ResourceLoader.EditorTextureResource.Get("ikp_gizmo_target_edit"))) 
						: m_gizmoTargetImageEdit;
			}
		}


		void OnDrawGizmos()
		{
			if (editMode)
			{
				gizmoTargetImageEdit.position = transform.position;
				SETUtil.EditorUtil.DrawSceneElement(gizmoTargetImageEdit);
			}
		}

		void OnDrawGizmosSelected()
		{
			if (editMode)
			{
				gizmoTargetImageEdit.position = transform.position;
				SETUtil.EditorUtil.DrawSceneElement(gizmoTargetImageEdit);
			}
			else
			{
				gizmoTargetImage.position = transform.position;
				SETUtil.EditorUtil.DrawSceneElement(gizmoTargetImage);
			}
		}

		internal void StartEdit()
		{
			initialRot = transform.rotation;
			transform.rotation *= offset;
			editMode = (true);
		}

		internal void EndEdit()
		{
			EndEdit(true);
		}

		internal void EndEdit(bool confirmation)
		{
			if (confirmation)
				SaveOffset();
			editMode = (false);
			transform.rotation = initialRot;
		}

		internal void SaveOffset()
		{
			Quaternion offset = IKPUtils.GetRotationOffset(initialRot, transform.rotation);
			SetRotationOffset(offset);
			PlayerPrefs.SetFloat("ikpt_x", offset.x);
			PlayerPrefs.SetFloat("ikpt_y", offset.y);
			PlayerPrefs.SetFloat("ikpt_z", offset.z);
			PlayerPrefs.SetFloat("ikpt_w", offset.w);
		}

		public Vector3 GetPosition()
		{
			return transform.position;
		}

		public Quaternion GetRotation()
		{
			if (offset == IKPUtils.NULL_QUATERNION)
			{
				SetRotationOffset(Quaternion.identity);
			}

			if (!editMode)
			{
				Quaternion o = transform.rotation;
				o *= offset;
				return o;
			}
			else
			{
				return transform.rotation;
			}
		}

		internal void ClearRotationOffset()
		{
			SetRotationOffset(Quaternion.identity);
		}

		internal void LoadRotationOffset()
		{
			Quaternion o = LoadSavedRotationOffset();
			SetRotationOffset(o);
		}

		private Quaternion LoadSavedRotationOffset()
		{
			Quaternion q = new Quaternion(PlayerPrefs.GetFloat("ikpt_x"), PlayerPrefs.GetFloat("ikpt_y"), PlayerPrefs.GetFloat("ikpt_z"), PlayerPrefs.GetFloat("ikpt_w"));
			if (q != IKPUtils.NULL_QUATERNION)
				return q;

			return Quaternion.identity;
		}


		private void SetRotationOffset(Quaternion q)
		{
#if UNITY_EDITOR
			SerializedObject s = new SerializedObject(this);
			SerializedProperty p_o = s.FindProperty(nameof(offset));
			p_o.quaternionValue = q;
			s.ApplyModifiedProperties();
#endif
		}
	}
}
