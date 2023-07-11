using Omnix.Data;
using System;
using UnityEditor;
using UnityEngine;


namespace Omnix.Core
{
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class OmnixEditor : Editor
    {
        Func<bool> DrawCallback;
        private FirstLayoutWrapper _layoutWrapper;

        private void OnEnable()
        {
            ClassDrawerData editorData = DataHub.GetLayoutData(target.GetType());
            if (editorData == null)
            {
                DrawCallback = base.DrawDefaultInspector;
                return;
            }
            
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(editorData.targetClassGuid));
            if (script == null || script.GetClass() != target.GetType())
            {
                DrawCallback = base.DrawDefaultInspector;
                return;
            }
            
            editorData.UpdateMap();
            _layoutWrapper = new FirstLayoutWrapper(editorData, script, target, ClassMember.GetMembersDictionary(serializedObject), false);
            DrawCallback = DrawOmnix;
        }

        public override void OnInspectorGUI()
        {
            DrawCallback.Invoke(); 
        }

        /// <summary>
        /// Draw Omnix Inspector.
        /// </summary>
        /// <returns> true </returns>
        private bool DrawOmnix()
        {
            if (OmnixEditorContext.IsEditing)
            {
                EditorGUILayout.LabelField("Cannot draw editor while Preview Window is opened.", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("If you've closed the Preview Window, then Reselect this object.", EditorStyles.boldLabel);
                DrawDefaultInspector();
                return true;
            }

            FirstLayoutWrapper.Current = _layoutWrapper;
            if (_layoutWrapper.OnGUI())
            {
                serializedObject.ApplyModifiedProperties();
            }
            return true;
        }
    }


    /*[CustomPropertyDrawer(typeof(OmnixMonoBehaviour), useForChildren: true)]
    public class OmnixDrawer : PropertyDrawer
    {
        private Dictionary<string, SerializedProperty> _properties = new Dictionary<string, SerializedProperty>();
        private Dictionary<string, MasterLayout> _stashedLayouts;
        private LayoutData _layoutData;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_layoutData == null)
            {
                if (!DataHub.TryGetLayoutData(property.type, out _layoutData))
                {
                    return EditorGUIUtility.singleLineHeight * 4f;
                }
            }

            if (_stashedLayouts == null)
            {
                _stashedLayouts = new Dictionary<string, MasterLayout>();
            }


            if (_stashedLayouts.ContainsKey(property.propertyPath))
            {
                return EditorGUIUtility.singleLineHeight + _stashedLayouts[property.propertyPath].CurrentHeight;
            }

            UpdateChildProperties(property);
            MasterLayout layout = new MasterLayout(_layoutData, new Getters(_properties, property.serializedObject.targetObject));
            layout.RefreshSize(EditorGUIUtility.currentViewWidth);
            _stashedLayouts.Add(property.propertyPath, layout);
            return EditorGUIUtility.singleLineHeight + layout.CurrentHeight;
        }

        private void UpdateChildProperties(SerializedProperty property)
        {
            _properties.Clear();

            SerializedProperty itterator = property.Copy();
            if (property.Next(true))
            {
                _properties.Add(itterator.name, itterator.Copy());
            }
            
            while (property.Next(false))
            {
                _properties.Add(itterator.name, itterator.Copy());
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label);

            rect.y += rect.height;
            rect.height = position.height - rect.height;
            if (_stashedLayouts.TryGetValue(property.propertyPath, out MasterLayout layout))
            {
                layout.Draw(rect);
            }
            else
            {
                rect.height *= 0.666f;
                EditorGUI.HelpBox(rect, "Cannot find layout, maybe because the class was renamed. Select new name below.", MessageType.Error);
                rect.y += rect.height;
                rect.height *= 0.5f;
                EditorGUI.LabelField(rect, "Not Implemented yet");
                throw new NotImplementedException();
            }
        }
    }*/




}