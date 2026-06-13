using STSLite.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace STSLite.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIManager>();
                    if (_instance == null)
                    {
                        _instance = Resources.Load<UIManager>("Prefabs/UI/UIManager");
                    }

                    if (_instance == null)
                    {
                        throw new InvalidOperationException("UIManager instance not found.");
                    }
                }

                return _instance;
            }
        }

        private static UIManager _instance;

        private readonly Dictionary<Type, UIBase> _openedUis = new Dictionary<Type, UIBase>();

        public UIBlackScreen Transition { get; private set; }

        public T Show<T>() where T : UIBase
        {
            Type uiType = typeof(T);

            if (_openedUis.TryGetValue(uiType, out UIBase openedUi) && openedUi)
            {
                openedUi.gameObject.SetActive(true);
                openedUi.Resume();
                return (T)openedUi;
            }

            T createdUi = Create<T>();
            _openedUis[uiType] = createdUi;
            SetStaticInstance(createdUi);
            createdUi.Begin();
            createdUi.gameObject.SetActive(true);
            createdUi.Resume();
            return createdUi;
        }

        public void Hide<T>() where T : UIBase
        {
            Type uiType = typeof(T);

            if (_openedUis.TryGetValue(uiType, out UIBase openedUi) && openedUi)
            {
                openedUi.Pause();
                openedUi.gameObject.SetActive(false);
            }
        }

        public void Close<T>() where T : UIBase
        {
            Type uiType = typeof(T);

            if (!_openedUis.TryGetValue(uiType, out UIBase openedUi))
            {
                return;
            }

            _openedUis.Remove(uiType);

            if (openedUi)
            {
                openedUi.Pause();
                openedUi.Finish();
                ClearStaticInstance(openedUi);
                Destroy(openedUi.gameObject);
            }
        }

        private T Create<T>() where T : UIBase
        {
            Type uiType = typeof(T);

            if (uiType.IsAbstract)
            {
                throw new InvalidOperationException($"{uiType.Name} is abstract and cannot be created.");
            }

            UIInfo uiInfo = UIRegistry.GetUIInfo(uiType);

            if (string.IsNullOrEmpty(uiInfo.PrefabPath))
            {
                throw new InvalidOperationException($"{uiType.Name} requires a prefab path.");
            }

            GameObject prefab = LoadPrefab(uiInfo.PrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"UI prefab not found. Type: {uiType.Name}, Path: {uiInfo.PrefabPath}");
            }

            GameObject instance = Instantiate(prefab, transform);
            instance.name = prefab.name;

            T ui = instance.GetComponent<T>();

            if (ui == null)
            {
                Destroy(instance);
                throw new InvalidOperationException($"{prefab.name} does not have a {uiType.Name} component.");
            }

            return ui;
        }

        private GameObject LoadPrefab(string prefabPath)
        {
            return Resources.Load<GameObject>(prefabPath);
        }

        private void SetStaticInstance(UIBase ui)
        {
            PropertyInfo instanceProperty = GetInstanceProperty(ui.GetType());

            if (instanceProperty == null)
            {
                return;
            }

            instanceProperty.GetSetMethod(true).Invoke(null, new object[] { ui });

            if (ui is UIBlackScreen transition)
            {
                Transition = transition;
            }
        }

        private void ClearStaticInstance(UIBase ui)
        {
            PropertyInfo instanceProperty = GetInstanceProperty(ui.GetType());

            if (instanceProperty == null || !ReferenceEquals(instanceProperty.GetValue(null), ui))
            {
                return;
            }

            instanceProperty.GetSetMethod(true).Invoke(null, new object[] { null });

            if (ui is UIBlackScreen)
            {
                Transition = null;
            }
        }

        private PropertyInfo GetInstanceProperty(Type uiType)
        {
            PropertyInfo instanceProperty = uiType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

            if (instanceProperty == null || !instanceProperty.PropertyType.IsAssignableFrom(uiType))
            {
                return null;
            }

            MethodInfo setter = instanceProperty.GetSetMethod(true);

            if (setter == null || !setter.IsStatic)
            {
                return null;
            }

            return instanceProperty;
        }
    }
}