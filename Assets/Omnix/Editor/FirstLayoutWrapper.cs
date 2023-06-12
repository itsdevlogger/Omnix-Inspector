using System;
using System.Collections.Generic;
using System.Reflection;
using Omnix.Data;
using Omnix.Preview;
using UnityEditor;
using UnityEngine;

namespace Omnix.Core
{
    /// <summary>
    /// Wrapper for the Layout of a Class
    /// </summary>
    public class FirstLayoutWrapper
    {
        public static FirstLayoutWrapper Current;
        
        private readonly Dictionary<int, ElementBase> _allElements;
        public MonoScript MonoScript;
        public readonly LayoutBase FirstLayout;
        public readonly ClassDrawerData DrawerData;
        public readonly Dictionary<string, ClassMember> Serialized;
        public readonly object TargetComponent;
        public readonly Type TargetType;

        private int _currentIDCounter;

        public int GetUniqueElementID(ElementBase element)
        {
            _currentIDCounter++;
            _allElements.Add(_currentIDCounter, element);
            return _currentIDCounter;
        }

        public FirstLayoutWrapper(ClassDrawerData drawerData, MonoScript monoScript, object targetComponent, Dictionary<string, ClassMember> serialized, bool isPreview)
        {
            Current = this;
            _allElements = new Dictionary<int, ElementBase>();
            DrawerData = drawerData;
            MonoScript = monoScript;
            Serialized = serialized;
            TargetComponent = targetComponent;

            _currentIDCounter = 0;
            TargetType = targetComponent.GetType();

            if (isPreview) FirstLayout = (LayoutBase)ElementBase.NewPreviewElement(drawerData.firstLayout, null);
            else FirstLayout = (LayoutBase)ElementBase.NewUIElement(drawerData.firstLayout, null);
        }

        /// <summary>
        /// Update the size and position of all the children
        /// </summary>
        public void RefreshLayouts()
        {

            foreach (KeyValuePair<int, ElementBase> element in _allElements)
            {
                element.Value.IsHidden = element.Value.IsHiddenCheck();
            }

            FirstLayout.RefreshSize(FirstLayout.TotalRect.width);
            FirstLayout.RefreshPosition(FirstLayout.TotalRect.x, FirstLayout.TotalRect.y);
        }

        /// <returns>has anything changed</returns>
        public bool OnGUI()
        {
            if (DrawerData.showScriptPickup)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Script", MonoScript, typeof(MonoScript), allowSceneObjects: false);
                GUI.enabled = true;
            }

            Rect totalRect = EditorGUILayout.GetControlRect(false, FirstLayout.TotalRect.height);
            if (totalRect.width > 1 && totalRect.AreDifferent(FirstLayout.TotalRect))
            {
                FirstLayout.TotalRect = totalRect;
                RefreshLayouts();
            }

            EditorGUI.BeginChangeCheck();
            FirstLayout.Draw();
            if (EditorGUI.EndChangeCheck())
            {
                FirstLayout.TotalRect = totalRect;
                Helpers.RepaintAllWindows();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get action to call a method with given name on currently inspecting object
        /// </summary>
        public Action Method(string name)
        {
            if (string.IsNullOrEmpty(name) || name == DataClassBase.CallbackTargetForNone) return null;
            
            foreach (MethodInfo methodInfo in TargetType.GetMethods(Helpers.Flags))
            {
                if (methodInfo.Name != name || methodInfo.GetParameters().Length != 0) continue;
                return () => methodInfo.Invoke(TargetComponent, null);
            }
            return null;
        }

        /// <summary>
        /// Get action to call a method with given name on currently inspecting object
        /// </summary>
        public Func<T> MethodWithReturnType<T>(string name)
        {
            Type type = typeof(T);
            foreach (MethodInfo methodInfo in TargetType.GetMethods(Helpers.Flags))
            {
                if (methodInfo.Name != name || methodInfo.GetParameters().Length != 0 || methodInfo.ReturnType != type) continue;
                return () => (T)methodInfo.Invoke(TargetComponent, null);
            }
            return null;
        }

        /// <summary>
        /// Get Property of specific name
        /// </summary>
        public PropertyInfo Property(string name)
        {

            foreach (PropertyInfo propertyInfo in TargetType.GetProperties(Helpers.Flags))
            {
                if (propertyInfo.Name == name)
                {
                    return propertyInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// Get Field of specific name
        /// </summary>
        public FieldInfo Field(string name)
        {
            foreach (FieldInfo fieldInfo in TargetType.GetFields(Helpers.Flags))
            {
                if (fieldInfo.Name == name)
                {
                    return fieldInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// Get function to check if the object is Hidden
        /// </summary>
        /// <param name="data"> Data of the object for which the fucntion is needed </param>
        public Func<bool> GetIsHiddenCheckerFunction(DataClassBase data)
        {
            switch (data.showIfTargetType)
            {
                case ShowIfTargetType.AlwaysShow: return () => false;
                case ShowIfTargetType.AlwaysHide: return () => true;
                case ShowIfTargetType.Method: return this.MethodWithReturnType<bool>(data.showIfTarget);

                case ShowIfTargetType.Property:
                {
                    PropertyInfo property = this.Property(data.showIfTarget);
                    if (property == null) return () => false;
                    if (property.PropertyType != typeof(bool)) return () => false;
                    return () => !(bool)property.GetValue(this.TargetComponent);
                }

                case ShowIfTargetType.Field:
                {
                    FieldInfo field = this.Field(data.showIfTarget);
                    if (field == null) return () => false;
                    if (field.FieldType != typeof(bool)) return () => false;
                    return () => !(bool)field.GetValue(this.TargetComponent);
                }
                default: return () => false;
            }
        }
        
        /// <summary>
        /// Save all the data to DataHub
        /// </summary>
        public void SaveData()
        {
            Helpers.CallSaveData(closingAllWindows: true);
            List<LayoutData> layouts = new List<LayoutData>();
            List<PropertyData> properties = new List<PropertyData>();
            ((LayoutPreview)FirstLayout).SaveDataRecursive(ref layouts, ref properties);
            DrawerData.Reset(FirstLayout.MyData, layouts.ToArray(), properties.ToArray());
        }

        /// <summary>
        /// Get the element from given GUID
        /// </summary>
        public static ElementBase GetElementFromId(int id) => Current._allElements[id];
    }
}