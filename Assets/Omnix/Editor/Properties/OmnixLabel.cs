using Omnix.Core;
using Omnix.Data;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Omnix.Properties
{
    /// <summary>
    /// Static label in the Inspector
    /// </summary>
    public class OmnixLabel : PropertyBase
    {
        public override float GetDefaultHeight() => EditorGUIUtility.singleLineHeight;
        private readonly GUIStyle _style;

        public OmnixLabel(PropertyData data, LayoutBase parent) : base(data, parent)
        {
            DrawAction = DrawInner;
            try { _style = JsonConvert.DeserializeObject<GUIStyle>(data.extraData); }
            catch { }
            if (_style == null) _style = DataHub.CurrentSkin.labelStyle;

        }

        private void DrawInner()
        {
            EditorGUI.LabelField(TotalRect, MyData.content, _style);
        }

        public override void DrawOmnixInspector()
        {
            HelpersDraw.ContentFull(MyData);
            base.DrawOmnixInspector();
            base.DrawStyleSheet(_style);
        }
    }
}