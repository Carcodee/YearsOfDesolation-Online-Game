using SETUtil.Types;
using System;
using UnityEngine;

namespace IKPn
{
    public partial class IKPModule_LowerBody
    {
        public enum GrounderType
        {
            None = 0,
            StandardGrounder = 1,
            SimpleGrounder = 2,
        }

        internal abstract class Grounder
        {
            protected IKPModule_LowerBody lowerBody;

            public Grounder(IKPModule_LowerBody lowerBody)
            {
                this.lowerBody = lowerBody;
            }

            public abstract void DoBeforeLegs(TargetProcessResult leftTarget, TargetProcessResult rightTarget);

            public abstract void DoAfterLegs();

            internal static bool IsUsingSelected(Grounder grounder, GrounderType grounderBehavior)
            {
                switch (grounderBehavior)
                {
                    case GrounderType.StandardGrounder:
                        return grounder is StandardGrounder;
                    case GrounderType.SimpleGrounder:
                        return grounder is SimpleGrounder;
                    case GrounderType.None:
                        return grounder == null;
                }

                throw new Exception($"Attempted to check for an undefined grounder type {grounderBehavior}");
            }

            /// <summary>
            /// This method predicts where the feet will be after the lower body IK runs.
            /// </summary>
            protected void GetFootPredictions(TargetProcessResult leftTarget, TargetProcessResult rightTarget, out Vector3 _leftPositionFootPredictTarget, out Vector3 _rightPositionFootPredictTarget)
            {
                Func<Vector3, Vector3, float, float, Vector3> _funcGetPredictedFootPos = (target, thigh, knee, foot) =>
                {
                    return thigh + (target - thigh).normalized * Mathf.Clamp(Vector3.Distance(thigh, target), IKPUtils.LIMB_DELTA_STRETCH_LIMIT, knee + foot - IKPUtils.LIMB_DELTA_STRETCH_LIMIT);
                };

                var _bodySetup = lowerBody.bodySetup;

                _leftPositionFootPredictTarget = _funcGetPredictedFootPos(leftTarget.targetTrDt.position, _bodySetup.leftThigh.position, _bodySetup.leftKneeDistance, _bodySetup.leftFootDistance);
                _rightPositionFootPredictTarget = _funcGetPredictedFootPos(rightTarget.targetTrDt.position, _bodySetup.rightThigh.position, _bodySetup.rightKneeDistance, _bodySetup.rightFootDistance);
            }

        }

        internal class StandardGrounder : Grounder
        {
            private Vector3 smoothDropValue;

            public StandardGrounder(IKPModule_LowerBody lowerBody) : base(lowerBody)
            {
            }

            public override void DoBeforeLegs(TargetProcessResult leftTarget, TargetProcessResult rightTarget)
            {
                var _dropVector = lowerBody.dropVector;
                var _bilatBase = lowerBody.bilatBase;

                //apply drop offsets the leg target by the drop amount in order to reach the surface
                float
                    _reach = lowerBody.GetProperty(p_grounderReach);
                Vector3 _dropLRG = Vector3.zero;

                // predict feet
                Vector3 _leftPositionFootPredictTarget;
                Vector3 _rightPositionFootPredictTarget;
                GetFootPredictions(leftTarget, rightTarget, out _leftPositionFootPredictTarget, out _rightPositionFootPredictTarget);

                RaycastHit _hitInfo;
                float GetDrop(Vector3 thighPos, Vector3 targetPos, Vector3 footPrediction)
                {
                    var _footPredictionCorrected = footPrediction + _dropVector * lowerBody.feetOffset;
                    var _footToTarget = targetPos - _footPredictionCorrected;
                    if (_footToTarget == Vector3.zero)
                    {
                        return 0;
                    }

                    var _projectedTargetPos = Vector3.Project(_footToTarget, _dropVector);
                    float _dropClamp = Mathf.Min(_projectedTargetPos.magnitude, _reach);// * lowerBody.GetProperty(p_leftLegWeight);
                    var _rayPt = thighPos;
                    var _rayVec = targetPos - _rayPt;

                    if (lowerBody.SphereCast(out _hitInfo, _rayPt, _rayVec, Mathf.Infinity))
                    {
                        //Debug.DrawLine(footPrediction, _hitInfo.point, Color.cyan, 0.1f, false);
                        var _dropDistance = Vector3.Distance(_footPredictionCorrected, _hitInfo.point);
                        return Mathf.Clamp(_dropDistance, 0f, _dropClamp);
                    }
                    else
                    {
                        return _dropClamp;
                    }
                }

                // raycast
                if (_bilatBase.hasLeft)
                {
                    _dropLRG.x = GetDrop(lowerBody.bodySetup.leftThigh.position, leftTarget.targetTrDt.position, _leftPositionFootPredictTarget);
                }

                // raycast
                if (_bilatBase.hasRight)
                {
                    _dropLRG.y = GetDrop(lowerBody.bodySetup.rightThigh.position, rightTarget.targetTrDt.position, _rightPositionFootPredictTarget);
                }


                //apply feet offset and get the max drop for z
                _dropLRG.z = Mathf.Max(_dropLRG.x, _dropLRG.y);

                var _fallSpeed = lowerBody.GetProperty(p_grounderFallSpeed);
                var _reactSpeed = lowerBody.GetProperty(p_grounderReactSpeed);

                if (_reactSpeed * _fallSpeed <= 0) _reactSpeed = _fallSpeed = float.MaxValue;
                var _legClips = rightTarget.legClips || leftTarget.legClips;

                smoothDropValue = Vector3.Lerp(smoothDropValue, _dropLRG, Time.deltaTime * (_legClips ? _reactSpeed : _fallSpeed));
                smoothDropValue *= lowerBody.GetProperty(p_grounderWeight);

                // offset leg targets
                Vector3 _leftDropFinal =  -(smoothDropValue.y - smoothDropValue.x) * _dropVector;
                Vector3 _rightDropFinal = -(smoothDropValue.x - smoothDropValue.y) * _dropVector;
                leftTarget.targetTrDt.position += _leftDropFinal;
                rightTarget.targetTrDt.position += _rightDropFinal;
                leftTarget.unweightedTargetPosition += _leftDropFinal;
                rightTarget.unweightedTargetPosition += _rightDropFinal;
                // for proper feet IK
                leftTarget.dropFoot = smoothDropValue.x * _dropVector;
                rightTarget.dropFoot = smoothDropValue.y * _dropVector;
                leftTarget.predictionRecordFoot = _leftPositionFootPredictTarget;
                rightTarget.predictionRecordFoot = _rightPositionFootPredictTarget;

            }

            public override void DoAfterLegs()
            {
                lowerBody.bodySetup.hips.position = lowerBody.bodySetup.hips.position + lowerBody.dropVector * (smoothDropValue.z - lowerBody.feetOffset);
            }
        }

        internal class SimpleGrounder : Grounder
        {
            private float grounderDropSmooth = 0f;

            public SimpleGrounder(IKPModule_LowerBody lowerBody) : base(lowerBody)
            {
            }

            public override void DoBeforeLegs(TargetProcessResult leftTarget, TargetProcessResult rightTarget)
            {
                Vector3 _leftPositionFootPredictTarget, _rightPositionFootPredictTarget;
                GetFootPredictions(leftTarget, rightTarget, out _leftPositionFootPredictTarget, out _rightPositionFootPredictTarget);

                float _reach = lowerBody.GetProperty(p_grounderReach);
                float _grWeight = lowerBody.GetProperty(p_grounderWeight);
                var _dropVector = lowerBody.dropVector;
                var _feetOffset = lowerBody.feetOffset;

                Func<Vector3, Vector3, float, Vector3> _funcGetDropTarget = (toe, target, weight) =>
                {
                    RaycastHit _hit;
                    var _hasHit = lowerBody.SphereCast(out _hit, toe - _dropVector, _dropVector, Mathf.Infinity);
                    return Vector3.Lerp(_hasHit ? _hit.point : _dropVector * _reach, target, weight);
                };

                //project targets
                var _lTgtProjected
                    = Vector3.Project(_funcGetDropTarget(_leftPositionFootPredictTarget
                        , leftTarget.unweightedTargetPosition, lowerBody.GetProperty(p_leftLegWeight)) - _leftPositionFootPredictTarget, _dropVector);
                var _lm = _lTgtProjected.magnitude;
                var _rTgtProjected
                    = Vector3.Project(_funcGetDropTarget(_rightPositionFootPredictTarget
                        , rightTarget.unweightedTargetPosition, lowerBody.GetProperty(p_rightLegWeight)) - _rightPositionFootPredictTarget, _dropVector);
                var _rm = _rTgtProjected.magnitude;

                // get longest drop
                var _drop = Mathf.Min(_reach, _lm, _rm);

                var _hipsDrop = (_drop - _feetOffset) * _grWeight;

                // dampen
                var _fallSpeed = lowerBody.GetProperty(p_grounderFallSpeed);
                var _reactSpeed = lowerBody.GetProperty(p_grounderReactSpeed);

                if (_reactSpeed * _fallSpeed <= 0) _reactSpeed = _fallSpeed = float.MaxValue;

                var _legClips = rightTarget.legClips || leftTarget.legClips;
                grounderDropSmooth
                    = Mathf.Lerp(grounderDropSmooth, _hipsDrop, Time.deltaTime * (_legClips ? _reactSpeed : _fallSpeed));

                // for proper feet IK
                leftTarget.dropFoot = grounderDropSmooth * _dropVector;
                rightTarget.dropFoot = leftTarget.dropFoot;
                leftTarget.predictionRecordFoot = _leftPositionFootPredictTarget;
                rightTarget.predictionRecordFoot = _rightPositionFootPredictTarget;
            }

            public override void DoAfterLegs()
            {
                lowerBody.bodySetup.hips.position = lowerBody.bodySetup.hips.position + lowerBody.dropVector * grounderDropSmooth;
            }
        }

    }
}
