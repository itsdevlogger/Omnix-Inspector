using System;
using Omnix.Data;
using UnityEditor;
using UnityEngine;

namespace Omnix.Windows
{
    public class OmnixElementsWindow : OmnixWindowBase
    {
        public static event Action<ElementType> OnDragStart; 
        
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            CreateDraggable(ElementType.Button, "Button");
            CreateDraggable(ElementType.Label, "Label");
            CreateDraggable(ElementType.InputField, "Property");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            CreateDraggable(ElementType.HelpBox, "Help Box");
            CreateDraggable(ElementType.Spacer, "Spacer");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            CreateDraggable(ElementType.VerticalLayout, "Vertical");
            CreateDraggable(ElementType.HorizontalLayout, "Horizontal");
            CreateDraggable(ElementType.PagedLayout, "Paged");
            EditorGUILayout.EndHorizontal();
        }

        private void CreateDraggable(ElementType elementType, string displayName)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 50f);
            DataHub.CurrentSkin.draggable.StartArea(rect);
            GUI.Label(rect, displayName);
            DataHub.CurrentSkin.draggable.EndArea();

            Event current = Event.current;
            if (rect.Contains(current.mousePosition) && current.isMouse && current.button == 0 && current.type == EventType.MouseDown)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("OmnixET", elementType);
                DragAndDrop.StartDrag(elementType.ToString());
                OnDragStart?.Invoke(elementType);
            }
            Repaint();
        }
    }
}