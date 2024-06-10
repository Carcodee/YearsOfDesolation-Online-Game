using UnityEngine;

#if UNITY_EDITOR
#endif

namespace IKPn
{
    //extensions
    public static class WeaponModuleExtend
	{
		///<summary> Assigns a weapon transform to the weapon modules </summary>
		public static void SetWeapon(this IKP ikp, Transform t)
		{
			if (ikp.HasModule(ModuleSignatures.WEAPON))
				(ikp.GetModule(ModuleSignatures.WEAPON) as IKPModule_Weapon).SetWeapon(t);

			if (ikp.HasModule(ModuleSignatures.WEAPON_COLLISION))
				(ikp.GetModule(ModuleSignatures.WEAPON_COLLISION) as IKPModule_WeaponCollision).SetWeapon(t);
		}

		public static void SetWeaponMode(this IKP ikp, IKPAimTargetMode atm)
		{
			if (!ikp.HasModule(ModuleSignatures.WEAPON))
				return;
			IKPModule_Weapon _mw = ikp.GetModule(ModuleSignatures.WEAPON) as IKPModule_Weapon;
			_mw.SetAimTargetMode(atm);
		}

		public static void SetWeaponMode(this IKP ikp, IKPAimTargetMode atm, Side side)
		{
			ikp.SetWeaponMode(atm);
			if (!ikp.HasModule(ModuleSignatures.WEAPON))
			{
				return;
			}

			var _mw = ikp.GetModule(ModuleSignatures.WEAPON) as IKPModule_Weapon;
			_mw.SetHandSide(side);
		}

		public static void SetWeaponTarget(this IKP ikp, Vector3 position, Relative relativeMode = Relative.World)
		{
			if (ikp.HasModule(ModuleSignatures.WEAPON))
			{
				IKPModule_Weapon _weaponModule = ikp.GetModule(ModuleSignatures.WEAPON) as IKPModule_Weapon;
				_weaponModule.SetTarget(position, relativeMode);
			}
		}

		public static void SetWeaponTarget(this IKP ikp, Transform transform)
		{
			if (ikp.HasModule(ModuleSignatures.WEAPON))
			{
				IKPModule_Weapon _weaponModule = ikp.GetModule(ModuleSignatures.WEAPON) as IKPModule_Weapon;
				_weaponModule.SetTarget(transform);
			}
		}

		public static void SetWeaponTarget(this IKP ikp, IKPTarget ikpTarget)
		{
			if (ikp.HasModule(ModuleSignatures.WEAPON))
			{
				IKPModule_Weapon _weaponModule = ikp.GetModule(ModuleSignatures.WEAPON) as IKPModule_Weapon;
				_weaponModule.SetTarget(ikpTarget);
			}
		}
	}
}
