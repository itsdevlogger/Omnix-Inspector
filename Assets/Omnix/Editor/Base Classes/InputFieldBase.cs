using System;
using JetBrains.Annotations;
using Omnix.Core;
using Omnix.Data;
using Omnix.Preview;
using Omnix.Windows.SearchProviders;
using UnityEditor;
using UnityEngine;


namespace Omnix.InputFields
{
    /// <summary> Base class for all InputFields. </summary>
    public abstract class InputFieldBase : PropertyBase
    {
        /// <summary> Class Member (Property or Field) that is being drawn. </summary>
        public readonly ClassMember TargetMember;
        public override float GetDefaultHeight()
        {
            if (TargetMember.CanWrite)
                return EditorGUI.GetPropertyHeight(TargetMember.Serialized, TargetMember.Serialized.isExpanded);
            return EditorGUIUtility.singleLineHeight;
        }

        protected InputFieldBase(ClassMember targetMember, [NotNull] PropertyData myData, LayoutBase parent) : base(myData, parent)
        {
            TargetMember = targetMember;
            if (targetMember == null || targetMember.CanWrite) return;
            
            // If this code is being executed means either
            //     we dont have a target member
            //     or we can't change its value
            // In either case we dont want to check for OnValueChangedCallback 
            this.NoCheckChange = true;
        }

        public override void DrawOmnixInspector()
        {
            if (HelpersDraw.DropDrown("Input Field"))
            {
                DrawPropertySpecificInspector();
            }
            
            HelpersDraw.ContentFull(MyData);
            base.DrawOmnixInspector();
            base.DrawStyleSheet();
        }
        
        /// <summary> Draw "Input Field" menu in the Omnix Inspector </summary>
        public virtual void DrawPropertySpecificInspector()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Field");
                if (GUILayout.Button(MyData.target, EditorStyles.popup))
                {
                    SearchProviderOmnix.Create(this, SearchProviderOmnix.TreeType.PropertyTarget);
                }
            }
            EditorGUILayout.EndHorizontal();
            HelpersDraw.ValueChangedCallback(this, "On Value Changed");
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Change the target field of a pre-made InputField
        /// </summary>
        public static void ChangePropertyTarget(InputFieldBase property, string newTarget)
        {
            property.MyData.target = newTarget;

            PropertyBase newProp = NewProperty(property.MyData, property.Parent);
            if (!(newProp is InputFieldBase newField)) throw new Exception($"Something went wrong, asked for InputFieldBase got {newProp.GetType()} for {property.MyData.elementType}");
            if (property is IfUnknown)
            {
                property.MyData.content.text = newField.TargetMember.DisplayName;
                property.MyData.nameInKsHierarchy = newField.MyData.nameInKsHierarchy;
            }

            LayoutPreview parent = (LayoutPreview)property.Parent;
            int index = parent.IndexOf(property);
            parent.RemoveChild(property);
            parent.InsertChild(newField, index);
        }
    }
}