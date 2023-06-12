using Omnix.Core;
using Omnix.Data;
using UnityEditor;

namespace Omnix.Properties
{
    /// <summary>
    /// Spacer. Empty Space in Inspector.
    /// </summary>
    public class OmnixSpacer : PropertyBase
    {
        public override float GetDefaultHeight() => MyData.heightValue;

        public OmnixSpacer(PropertyData data, LayoutBase parent) : base(data, parent)
        {
            DrawAction = () => { } ;
            if (data.heightType == SizeType.Auto && data.widthType == SizeType.Auto)
            {
                data.heightType = SizeType.Pixels;
                data.widthType = SizeType.Pixels;
            }
            if (data.heightValue <= 0) data.heightValue = EditorGUIUtility.singleLineHeight;
            if (data.widthValue <= 0) data.widthValue = EditorGUIUtility.singleLineHeight;
        }

        public override void DrawOmnixInspector()
        {
            base.DrawOmnixInspector();
            base.DrawStyleSheet();
        }
    }
}