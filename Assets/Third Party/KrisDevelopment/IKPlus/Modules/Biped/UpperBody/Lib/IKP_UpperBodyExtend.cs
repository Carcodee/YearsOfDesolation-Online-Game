using UnityEngine;

namespace IKPn
{
	public static class UpperBodyExtend
	{
		public static void SetChestTargetMode(this IKP ikp, ChestTargetMode ct)
		{
			if (!ikp.HasModule(ModuleSignatures.UPPER_HUMANOID))
				return;
			IKPModule_UpperBody _upperBody = ikp.GetModule(ModuleSignatures.UPPER_HUMANOID) as IKPModule_UpperBody;
			_upperBody.SetChestTargetMode(ct);
		}

		public static void SetHandTarget(this IKP ikp, Side side, IKPTarget ikpTarget)
		{
			if (ikp.HasModule(ModuleSignatures.UPPER_HUMANOID))
			{
				IKPModule_UpperBody _upperModule = ikp.GetModule(ModuleSignatures.UPPER_HUMANOID) as IKPModule_UpperBody;
				_upperModule.bilatBase.SetTarget(side, ikpTarget);
			}
		}

		public static void SetHandTarget(this IKP ikp, Side side, Transform transform)
		{
			if (ikp.HasModule(ModuleSignatures.UPPER_HUMANOID))
			{
				IKPModule_UpperBody _upperModule = ikp.GetModule(ModuleSignatures.UPPER_HUMANOID) as IKPModule_UpperBody;
				_upperModule.bilatBase.SetTarget(side, transform);
			}
		}

		public static void SetHandTarget(this IKP ikp, Side side, Vector3 position, Quaternion? rotation)
		{
			if (ikp.HasModule(ModuleSignatures.UPPER_HUMANOID))
			{
				IKPModule_UpperBody _upperModule = ikp.GetModule(ModuleSignatures.UPPER_HUMANOID) as IKPModule_UpperBody;
				_upperModule.bilatBase.SetTarget(side, position, rotation);
			}
		}
	}
}
