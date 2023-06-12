using Omnix.Core;
using Omnix.Data;
using Omnix.Preview;
using UnityEditor;
using UnityEngine;

namespace Omnix.Windows
{
    public class OmnixInspectorWindow : OmnixWindowBase
    {
        private Vector2 _scrollPos;
        private ElementType _currentElementType;
        
        private void OnEnable()
        {
            if (Actives.Preview == null)
            {
                this.Close();
                return;
            }
            
            this.minSize = new Vector2(510f, 510f);
            HelpersDraw.PrepareDropDowns(_currentElementType);
        }

        private void OnGUI()
        {
            if (Actives.Hierarchy.HasMultipleSelected)
            {
                DrawMultiSelect();
                return;
            }

            ElementBase element = Actives.Hierarchy.SelectedElement;
            if (element == null)
            {
                HelpersDraw.ScriptPickupToggle();
                return;
            }
            
            if (element.Data.elementType != _currentElementType)
            {
                _currentElementType = element.Data.elementType;
                HelpersDraw.PrepareDropDowns(_currentElementType);
            }
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, alwaysShowHorizontal: false, alwaysShowVertical: false);
            {
                EditorGUI.BeginChangeCheck();
                {
                    element.Data.nameInKsHierarchy = EditorGUILayout.TextField(element.Data.nameInKsHierarchy);
                    HelpersDraw.ResetDdIndex();
                    element.DrawOmnixInspector();
                    HelpersDraw.ScriptPickupToggle();
                }
                if (EditorGUI.EndChangeCheck()) Helpers.RepaintAllWindows();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawMultiSelect()
        {
            EditorGUILayout.LabelField("Support for Multi-Elements editing is coming soon...");

            if (!GUILayout.Button("Delete All")) return;

            foreach (ElementBase selEl in Actives.Hierarchy.LoopSelection())
            {
                if (selEl.Parent != null)
                    ((LayoutPreview)selEl.Parent).RemoveChild(selEl);
            }

            OmnixHierarchyWindow.SelectElement(null);
            Helpers.RepaintAllWindows();
        }

        private void DrawPropertyInspector()
        {
            /*PropertyBase propertyBase = (PropertyBase)_currentElement;
            PropertyData proData = propertyBase.MyData;

            switch (proData.elementType)
            {
                case ElementType.InputField:
                {
                    if (DropDrown("Property", ref _dd0))
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Field");
                        if (GUILayout.Button(proData.target, EditorStyles.popup)) 
                            SearchProviderOmnix.Create(propertyBase, SearchProviderOmnix.TreeType.PropertyTarget);
                        EditorGUILayout.EndHorizontal();
                        Draw_ValueChangedCallback("On Value Changed");
                        if (propertyBase is InputFieldBase fieldBase)
                            fieldBase.DrawPropertySpecificInspector();
                        EditorGUI.indentLevel--;
                    }
                    break;
                }
                case ElementType.Button:
                    Draw_ValueChangedCallback("On Clicked");
                    break;
                case ElementType.HelpBox:
                {
                    
                    break;
                }
            }*/
            
            /*if (DropDrown("Content", ref _dd1))
            {
                EditorGUI.indentLevel++;
                switch (proData.elementType)
                {
                    /*case ElementType.InputField:
                    case ElementType.Label:
                    case ElementType.Button:
                    {
                        Draw_ContentFull(proData);
                        break;
                    }#1#
                    case ElementType.Spacer:
                    {
                        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                        break;
                    }
                    case ElementType.HelpBox:
                    {
                        
                        break;
                    }
                    
                }
                EditorGUI.indentLevel--;
                
            }*/
        }
    }
}