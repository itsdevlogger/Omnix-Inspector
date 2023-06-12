using System;
using System.Diagnostics.CodeAnalysis;
using Omnix.Core;
using Omnix.Data;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Omnix.InputFields
{
    /// <summary>
    /// Number clasd. float, int, double, long
    /// </summary>
    public class IfNumber : InputFieldBase
    {
        /// <summary>
        /// How to draw this Input Field
        /// </summary>
        private static readonly GUIContent[] DrawerStyles = new GUIContent[]
        {
            new GUIContent("Default"), 
            new GUIContent("RangeSlider"),
        };
        
        private readonly ExtraData _extraData;
        

        public IfNumber(ClassMember targetMember, PropertyData myData, LayoutBase parent) : base(targetMember, myData, parent)
        {
            try
            {
                _extraData = JsonConvert.DeserializeObject<ExtraData>(MyData.extraData);
            }
            catch { }

            if (_extraData == null)
            {
                _extraData = new ExtraData()
                {
                    drawerStyle = 0,
                    sliderMinF = 0f,
                    sliderMaxF = 1f,
                    sliderMinI = 0,
                    sliderMaxI = 1,
                };
            }
            Helpers.OnSaveData += (_) => MyData.extraData = JsonConvert.SerializeObject(_extraData);
            DrawAction = GetDrawer();
        }

        private Action GetDrawer()
        {
            Type valueType = TargetMember.ValueType;
            switch (_extraData.drawerStyle)
            {
                case 0:
                {
                    if (valueType == typeof(float)) return () => TargetMember.floatValue = EditorGUI.FloatField(TotalRect, MyData.content, TargetMember.floatValue);
                    if (valueType == typeof(int)) return () => TargetMember.intValue = EditorGUI.IntField(TotalRect, MyData.content, TargetMember.intValue);
                    break;
                }

                case 1:
                {
                    if (valueType == typeof(float)) return () => TargetMember.floatValue = EditorGUI.Slider(TotalRect, MyData.content, TargetMember.floatValue, _extraData.sliderMinF, _extraData.sliderMaxF);
                    if (valueType == typeof(int)) return () => TargetMember.intValue   = EditorGUI.IntSlider(TotalRect, MyData.content, TargetMember.intValue, _extraData.sliderMinI, _extraData.sliderMaxI);
                    break;
                }
            }

            throw new Exception($"Not implemented Drawer Style {_extraData.drawerStyle} for {valueType}");
        }

        public override void DrawPropertySpecificInspector()
        {
            base.DrawPropertySpecificInspector();

            EditorGUI.indentLevel++;
            int style = EditorGUILayout.Popup(new GUIContent("Draw Mode"), _extraData.drawerStyle, DrawerStyles);
            if (style != _extraData.drawerStyle)
            {
                _extraData.drawerStyle = style;
                DrawAction = GetDrawer();
            }

            if (style == 1)
            {
                EditorGUI.indentLevel++;
                if (TargetMember.ValueType == typeof(float))
                {
                    _extraData.sliderMinF = EditorGUILayout.FloatField("Lower Bound", _extraData.sliderMinF);
                    _extraData.sliderMaxF = EditorGUILayout.FloatField("Upper Bound", _extraData.sliderMaxF);
                }
                else
                {
                    _extraData.sliderMinI = EditorGUILayout.IntField("Lower Bound", _extraData.sliderMinI);
                    _extraData.sliderMaxI = EditorGUILayout.IntField("Upper Bound", _extraData.sliderMaxI);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        public override float GetDefaultHeight() => EditorGUIUtility.singleLineHeight;
        
        [Serializable]
        private class ExtraData
        {
            public int drawerStyle;
            public float sliderMinF;
            public float sliderMaxF;
            public int sliderMinI;
            public int sliderMaxI;
        }
    }
}