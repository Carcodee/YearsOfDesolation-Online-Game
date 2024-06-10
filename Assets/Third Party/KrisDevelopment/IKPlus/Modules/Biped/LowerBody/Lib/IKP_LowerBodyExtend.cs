using UnityEngine;

#if UNITY_EDITOR
#endif


namespace IKPn
{
	/// <summary>
	/// Extend IKP to for Lower Body module
	/// </summary>
    public static class LowerBodyExtend
	{
		public static void SetLegTarget(this IKP ikp, Side side, Vector3 position, Quaternion? rotation)
		{
			if (ikp.HasModule("Lower"))
			{
				IKPModule_LowerBody _lowerModule = ikp.GetModule("Lower") as IKPModule_LowerBody;
				_lowerModule.bilatBase.SetTarget(side, position, rotation);
			}
		}

		public static void SetLegTarget(this IKP ikp, Side side, Transform tr)
		{
			if (ikp.HasModule("Lower"))
			{
				IKPModule_LowerBody _lowerModule = ikp.GetModule("Lower") as IKPModule_LowerBody;
				_lowerModule.bilatBase.SetTarget(side, tr);
			}
		}

		public static void SetLegTarget(this IKP ikp, Side side, IKPTarget ikpTarget)
		{
			if (ikp.HasModule("Lower"))
			{
				IKPModule_LowerBody _lowerModule = ikp.GetModule("Lower") as IKPModule_LowerBody;
				_lowerModule.bilatBase.SetTarget(side, ikpTarget);
			}
		}
	}

}
