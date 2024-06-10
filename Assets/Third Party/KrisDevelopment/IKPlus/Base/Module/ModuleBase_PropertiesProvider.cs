// IKP - by Hristo Ivanov (Kris Development)

using System;
using UnityEngine;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
    public abstract class ModuleBase_PropertiesProvider : IKPModule
	{
		[HideInInspector, SerializeField] private float[] properties = new float[] {1,0,0,1};

#if UNITY_EDITOR
		protected SerializedProperty m_so_properties;
#endif


#if UNITY_EDITOR
		protected override void InitializeSerializedProperties()
		{
			base.InitializeSerializedProperties();
			m_so_properties = serialized.FindProperty(nameof(properties));
		}
#endif

		internal int PropertyCount()
		{
			return properties.Length;
		}

		//setting properties
		public void SetProperties(float?[] values)
		{
			if (IKPModule_EditorSimulation.isAnyInstancePlaying)
				return;

#if UNITY_EDITOR
			InitSerializedPropertiesIfNeeded();
			ApplyModifiedProperties();
			serialized.Update();
			//if the variables are set through the inspector it is acceptable to resize the array of properties
			if (values.Length > ((properties != null) ? properties.Length : 0)) //up-size the array to fit in more properties if the values are set through the inspector
			{
				m_so_properties.arraySize = values.Length;
			}

			for (int i = 0; i < m_so_properties.arraySize && i < values.Length; i++)
			{
				if (values[i] != null)
					m_so_properties.GetArrayElementAtIndex(i).floatValue = (float)values[i];
			}

			ApplyModifiedProperties();
#else

			for (int i = 0; i < values.Length; i++)
            {
				SetProperty(i, values[i] ?? 0);
            }
#endif
		}

		public float[] GetProperties()
		{
			return properties ?? new float[0];
		}

		public void SetProperty(int propertyIndex, float value)
		{
			if (IKPModule_EditorSimulation.isAnyInstancePlaying)
			{
				return;
			}

#if UNITY_EDITOR
			InitSerializedPropertiesIfNeeded(); //initialize the serialized properties
			ApplyModifiedProperties();
			serialized.Update();
			//if the variables are set through the inspector it is acceptable to resize the array of properties
			if (propertyIndex >= m_so_properties.arraySize)
				m_so_properties.arraySize = propertyIndex + 1;
			m_so_properties.GetArrayElementAtIndex(propertyIndex).floatValue = value;
			ApplyModifiedProperties();
#else
			if(properties == null)
            {
				properties = new float[0];
            }

			if(properties.Length <= propertyIndex)
            {
				Array.Resize(ref properties, propertyIndex + 1);
            }

			properties[propertyIndex] = value;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetProperty(int propertyIndex)
		{
			return (properties?.Length > propertyIndex) ? properties[propertyIndex] : 0;
		}

		/// <summary>
		/// Draw the numeric property field GUI for a single property at the given property ID
		/// </summary>
		protected void DrawPropertyGUI(int propertyId, bool rangeType = false)
		{
			DrawPropertyGUI(propertyId, rangeType, 0, 1);
		}
        
		/// <summary>
		/// Draw the numeric property field GUI for a single property at the given property ID
		/// </summary>
		protected void DrawPropertyGUI(int propertyId, bool rangeType, float minRange, float maxRange)
		{
#if UNITY_EDITOR
			EditorGUI.BeginDisabledGroup(IKPModule_EditorSimulation.isAnyInstancePlaying);
			{
				GUIContent _guiContent = new GUIContent(SETUtil.StringUtil.WordSplit(IKPUtils.GetPropertyName(this, propertyId)), "Property ID: " + propertyId);
				if (rangeType)
					SetProperty(propertyId, EditorGUILayout.Slider(_guiContent, GetProperty(propertyId), minRange, maxRange));
				else
					SetProperty(propertyId, EditorGUILayout.FloatField(_guiContent, GetProperty(propertyId)));
			}
			EditorGUI.EndDisabledGroup();
#endif
		}
    }
}
