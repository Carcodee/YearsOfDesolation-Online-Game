////////////////////////////////////////
//    Shared Editor Tool Utilities    //
//    by Kris Development             //
////////////////////////////////////////

//License: MIT
//GitLab: https://gitlab.com/KrisDevelopment/SETUtil


using System;
using System.Reflection;

namespace SETUtil.Volatile
{
    /// <summary>
    /// This utility exposes useful Unity internal methods by reflection
    /// </summary>
    public static class InternalInvokerUtils
    {
        #region personal utils

        private static Type GetTypeByName(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Contains(name))
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        private static MethodInfo GetMethodByName(string typeName, string name)
        {
            return GetTypeByName(typeName)?.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        }

        #endregion

        /// <summary>
        /// Returns the MethodInfo for the tool that migrates selected project materials
        /// </summary>
        /// <returns></returns>
        //[TestNotNull]
        public static MethodInfo GetURPToolMethodInfo()
        {
            return GetMethodByName("UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader", "UpgradeProjectMaterials");
        }

        /// <summary>
        /// Returns the MethodInfo for the tool that migrates selected project materials
        /// </summary>
        //[TestNotNull]
        public static MethodInfo GetHDRPToolMethodInfo()
        {
            return GetMethodByName("UnityEditor.Rendering.HighDefinition.UpgradeStandardShaderMaterials", "UpgradeMaterialsProject");
        }
    }
}