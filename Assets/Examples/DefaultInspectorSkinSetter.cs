using Omnix.Data;
using UnityEngine;

[ExecuteAlways]
public class DefaultInspectorSkinSetter : MonoBehaviour
{

    private bool _setButton;
    private bool _setLabel;
    public OmnixInspectorSkin Skin;

    [ContextMenu("ResetButtonStyle")]
    private void ResetButtonStyle() => _setButton = true;

    [ContextMenu("ResetLabelStyle")]
    private void ResetLabelStyle() => _setLabel = true;

    private void OnDrawGizmos()
    {
        if (_setButton)
        {
            Skin.buttonStyle = new GUIStyle(GUI.skin.button);
            Debug.Log("Setted Button");
        }
        if (_setLabel)
        {
            Skin.labelStyle = new GUIStyle(GUI.skin.label);
            Debug.Log("Setted Label");
        }

        _setButton = false;
        _setLabel = false;
    }
}