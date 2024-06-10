using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn
{
	public class RuntimeDebugDisplay : MonoBehaviour
	{
		public IKP ikp;

		void OnGUI ()
		{
			if(ikp == null){
				return;
			}

			//ikp.AutomaticSetup;
			//ikp.IsConfigured();


			GUILayout.Label("Runtime IKP Debugging:");

			foreach(var _signature in ModuleManager.GetSignatures())
			{
				var _module = ikp.GetModule(_signature);
				if(_module)
				{
					if(GUILayout.Button($"{_signature} enabled: {_module.IsActive()}")){
						_module.SetActive(!_module.IsActive());
					}
				}
			}
		}
	}
}
