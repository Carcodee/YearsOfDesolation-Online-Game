#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KrisDevelopment.DistributedInternalUtilities;
using NUnit.Framework;
using SETUtil.Types;
using UnityEngine;
using UnityEngine.TestTools;

namespace IKPn.Tests
{
    public class IKPTests_General
    {
        class TestScene : EditorTestScene
        {
            protected override string sceneResourceName => "ikp_pose_validation_scene";
        }

        /// <summary>
        /// Runs the editor simulation for a while and checks if the bones are still at where the snapshot was created.
        /// If you need to create new snapshots, use the tool "Tools/Kris Development/IKP/Testing/Create Pose Snapshots For Current Scene"
        /// located in the PoseValidator script.
        /// </summary>
        [UnityTest]
        public IEnumerator TestPoseRegression()
        {
            using (new TestScene())
            {
                var _ikps = GameObject.FindObjectsOfType<IKP>();
                foreach (var _ik in _ikps)
                {
                    _ik.GetModule<IKPModule_EditorSimulation>(ModuleSignatures.EDITOR_SIM).Run();
                }

                // wait a few frames so the pose can settle
                yield return SETUtil.EditorOnly.EditorCoroutine.Wait(5);

                try
                {
                    // test pose of each IK in the scene
                    foreach (var _poseValidator in GameObject.FindObjectsOfType<IKPTesting_PoseValidator>())
                    {
                        var _logErrors = new StringBuilder();
                        Assert.IsTrue(_poseValidator.IsValid(_logErrors), $"POSE REGRESSION on {_poseValidator.name}.\n{_logErrors}");
                    }
                }
                finally
                {
                    foreach (var _ik in _ikps)
                    {
                        _ik.GetModule<IKPModule_EditorSimulation>(ModuleSignatures.EDITOR_SIM).Stop();
                    }
                }
            }
        }
    }
}
#endif