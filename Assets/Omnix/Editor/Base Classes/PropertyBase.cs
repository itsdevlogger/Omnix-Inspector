using System;
using System.Diagnostics.CodeAnalysis;
using Omnix.Data;
using UnityEditor;
using UnityEngine;

namespace Omnix.Core
{
    /// <summary>
    /// Base class for InputFields and Inert Elements (Button, Label, Spaced, HelpBox)
    /// </summary>
    public abstract class PropertyBase : ElementBase
    {
        /// <summary> Name that will be displayed when the property is not selected or not found </summary>
        public static readonly string UnknownPropertyName = "Unknown Property";

        public override sealed bool IsProperty => true;
        
        /// <returns>Height when user dont specify the height</returns>
        public abstract float GetDefaultHeight();

        public readonly PropertyData MyData;

        protected PropertyBase(PropertyData data, LayoutBase parent) : base(data, parent)
        {
            MyData = data;
        }

        public override void RefreshSize(float width)
        {
            float curW = MyData.widthType switch
            {
                SizeType.Auto => width,
                SizeType.Pixels => Mathf.Clamp(MyData.widthValue, 0, width),
                _ => throw new Exception($"Not implemented for height type {MyData.heightType}")
            };

            float curH = MyData.heightType switch
            {
                SizeType.Auto => GetDefaultHeight(),
                SizeType.Pixels => MyData.heightValue,
                _ => throw new Exception($"Not implemented for height type {MyData.heightType}")
            };
            
            TotalRect.width = curW - MyData.padding.horizontal;
            TotalRect.height = curH - MyData.padding.vertical;
        }

        public override void RefreshPosition(float x, float y)
        {
            TotalRect.x = x + MyData.padding.left;
            TotalRect.y = y + MyData.padding.top;
        }

        public override void PrintInfo(string indentLevel)
        {
            Debug.Log($"{indentLevel}{Data.elementType} {Data.GUID}");
        }

        /// <summary> Draw menu for Visibility and Placement in the omnix inspector </summary>
        public override void DrawOmnixInspector()
        {
            HelpersDraw.Visibility(this);

            if (HelpersDraw.DropDrown("Placement"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Width");
                MyData.widthType = (SizeType)EditorGUILayout.EnumPopup(MyData.widthType);
                if (MyData.widthType != SizeType.Auto) MyData.widthValue = EditorGUILayout.FloatField(MyData.widthValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Height");
                MyData.heightType = (SizeType)EditorGUILayout.EnumPopup(MyData.heightType);
                if (MyData.heightType != SizeType.Auto) MyData.heightValue = EditorGUILayout.FloatField(MyData.heightValue);
                EditorGUILayout.EndHorizontal();

                HelpersDraw.Paddings(MyData);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            }
        }

        /// <summary> Draw Style Sheet in the omnix inspector </summary>
        protected void DrawStyleSheet()
        {
            if (HelpersDraw.DropDrown("Style Sheet"))
            {
                EditorGUI.indentLevel++;
                HelpersDraw.DrawStyleSheet(MyData.styleSheet);
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary> Draw Style Sheet and guiStyle editor in the omnix inspector </summary>
        protected void DrawStyleSheet(GUIStyle guiStyle)
        {
            if (HelpersDraw.DropDrown("Style Sheet"))
            {
                EditorGUI.indentLevel++;
                HelpersDraw.DrawStyleSheet(MyData.styleSheet);
                if (MyData.styleSheet.hasCustomStyle) HelpersDraw.DrawGuiStyle(guiStyle);
                EditorGUI.indentLevel--;
            }
        }
    }
}