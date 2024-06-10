using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IKPModuleAttribute : Attribute
    {
		public string displayName = "";
		private string signature = "";
		
        public int
            inspectorOrder = 0,
            updateOrder = 0;
        
        public string iconPath;


        public IKPModuleAttribute (string signature)
        {
            this.signature = signature;
        }

        public static List<IKPModuleLinker> GetModuleLinkers ()
        {
            var _assembly = System.Reflection.Assembly.GetCallingAssembly();
            var _types = from _type in _assembly.GetTypes() where Attribute.IsDefined(_type, typeof(IKPModuleAttribute)) select _type;
            var _linkers = new List<IKPModuleLinker>();
            
            foreach(var _type in _types){
                var _moduleAttribute = (IKPModuleAttribute) _type.GetCustomAttributes(typeof(IKPModuleAttribute), false).First();
                var _icon = Resources.Load<Texture2D>(_moduleAttribute.iconPath);
                _linkers.Add(new IKPModuleLinker(_moduleAttribute.signature, _type, _moduleAttribute.inspectorOrder, _moduleAttribute.updateOrder, _moduleAttribute.displayName, _icon));
            }

            return _linkers;
        }
    }
}
