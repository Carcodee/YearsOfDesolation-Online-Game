using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace IKPn
{
    public class IKPTesting_PoseValidator : MonoBehaviour
    {
        [System.Serializable]
        public class Record
        {
            public Transform bone;
            public Vector3 position;
        }

        private const float ERROR_DELTA = 0.05f;
        [SerializeField] private List<Record> snapshot = new List<Record>();

        [ContextMenu("Create Snapshot")]
        public void SnapshotFromCurrentState ()
        {
#if UNITY_EDITOR

            snapshot.Clear ();
            foreach(Transform t in SETUtil.SceneUtil.CollectAllChildren(transform))
            {
                snapshot.Add(new Record() { bone = t, position = t.position });
            }
            UnityEditor.EditorUtility.SetDirty(this);
#else
            Debug.LogError("Not supported in build");
#endif
        }

        public bool IsValid(StringBuilder outLogErrors)
        {
            bool valid = true;
            foreach (var record in snapshot)
            {
                if (Vector3.SqrMagnitude(record.bone.position - record.position) > ERROR_DELTA)
                {
                    valid = false;
                    outLogErrors.AppendLine(record.bone.name);
                }
            }

            return valid;
        }

        /// <summary>
        /// Print in the console if the state is valid
        /// </summary>
        [ContextMenu("Validate")]
        private void PrintValidate()
        {
            StringBuilder logErrors = new StringBuilder();
            if (IsValid(logErrors))
            {
                Debug.Log("OK");
            }
            else
            {
                Debug.LogError($"POSE REGRESSION.\n{logErrors}");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Kris Development/IKP/Testing/Create Pose Snapshots For Current Scene")]
        public static void CreateSnapshotsTool ()
        {
            IEnumerator CreateSnapshotsCrt()
            {
                var _ikps = GameObject.FindObjectsOfType<IKP>();
                foreach (var _ik in _ikps)
                {
                    _ik.GetModule<IKPModule_EditorSimulation>(ModuleSignatures.EDITOR_SIM).Run();
                }

                UnityEditor.EditorUtility.DisplayProgressBar("Create Pose Snapshots", "", 0f);
                yield return SETUtil.EditorOnly.EditorCoroutine.Wait(1);
                UnityEditor.EditorUtility.DisplayProgressBar("Create Pose Snapshots", "", 0.2f);
                yield return SETUtil.EditorOnly.EditorCoroutine.Wait(1);
                UnityEditor.EditorUtility.DisplayProgressBar("Create Pose Snapshots", "", 0.4f);
                yield return SETUtil.EditorOnly.EditorCoroutine.Wait(1);
                UnityEditor.EditorUtility.DisplayProgressBar("Create Pose Snapshots", "", 0.6f);
                yield return SETUtil.EditorOnly.EditorCoroutine.Wait(1);
                UnityEditor.EditorUtility.DisplayProgressBar("Create Pose Snapshots", "", 0.8f);
                yield return SETUtil.EditorOnly.EditorCoroutine.Wait(1);

                UnityEditor.EditorUtility.DisplayProgressBar("Create Pose Snapshots", "", 0.9f);

                foreach (var _poseValidator in GameObject.FindObjectsOfType<IKPTesting_PoseValidator>())
                {
                    _poseValidator.SnapshotFromCurrentState();
                }

                foreach (var _ik in _ikps)
                {
                    _ik.GetModule<IKPModule_EditorSimulation>(ModuleSignatures.EDITOR_SIM).Stop();
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                UnityEditor.EditorUtility.ClearProgressBar();
            }

            SETUtil.EditorOnly.EditorCoroutine.Start(CreateSnapshotsCrt());
            
        }
#endif
    }
}