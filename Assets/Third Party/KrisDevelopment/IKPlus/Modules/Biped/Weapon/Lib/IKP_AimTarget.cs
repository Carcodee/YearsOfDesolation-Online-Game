#if UNITY_EDITOR
#endif

namespace IKPn
{
    [System.Serializable]
	public class IKPAimTarget : IKPTarget
	{
		public IKPAimTargetMode mode;

		public IKPAimTarget() : base()
		{
			mode = IKPAimTargetMode.TargetInheritance;
		}

		public IKPAimTarget(IKPTarget t) : base()
		{
			base.Copy(t);
			mode = IKPAimTargetMode.NewTarget;
		}

		public IKPAimTarget(IKPTarget t, IKPAimTargetMode tgtMode) : base()
		{
			base.Copy(t);
			mode = tgtMode;
		}

		public void Copy(IKPAimTarget aimTarget)
		{
			base.Copy((IKPTarget)aimTarget);
			mode = aimTarget.mode;
		}
	}
}
