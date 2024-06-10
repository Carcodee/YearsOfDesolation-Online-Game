using SETUtil.Common.Attributes;
using SETUtil.Common.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IKPn
{
    /// <summary>
    /// Mimic the dummy before IKPUpdate is run and before any animation is applied.
    /// </summary>
    [IKPModule(ModuleSignatures.DUMMY_STATE,
        displayName = "Dummy State",
        iconPath = "IKP/ikp_dummy_state_icon",
        inspectorOrder = -10,
        updateOrder = 2)]
    [AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Dummy State")]
    [DisallowMultipleComponent]
    public class IKPModule_DummyState : ModuleBase_Simple
    {
        public enum MimicTransform
        {
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale = 1 << 2,
        }

        [Tooltip("The source skeleton/armature to copy the positions and orientation from.")]
        [SerializeField] private Transform dummyRoot;

        [Tooltip("Set to mimic the dummy root and children before IKPUpdate is run and before any animation is applied.")]
        [SerializeField] private Transform targetRoot;

        [DrawEnumFlags]
        [Tooltip("Which components of the transforms should the target mimic. All by defailt.")]
        [SerializeField] private MimicTransform mimicTransform = MimicTransform.Position | MimicTransform.Rotation | MimicTransform.Scale;

        [NonSerialized] private Dictionary<Transform, Transform> map = new Dictionary<Transform, Transform>();

#if UNITY_EDITOR
        [NonSerialized] private Dictionary<string, SerializedProperty> serializedProperties = new Dictionary<string, SerializedProperty>();
#endif

        internal override void Init(Transform origin)
        {
            map.Clear();
            map.Add(dummyRoot, targetRoot);
            CollectMapping(dummyRoot, targetRoot, map);
        }

        public override bool Validate(List<ValidationResult> outValidationResult)
        {
            if (!dummyRoot)
            {
                outValidationResult.Add(new ValidationResult()
                {
                    outcome = ValidationResult.Outcome.CriticalError,
                    message = $"Missing {nameof(dummyRoot)}"
                });
            }

            if (!targetRoot)
            {
                outValidationResult.Add(new ValidationResult()
                {
                    outcome = ValidationResult.Outcome.CriticalError,
                    message = $"Missing {nameof(targetRoot)}"
                });
            }

            return base.Validate(outValidationResult);
        }

        private static void CollectMapping(Transform dummyNode, Transform targetNode, Dictionary<Transform, Transform> outMap)
        {
            foreach (Transform d in dummyNode)
            {
                var _dChildCount = d.childCount;
                Transform _targetMatch = null;
                foreach (Transform t in targetNode)
                {
                    if (t.name == d.name)
                    {
                        _targetMatch = t;
                        break;
                    }
                }

                if (_targetMatch)
                {
                    outMap.Add(d, _targetMatch);
                    if (_dChildCount > 0)
                    {
                        CollectMapping(d, _targetMatch, outMap);
                    }
                }

            }
        }

        public override Transform[] CollectTransformDependencies()
        {
            return base.CollectTransformDependencies().Concat(new Transform[] { dummyRoot, targetRoot }).ToArray();
        }

        public override ExecutionFlag IKPPreUpdate()
        {
            if (base.IKPPreUpdate() == ExecutionFlag.Break)
            {
                return ExecutionFlag.Break;
            }

            foreach (var m in map)
            {
                if (mimicTransform.ContainsFlag(MimicTransform.Position))
                {
                    m.Value.transform.position = m.Key.position;
                }

                if (mimicTransform.ContainsFlag(MimicTransform.Rotation))
                {
                    m.Value.transform.rotation = m.Key.rotation;
                }

                if (mimicTransform.ContainsFlag(MimicTransform.Scale))
                {
                    m.Value.transform.localScale = m.Key.localScale;
                }
            }

            return ExecutionFlag.Continue;
        }

#if UNITY_EDITOR
        public override void DrawEditorGUI()
        {
            base.DrawEditorGUI();
            foreach (var p in serializedProperties)
            {
                EditorGUILayout.PropertyField(p.Value);
            }
        }

        protected override void InitializeSerializedProperties()
        {
            base.InitializeSerializedProperties();
            serializedProperties.Clear();
            serializedProperties.Add(nameof(dummyRoot), serialized.FindProperty(nameof(dummyRoot)));
            serializedProperties.Add(nameof(targetRoot), serialized.FindProperty(nameof(targetRoot)));
            serializedProperties.Add(nameof(mimicTransform), serialized.FindProperty(nameof(mimicTransform)));
        }
#endif
    }
}