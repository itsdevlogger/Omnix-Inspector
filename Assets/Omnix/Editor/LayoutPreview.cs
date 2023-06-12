using System;
using System.Collections.Generic;
using System.Linq;
using Omnix.Core;
using Omnix.Data;
using Omnix.Windows.SearchProviders;
using UnityEditor;
using UnityEngine;

namespace Omnix.Preview
{
    public class LayoutPreview : LayoutBase
    {
        public override IEnumerable<ElementBase> Children => _children;
        public override int ChildCount => _children.Count;

        private readonly List<ElementBase> _children;
        
        public LayoutPreview(LayoutData layoutData, LayoutBase parent) : base(parent, layoutData)
        {
            _children = new List<ElementBase>(layoutData.children.Length);
            Helpers.OnSaveData += this.SaveCachedData;
            LayoutType = layoutData.elementType;

            foreach (string child in layoutData.children)
            {
                if (FirstLayoutWrapper.Current.DrawerData.TryGet(child, out DataClassBase childData))
                {
                    _children.Add(NewPreviewElement(childData, this, -2));
                }
            }
            
            DiscardChildren((ElementBase ele) => ele == null || !Drawer.AcceptsChild(ele.Data.elementType));
        }
        
        private void SaveCachedData(bool closingWindows)
        {
            if (closingWindows) return;
            MyData.children = new string[_children.Count];
            
            int i = 0;
            foreach (ElementBase child in _children)
            {
                MyData.children[i] = child.Data.GUID;
                i++;
            }
        }

        public void RemoveChild(ElementBase child)
        {
            if (child?.Data == null) return;

            _children.Remove(child);
            SetElementParent(child, null);
        }

        public void AppendChild(ElementBase child)
        {
            if (child == null) return;
            if (child.Data == null) return;
            if (!Drawer.AcceptsChild(child.Data.elementType)) return;

            _children.Add(child);
            SetElementParent(child, this);
        }

        /// <summary>
        /// Register an element as a child of this layout.
        /// </summary>
        /// <param name="child"> Element </param>
        /// <param name="index"> At what index this should be inserted. -1 means last, -2 means don't insert, 0 - inf means insert at given index</param>
        public void InsertChild(ElementBase child, int index)
        {
            if (index == -2) return;
            if (child == null) return;
            if (child.Data == null) return;
            if (!Drawer.AcceptsChild(child.Data.elementType)) return;

            if (index < 0 || ChildCount <= index) _children.Add(child);
            else _children.Insert(index, child);
            SetElementParent(child, this);
        }

        public void ChangeChildIndex(ElementBase child, int newIndex)
        {
            RemoveChild(child);
            InsertChild(child, newIndex);
        }

        public override void DrawOmnixInspector()
        {
            if (HelpersDraw.DropDrown("Layout"))
            {
                EditorGUI.indentLevel++;
                MyData.showHeader = EditorGUILayout.Toggle("Show Header", MyData.showHeader);
                int lt = (int)MyData.elementType;
                int nlt = EditorGUILayout.Popup(new GUIContent("Layout Type"), lt, HelpersDraw.LayoutTypes);
                if (lt != nlt) this.LayoutType = (ElementType)nlt;
                HelpersDraw.ValueChangedCallback(this, "On Value Changed");
                EditorGUI.indentLevel--;
            }
            
            HelpersDraw.Visibility(this);

            if (HelpersDraw.DropDrown("Placement"))
            {
                EditorGUI.indentLevel++;
                HelpersDraw.Paddings(this.MyData);
                EditorGUI.indentLevel--;
            }
            
            if (HelpersDraw.DropDrown("Style Sheet"))
            {
                EditorGUI.indentLevel++;
                HelpersDraw.DrawStyleSheet(MyData.styleSheet);
                EditorGUI.indentLevel--;
            }
            
            Rect rect = EditorGUILayout.GetControlRect(true, 30f);
            rect.x = (rect.width - 250f) * 0.5f;
            rect.width = 250f;
            if (GUI.Button(rect, "Add Child"))
            {
                SearchProviderOmnix.Create(this, -1);
            }
        }

        public bool AcceptsChild(ElementType elementType) => Drawer.AcceptsChild(elementType);
        public int IndexOf(ElementBase child) => _children.IndexOf(child);
        public void DiscardChildren(Predicate<ElementBase> validator) => _children.RemoveAll(validator);
        private static void SetElementParent(ElementBase element, LayoutBase parent) => element.GetType().GetField("Parent").SetValue(element, parent);
        
        internal void SaveDataRecursive(ref List<LayoutData> layouts, ref List<PropertyData> properties)
        {
            int index = 0;
            foreach (ElementBase element in _children)
            {
                if (element.Data == null)
                {
                    if (element.Parent == null) Debug.Log($"Data null for base layout");
                    else Debug.Log($"Null data for {index}th child of {element.Parent}");
                    continue;
                }
                index++;
            }
            
            
            MyData.children = new string[index];
            index = 0;
            foreach (ElementBase element in _children)
            {
                if (element.Data == null) continue;
                
                if (element.IsProperty)
                {
                    PropertyBase property = (PropertyBase)element;
                    properties.Add(property.MyData);
                }
                else
                {
                    LayoutPreview layout = (LayoutPreview)element;
                    layouts.Add(layout.MyData);
                    layout.SaveDataRecursive(ref layouts, ref properties);
                }
                MyData.children[index] = element.Data.GUID;
                index++;
            }
        }
    }
}