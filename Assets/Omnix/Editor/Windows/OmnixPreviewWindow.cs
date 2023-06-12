using System;
using System.Collections.Generic;
using Omnix.Core;
using Omnix.Data;
using Omnix.Preview;
using Omnix.Windows.SearchProviders;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Omnix.Windows
{
    public class OmnixPreviewWindow : OmnixWindowBase
    {
        private readonly Color _hoveColor = new Color(0.35f, 0.64f, 0.64f, 0.44f);

        private Action _drawAction;
        private static SerializedObject _serializedObject;
        
        public void ObjectDropped() => _drawAction = DrawDefault;
        
        private void OnEnable()
        {
            _drawAction = DrawDefault;
        }
        
        private void OnDisable()
        {
            FirstLayoutWrapper.Current.SaveData();
            
            DataHub.Init();
            SearchProviderOmnix.ClearTrees();
            Actives.CloseAll(this);
            Helpers.ClearCallbacks();
            OmnixEditorContext.DestroyCurrent();
            _serializedObject = null;
        }
        
        private void OnGUI()
        {
            _drawAction();
        }
        
        public static void TryInit(MonoScript script)
        {
            GameObject editorContext = OmnixEditorContext.GetNew().gameObject;
            
            Type classType = script.GetClass();
            Component component = editorContext.AddComponent(classType);
            _serializedObject = new SerializedObject(component);
            
            DataHub.Init();
            ClassDrawerData editorData = DataHub.GetLayoutData(script.GetClass());
            if (editorData == null)
            {
                string guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(script)).ToString();
                LayoutData firstLayout = new LayoutData("Root", ElementType.VerticalLayout);
                editorData = new ClassDrawerData(guid) { firstLayout = firstLayout };
                editorData.Register(firstLayout);
                DataHub.Instance.layouts ??= new List<ClassDrawerData>();
                DataHub.Instance.layouts.Add(editorData);
            }
            
            editorData.UpdateMap();
            FirstLayoutWrapper layoutWrapper = new FirstLayoutWrapper(editorData, script, component, ClassMember.GetMembersDictionary(_serializedObject), true);
            SearchProviderOmnix.PopulateAllTrees(classType);
            layoutWrapper.RefreshLayouts();
        }
        
        private void DrawDefault()
        {
            if (FirstLayoutWrapper.Current.OnGUI())
            {
                _serializedObject.ApplyModifiedProperties();
            }
            
            Event current = Event.current;
            if (!current.control && !current.command) return;
            DrawQuickSelect(current);
            Repaint();
        }
        
        private void DrawQuickSelect(Event current)
        {
            ElementBase element;
            element = GetElementAt((LayoutPreview)FirstLayoutWrapper.Current.FirstLayout, current.mousePosition, wantProperty: !current.shift);
            
            if (element == null)
            {
                if (!current.shift) return;
                element = FirstLayoutWrapper.Current.FirstLayout;
            }

            Color color = GUI.color;
            GUI.color = _hoveColor;
            GUI.DrawTexture(element.TotalRect, EditorGUIUtility.whiteTexture);
            GUI.color = color;
            OmnixHierarchyWindow.SelectElement(element);
        }
        
        private static ElementBase GetElementAt(LayoutPreview layout, Vector2 mousePos, bool wantProperty)
        {
            ElementBase currentCandidate = null;

            foreach (ElementBase child in layout.Children)
            {
                if (child.IsProperty == wantProperty && child.TotalRect.Contains(mousePos)) currentCandidate = child;
                if (!(child is LayoutPreview childLayout)) continue;
                
                ElementBase elementBase = GetElementAt(childLayout, mousePos, wantProperty);
                if (elementBase != null) currentCandidate = elementBase;
            }
            return currentCandidate;
        }
        
    }
}