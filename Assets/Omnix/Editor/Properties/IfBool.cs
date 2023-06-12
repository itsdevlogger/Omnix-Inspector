using System;
using JetBrains.Annotations;
using Omnix.Core;
using Omnix.Data;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Omnix.InputFields
{
    public class IfBool : InputFieldBase
    {
        private static readonly GUIContent[] DrawerStyles = new GUIContent[]
        {
            new GUIContent("Default"),
            new GUIContent("Toggle Left"),
            new GUIContent("Toggle Button"),
        };

        private readonly ExtraData _extraData;

        public IfBool(ClassMember targetMember, [NotNull] PropertyData myData, LayoutBase parent) : base(targetMember, myData, parent)
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
                    backActive = new float[] { 0f, 1f, 0f, 0.50f },
                    backInactive = new float[] { 1f, 0f, 0f, 0.50f },
                    textActive = new float[] { 0f, 1f, 0f, 1f },
                    textInactive = new float[] { 1f, 0f, 0f, 1f },
                };
            }
            
            _extraData.Load();
            Helpers.OnSaveData += SaveData;
            DrawAction = GetDrawer();
        }

        public override float GetDefaultHeight() => EditorGUIUtility.singleLineHeight;
        
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
            if (style == 2)
            {
                EditorGUI.indentLevel++;
                _extraData.BackActive = EditorGUILayout.ColorField("Enabled Back Color", _extraData.BackActive);
                _extraData.BackInactive = EditorGUILayout.ColorField("Disabled Back Color", _extraData.BackInactive);
                EditorGUILayout.Space(3f);
                _extraData.TextActive = EditorGUILayout.ColorField("Enabled Text Color", _extraData.TextActive);
                _extraData.TextInactive = EditorGUILayout.ColorField("Disabled Text Color", _extraData.TextInactive);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private void SaveData(bool closingAllWindows)
        {
            _extraData.Save();
            MyData.extraData = JsonConvert.SerializeObject(_extraData);
        }

        private Action GetDrawer()
        {
            
            return _extraData.drawerStyle switch
            {
                0 => (() => TargetMember.boolValue = EditorGUI.Toggle(TotalRect, MyData.content, TargetMember.boolValue)),
                1 => (() => TargetMember.boolValue = EditorGUI.ToggleLeft(TotalRect, MyData.content, TargetMember.boolValue)),
                2 => ToggleButton,
                _ => throw new Exception($"Not implemented Drawer Style {_extraData.drawerStyle} for {TargetMember.ValueType}")
            };
        }

        private void ToggleButton()
        {
            Color backCol = GUI.backgroundColor;
            Color contCol = GUI.contentColor;

            if (TargetMember.boolValue)
            {
                GUI.backgroundColor = _extraData.BackActive;
                GUI.contentColor = _extraData.TextActive;
            }
            else
            {
                GUI.backgroundColor = _extraData.BackInactive;
                GUI.contentColor = _extraData.TextInactive;
            }

            if (GUI.Button(TotalRect, MyData.content))
            {
                TargetMember.boolValue = !TargetMember.boolValue;
            }

            GUI.backgroundColor = backCol;
            GUI.contentColor = contCol;
        }

        [Serializable]
        private class ExtraData
        {
            public int drawerStyle;
            public float[] backInactive;
            public float[] textInactive;
            public float[] backActive;
            public float[] textActive;

            [JsonIgnore]
            public Color BackInactive, TextInactive, BackActive, TextActive;

            public void Load()
            {
                BackInactive = new Color(backInactive[0], backInactive[1], backInactive[2], backInactive[3]);
                TextInactive = new Color(textInactive[0], textInactive[1], textInactive[2], textInactive[3]);
                BackActive = new Color(backActive[0], backActive[1], backActive[2], backActive[3]);
                TextActive = new Color(textActive[0], textActive[1], textActive[2], textActive[3]);
            }

            public void Save()
            {
                backInactive = new float[] { BackInactive.r, BackInactive.g, BackInactive.b, BackInactive.a };
                textInactive = new float[] { TextInactive.r, TextInactive.g, TextInactive.b, TextInactive.a };
                backActive = new float[] { BackActive.r, BackActive.g, BackActive.b, BackActive.a };
                textActive = new float[] { TextActive.r, TextActive.g, TextActive.b, TextActive.a };
            }
        }
    }
}

