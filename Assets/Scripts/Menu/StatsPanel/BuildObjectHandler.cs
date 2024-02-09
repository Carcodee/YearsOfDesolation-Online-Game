using System;
using UnityEngine;

namespace Menu.StatsPanel
{
    public class BuildObjectHandler : MonoBehaviour
    {
        public BuildType buildType;
        public PlayerBuild playerBuildObject;

        private void OnValidate()
        {
            // if (buildType == BuildType.AK_Pistol)
            // {
            //     playerBuildObject = new AK_PistolBuild();
            // }
            // // else if (buildType == BuildType.AK_Rifle)
            // // {
            // // }
            // // else if (buildType == BuildType.AK_Shotgun)
            // // {
            // // }
        }
    }
}

public enum BuildType
{
    AK_Pistol,
    AK_Rifle,
    AK_Shotgun
}
