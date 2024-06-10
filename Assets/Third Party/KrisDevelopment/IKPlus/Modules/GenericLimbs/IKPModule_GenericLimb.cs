
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using SETUtil.Common.Extend;
using SETUtil.Types;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Profiling;
#endif


namespace IKPn
{
    [IKPModule(ModuleSignatures.GENERIC_LIMBS, iconPath = "IKP/generic_limb_icon", inspectorOrder = 21, updateOrder = 25)]
    [AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Generic Limbs")]
    [DisallowMultipleComponent]
    public class IKPModule_GenericLimb : ModuleBase_StandardBoneLogicSSP
    {
        public enum OrientationMode
        {
            Parametric = 0,
            Positional = 1,
        }

        public enum MatchRotation
        {
            None = 0,
            TipBone = 1 << 0,
            LastTwoBones = 1 << 1 | TipBone,
        }

        public enum Property
        {
            Weight = 0,
            Speed = 1,
            Orientation = 2,
            Stretch = 3,
        }

        public enum LegWorldUp
        {
            FromOrigin = 0,
            World = 1,
        }

        public enum SolverEnhance
        {
            None = 0,
            JustifyTargetByRootRotation = 1,
            JustifyTarget_ByHeroSolve = 2,
            JustifyTarget_ByHeroSolveWithSInversion = 3,
        }

        public const int
            p_weight = (int)Property.Weight,
            p_speed = (int)Property.Speed,
            p_orientation = (int)Property.Orientation,
            p_stretch = (int)Property.Stretch;

        // ----

        public float weight => GetProperty(p_weight);
        public float speed => GetProperty(p_speed);
        public float orientation => GetProperty(p_orientation);
        public float stretch => GetProperty(p_stretch);

        // ----

        private const float MIN_STRETCH = 0.01f;

        /// <summary> 
        /// Close enough to do early terminate of the FABRIK iteration cycle 
        /// </summary>
        private const float FABRIK_TERMINATION_DELTA = 0.0009f;

        /// <summary> 
        /// Relative orientation (local spaces) originally use a method that relies on angle-based quadrant checks to calculate the 'up' vector. 
        /// The new method is much faster but less accurate and can produce vastly different behavior in some cases, 
        /// however it's fine to use it in the context of generic limbs.
        /// </summary>
        private const bool FAST_LOCAL_SPACE = true;

        // ----

        [Range(1, 80), SerializeField] internal int iterations = 3;
        [Range(0, 3), SerializeField] internal int chaikinIterations = 2;
        [SerializeField] public bool useOrientation = true;

        [Tooltip("If true, local rotaions of bones will be zeroed before Bone Rotation Offsets are calculated.")]
        [SerializeField] public bool resetToUniformRotatonsOnInit = false;
        [Tooltip("If true, the tip of the limb will be oriented vertically, as if it attempts to step onto a surface.")]
        [SerializeField] public bool isLeg = false;
        [SerializeField] public LegWorldUp legWorldUp = LegWorldUp.World;

        [SerializeField] public bool stretchable = false;
        [SerializeField] public bool stretchIndependentOfWeight = false;
        [SerializeField] public bool stretchAlwaysReach = false;
        [SerializeField] public bool is2D = false;
        [SerializeField] public MatchRotation matchTargetRotation = MatchRotation.None;
        /// <summary>
        /// post FABRIK operation to rotate the entire limb from the base slightly to reach even closer to the target.
        /// </summary>
        [Tooltip("What additional method to use in order to improve accuracy, allowing you to use fewer iterations")]
        [SerializeField] public SolverEnhance solverEnhance = SolverEnhance.JustifyTargetByRootRotation;

        [Tooltip("What bone index to prefer when doing the mid joint on the HERO solver." +
         "-1 would be the closest to the middle as possible, anything else will aim for that bone index if available.")]
        [SerializeField] public int heroSolvePreferJointIndex = -1; 

        [SerializeField] public OrientationMode orientationMode = OrientationMode.Parametric;
        [SerializeField] private IKPTarget target = new IKPTarget();
        [SerializeField] private List<Bone> bones = new List<Bone>();
        [SerializeField] private List<Transform> orientationGuides = new List<Transform>();

        [Tooltip("Joint max bend limits in degrees")]
        [Range(180, 1), SerializeField] public float limitAngles = 180f;
        public bool useAngleLimits => limitAngles < 179.9f;

        ///<summary> Origin is provided to establish a local coordinate space. </summary>
        [NonSerialized] private Bone originBone;
        [NonSerialized] private List<float> lengths = new List<float>();
        [NonSerialized] private List<Vector3> points = new List<Vector3>();
        [NonSerialized] private List<Quaternion> boneRotationsTargets = new List<Quaternion>();
        [NonSerialized] private float maxLimbDistance = 0;

        private float[] boneLengthsCached = new float[0];

        // Cached
        int indexLastPoint = -1;


#if UNITY_EDITOR
        [NonSerialized] private Dictionary<string, SerializedProperty> serializedProperties = new Dictionary<string, SerializedProperty>();
        
        SerializedProperty sp_limbJointsArray => serializedProperties[nameof(bones)];

#endif

        /// <summary>
        /// Record bones state properties, etc.
        /// </summary>
        internal override void Init(Transform origin)
        {
            originBone = new Bone(origin);
            originBone.Setup(null);

            // initialize bone rotations
            if (BonesCount() > 1)
            {
                if (resetToUniformRotatonsOnInit)
                {
                    foreach (var _bone in bones) {
                        _bone.transform.localRotation = Quaternion.identity;
                    }
                }

                var _reference = new IKPLocalSpace(originBone.rotation);

                for (int i = 0; i < BonesCount(); i++)
                {
                    var _bone = bones[i];
                    if (i < BonesCount() - 1)
                    {
                        var _next = bones[i + 1];

                        var _limbLS = IKPUtils.CalculateLimbLocalSpace(_reference, _next.position - _bone.position, FAST_LOCAL_SPACE);
                        _bone.Setup(_limbLS);
                    }
                    else
                    {
                        // last bone copies the LS of the previous
                        var _prev = bones[i - 1];
                        var _limbLS = IKPUtils.CalculateLimbLocalSpace(_reference, _bone.position - _prev.position, FAST_LOCAL_SPACE);
                        _bone.Setup(_limbLS);
                    }
                }
            }

            maxLimbDistance = 0;
            points.Clear();
            lengths.Clear();
            var _bonesCount = BonesCount();

            for (int i = 0; i < _bonesCount; i++)
            {
                var _bone = GetBone(i);
                var _boneLength = i == _bonesCount - 1 ? 0 : Vector3.Distance(_bone.position, GetBone(i + 1).position);
                lengths.Add(_boneLength);
                maxLimbDistance += _boneLength;
                points.Add(_bone.position);
            }

            indexLastPoint = points.Count - 1;

            boneRotationsTargets = bones.Select(a => a.rotation).ToList();
        }

        public override bool Validate(List<ValidationResult> outValidationResult)
        {
            const int MIN_BONES_REQUIREMENT = 2;

            if (BonesCount() < MIN_BONES_REQUIREMENT)
            {
                outValidationResult.Add(new ValidationResult()
                {
                    message = $"Insufficient amount of bones {BonesCount()}/{MIN_BONES_REQUIREMENT}!",
                    outcome = ValidationResult.Outcome.CriticalError,
                });
            }

            foreach (var _bone in bones)
            {
                if (_bone == null || !_bone.valid)
                {
                    outValidationResult.Add(new ValidationResult()
                    {
                        message = "Missing one or more bones!",
                        outcome = ValidationResult.Outcome.CriticalError,
                    });
                    break;
                }
            }

            foreach (var _bone in bones)
            {
                if (_bone != null && ikp?.origin != null && _bone.transform == ikp.origin)
                {
                    outValidationResult.Add(new ValidationResult()
                    {
                        message = $"The IKP origin {ikp.origin} can not be part of the joints!",
                        outcome = ValidationResult.Outcome.CriticalError,
                    });
                    break;
                }
            }

            if (speed <= 0)
            {
                outValidationResult.Add(new ValidationResult()
                {
                    message = $"Speed should not be zero!",
                    outcome = ValidationResult.Outcome.Warning,
                });
            }

            if (orientationMode == OrientationMode.Positional)
            {
                if (orientationGuides.Any(a => a == null) || orientationGuides.Count == 0)
                {
                    outValidationResult.Add(new ValidationResult()
                    {
                        message = $"Assign all {SETUtil.StringUtil.WordSplit(nameof(orientationGuides), true)} for {nameof(OrientationMode.Positional)} orientation mode!",
                        outcome = ValidationResult.Outcome.CriticalError,
                    });
                }
            }

            // Make sure stretch is initialized properly with the new update
            if (stretch < MIN_STRETCH)
            {
                SetProperty(p_stretch, 1);
            }

            return base.Validate(outValidationResult);
        }


        public override ExecutionFlag IKPUpdate()
        {
            if (base.IKPUpdate() == ExecutionFlag.Break)
            {
                return ExecutionFlag.Break;
            }

            var _bonesCount = BonesCount();

            if (_bonesCount < 2 || points.Count == 0)
            {
                return ExecutionFlag.Break;
            }

#if UNITY_EDITOR
            Profiler.BeginSample($"IK Module Update: {name}");
#endif
            Vector3 _pivotPoint = bones.First().position;
            Vector3 _targetPoint = target.Get(originBone).position;
            var _originZ = ikp.origin.transform.position.z;

            if (is2D)
            {
                _targetPoint.z = _originZ;
            }

            var _trueTargetVector = (_targetPoint - _pivotPoint);
            var _targetVectorNormal = _trueTargetVector.normalized;
            float _distanceToTarget = _trueTargetVector.magnitude;
            var _angleLimit = Mathf.Lerp(180f, limitAngles, weight);
            var _angleLimitx2 = _angleLimit * 2f;

            var _originRotLSP = new IKPLocalSpace(ikp.origin.rotation);
            var _useAngleLimits = useAngleLimits;

            //cache bone lengths (note: maybe also do on Start)
            if (boneLengthsCached.Length != _bonesCount)
            {
                boneLengthsCached = new float[_bonesCount];
            }
            for (int i = 0; i < _bonesCount; i++)
            {
                boneLengthsCached[i] = GetLength(i);
            }

            if (useOrientation || !CaseGoalOutOfReach(_distanceToTarget, _targetVectorNormal, _pivotPoint))
            {
                if (useOrientation)
                {
                    if (orientationMode == OrientationMode.Positional)
                    {
                        PreparePositionalOrientation(_pivotPoint, _targetVectorNormal, boneLengthsCached);
                    }
                    else
                    {
                        var _orientationVector = IKPUtils.Circle(new ProjectionPlane(originBone.right, originBone.up), orientation);

                        // reach in a straight line
                        PrepareParametricOrientation(_pivotPoint, _orientationVector, boneLengthsCached);
                    }
                }

#if UNITY_EDITOR
                Profiler.BeginSample($"FABRIK ({points.Count} points)");
#endif
                //FABRIK:
                var _iterations = useOrientation ? iterations : 1;

                const int _indexFirstPoint = 0;

                for (var j = 0; j < _iterations; j++)
                {
                    // backward
#if UNITY_EDITOR
                    Profiler.BeginSample("Backward");
#endif
                    points[indexLastPoint] = _targetPoint;

                    //Note: That's not the actual tip bone, but rather the bone before the tip which contributes to the rotation.
                    var _startingIndexB = points.Count - 2;
                    var _lastVector = Vector3.zero;
                    for (int i = _startingIndexB; i >= 0; i--)
                    {
                        var _boneLength = boneLengthsCached[i];

                        var _prev = points[i + 1];

                        if (isLeg && i == _startingIndexB)
                        {
                            // make the leg step on things
                            points[i] = _prev + LegUp() * _boneLength;
                        }
                        else
                        {
                            // common case
                            bool _normalized = false;
                            var _boneVector = points[i] - _prev;
                            var _boneVectorTrue = _boneVector;

                            // valid angle
                            if (_useAngleLimits && i != _startingIndexB && Vector3.Angle(_boneVector, _lastVector) > _angleLimit)
                            {
                                _normalized = true;
                                _boneVector = -IKPUtils.LimitedAngle(IKPUtils.CalculateLimbLocalSpace(_originRotLSP, -_lastVector, FAST_LOCAL_SPACE), _angleLimitx2, -_boneVector, false).normalized;
                            }

                            points[i] = (_normalized ? _boneVector : _boneVector.normalized) * _boneLength + _prev;
                            _lastVector = (_normalized ? _boneVector * _boneVectorTrue.magnitude : _boneVector);
                            // Note: coefficient of squares!
                        }

                        if (is2D)
                        {
                            points[i] = new Vector3(points[i].x, points[i].y, _originZ);
                        }
                    }
#if UNITY_EDITOR
                    Profiler.EndSample();
#endif

                    // forward
                    points[_indexFirstPoint] = _pivotPoint;

                    for (int i = 1; i < points.Count; i++)
                    {
                        var _prev = points[i - 1];
                        var _boneVector = points[i] - _prev;
                        points[i] = _boneVector.normalized * boneLengthsCached[i - 1] + _prev;
                    }

                    // terminate if close enough
                    if (Vector3.SqrMagnitude(points[indexLastPoint] - _targetPoint) < FABRIK_TERMINATION_DELTA)
                    {
                        break;
                    }
                }

#if UNITY_EDITOR
                Profiler.EndSample();
#endif
                if (points.Count >= 3 && useOrientation)
                {
                    if (solverEnhance == SolverEnhance.JustifyTargetByRootRotation)
                    {
                        Vector3 _base = points[_indexFirstPoint];
                        Vector3 _tip = points[indexLastPoint];

                        // rotate the base to reach even closer to the target
                        var _mtx = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(_tip - _base, _targetVectorNormal), Vector3.one);
                        for (var i = 0; i <= indexLastPoint; i++)
                        {
                            points[i] = _mtx.MultiplyPoint3x4(points[i] - _base) + _base;
                        }

                    }
                    else if (solverEnhance >= SolverEnhance.JustifyTarget_ByHeroSolve)
                    {
#if UNITY_EDITOR
                        Profiler.BeginSample($"HERO SOLVE");
#endif
                        // HERO solve for maximum percision

                        Vector3 _RotatedPoint(Vector3 point, Quaternion rotation, Vector3 pivot)
                        {
                            var _vector = point - pivot;
                            return rotation * _vector + pivot;
                        }

                        // Optimization Hint: move these common vars on the outer block
                        int _indexMiddle = heroSolvePreferJointIndex < 0 ? points.Count / 2 : Mathf.Min(heroSolvePreferJointIndex, points.Count - 1);
                        var _indMidNext = _indexMiddle + 1;


                        Vector3 _base = points[_indexFirstPoint];
                        Vector3 _joint = points[_indexMiddle];
                        Vector3 _tip = points[indexLastPoint];

                        float _baseToJointDst2_a = Vector3.SqrMagnitude(_joint - _base);
                        float _baseToJointDst_a = Mathf.Sqrt(_baseToJointDst2_a);

                        float _baseToTipDst2_c = Vector3.SqrMagnitude(_tip - _base);
                        float _baseToTipDst_c = Mathf.Sqrt(_baseToTipDst2_c);

                        float _jointToTipDst2_b = Vector3.SqrMagnitude(_tip - _joint);
                        float _jointToTipDst_b = Mathf.Sqrt(_jointToTipDst2_b);

                        float _limbMaxReach = _baseToJointDst_a + _jointToTipDst_b;
                        float _limbReach = Mathf.Min(_limbMaxReach, _distanceToTarget);
                        float _limbReach2 = _limbReach * _limbReach;

                        var _d = (_jointToTipDst2_b - _baseToJointDst2_a + _limbReach2) / (2f * _limbReach);
                        float _h = Mathf.Sqrt(Mathf.Abs(_jointToTipDst2_b - (_d * _d)));

                        var _jointProjection = _base + _targetVectorNormal * (_limbReach - _d);
                        var _jointHeightVector = (_joint - _jointProjection).normalized;
                        Vector3 _jointTargetPos = _jointProjection +
                            _h * _jointHeightVector;

                        Vector3 _fromJointToTarget = _targetPoint - _jointTargetPos;
                        var _fromJointToTargetNrm = _fromJointToTarget.normalized;


                        //Debug.DrawLine(_base , _joint, Color.blue);
                        //Debug.DrawLine(_joint, _tip, Color.blue);
                        //Debug.DrawLine(_base + _targetVectorNormal * (_limbReach - _d), _jointTargetPos);

                        // rotate the base group to point towards the joint
                        {
                            var _from = _joint - _base;
                            var _to = _jointTargetPos - _base;
                            var _rot = Quaternion.FromToRotation(_from, _to);

                            for (var i = 0; i <= indexLastPoint; i++)
                            {
                                //Debug.DrawLine(points[i], _RotatedPoint(points[i], _rot, _base));
                                points[i] = _RotatedPoint(points[i], _rot, _base);
                            }
                        }
                        // rotate the joint group to point towards the target
                        {
                            var _from = points[indexLastPoint] - points[_indexMiddle];
                            var _to = _fromJointToTarget;
                            var _rot = Quaternion.FromToRotation(_from, _to);

                            bool _useSInversion = solverEnhance == SolverEnhance.JustifyTarget_ByHeroSolveWithSInversion;

                            if (_useSInversion)
                            {
                                for (var i = _indMidNext; i <= indexLastPoint; i++)
                                {
                                    points[i] = _RotatedPoint(points[i], _rot, points[_indexMiddle]);
                                    //-----------
                                    // Do a mirror of the lower group alont the joint-tip line.
                                    // To account for the 3-shape artifact at the joint it will continue on as an S-shape.
                                    var _p = points[i];
                                    var _pRelative = _p - points[_indexMiddle];
                                    //Debug.DrawLine(points[i], points[i] + 2 * (points[_indexMiddle] + Vector3.Project(_pRelative, _targetPoint - points[_indexMiddle]) - points[i]));
                                    points[i] = points[i] + 2 * (points[_indexMiddle] + Vector3.Project(_pRelative, _targetPoint - points[_indexMiddle]) - points[i]);
                                    //-----------
                                }
                            }
                            else
                            {
                                for (var i = _indMidNext; i <= indexLastPoint; i++)
                                {
                                    points[i] = _RotatedPoint(points[i], _rot, points[_indexMiddle]);
                                }
                            }
                        }

                        //Debug.DrawLine(_base, _jointTargetPos, Color.green);
                        //Debug.DrawLine(_jointTargetPos, _jointTargetPos + _fromJointToTarget.normalized * _jointToTipDst_b, Color.green);
                        //Debug.DrawLine(_base, _base + _targetVectorNormal * _limbReach, Color.green);
#if UNITY_EDITOR
                        Profiler.EndSample();
#endif
                    }
                }

                // Before stretching the end bone(s) to always reach the target,
                // it needs to be pointed at it first.
                if (stretchable && stretchAlwaysReach)
                {
                    points[indexLastPoint] = _targetPoint;
                }

#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }

            // set bone orientations
            {
                int _last = _bonesCount - 1;
                var _prevRotationTgt = Quaternion.identity;
                var _prevBoneRotation = Quaternion.identity;
                Vector3 _upReference = ikp.origin.up;

                for (int i = 0; i < _bonesCount; i++)
                {
                    // define rotation
                    Quaternion _r;
                    if (i < _last)
                    {
                        // look at next
                        _r = Quaternion.LookRotation(points[i + 1] - points[i], _upReference);
                        _prevRotationTgt = _r;
                    }
                    else
                    {
                        // it is important for the limb to have a 'tip' bone.
                        _r = _prevRotationTgt;
                    }

                    // match target rotation
                    if (i > _bonesCount - 3 && matchTargetRotation != MatchRotation.None)
                    {
                        Quaternion _tgtRotationMatching = Quaternion.identity;

                        if (matchTargetRotation != MatchRotation.None)
                        {
                            _tgtRotationMatching = Quaternion.Lerp(target.GetRotation(), _r, (_distanceToTarget - maxLimbDistance) / maxLimbDistance);
                        }

                        if (matchTargetRotation.ContainsFlag(MatchRotation.TipBone) && i == _last)
                        {
                            _r = _tgtRotationMatching;
                        }

                        if (matchTargetRotation.ContainsFlag(MatchRotation.LastTwoBones) && i == _last - 1 && i > 0)
                        {
                            _r = _tgtRotationMatching;
                        }
                    }

                    // damp
                    Quaternion _dampDesiredRotation = Quaternion.Lerp(boneRotationsTargets[i], _r, speed * Time.deltaTime);
                    boneRotationsTargets[i] = _dampDesiredRotation;

                    // weight
                    bones[i].rotation = Quaternion.Lerp(bones[i].rotation, _dampDesiredRotation, weight);
                    if (is2D)
                    {
                        var _br = bones[i].rotation;
                        var _fwd = _br * Vector3.forward;
                        _fwd.z = _originZ;
                        var _up = _br * Vector3.up;
                        _up.z = _originZ;
                        bones[i].rotation = Quaternion.LookRotation(_fwd, _up);
                    }

                    // Fix rotation limits.
                    // While interpolating between target states, some bending may occur which can violate the angle limits.
                    // This is a fix for that.
                    if (_useAngleLimits && i != 0 && i != _last)
                    {
                        var _vector = IKPUtils.LimitedAngle(new IKPLocalSpace(_prevBoneRotation), _angleLimit, bones[i].forward, false);
                        bones[i].rotation = Quaternion.Lerp(bones[i].rotation, Quaternion.LookRotation(_vector, bones[i].up), weight);
                    }

                    _prevBoneRotation = bones[i].rotation;

                    // record up ref for next bone
                    _upReference = _r * Vector3.up;
                }
            }

            // set bone positions
            for (int i = 1; i < _bonesCount; i++)
            {
                var t = bones[i].transform;
                var s = t.lossyScale;
                var _scaleMatrix = Matrix4x4.Scale(s);
                var _len = boneLengthsCached[i - 1];

                // stretch the end bone(s) to always reach the target
                if (stretchable && stretchAlwaysReach && i == bones.Count - 1)
                {
                    _len = Vector3.Distance(points[i], _targetPoint);
                }

                t.localPosition = _scaleMatrix.inverse.MultiplyPoint(bones[i].initialLocalPositionNormalized * _len);
            }

            return ExecutionFlag.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 LegUp()
        {
            switch (legWorldUp)
            {
                case LegWorldUp.FromOrigin:
                    return ikp.origin.up;
                case LegWorldUp.World:
                    return Vector3.up;
            }

            throw new ArgumentOutOfRangeException("Undefined " + nameof(LegUp));
        }

        /// <summary>
        /// using Chaikin curve to evaluate orientation.
        /// Note: use normalized target vector.
        /// </summary>
        private void PreparePositionalOrientation(Vector3 pivotPoint, Vector3 targetVector, float[] boneLengthsCache)
        {
            float _distance = 0;

            var _pts = new Vector3[orientationGuides.Count + 2];
            _pts[0] = pivotPoint;
            _pts[_pts.Length - 1] = targetVector * (maxLimbDistance + 1) + pivotPoint;

            for (int p = 0; p < orientationGuides.Count; p++)
            {
                _pts[p + 1] = orientationGuides[p].position;
            }

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = SETUtil.MathUtil.EvaluateChaikinCurve(_distance, _pts, (uint)chaikinIterations);
                // if (i > 0) {
                //     Debug.DrawLine(points[i - 1], points[i]);
                // }
                if (i < points.Count - 1)
                {
                    _distance += boneLengthsCache[i];
                }
            }
        }

        /// <summary>
        /// Align the points onto the given orientation vector
        /// </summary>
        private void PrepareParametricOrientation(Vector3 pivotPoint, Vector3 orientationVector, float[] boneLengthsCache)
        {
            float _distance = 0;

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = orientationVector * _distance + pivotPoint;

                if (i < points.Count - 1)
                {
                    _distance += boneLengthsCache[i];
                }
            }
        }

        /// <summary>
        /// Checks if the goal is within reach. If within reach return true. If not within reach, straighten the boens and return false;
        /// </summary>
        private bool CaseGoalOutOfReach(float distanceToTarget, Vector3 targetVectorNormal, Vector3 pivotPoint)
        {
            if (maxLimbDistance > distanceToTarget)
            {
                // return in reach and expect the following code to handle it
                return false;
            }
            else
            {
                // reach in a straight line
                float _distance = 0;

                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = targetVectorNormal * _distance + pivotPoint;

                    if (i < points.Count - 1)
                    {
                        _distance += GetLength(i);
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Generates a setup automatically. Sometimes it's not perfect but can be used as a starting point.
        /// </summary>
        public override void AutoSetup(BodySetupContext bodySetupContext, StringBuilder outLog)
        {
            var _bones = new List<Transform>();

            if (bodySetupContext.allBones.Length == 0)
            {
                // prefer to use a specific set of bones, but fallback to root bone hierarchy if none specified
                AppendChildBoneRecursive(_bones, bodySetupContext.root);
            }
            else
            {
                _bones = bodySetupContext.allBones.ToList();
            }

            bones.Clear();

            foreach (var _bone in _bones)
            {
                bones.Add(new Bone(_bone));
            }

#if UNITY_EDITOR
            InitSerializedPropertiesIfNeeded();

            sp_limbJointsArray.arraySize = _bones.Count;
            for (int i = 0; i < _bones.Count; i++)
            {
                Transform _bone = _bones[i];
                sp_limbJointsArray.GetArrayElementAtIndex(i).FindPropertyRelative(Bone.TRANSFORM_PROPERTY_NAME).objectReferenceValue = _bone;
            }

            ApplyModifiedProperties();
#endif
            base.AutoSetup(bodySetupContext, outLog);
        }

        public override Transform[] CollectTransformDependencies()
        {
            return base.CollectTransformDependencies().Concat(bones.Select(a => a.transform)).ToArray();
        }

        /// <summary>
        /// Adds the first child to the list and does that recursively for the added child
        /// Used during auto-setup to fill-in bones.
        /// </summary>
        private static void AppendChildBoneRecursive(List<Transform> to, Transform from)
        {
            if (from.childCount > 0)
            {
                var _child = from.GetChild(0);
                to.Add(_child);
                AppendChildBoneRecursive(to, _child);
            }
        }

        public void SetStretchable(bool enabled)
        {
            this.stretchable = enabled;
        }

        public void SetTarget(Vector3 point)
        {
            SetTarget(point, Quaternion.identity);
        }

        public void SetTarget(Vector3 point, Quaternion rotation)
        {
            target = new IKPTarget(Relative.World, point, rotation);
        }

        public void SetTarget(IKPTarget ikpTarget)
        {
            target = ikpTarget;
        }

        public void SetTarget(Transform transform)
        {
            target.Set(transform);
        }


        /// <summary>
        /// Returns information about the target position and rotation
        /// </summary>
        public TransformData GetCurrentTargetTransformData()
        {
            return originBone != null ? target.Get(originBone) : target.Get(ikp ? ikp.origin : transform);
        }

        /// <summary>
        /// Returns the target transform if the IKP target uses it
        /// </summary>
        public Transform GetCurrentTransformTargetIfAny()
        {
            return target?.currentTransformTargetIfAny;
        }

        private int BonesCount()
        {
            return bones.Count;
        }

        private Bone GetBone(int i)
        {
            return bones[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetLength(int i)
        {
            return (stretchable) ? lengths[i] * (stretchIndependentOfWeight ? stretch : Mathf.LerpUnclamped(1, stretch, weight)) : lengths[i];
        }

        private void QuickAssignBones(Transform quickAssignRoot)
        {
            var _bones = new List<Bone>();

            var _currentNode = quickAssignRoot;
            do
            {
                _bones.Add(new Bone(_currentNode));
                Transform _nextNode;

                if (_currentNode.childCount > 0)
                {
                    _nextNode = _currentNode.GetChild(0);
                }
                else
                {
                    break;
                }

                foreach (Transform _child in _currentNode)
                {
                    if (_child.childCount > 0)
                    {
                        _nextNode = _child;
                        break;
                    }
                }

                _currentNode = _nextNode;
            }
            while (_currentNode.childCount > 0);
            bones = _bones;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }


#if UNITY_EDITOR
        protected override void InitializeSerializedProperties()
        {
            base.InitializeSerializedProperties();
            serializedProperties.Add(nameof(iterations), serialized.FindProperty(nameof(iterations)));
            serializedProperties.Add(nameof(chaikinIterations), serialized.FindProperty(nameof(chaikinIterations)));
            serializedProperties.Add(nameof(useOrientation), serialized.FindProperty(nameof(useOrientation)));
            serializedProperties.Add(nameof(orientationMode), serialized.FindProperty(nameof(orientationMode)));
            serializedProperties.Add(nameof(orientationGuides), serialized.FindProperty(nameof(orientationGuides)));
            serializedProperties.Add(nameof(isLeg), serialized.FindProperty(nameof(isLeg)));
            serializedProperties.Add(nameof(resetToUniformRotatonsOnInit), serialized.FindProperty(nameof(resetToUniformRotatonsOnInit)));
            serializedProperties.Add(nameof(legWorldUp), serialized.FindProperty(nameof(legWorldUp)));
            serializedProperties.Add(nameof(limitAngles), serialized.FindProperty(nameof(limitAngles)));
            serializedProperties.Add(nameof(stretchable), serialized.FindProperty(nameof(stretchable)));
            serializedProperties.Add(nameof(is2D), serialized.FindProperty(nameof(is2D)));
            serializedProperties.Add(nameof(stretchIndependentOfWeight), serialized.FindProperty(nameof(stretchIndependentOfWeight)));
            serializedProperties.Add(nameof(stretchAlwaysReach), serialized.FindProperty(nameof(stretchAlwaysReach)));
            serializedProperties.Add(nameof(matchTargetRotation), serialized.FindProperty(nameof(matchTargetRotation)));
            serializedProperties.Add(nameof(bones), serialized.FindProperty(nameof(bones)));
            serializedProperties.Add(nameof(target), serialized.FindProperty(nameof(target)));
            serializedProperties.Add(nameof(solverEnhance), serialized.FindProperty(nameof(solverEnhance)));
            serializedProperties.Add(nameof(heroSolvePreferJointIndex), serialized.FindProperty(nameof(heroSolvePreferJointIndex)));
        }

        public override void DrawEditorGUI()
        {
            base.DrawEditorGUI();
            if (serializedProperties.Count == 0) return;

            IKPEditorUtils.DrawTargetGUI(serializedProperties[nameof(target)], IKPUtils.NONE_PROPERTY_NAME);
        }

        protected override void DrawSetup()
        {
            base.DrawSetup();
            var _quickAssignRoot = (Transform)EditorGUILayout.ObjectField("Quick Assign Bones", null, typeof(Transform), true);
            if (_quickAssignRoot != null)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Quick Assign Bones", $"Populate the bones array with children of object {_quickAssignRoot} ? This action does not support undo.", "Yes", "No"))
                {
                    QuickAssignBones(_quickAssignRoot);
                }
            }

            if (serializedProperties.Count == 0)
            {
                return;
            }

            IKPEditorUtils.DrawBonesArraySerializedProperty(sp_limbJointsArray, true);
        }

        protected override void DrawSettings()
        {
            base.DrawSettings();
            if (serializedProperties.Count == 0) return;

            EditorGUILayout.PropertyField(serializedProperties[nameof(resetToUniformRotatonsOnInit)]);
            EditorGUILayout.PropertyField(serializedProperties[nameof(isLeg)]);
            if (isLeg) EditorGUILayout.PropertyField(serializedProperties[nameof(legWorldUp)]);
            EditorGUILayout.PropertyField(serializedProperties[nameof(stretchable)]);
            if (stretchable) EditorGUILayout.PropertyField(serializedProperties[nameof(stretchIndependentOfWeight)]);
            if (stretchable) EditorGUILayout.PropertyField(serializedProperties[nameof(stretchAlwaysReach)]);
            EditorGUILayout.PropertyField(serializedProperties[nameof(is2D)]);
            EditorGUILayout.PropertyField(serializedProperties[nameof(limitAngles)]); //
            EditorGUILayout.PropertyField(serializedProperties[nameof(matchTargetRotation)]);
            EditorGUILayout.PropertyField(serializedProperties[nameof(useOrientation)]);

            EditorGUI.BeginDisabledGroup(!useOrientation);
            {
                EditorGUILayout.PropertyField(serializedProperties[nameof(solverEnhance)]);
                if(solverEnhance >= SolverEnhance.JustifyTarget_ByHeroSolve)
                {
                    EditorGUILayout.PropertyField(serializedProperties[nameof(heroSolvePreferJointIndex)]);
                }

                EditorGUILayout.PropertyField(serializedProperties[nameof(iterations)], new GUIContent("Solver Iterations"));

                EditorGUILayout.PropertyField(serializedProperties[nameof(orientationMode)]);

                if (orientationMode == OrientationMode.Positional)
                {
                    GUILayout.Label("Positonal Orientation:");
                    EditorGUILayout.PropertyField(serializedProperties[nameof(orientationGuides)]);
                    EditorGUILayout.PropertyField(serializedProperties[nameof(chaikinIterations)]);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        protected override void DrawProperties()
        {
            base.DrawProperties();
            DrawPropertyGUI(p_weight, true, 0, 1);
            DrawPropertyGUI(p_speed);

            if (stretchable)
            {
                DrawPropertyGUI(p_stretch, true, MIN_STRETCH, 2);
            }

            EditorGUI.BeginDisabledGroup(!useOrientation);
            {
                if (orientationMode == OrientationMode.Parametric)
                {
                    GUILayout.Label("Parametric orientation:");
                    DrawPropertyGUI(p_orientation, true, 0, 1);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void OnDrawGizmosSelected()
        {
            if (bones.Count > 2)
            {
                for (int i = 0; i < bones.Count - 1; i++)
                {
                    if (!bones[i].valid || !bones[i + 1].valid)
                    {
                        break;
                    }

                    IKPn.IKPEditorUtils.PaintBone(bones[i].position, bones[i + 1].position, GizmoPalette.Green, true);
                    if (Application.isPlaying)
                    {
                        IKPn.IKPEditorUtils.PaintBone(points[i], points[i + 1], Color.Lerp(Color.black, Color.yellow, weight), true);
                    }
                }
            }

            if (orientationMode == OrientationMode.Positional && expandGUI)
            {
                if (bones.Count > 0 && bones[0].valid && orientationGuides.Count > 0 && orientationGuides[0])
                {
                    IKPn.IKPEditorUtils.PaintBone(bones[0].position, orientationGuides[0].position, GizmoPalette.White, true);
                }
                for (int i = 0; i < orientationGuides.Count - 1; i++)
                {
                    if (orientationGuides[i] == null || orientationGuides[i + 1] == null)
                    {
                        break;
                    }

                    IKPn.IKPEditorUtils.PaintBone(orientationGuides[i].position, orientationGuides[i + 1].position, GizmoPalette.White, true);
                }
            }
        }
#endif
    }
}
