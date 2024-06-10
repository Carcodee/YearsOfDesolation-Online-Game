// IKP - by Hristo Ivanov (Kris Development)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IKPn
{
	public static class ModuleManager
	{
		public const int MODULE_ORDER_BUFFER = 100; //used by the inspector sorting, as constructor time module counting/loading cannot be performed

		[System.NonSerialized] private static Dictionary<string, IKPModuleLinker> linkers = new Dictionary<string, IKPModuleLinker>();
		private static bool init = false;


		//accessors
		public static int Count
		{
			get
			{
				if (!init) LoadLinkers();
				return (linkers != null) ? linkers.Count : 0;
			}
		}

		//getters
		public static bool Has(string moduleSignature)
		{
			if (!init) LoadLinkers();
			return linkers.ContainsKey(moduleSignature);
		}

		public static IEnumerable<string> GetSignatures()
		{
			if (!init) LoadLinkers();
			return linkers.Select(a => a.Value.signature);
		}

		public static string GetName(string moduleSignature)
		{
			if (!init) LoadLinkers();
			return Linker(moduleSignature).displayName;
		}

		public static IKPModuleLinker TypeToModuleLinker(Type moduleType)
		{
			if (!init) LoadLinkers();

			foreach(var _linker in linkers){
				if(_linker.Value.type == moduleType){
					return _linker.Value;
				}
			}

			throw new Exception(string.Format("Failed to find a linker for type {0}", moduleType));
		}

		public static IKPModuleLinker Linker(string signature)
		{
			if (Has(signature))
				return linkers[signature];
			return null;
		}

		public static System.Type GetModuleType(string signature)
		{
			if (!init) LoadLinkers();
			
			foreach (var _pair in linkers) {
				if (_pair.Value != null && _pair.Value.signature == signature) {
					return _pair.Value.type;
				}
			}

			return null;
		}

		public static int GetInspectorOrder(string signature)
		{
			if (Has(signature))
				return linkers[signature].inspectorOrder;
			return 0;
		}

		public static int GetUpdateOrder(string signature)
		{
			if (Has(signature))
				return linkers[signature].updateOrder;
			return 0;
		}

		public static void LoadLinkers()
		{
			linkers.Clear();
			var _linkerObjects = IKPModuleAttribute.GetModuleLinkers();

			foreach (var _linker in _linkerObjects) {
				linkers.Add(_linker.signature, _linker);
			}
				
			init = true;
		}
	}
}
