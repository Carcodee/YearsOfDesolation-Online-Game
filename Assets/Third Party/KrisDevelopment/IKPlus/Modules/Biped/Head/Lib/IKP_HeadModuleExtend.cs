using UnityEngine;

#if UNITY_EDITOR
#endif

namespace IKPn
{
    // ---------------------------
    public static class HeadModuleExtend
	{
		public static void SetLookTarget(this IKP ikp, Vector3 position, Relative relativeMode = Relative.World)
		{
			if (ikp.HasModule(ModuleSignatures.HEAD))
			{
				IKPModule_Head _headModule = ikp.GetModule(ModuleSignatures.HEAD) as IKPModule_Head;
				_headModule.SetTarget(position, relativeMode);
			}
		}

		public static void SetLookTarget(this IKP ikp, Transform tr)
		{
			if (ikp.HasModule(ModuleSignatures.HEAD))
			{
				IKPModule_Head _headModule = ikp.GetModule(ModuleSignatures.HEAD) as IKPModule_Head;
				_headModule.SetTarget(tr);
			}
		}

		public static void SetLookTarget(this IKP ikp, IKPTarget ikpTarget)
		{
			if (ikp.HasModule(ModuleSignatures.HEAD))
			{
				IKPModule_Head _headModule = ikp.GetModule(ModuleSignatures.HEAD) as IKPModule_Head;
				_headModule.SetTarget(ikpTarget);
			}
		}
	}
}
