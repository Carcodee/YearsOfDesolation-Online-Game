using IKPn.Blend;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IKPn
{
    // Public interface to IKP
    public partial class IKP
    {
        public int GetModulesCount()
        {
            return moduleInstancesData.Count;
        }

        public void SetActive(bool state)
        {
            enabled = state;
        }

        /// <summary>
        /// Set specific property value by index. 
        /// </summary>
        public void SetProperty(int property, float value, string moduleSignature)
        {
            TryDoForModulePropertiesInternal(moduleSignature, (m) => m.SetProperty(property, value));
        }

        /// <summary>
        /// Set properties bulk.
        /// </summary>
        public void SetProperties(float?[] values, string moduleSignature)
        {
            TryDoForModulePropertiesInternal(moduleSignature, (m) => m.SetProperties(values));
        }

        /// <summary>
        /// Set properties from blend machine.
        /// </summary>
        public void SetProperties(BlendAnimationValue val, string moduleSignature)
        {
            SetProperties(val.properties, moduleSignature);
            if (val.toggle != null)
            {
                ToggleModule(moduleSignature, (bool)val.toggle);
            }
        }

        public float GetProperty(int propertyIndex, string moduleSignature)
        {
            if (!HasModule(moduleSignature))
            {
                return 0;
            }

            ModuleBase_PropertiesProvider _m = null;

            if ((_m = GetModule(moduleSignature) as ModuleBase_PropertiesProvider) != null)
            {
                return _m.GetProperty(propertyIndex);
            }
            else
            {
                Debug.LogError($"[ERROR] Could cast module {moduleSignature} to {nameof(ModuleBase_PropertiesProvider)}");
                return 0;
            }
        }

        public void ToggleAllModules(bool state)
        {
            if (moduleInstancesData != null)
            {
                foreach (ModuleInstanceData m in moduleInstancesData)
                {
                    if (m.module != null)
                    {
                        m.module.SetActive(state);
                    }
                }
            }
        }

        /// <summary>
        /// Enable / disable a module by signature. Optionally you can set the desired state.
        /// </summary>
        public void ToggleModule(string moduleSignature, bool? state = null)
        {

            IKPModule _im;
            if ((_im = GetModule(moduleSignature)) != null)
            {
                _im.SetActive(state ?? !_im.IsActive());
            }
#if UNITY_EDITOR
            else if (!Application.isPlaying)
            {
                AddModule(moduleSignature);
            }
            else
            {
                Debug.LogError("Trying to add a module while the game is running!");
            }
#endif
        }

        /// <summary>
        /// Delete a module from the object
        /// </summary>
        public void RemoveModule(string signature)
        {
            if (!HasModule(signature))
            {
                return;
            }

            var module = GetModule(signature);
            SETUtil.SceneUtil.SmartDestroy(module);
            moduleInstancesData.RemoveAll(a => a == null || !a.valid);

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
        }

        public bool HasModule(string moduleSignature)
        {
            return GetModule(moduleSignature) != null;
        }

        public IKPModule GetModule(string moduleSignature)
        {
            var _moduleInstance = moduleInstancesData.FirstOrDefault(a => a.signature == moduleSignature)?.module;

            if (_moduleInstance == null)
            {
                // Maybe instanceData still hasn't been populated, so attempt to get it through GetCompoennt
                // This is a bit of a hack, maybe have this trigger full module instance data initialization
                foreach (var _module in GetComponents<IKPModule>())
                {
                    if (ModuleManager.TypeToModuleLinker(_module.GetType()).signature == moduleSignature)
                    {
                        AttachNewModule_Internal(_module, moduleSignature);
                        _moduleInstance = _module;
                        break;
                    }
                }
            }
            return _moduleInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModule<T>(string moduleSignature) where T : IKPModule
        {
            return (T)GetModule(moduleSignature);
        }

        /// <summary>
        /// Registers a module into the list of ikp module instances
        /// </summary>
        public void Attach(IKPModule moduleComponent)
        {
            var _linker = ModuleManager.TypeToModuleLinker(moduleComponent.GetType());

            if (_linker == null)
            {
                throw new Exception(string.Format("Module linker at {0} is NULL", moduleComponent.name));
            }

            var _signature = _linker.signature;

            if (!HasModule(_signature))
            {
                AttachNewModule_Internal(moduleComponent, _signature);
            }
            else
            {
                // restore linking
                var _instanceData = moduleInstancesData.First(a => a.signature == _signature);
                if (_instanceData.module != moduleComponent)
                {
                    // only one module of certain signature / type is allowed per ikp instance
                    SETUtil.SceneUtil.SmartDestroy(moduleComponent);

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif

                    throw new Exception("Trying to add multiple modules of the same type. They either share the same signature or the same object type! Removed redundant module component.");
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        private void AttachNewModule_Internal(IKPModule moduleComponent, string signature)
        {
            var _existingModuleInstanceData = moduleInstancesData.FirstOrDefault(a => a.signature == signature);

            if (_existingModuleInstanceData != null && _existingModuleInstanceData.module == null)
            {
                // restoring the linking prevents multiple module instanc data entries pointing to the same component
                _existingModuleInstanceData.module = moduleComponent;
            }
            else
            {
                moduleInstancesData.Add(new ModuleInstanceData()
                {
                    module = moduleComponent,
                    signature = signature,
                });
            }

            // sort by execution order
            moduleInstancesData.Sort((a, b) => ModuleManager.GetUpdateOrder(a.signature).CompareTo(ModuleManager.GetUpdateOrder(b.signature)));

        }

        /// <summary>
        /// Remove from the list of module instances
        /// </summary>
        public void Detach(IKPModule moduleComponent)
        {
            var _signature = ModuleManager.TypeToModuleLinker(moduleComponent.GetType()).signature;
            moduleInstancesData.Remove(moduleInstancesData.FirstOrDefault(a => a.signature == _signature));

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }




        /// <summary>
        /// Generate bone rotation offsets, etc automatiaclly.
        /// If no params are provided, the operation runs for all available modules.
        /// </summary>
        public void Init()
        {
            m_origin = origin;

            // fix off-screen jitter because of the culled animations
            var _animatorComponent = GetComponent<Animator>();
            if (_animatorComponent)
            {
                _animatorComponent.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            var _allModuleSignatures = ModuleManager.GetSignatures();

            foreach (var _moduleSignature in _allModuleSignatures)
            {
                if (HasModule(_moduleSignature))
                {
                    var _module = GetModule(_moduleSignature);
                    if (_module.Validate())
                    {
                        // Note: if a module doesn't have proper setup when this Init is called, it will have to be called 'manually' later on.
                        SetSetupInProgress(true);
                        _module.Init(origin);
                        SetSetupInProgress(false);
                    }
                }
            }

            foreach(var child in ikChildren)
            {
                child.manuallyUpdated = true;
            }
        }

        /// <summary>
        /// Check the specified modules if they have been properly initialized.
        /// If no params are provided, all available modules are checked;
        /// </summary>
        public bool IsConfigured(List<ValidationResult> validaitonResults = null, params string[] moduleSignatures)
        {
            foreach (var _moduleSignature in (moduleSignatures.Length == 0 ? moduleInstancesData.Select(a => a.signature) : moduleSignatures))
            {
                if (HasModule(_moduleSignature))
                {
                    MODULE_VALIDATE_RESULTS_BUFFER.Clear();
                    if (!GetModule(_moduleSignature).Validate(validaitonResults ?? MODULE_VALIDATE_RESULTS_BUFFER))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
