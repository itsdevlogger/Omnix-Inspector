using System.Linq;
using Omnix.Core;
using Omnix.Data;
using UnityEditor;

namespace Omnix.Properties
{
    /// <summary>
    /// Helpbox object to display in Inspector
    /// </summary>
    public class OmnixHelpBox : PropertyBase
    {
        public override float GetDefaultHeight() => _defaultHeight;
        public MessageType MessageType
        {
            get => _messageType;
            set
            {
                if (_messageType == value) return;
                _messageType = value;
                DrawAction = () => { EditorGUI.HelpBox(TotalRect, MyData.content.text, _messageType); };
            }
        }
        
        private MessageType _messageType;
        private float _defaultHeight;

        public OmnixHelpBox(PropertyData data, LayoutBase parent) : base(data, parent)
        {
            if (int.TryParse(data.extraData, out int mt)) _messageType = (MessageType)mt;
            else _messageType = MessageType.None;
            DrawAction = () => { EditorGUI.HelpBox(TotalRect, MyData.content.text, _messageType); };
            UpdateDefaultHeight();
        }

        public override void DrawOmnixInspector()
        {
            if (HelpersDraw.DropDrown("HelpBox"))
            {
                MessageType = (MessageType)EditorGUILayout.EnumPopup("Message Type", MessageType);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Text");
                string text = EditorGUILayout.TextArea(MyData.content.text);
                if (text != MyData.content.text)
                {
                    MyData.content.text = text;
                    UpdateDefaultHeight();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            }
            
            base.DrawOmnixInspector();
        }

        public void UpdateDefaultHeight()
        {
            int count = MyData.content.text.Count((c => c == '\n'));
            if (count <= 1) _defaultHeight = EditorGUIUtility.singleLineHeight * 2f;
            else if (count == 2) _defaultHeight = EditorGUIUtility.singleLineHeight * 2.5f;
            else _defaultHeight = EditorGUIUtility.singleLineHeight * count;
        }
    }
}