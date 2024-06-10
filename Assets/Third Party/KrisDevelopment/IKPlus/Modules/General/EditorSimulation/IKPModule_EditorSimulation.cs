using UnityEngine;
using SETUtil.Extend;
using SETUtil.Types;
using System.Collections.Generic;
using System.Linq;
using SETUtil.SceneUI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{

    [IKPModule(ModuleSignatures.EDITOR_SIM, displayName = "Editor Simulation", inspectorOrder = 40, updateOrder = -1, iconPath = "IKP/ikp_editorsim_icon")]
    [AddComponentMenu(IKPUtils.MODULE_COMPONENT_MENU + "/Editor Simulation")]
    [DisallowMultipleComponent]
	[ExecuteInEditMode]
    public class IKPModule_EditorSimulation : ModuleBase_Simple, iOrderedComponent
    {
        //SETUtil.OrderedComponent:
        public override int OrderIndex()
        {
            return IKP.ORDER_INDEX + ModuleManager.GetInspectorOrder(ModuleSignatures.EDITOR_SIM);
        }

        //accessors and properties:
        private static HashSet<IKPModule_EditorSimulation> activeInstances = new HashSet<IKPModule_EditorSimulation>();
        public static bool isAnyInstancePlaying => activeInstances.Any(a => a != null && a.running);
        private bool running => isCurrentInstancePlaying && active && !Application.isPlaying;
        public bool isCurrentInstancePlaying { get; private set; }

        //private:
        private TransformData[] initialPose = new TransformData[0];

        private Transform origin => ikp.origin;

        private static iGUIElement instanceRunningIcon = null;
        private static iGUIElement instanceRunningLabel = null;


        internal override void Init(Transform origin) { }


#if UNITY_EDITOR
        public override void DrawEditorGUI()
        {
            if (!isCurrentInstancePlaying)
            {
                if (GUILayout.Button("Start Simulation")
                    && EditorUtility.DisplayDialog("Run Editor Sim?",
                        "Are you sure, you want to start an editor simulation? IKP Target propertirs may get modified during the simulation.", "Yes", "No"))
                    Run();
            }
            else if (GUILayout.Button("End Simulation"))
            {
                Stop();
            }

            if (GUILayout.Button("Rollback Pose"))
            {
                LoadPose();
            }
        }
#endif

        public void Run()
        {
            if (Application.isPlaying)
                return;

            //begin the simulation
            if (RecordPose())
            {
                ikp.Start(); //call start for all modules
                activeInstances.Add(this);
                isCurrentInstancePlaying = true;

                //call the first simulation
                ikp.IKPPreUpdate();
            }
            else
            {
                Debug.LogError("[IKPModule_EditorSimulation.Run ERROR] Failed to record the pose! Simulation prevented!");
            }

#if UNITY_EDITOR
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
#endif
        }

        public void Stop()
        {
            //end the simulation
            isCurrentInstancePlaying = false;
            activeInstances.Remove(this);

            LoadPose();
#if UNITY_EDITOR
            EditorApplication.update -= Update;
#endif
        }

        //private:
        void Start()
        {
            if (isCurrentInstancePlaying && !Application.isPlaying && active)
                ikp.Start();
        }

        void Update()
        {
            //call ikp's update
            if (running)
                ikp.IKPPreUpdate();
        }

        void LateUpdate()
        {
            //call ikp's late update
            if (isCurrentInstancePlaying)
                if (!Application.isPlaying && active)
                    ikp.IKPUpdate();
        }

        void OnDrawGizmos()
        {
            //draw warning/reminder label on screen when a simulation is playing
            if (isAnyInstancePlaying)
            {
                // lazy init
                if (instanceRunningIcon == null)
                {
                    instanceRunningIcon = new SETUtil.SceneUI.GUIImage(IKPStyle.warningIcon);
                }
                if (instanceRunningLabel == null)
                {
                    instanceRunningLabel = new SETUtil.SceneUI.GUILabel("IKP Editor Simulation running!") { fontStyle = FontStyle.Bold };
                }

                // draw
                const float _labelCenterOffset = 70;
                const float _heightOffset = 45;
                SETUtil.EditorUtil.DrawSceneElement(instanceRunningIcon, new Rect(Screen.width / 2 - 20 - _labelCenterOffset, _heightOffset, 18, 18));
                SETUtil.EditorUtil.DrawSceneElement(instanceRunningLabel, new Rect(Screen.width / 2 - _labelCenterOffset, _heightOffset, 250, 20));
            }
        }

        bool RecordPose()
        {
            if (!origin)
                return false; //failed

            Transform[] _objects = SETUtil.SceneUtil.CollectAllChildren(this.transform, true);
            CollectExternalDependencies(ref _objects);

            if (_objects != null)
            {
                initialPose = new TransformData[_objects.Length];
                initialPose.InitElements<TransformData>();

                for (int i = 0; i < initialPose.Length; i++)
                {
                    if (_objects[i])
                        initialPose[i].Set(_objects[i]);
                }
            }

            return true; //the execution was a success
        }

        void LoadPose()
        {
            if (!origin)
            {
                Debug.LogError("[IKPModule_EditorSimulation.Stop ERROR] Failed to load the pose!");
            }

            Transform[] _objects = SETUtil.SceneUtil.CollectAllChildren(this.transform, true);
            CollectExternalDependencies(ref _objects);

            if (_objects != null)
            {
                if (initialPose.Length != _objects.Length)
                    Debug.LogWarning("[LoadPose WARNING] Pose data record failed to validate (children hierarchy may have been modified), loaded pose might be incorrect!");

                for (int i = 0; i < initialPose.Length && i < _objects.Length; i++)
                {
                    if (_objects[i])
                        _objects[i].Set(initialPose[i]);
                }
            }
        }

        private void CollectExternalDependencies(ref Transform[] objects)
        {
            Transform[] _dependencies = ikp.CollectTransformDependencies(); //collect transform dependencies
            foreach (Transform d in _dependencies)
            {
                bool _unique = true;
                foreach (var o in objects)
                    if (d == o)
                    {
                        _unique = false;
                    }

                if (_unique)
                    SETUtil.Deprecated.ArrUtil.AddElement<Transform>(ref objects, d);
            }
        }


        private void OnDisable()
        {
            Stop();
        }

        protected override void OnDestroy()
        {
            Stop();
            base.OnDestroy();
        }

    }
}
