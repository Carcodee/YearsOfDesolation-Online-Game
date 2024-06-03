using System;
using UnityEngine.Rendering;

namespace Knife.HDRPOutline.Core
{
    [Serializable]
    public class OutlineIterationsParameter : VolumeParameter<OutlineIterations>
    {
        public OutlineIterationsParameter(OutlineIterations value, bool overrideState) : base(value, overrideState)
        {
        }
    }
}