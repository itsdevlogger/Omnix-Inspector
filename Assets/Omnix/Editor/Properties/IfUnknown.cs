using JetBrains.Annotations;
using Omnix.Core;
using Omnix.Data;
using Omnix.Windows.SearchProviders;
using UnityEditor;
using UnityEngine;

namespace Omnix.InputFields
{
    /// <summary>
    /// Drawer when the type of Input Field is not known or the property is not assigned
    /// </summary>
    public class IfUnknown : InputFieldBase
    {
        public IfUnknown([NotNull]  PropertyData myData, LayoutBase parent) : base(null, myData, parent)
        {
            DrawAction = DrawInner;
        }

        public override float GetDefaultHeight() => EditorGUIUtility.singleLineHeight;

        private void DrawInner()
        {
            Rect rect = EditorGUI.PrefixLabel(TotalRect, MyData.content);
            if (GUI.Button(rect, "Not Selected", EditorStyles.toolbarDropDown))
            {
                SearchProviderOmnix.Create(this, SearchProviderOmnix.TreeType.PropertyTarget);
            }
        }
    }
}