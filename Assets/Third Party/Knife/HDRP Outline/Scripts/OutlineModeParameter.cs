using System;
using UnityEngine.Rendering;

namespace Knife.HDRPOutline.Core
{
    [Serializable]
    public class OutlineModeParameter : VolumeParameter<OutlineMode>
    {
        public OutlineModeParameter(OutlineMode value, bool overrideState) : base(value, overrideState)
        {
        }
    }
}