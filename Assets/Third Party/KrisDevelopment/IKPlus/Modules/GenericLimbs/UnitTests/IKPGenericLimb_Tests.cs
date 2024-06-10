#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SETUtil.Types;
using UnityEngine;
using UnityEngine.TestTools;

namespace IKPn.Tests
{
    public class IKPTests_GenericLimb
    {
        [Test]
        public void GenLimbIKPTargetTransformGettersConsistency_Test()
        {
            var _transform = new GameObject().transform;
            _transform.position = Vector3.one * 69;
            _transform.rotation = Quaternion.LookRotation(Vector3.up + Vector3.forward + Vector3.right);
            var _ikp = new GameObject().AddComponent<IKP>();
            _ikp.ToggleModule(ModuleSignatures.GENERIC_LIMBS, true);
            var _genericLimb = (IKPModule_GenericLimb)_ikp.GetModule(ModuleSignatures.GENERIC_LIMBS);
            var _allBones = new Transform[] {
                    new GameObject().transform,
                    new GameObject().transform,
                    new GameObject().transform,
                };
            _genericLimb.AutoSetup(new BodySetupContext()
            {
                allBones = _allBones,
            }, new System.Text.StringBuilder());
            _ikp.SetGenericLimbTarget(_transform);
            var _gt = _genericLimb.GetCurrentTargetTransformData();
            Debug.Assert(_genericLimb.Validate(new List<ValidationResult>()), "Generic limb initialization procedure is incomplete.");
            Debug.Assert((new TransformData(_transform).position - _gt.position).sqrMagnitude < 0.01, "Inconsistencies in IKP Target transform data getter - position");
            Debug.Assert((new TransformData(_transform).rotation.eulerAngles - _gt.rotation.eulerAngles).sqrMagnitude < 0.01, "Inconsistencies in IKP Target transform data getter - rotation");
            SETUtil.SceneUtil.SmartDestroy(_transform.gameObject);
            foreach(var t in _allBones)
            {
                SETUtil.SceneUtil.SmartDestroy(t.gameObject);
            }
        }
    }
}
#endif