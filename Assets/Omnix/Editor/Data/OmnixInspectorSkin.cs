using UnityEngine;

namespace Omnix.Data
{
    /// <summary>
    /// Skin for an inspect. Update this to change the look and feel of every custor editor.
    /// </summary>
    [CreateAssetMenu(fileName = "Omnix Inspector Skin", menuName = "Omnix/Inspector Skin")]
    public class OmnixInspectorSkin : ScriptableObject
    {
        public OmnixStyleSheet draggable;
        public OmnixStyleSheet dropDown;
        public GUIStyle buttonStyle;
        public GUIStyle labelStyle;
    }
}