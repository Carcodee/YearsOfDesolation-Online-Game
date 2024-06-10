
#if UNITY_EDITOR

using UnityEditor;

namespace IKPn
{
	/// <summary>
	/// Draws handles during hand correction
	/// </summary>
	[CustomEditor(typeof(IKPModule_UpperBody))]
	public class UpperBodyModuleHandles : ModuleEditor
	{
		IKPModule_UpperBody module;

		void OnSceneGUI()
		{
			module = (IKPModule_UpperBody)target;

			if (module.isCurrentlyCorrectingHands)
			{
				//L
				if (module.Has(Side.Left) && module.correctingHands == 0)
					module.DrawHandCorrection(Side.Left);

				//R
				if (module.Has(Side.Right) && module.correctingHands == 1)
					module.DrawHandCorrection(Side.Right);
			}
		}
	}
}
#endif
