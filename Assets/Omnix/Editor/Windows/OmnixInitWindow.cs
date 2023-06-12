using System;
using UnityEditor;
using UnityEngine;


namespace Omnix.Windows
{
    public class OmnixInitWindow : OmnixWindowBase
    {
        private static readonly string InvalidMsg = "Invalid Target, must derive from one of \"MonoBehavior\", \"IOmnixProperty\", \"OmnixScriptable\" or \"OmnixEditorWindow\"";
        private MonoScript _targetScript;

        private void OnGUI()
        {
            if (OmnixEditorContext.IsEditing)
            {
                EditorGUILayout.LabelField("Another Editor Maker is already opened. Close it before opening new one.");
                EditorGUILayout.LabelField("If no preview window is opened, then try refreshing or delete the \"(Omnix Editor Context)\" GameObject.");
                return;
            }
            
            
            _targetScript = (MonoScript)EditorGUILayout.ObjectField("Target", _targetScript, typeof(MonoScript), false);

            if (_targetScript == null)
            {
                EditorGUILayout.HelpBox("Target cant be empty", MessageType.Error);
                return;
            }


            Type clss = _targetScript.GetClass();
            if (clss == null)
            {
                EditorGUILayout.HelpBox(InvalidMsg, MessageType.Error);
                return;
            }

            if (clss.IsSubclassOf(typeof(MonoBehaviour)))
            {
                EditorGUILayout.HelpBox($"Create Editor for {clss}", MessageType.Info);
                if (GUILayout.Button("Continue")) OpenForEditor();
            }
            else
            {
                EditorGUILayout.HelpBox(InvalidMsg, MessageType.Error);
            }
        }
        
        private void OpenForEditor()
        {
            // try
            // {
                OmnixPreviewWindow.TryInit(_targetScript);
                Actives.OpenAll();
                this.Close();
            // }
            // catch { }
        }
    }
}