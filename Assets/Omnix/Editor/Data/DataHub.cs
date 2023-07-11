using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Omnix.Core;

namespace Omnix.Data
{
    /// <summary>
    /// Data holder class for all the layouts and all the elements
    /// </summary>
    public class DataHub : ScriptableObject
    {
        private static DataHub _instance;
        private static Dictionary<Type, ClassDrawerData> LayoutsMap = new Dictionary<Type, ClassDrawerData>();
        private static bool _hasInitialized = false;

        /// <summary>
        /// GUID of datahub object in the project.
        /// </summary>
        public static string Guid => "f96dd5a878f9bed43abb6600096d0200";
        
        public static OmnixInspectorSkin CurrentSkin => Instance.currentSkin;

        public static DataHub Instance
        {
            get
            {
                if (_instance == null) _instance = AssetDatabase.LoadAssetAtPath<DataHub>(AssetDatabase.GUIDToAssetPath(Guid));
                return _instance;
            }
        }

        [SerializeField] private OmnixInspectorSkin currentSkin;
        public List<ClassDrawerData> layouts;

        public static ClassDrawerData GetLayoutData(Type type)
        {
            if (Instance == null)
            {
                Debug.Log("Cannot find Omnix Editor Data. Did you delete the \"Layouts Data\" Scriptable?");
                return null;
            }

            if (!_hasInitialized) Init();
            return LayoutsMap.ContainsKey(type) ? LayoutsMap[type] : null;
        }
        
        [InitializeOnLoadMethod]
        public static void Init()
        {
            _hasInitialized = true;
            LayoutsMap.Clear();
            if (Instance.layouts == null) return;

            
            foreach (ClassDrawerData layout in _instance.layouts)
            {
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(layout.targetClassGuid));
                if (monoScript == null) continue;
                Type type = monoScript.GetClass();
                if (type != null) LayoutsMap.Add(type, layout);
            }
        }
        
        [ContextMenu("Refresh")]
        public void RefreshAll()
        {
            FirstLayoutWrapper.Current.FirstLayout.PrintInfo("");
        }
    }
}
