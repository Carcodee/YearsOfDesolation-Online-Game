// IKP - by Hristo Ivanov (Kris Development)

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SETUtil.Extend;
using SETUtil.Types;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using UnityEngine.SocialPlatforms;

namespace IKPn
{
    public static class IKPUtils
    {
        public const float LIMB_DELTA_STRETCH_LIMIT = 0.001f;
        public const string NONE_PROPERTY_NAME = "None";

        /// <summary> Used for comparison operations.</summary>
        public static readonly Quaternion NULL_QUATERNION = new Quaternion(0f, 0f, 0f, 0f);

        /// <summary> Multiplying a rotation by this value does nothing.</summary>
        public static readonly Quaternion INDIFFERENT_QUATERNION = new Quaternion(0f, 0f, 0f, 1f);

        public const string IKP_TOOL_MENU = "Window/Kris Development/IK Plus";
        public const string IKP_COMPONENT_MENU = "IK Plus";
        public const string MODULE_COMPONENT_MENU = IKP_COMPONENT_MENU + "/Modules";


        [NonSerialized] private static string[] m_moduleSignatures;
        public static string[] moduleSignatures
        {
            get
            {
                return m_moduleSignatures ?? (m_moduleSignatures = ModuleManager.GetSignatures().ToArray());
            }
        }

        /// <summary>
        /// Faster and more readable than Mathf.Pow(x,2)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pw2(float n)
        {
            return n * n;
        }

        /// <summary>
        /// returns an ease-in function that starts from 0.01 and for x = 1 meets point (1,1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LogToOne(float x)
        {
            return (Mathf.Log(Pw2(x)) + 4f) / 4f;
        }

        public static Vector3 ClampVector(Vector3 a, Vector3 b, Vector3 n)
        {
            return new Vector3(
                Mathf.Clamp(a.x, b.x, n.x),
                Mathf.Clamp(a.y, b.y, n.y),
                Mathf.Clamp(a.z, b.z, n.z));
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 NormalVector(Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }

        public static float SignedProjectionMagnitude(Vector3 vec, Vector3 normal)
        {
            return Vector3.Project(vec, normal).magnitude * ((Vector3.Angle(vec, normal) > 90f) ? -1f : 1f);
        }

        public static Vector3 GetVectorOffset(Transform t, Vector3 p)
        {
            return GetVectorOffset(t.ToTransformData(), p);
        }

        public static Vector3 GetVectorOffset(TransformData t, Vector3 p)
        {
            Vector3 v = p - t.position;
            Vector3 v3 = new Vector3(SignedProjectionMagnitude(v, t.right), SignedProjectionMagnitude(v, t.up), SignedProjectionMagnitude(v, t.forward));
            return v3;
        }

        public static Vector3 ApplyVectorOffset(Transform t, Vector3 offset3f)
        {
            return ApplyVectorOffset(t.ToTransformData(), offset3f);
        }

        public static Vector3 ApplyVectorOffset(TransformData t, Vector3 offset3f)
        {
            Vector3 v3 =
                t.right * offset3f.x
                + t.up * offset3f.y
                + t.forward * offset3f.z;
            v3 += t.position;
            return v3;
        }

        public static Vector3 ProjectVector(ProjectionPlane p, Vector3 v)
        {
            return ProjectVector(p, v, false);
        }
        public static Vector3 ProjectVector(ProjectionPlane p, Vector3 v, bool proportional)
        {
            Vector3 n1 = Vector3.Project(v, p.vector1);
            Vector3 n2 = Vector3.Project(v, p.vector2);
            Vector2 scale = new Vector2(proportional ? p.vector1.magnitude : 1f, proportional ? p.vector2.magnitude : 1f);
            Vector3 v1 = n1 * scale.x + n2 * scale.y;
            return v1;
        }


        /// <summary>
        /// Get the rotation value as quaterion, required to go from the master orientation to the slave orientation
        /// Multiply by the inverse to go from slave to master.
        /// </summary>
        public static Quaternion GetRotationOffset(Transform master, Transform slave)
        {
            return GetRotationOffset(master.rotation, slave.rotation);
        }

        /// <summary>
        /// Get the rotation value as quaterion, required to go from the master orientation to the slave orientation
        /// Multiply by the inverse to go from slave to master.
        /// </summary>
        public static Quaternion GetRotationOffset(Quaternion master, Quaternion slave)
        {
            //source is always the forward facing vector and the offset is how much does the affected need to rotate in order to reach that forward vector
            Quaternion offset = Quaternion.Inverse(master) * slave;
            return offset;
        }

        public static TransformData GetTransformDataOffset(TransformData master, TransformData slave)
        {
            TransformData _offset = new TransformData();
            _offset.position = GetVectorOffset(master, slave.position);
            _offset.rotation = GetRotationOffset(master.rotation, slave.rotation);
            return _offset;
        }

        public static TransformData ApplyTransformDataOffset(TransformData source, TransformData offset)
        {
            return new TransformData(ApplyVectorOffset(source, offset.position), source.rotation * offset.rotation);
        }

        public static TransformData CalculateSmoothRelative(ref TransformData smoothOffset, TransformData target, Transform origin, float speed)
        {
            TransformData
                _originTrDt = origin.ToTransformData(),
                _offset = GetTransformDataOffset(_originTrDt, target), //step 1: get target offset
                _output;

            smoothOffset = TransformData.Lerp(smoothOffset, _offset, Time.deltaTime * speed); //step 2: lerp current offset to target one
            _output = ApplyTransformDataOffset(_originTrDt, smoothOffset); //return the target rotation
            return _output;
        }

        public static Vector3 Circle(ProjectionPlane d, float c)
        {
            return Circle(Vector3.zero, d, c);
        }

        public static Vector3 Circle(Vector3 point, ProjectionPlane d, float c)
        {
            c = c * Mathf.PI * 2f;
            return point + d.vector1 * Mathf.Sin(c) + d.vector2 * Mathf.Cos(c);
        }

        /// <summary>
        /// When limitPlaneOnly is true, resulting vector will always be laying on the limit plane (due to legacy reasons).
        /// When limitPlaneOnly is false, result will be within the cone defined by the max angle.
        /// </summary>
        public static Vector3 LimitedAngle(IKPLocalSpace ikpLsp, float maxAngle, Vector3 targetVector, bool limitPlaneOnly = true)
        {
            var _halfAngle = maxAngle / 2f;
            if (!limitPlaneOnly)
            {
                if (Vector3.Angle(ikpLsp.forward, targetVector) <= _halfAngle)
                {
                    return targetVector;
                }
            }

            //forward correction
            Vector3 _horizontal = Vector3.Project(targetVector, ikpLsp.right);
            float _forwardCorrection = Mathf.Lerp(1f, 0f, _horizontal.magnitude * 2f); //add more forward offset as the fallback target gets closer to the middle

            //fallback
            float _limitRadian = ((90f - _halfAngle) * Mathf.PI) / 180f;
            ProjectionPlane _pj = new ProjectionPlane(ikpLsp.up, ikpLsp.right);
            Vector3 _projectedTarget = IKPUtils.ProjectVector(_pj, targetVector, false);
            float _fallbackForward = _projectedTarget.magnitude * Mathf.Tan(_limitRadian) + _forwardCorrection;
            return _projectedTarget + ikpLsp.forward * _fallbackForward;
        }

        public static IKPLocalSpace CalculateLimbLocalSpace(IKPLocalSpace referencePoint, Vector3 pivot, Vector3 endPoint, bool useFastLSP = false)
        {
            return CalculateLimbLocalSpace(referencePoint, IKPUtils.NormalVector(pivot, endPoint), useFastLSP);
        }

        /// <summary>
        /// Most often used for cases where you want the rotation to remain inverted behind the reference point.
        /// diagram: +z /\| |\/ -z
        /// </summary>
        public static IKPLocalSpace CalculateLimbLocalSpace(IKPLocalSpace referencePoint, Vector3 targetVector, bool useFastLSP = false)
        {
            IKPLocalSpace _ikpLocalSpace = new IKPLocalSpace();
            if (useFastLSP)
            {
                // ------ FAST but inaccurate --------
                var q = Quaternion.LookRotation(targetVector, referencePoint.up);
                _ikpLocalSpace.forward = q * Vector3.forward;
                _ikpLocalSpace.right = q * Vector3.right;
                _ikpLocalSpace.up = q * Vector3.up;
            }
            else
            {

                // ------ Slow but accurate -------
                _ikpLocalSpace.forward = targetVector;

                //UP
                int _forwardVectorQuadrantVertical;

                if (Vector3.Angle(_ikpLocalSpace.forward, referencePoint.up) <= 90)
                {
                    if (Vector3.Angle(_ikpLocalSpace.forward, referencePoint.forward) <= 90)
                    {
                        _forwardVectorQuadrantVertical = 0;
                    }
                    else
                        _forwardVectorQuadrantVertical = 3;
                }
                else
                {
                    if (Vector3.Angle(_ikpLocalSpace.forward, referencePoint.forward) >= 90)
                    {
                        _forwardVectorQuadrantVertical = 2;
                    }
                    else
                        _forwardVectorQuadrantVertical = 1;
                }

                Vector3 _localSpaceUpVertical = Vector3.zero;
                ProjectionPlane _projectionPlaneVertical = new ProjectionPlane();
                _projectionPlaneVertical.vector1 = referencePoint.forward;
                _projectionPlaneVertical.vector2 = referencePoint.up;

                //vertical
                if (_forwardVectorQuadrantVertical == 0)
                {
                    _localSpaceUpVertical = GetPlanePerpendicular(_projectionPlaneVertical, _ikpLocalSpace.forward, referencePoint.up, -referencePoint.forward);
                }
                else if (_forwardVectorQuadrantVertical == 1)
                {
                    _localSpaceUpVertical = GetPlanePerpendicular(_projectionPlaneVertical, _ikpLocalSpace.forward, referencePoint.forward, referencePoint.up);
                }
                else if (_forwardVectorQuadrantVertical == 2)
                {
                    _localSpaceUpVertical = GetPlanePerpendicular(_projectionPlaneVertical, _ikpLocalSpace.forward, -referencePoint.up, referencePoint.forward);
                }
                else if (_forwardVectorQuadrantVertical == 3)
                {
                    _localSpaceUpVertical = GetPlanePerpendicular(_projectionPlaneVertical, _ikpLocalSpace.forward, -referencePoint.forward, -referencePoint.up);
                }

                _ikpLocalSpace.up = _localSpaceUpVertical;

                //RIGHT
                _ikpLocalSpace.right = Vector3.Cross(_ikpLocalSpace.up, _ikpLocalSpace.forward);

                _ikpLocalSpace.Normalize();
            }

            return _ikpLocalSpace;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetPlanePerpendicular(ProjectionPlane projectionPlane, Vector3 forwardVector, Vector3 lerpVector1, Vector3 lerpVector2)
        {
            float forwardAngle = Vector3.Angle(ProjectVector(projectionPlane, forwardVector), -lerpVector2);
            float lerpCof = forwardAngle / 90f;
            return Vector3.Normalize(Vector3.Lerp(lerpVector1, lerpVector2, lerpCof));
        }

        /// <summary>
        /// For HERO-based solvers, such as Upper, Lower, etc.
        /// Get world-space vector perpendicular to the base-target one.
        /// Using useLimbLocalSpace will enable the elbow to rotate consistently around all eight sectors
        /// for cases where you might have a limb going behind the reference bone (-z) and you want
        /// the orientation to remain inverted.
        /// </summary>
        public static Vector3 ElbowJointVector(Vector3 @base, Vector3 target, Bone reference, float axialRotation = 0.5f, bool useLimbLocalSpace = true)
        {
            var _direction = target - @base;
            IKPLocalSpace _limbLocalSpace = useLimbLocalSpace 
                ? IKPUtils.CalculateLimbLocalSpace(reference.GetLocalSpace(), _direction)
                : reference.GetLocalSpace();
            return IKPUtils.Circle(new ProjectionPlane(_limbLocalSpace.up,  _limbLocalSpace.right), axialRotation);
        }

        /// <summary>
        /// Match the bone name against a whitelist and exclude if part of a the blacklist
        /// </summary>
        public static bool MatchBoneName(Transform t, IEnumerable<string> whitelist, IEnumerable<string> blacklist = null)
        {
            return MatchBoneName(t.name, whitelist, blacklist);
        }

        /// <summary>
        /// Match the bone name against a whitelist and exclude if part of a the blacklist
        /// </summary>
        public static bool MatchBoneName(string t_name, IEnumerable<string> matchList, IEnumerable<string> blacklist = null)
        {
            var _blacklist = (blacklist ?? new List<string>()).Concat(BoneNamesLibrary.alwaysIgnore);
            if (_blacklist.Any(a =>
            {
                var word = a.TrimStart('@');
                if (a.StartsWith("@")) // if blacklist word is defined to sit at the start of the name
                {
                    return t_name.StartsWith(word, System.StringComparison.OrdinalIgnoreCase);
                }

                var _foundAtIndex = t_name.IndexOf(word, System.StringComparison.OrdinalIgnoreCase);
                return (_foundAtIndex > -1 // if it exists and is not a desired whitelist word 
                && !matchList.Any(a => (a.Length + _foundAtIndex) <= t_name.Length 
                    && t_name.Substring(_foundAtIndex, a.Length).Equals(a, StringComparison.OrdinalIgnoreCase)));
            }))
            {
                // part of the 'blacklist'
                return false;
            }

            //check a transform's name against a library of bone names
            foreach (var entry in matchList)
            {
                var word = entry.TrimStart('@');
                if (entry.StartsWith("@")) // if defined to sit at the start of the bone name
                {
                    return t_name.StartsWith(word, System.StringComparison.OrdinalIgnoreCase);
                }
                else if (t_name.IndexOf(word, System.StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return true;
                }
            }

            return false;
        }

        public static Rect SubtractRectPosition(Rect a, Rect b)
        {
            return new Rect(a.x - b.x, a.y - b.y, a.width, a.height);
        }

        public static Rect AddRectPosition(Rect a, Rect b)
        {
            return new Rect(a.x + b.x, a.y + b.y, a.width, a.height);
        }

        public static string GetPropertyName(System.Type type, int i)
        {
            if (!typeof(IKPModule).IsAssignableFrom(type))
            {
                Debug.LogError("[IKPu.GetPropertyName ERROR] Type " + type.ToString() + " is not an IKP Module!");
                return IKPUtils.NONE_PROPERTY_NAME;
            }

            string[] _pptNms = GetPropertyNames(type);
            if (_pptNms != null && i < _pptNms.Length)
            {
                return _pptNms[i];
            }

            Debug.LogError("[IKPu.GetPropertyName ERROR] Out of bounds property index " + i);
            return IKPUtils.NONE_PROPERTY_NAME;
        }

        public static string[] GetPropertyNames(System.Type type)
        {
            if (!typeof(IKPModule).IsAssignableFrom(type))
                return new string[0];

            System.Type _propType = type.GetNestedType("Property");
            if (_propType != null)
            {
                FieldInfo[] _fields = _propType.GetFields();
                string[] _names = new string[_fields.Length - 1];
                for (int i = 1; i < _fields.Length; i++)
                    _names[i - 1] = _fields[i].Name;

                return _names;
            }

            return new string[0];
        }

        public static string GetPropertyName(IKPModule o, int i)
        {
            return GetPropertyName(o.GetType(), i);
        }

        public static string[] GetPropertyNames(IKPModule o)
        {
            return GetPropertyNames(o.GetType());
        }

        public static string GetPropertyName(string moduleSignature, int propertyIndex)
        {
            IKPModuleLinker _linker = ModuleManager.Linker(moduleSignature);
            if (_linker != null)
            {
                return GetPropertyName(_linker.type, propertyIndex);
            }
            else
            {
                return IKPUtils.NONE_PROPERTY_NAME;
            }
        }

        public static string[] GetPropertyNames(string moduleSignature)
        {
            IKPModuleLinker _linker = ModuleManager.Linker(moduleSignature);

            if (_linker != null)
            {
                return GetPropertyNames(_linker.type);
            }

            return new string[0];
        }
    }
}
