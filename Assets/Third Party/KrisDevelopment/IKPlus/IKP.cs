using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using SETUtil.Types;
using System.Text;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
#endif

using OElement = SETUtil.Types.OrderElement;

namespace IKPn
{
    [AddComponentMenu(IKPUtils.IKP_COMPONENT_MENU + "/IKP")]
    [DisallowMultipleComponent]
    public sealed partial class IKP : MonoBehaviour
    {
        internal static readonly int ORDER_INDEX = 100;
        private static readonly List<ValidationResult> MODULE_VALIDATE_RESULTS_BUFFER = new List<ValidationResult>(12);

        private static readonly ComponentOrderList COMPONENT_ORDER = new ComponentOrderList(
            //components order:
            new List<OElement>{
                new OElement(typeof(IKP),                            ORDER_INDEX - 2),
                new OElement(typeof(IKP_Blender),                    ORDER_INDEX - 1),
				//(modules...)
				new OElement(typeof(Animator),                       ORDER_INDEX + ModuleManager.MODULE_ORDER_BUFFER + 20),
                new OElement(typeof(UnityEngine.AI.NavMeshAgent),    ORDER_INDEX + ModuleManager.MODULE_ORDER_BUFFER + 30),
                new OElement(typeof(UnityEngine.AI.NavMeshObstacle), ORDER_INDEX + ModuleManager.MODULE_ORDER_BUFFER + 40),
                new OElement(typeof(MeshFilter),                     ORDER_INDEX + ModuleManager.MODULE_ORDER_BUFFER + 50),
                new OElement(typeof(Renderer),                       ORDER_INDEX + ModuleManager.MODULE_ORDER_BUFFER + 60)
            });


        [NonSerialized] internal List<ModuleInstanceData> moduleInstancesData = new List<ModuleInstanceData>();

        [SerializeField] public LayerMask raycastingMask = 1;
        [SerializeField] public bool manuallyUpdated = false;
        [Tooltip("This skips the validation before each IK Update. Disabling it will increase pefromance but setup errors will not be handled. Use at your own risk!")]
        [SerializeField] public bool skipValidationInPlayMode = false;

        [Tooltip("'Slave' IKP systems, called in order after this one")]
        [SerializeField] public IKP[] ikChildren = new IKP[0];

        [SerializeField] internal IKPPose originalPose;
        [SerializeField] internal bool offsetConfigInProgress = false;

        private Transform m_origin;
        private List<KeyValuePair<Animator, bool>> animators = new List<KeyValuePair<Animator, bool>>();

        //accessors:
        internal Transform origin
        {
            get
            {
                if (!m_origin)
                    m_origin = this.transform;
                return m_origin;
            }
        }


        /// <summary>
        /// Forces the component order so that the IKP appears above the modules in the inspector
        /// </summary>
        internal void ForceComponentOrder()
        {
#if UNITY_EDITOR
            GameObject _target = this.gameObject;

            SETUtil.CompUtil.ForceOrder(_target, COMPONENT_ORDER); //do for instance

            //get prefab if available
#if UNITY_2019_1_OR_NEWER
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(this.gameObject);
#else
            GameObject prefab = PrefabUtility.GetPrefabParent(this.gameObject) as GameObject;
#endif
            if (prefab != null)
                _target = prefab;
            SETUtil.CompUtil.ForceOrder(_target, COMPONENT_ORDER); //do for prefab
#endif
        }

        private void TryDoForModulePropertiesInternal (string moduleSignature, Action<ModuleBase_PropertiesProvider> @do)
        {
            if (!HasModule(moduleSignature))
            {
                return;
            }

            ModuleBase_PropertiesProvider m = null;

            if ((m = GetModule(moduleSignature) as ModuleBase_PropertiesProvider) != null)
            {
                @do(m);
            }
            else
            {
                Debug.LogError($"[ERROR] Could cast module {moduleSignature} to {nameof(ModuleBase_PropertiesProvider)}");
            }
        }

       

#if UNITY_EDITOR

        /// <summary>
        /// Set the foldout option for all modules in the inspector
        /// </summary>
        internal void ExpandAll(bool state)
        {
            foreach (var _mid in moduleInstancesData)
            {
                var _signature = _mid.signature;

                if (HasModule(_signature))
                {
                    GetModule(_signature).SetExpand(state);
                }
            }
        }
#endif

        internal void SetSetupInProgress(bool state)
        {
            if (state && !offsetConfigInProgress)
            { //start setup
                // store and disable animators
                animators = GetComponentsInChildren<Animator>().Concat(GetComponentsInParent<Animator>()).Select(a => new KeyValuePair<Animator, bool>(a, a.enabled)).ToList();
                animators.ForEach(a => a.Key.enabled = false); 
                
                RecordPose();
                offsetConfigInProgress = true;
            }
            else
            { //end setup
                ResetPose();
                // enable back animators
                animators.ForEach(a => a.Key.enabled = a.Value);
                offsetConfigInProgress = false;
            }
        }

#if UNITY_EDITOR
        internal void EditorAutoSetupModules()
        {
            var _log = new StringBuilder();
            AutoSetupModules(_log);
            EditorUtility.DisplayDialog("Auto Assign Bones", _log.ToString(), "Ok");
        }
#endif

        private void AutoSetupModules(StringBuilder outLog)
        {
            var _bodySetupContext = new BodySetupContext()
            {
                root = origin,
                allBones = SETUtil.SceneUtil.CollectAllChildren(origin),
                animator = GetComponentInChildren<Animator>(true),
            };

            foreach (var _signature in IKPUtils.moduleSignatures)
            {
                if (HasModule(_signature))
                {
                    GetModule(_signature).AutoSetup(_bodySetupContext, outLog);
                }
            }
            outLog.AppendLine("*Done");
        }

#if UNITY_EDITOR
        private void AddModule(string moduleSignature)
        {
            if (ModuleManager.Has(moduleSignature))
            {
                if (!HasModule(moduleSignature))
                {
                    // housekeeping
                    moduleInstancesData.RemoveAll(a => a == null || !a.valid);

                    // add new
                    var m = Undo.AddComponent(gameObject, ModuleManager.GetModuleType(moduleSignature)) as IKPModule;
                    Attach(m);
                }
                else
                {
                    Debug.LogError(string.Format("Trying to add new module, but module with id {0} already exists!", moduleSignature));
                }
            }
            else
            {
                Debug.LogError(string.Format("Module manager has no reference of {0}", moduleSignature));
            }
        }
#endif


        internal void ResetPose()
        {
            if (originalPose == null)
            {
                Debug.LogError("[IKP RESET POSE] originalPose is null!");
                return;
            }

            for (int i = 0; i < originalPose.poseData.Length && i < originalPose.bones.Length; i++)
            {
                if (!originalPose.bones[i])
                {
                    continue;
                }

                originalPose.bones[i].localPosition = originalPose.poseData[i].position;
                originalPose.bones[i].localRotation = originalPose.poseData[i].rotation;
            }
        }

        internal void RecordPose()
        {
            Transform[] childObjects = SETUtil.SceneUtil.CollectAllChildren(origin);
            originalPose = new IKPPose(childObjects);
        }

        /// <summary>
        /// Returns a list of transforms that are in some way related to the current IKP, such as bones, etc.
        /// </summary>
        internal Transform[] CollectTransformDependencies()
        {
            Transform[] _dependencies = new Transform[0];
            for (int i = 0; i < moduleInstancesData.Count; i++)
            {
                if (moduleInstancesData[i] != null && moduleInstancesData[i].valid)
                {
                    Transform[] _moduleDependencies = moduleInstancesData[i].module.CollectTransformDependencies();
                    SETUtil.Deprecated.ArrUtil.Combine<Transform>(ref _dependencies, _moduleDependencies);
                }
            }

            return _dependencies;
        }

        // ----------------- UNITY CALLS --------------------
        #region Unity Calls

        internal void Start()
        {
            if (manuallyUpdated)
                return;

            Init();
        }

        private void Update()
        {
            if (manuallyUpdated)
                return;
            IKPPreUpdate();
        }

        public void IKPPreUpdate()
        {
            if (!enabled) return;

            //call modules for pre-animation update
            foreach (var _mi in moduleInstancesData)
            {
                MODULE_VALIDATE_RESULTS_BUFFER.Clear();
                if (_mi.valid && _mi.module.Validate(MODULE_VALIDATE_RESULTS_BUFFER) && _mi.module.IsActive())
                {
                    _mi.module.IKPPreUpdate();
                }
            }

            foreach (var child in ikChildren)
            {
                child.IKPPreUpdate();
            }
        }

        private void LateUpdate()
        {
            if (manuallyUpdated)
                return;
            Profiler.BeginSample($"IKP Uniy Update: {name}");
            IKPUpdate();
            Profiler.EndSample();
        }

        public void IKPUpdate()
        {
            if (!enabled) return;
            Profiler.BeginSample($"IKP Update: {name}");

            //call modules in a specific order to override animations
            foreach (var _mi in moduleInstancesData)
            {
                if (!_mi.module.IsActive())
                {
                    continue;
                }

                if (skipValidationInPlayMode) {
                    _mi.module.IKPUpdate();
                    continue;
                }

                MODULE_VALIDATE_RESULTS_BUFFER.Clear();
                if (_mi.valid && _mi.module.Validate(MODULE_VALIDATE_RESULTS_BUFFER))
                {
                    _mi.module.IKPUpdate();
                }
            }

            foreach (var child in ikChildren)
            {
                child.IKPUpdate();
            }
            Profiler.EndSample();
        }
        #endregion
    }

}
