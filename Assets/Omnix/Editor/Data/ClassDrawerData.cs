using System;
using System.Collections.Generic;
using Omnix.Core;
using UnityEngine;
namespace Omnix.Data
{
    /// <summary>
    /// Data class for an Layout of a MonoBehaviour class.
    /// </summary>
    [Serializable]
    public class ClassDrawerData
    {
        public bool showScriptPickup;
        public string targetClassGuid;
        public LayoutData firstLayout;
        [SerializeField] private LayoutData[] layouts;
        [SerializeField] private PropertyData[] properties;

        [NonSerialized] private Dictionary<string, DataClassBase> _dataMap;
        [NonSerialized] private Dictionary<string, DataClassBase> _tempDataMap;
        public ClassDrawerData(string targetClassGuid)
        {
            this.showScriptPickup = true;
            this.targetClassGuid = targetClassGuid;
            this.layouts = Array.Empty<LayoutData>();
            this.properties = Array.Empty<PropertyData>();
            _dataMap = new Dictionary<string, DataClassBase>();
            _tempDataMap = new Dictionary<string, DataClassBase>();
        }

        /// <summary>
        /// Try get data from an Element GUID
        /// </summary>
        /// <returns> true if the data is found, false otherwise. </returns>
        public bool TryGet(string elementGuid, out DataClassBase data)
        {
            data = null;
            if (string.IsNullOrEmpty(elementGuid)) return false;
            
            if (_dataMap.ContainsKey(elementGuid)) data = _dataMap[elementGuid];
            else if (_tempDataMap.ContainsKey(elementGuid)) data = _tempDataMap[elementGuid];

            return data != null;
        }

        /// <summary>
        /// Update the Dictionaries holding Data for all the elements
        /// </summary>
        public void UpdateMap()
        {
            if (_dataMap == null) _dataMap = new Dictionary<string, DataClassBase>();
            if (_tempDataMap == null) _tempDataMap = new Dictionary<string, DataClassBase>();
            
            foreach (LayoutData layout in layouts)
            {
                if (!_dataMap.ContainsKey(layout.GUID))
                {
                    _dataMap.Add(layout.GUID, layout);
                }
            }

            foreach (PropertyData property in properties)
            {
                if (!_dataMap.ContainsKey(property.GUID))
                {
                    _dataMap.Add(property.GUID, property);
                }
            }
        }

        /// <summary>
        /// Update the data for this Layout
        /// </summary>
        public void Reset(LayoutData theFirstLayout, LayoutData[] allLayouts, PropertyData[] allProperties)
        {
            this.firstLayout = theFirstLayout;
            this.layouts = allLayouts;
            this.properties = allProperties;
            UpdateMap();
        }

        /// <summary>
        /// Register an element in this Layout
        /// </summary>
        public void Register(DataClassBase dataBase)
        {
            if (_tempDataMap == null)
            {
                _tempDataMap = new Dictionary<string, DataClassBase>();
            }
            _tempDataMap.Add(dataBase.GUID, dataBase);
            Helpers.OnSaveData += SaveCachedData;
        }

        /// <summary>
        /// Save the data for thislayout to the DataHub
        /// </summary>
        /// <param name="closingAllWindows"></param>
        private void SaveCachedData(bool closingAllWindows)
        {
            if (closingAllWindows) return;

            Helpers.OnSaveData -= SaveCachedData;
            int maxCap = _dataMap.Count + _tempDataMap.Count;
            List<LayoutData> layoutsTemp = new List<LayoutData>(maxCap);
            List<PropertyData> propertiesTemp = new List<PropertyData>(maxCap);

            foreach (LayoutData layout in layouts)
            {
                layoutsTemp.Add(layout);
            }

            foreach (PropertyData property in properties)
            {
                propertiesTemp.Add(property);
            }


            foreach (KeyValuePair<string, DataClassBase> pair in _dataMap)
            {
                switch (pair.Value)
                {
                    case LayoutData ld:
                        layoutsTemp.Add(ld);
                        break;
                    case PropertyData pd:
                        propertiesTemp.Add(pd);
                        break;
                }
            }

            foreach (KeyValuePair<string, DataClassBase> pair in _tempDataMap)
            {
                switch (pair.Value)
                {
                    case LayoutData ld:
                        layoutsTemp.Add(ld);
                        break;
                    case PropertyData pd:
                        propertiesTemp.Add(pd);
                        break;
                }
            }

            layouts = layoutsTemp.ToArray();
            properties = propertiesTemp.ToArray();
            _dataMap.Clear();
            _tempDataMap.Clear();
            UpdateMap();
        }
    }
}