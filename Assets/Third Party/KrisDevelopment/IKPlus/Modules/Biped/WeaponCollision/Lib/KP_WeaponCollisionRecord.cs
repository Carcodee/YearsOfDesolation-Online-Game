using UnityEngine;

using TransformData = SETUtil.Types.TransformData;

#if UNITY_EDITOR
#endif


namespace IKPn
{
    // --------------------

    /// <summary>
    /// Facilitates the per-transform information needed by the WeaponCollision module
    /// </summary>
    internal class WeaponCollisionRecord
	{
		public readonly Transform transform;
		public readonly IKP_Weapon ikpWeapon = null;

		/// <summary>
		/// Tracks frame-to-frame weather the collision check has been invoked by the collision module 
		/// or by some external script
		/// </summary>
		public bool invokedThisFrame = false;
		public bool isNeutralState = true;
		public Vector3 chosenPushDirection = Vector3.up;
		public TransformData dampedState;
		public TransformData modifiedState;
		public TransformData persistentState;


		public WeaponCollisionRecord(Transform weaponTransform)
		{
			this.transform = weaponTransform;

			if (weaponTransform != null)
			{
				ikpWeapon = transform.GetComponent<IKP_Weapon>();
				persistentState = new TransformData(weaponTransform.localPosition, weaponTransform.localRotation);
				modifiedState = persistentState;
				dampedState = persistentState;
			}
		}
	}
}
