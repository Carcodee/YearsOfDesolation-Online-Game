using System;
using UnityEngine;

namespace IKPn
{
    public static class GenericLimbExtend
	{
		public static void SetGenericLimbTarget(this IKP ikp, Vector3 targetPosition)
		{
			Do(ikp, (m) => m.SetTarget(targetPosition));
		}
        
		public static void SetGenericLimbTarget(this IKP ikp, Vector3 targetPosition, Quaternion targetRotation)
		{
			Do(ikp, (m) => m.SetTarget(targetPosition, targetRotation));
		}

		public static void SetGenericLimbTarget(this IKP ikp, Transform transform)
		{
			Do(ikp, (m) => m.SetTarget(transform));
		}

		public static void SetGenericLimbTarget(this IKP ikp, IKPTarget ikpTarget)
		{
			Do(ikp, (m) => m.SetTarget(ikpTarget));
		}

		/// <summary>
		/// Sets the amount of IK solver (FABRIK) iterations.
		/// </summary>
		public static void SetGenericLimbIterations(this IKP ikp, int amount)
		{
			Do(ikp, (m) => m.iterations = amount);
		}

		/// <summary>
		/// Sets the amount of IK solver (FABRIK) iterations.
		/// </summary>
		public static void SetGenericLimbChaikinIterations(this IKP ikp, int amount)
		{
			Do(ikp, (m) => m.chaikinIterations = amount);
		}

		/// <summary>
		/// Toggle stretching
		/// </summary>
		public static void SetGenericLimbStretching(this IKP ikp, bool enabled)
		{
			Do(ikp, (m) => m.SetStretchable(enabled));
		}

		/// <summary>
		/// Enables stretching and sets the value
		/// </summary>
		public static void SetGenericLimbStretching(this IKP ikp, float value)
		{
			Do(ikp, (m) => { m.SetStretchable(true); m.SetProperty(IKPModule_GenericLimb.p_stretch, value); });
		}

		// ---- Util ----
		private static void Do(IKP ikp, Action<IKPModule_GenericLimb> action)
        {
			if (ikp.HasModule(ModuleSignatures.GENERIC_LIMBS))
			{
				IKPModule_GenericLimb _module = ikp.GetModule(ModuleSignatures.GENERIC_LIMBS) as IKPModule_GenericLimb;
				action(_module);
			}
		}
	}
}
