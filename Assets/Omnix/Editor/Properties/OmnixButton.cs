using System;
using Omnix.Core;
using Omnix.Data;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Omnix.Properties
{
    /// <summary>
    /// Button to display in Inspector
    /// </summary>
    public class OmnixButton : PropertyBase
    {
        public override float GetDefaultHeight() => EditorGUIUtility.singleLineHeight;
        private readonly GUIStyle _style;

        public OmnixButton(PropertyData data, LayoutBase parent) : base(data, parent)
        {
            try { _style = JsonConvert.DeserializeObject<GUIStyle>(data.extraData); }
            catch {  }

            if (_style == null) _style = DataHub.CurrentSkin.ButtonStyle;
            DrawAction = DrawInner;
            this.NoCheckChange = true;
        }

        public override void DrawOmnixInspector()
        {
            HelpersDraw.ValueChangedCallback(this, "On Clicked");
            HelpersDraw.ContentFull(MyData);
            base.DrawOmnixInspector();
            base.DrawStyleSheet(_style);
        }

        private void DrawInner()
        {
            Color backColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.clear;
            if (GUI.Button(TotalRect, MyData.content, _style))
            {
                OnValueChangedCallback?.Invoke();
            }
            GUI.backgroundColor = backColor;
        }
    }
}