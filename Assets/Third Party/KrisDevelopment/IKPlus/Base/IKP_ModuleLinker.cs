using System;
using System.Reflection;
using UnityEngine;
using IKPn;

namespace IKPn
{
    public class IKPModuleLinker
    {
        public string
            signature = "",
            displayName = "";

        public System.Type type;

        public int
            inspectorOrder = 0,
            updateOrder = 0;

        public Texture2D icon;


        public IKPModuleLinker(string signature, Type type, int inspectorOrder, int updateOrder, string displayName = "", Texture2D icon = null)
        {
            this.signature = signature;
            this.displayName = string.IsNullOrEmpty(displayName) ? SETUtil.StringUtil.WordSplit(signature, true) : displayName;
            this.type = type;
            this.inspectorOrder = inspectorOrder;
            this.updateOrder = updateOrder;
            this.icon = icon; 
        }
    }
}
