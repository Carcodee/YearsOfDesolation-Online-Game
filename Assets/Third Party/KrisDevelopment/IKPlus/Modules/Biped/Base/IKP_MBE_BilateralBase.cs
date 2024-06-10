using UnityEngine;
using SETUtil.Types;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
        //Bilateral symmetry class 
    [System.Serializable]
    internal class BilateralBase
    {
        [SerializeField]
        internal bool
            hasLeft = true, //indicate if setup should look for left/right limb
            hasRight = true;

        [SerializeField]
        private IKPTarget
            leftIKPTarget = new IKPTarget(), //user-defined IKP target settings
            rightIKPTarget = new IKPTarget(); //user-defined IKP target settings

        public const string PROPERTY_NAME_LEFT_IKP_TARGET = nameof(leftIKPTarget);
        public const string PROPERTY_NAME_RIGHT_IKP_TARGET = nameof(rightIKPTarget);


        internal void SetTarget(Side side, Vector3 pos) { SetTarget(side, pos, null); }
        internal void SetTarget(Side side, TransformData trdt) { SetTarget(side, trdt.position, trdt.rotation); }
        internal void SetTarget(Side side, Vector3 pos, Quaternion? rot, Relative relative = Relative.World)
        {
            IKPTarget _ikpTrg = GetTarget(side);
            _ikpTrg.Set(pos, rot ?? Quaternion.identity, relative);
        }

        internal void SetTarget(Side side, Transform tr)
        {
            SetTarget(side, new IKPTarget(tr));
        }

        internal void SetTarget(Side side, IKPTarget tgt)
        {
            IKPTarget _ikpTrg = GetTarget(side);
            _ikpTrg.Copy(tgt);
        }

        internal IKPTarget GetTarget(Side side)
        {
            switch (side)
            {
                case Side.Left:
                    return leftIKPTarget;

                case Side.Right:
                    return rightIKPTarget;
            }

            return null;
        }
    }
}
