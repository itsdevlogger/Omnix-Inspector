using System;
using UnityEditor;
using UnityEngine;

namespace Omnix.Data
{
    [Serializable]
    public class OmnixStyleSheet
    {
        public bool hasCustomStyle;
        public bool backgroundAlphaBlend;
        public ScaleMode backgroundScaleMode;
        public OmnixStyleState normal;
        public OmnixStyleState hover;

        private Color _lastContColor, _lastBackColor;

        // Called by unity for serialization
        private OmnixStyleSheet(OmnixStyleState normal, OmnixStyleState hover)
        {
            this.normal = normal;
            this.hover = hover;
        }

        public OmnixStyleSheet()
        {
            this.normal = new OmnixStyleState();
            this.hover = new OmnixStyleState();
            this.backgroundScaleMode = ScaleMode.StretchToFill;
            this.backgroundAlphaBlend = true;
        }

        /// <summary>
        /// Start drawing the element to which this StyleSheet belongs.
        /// This will update the colors to draw with.
        /// </summary>
        public void StartArea(Rect rect)
        {
            if (!hasCustomStyle) return;

            _lastContColor = GUI.contentColor;
            if (rect.Contains(Event.current.mousePosition))
            {
                hover.SetupColors(rect, backgroundAlphaBlend, backgroundScaleMode);
            }
            else
            {
                normal.SetupColors(rect, backgroundAlphaBlend, backgroundScaleMode);
            }
            _lastBackColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.clear;
        }

        /// <summary>
        /// Stop drawing the element to which this StyleSheet belongs.
        /// This will update the colors to default.
        /// </summary>
        public void EndArea()
        {
            if (hasCustomStyle)
            {
                GUI.contentColor = _lastContColor;
                GUI.backgroundColor = _lastBackColor;
            }
        }

        /// <summary>
        /// Get Unity GuiStyle from the given baseStyle
        /// </summary>
        private static GUIStyle GetGuiStyle(BaseStyles baseStyle)
        {
            // @formatter:off
            return baseStyle switch
            {
                BaseStyles.Auto                      => new GUIStyle(GUIStyle.none),
                BaseStyles.MiniLabel                 => new GUIStyle(EditorStyles.miniLabel),
                BaseStyles.LargeLabel                => new GUIStyle(EditorStyles.largeLabel),
                BaseStyles.BoldLabel                 => new GUIStyle(EditorStyles.boldLabel),
                BaseStyles.MiniBoldLabel             => new GUIStyle(EditorStyles.miniBoldLabel),
                BaseStyles.CenteredGreyMiniLabel     => new GUIStyle(EditorStyles.centeredGreyMiniLabel),
                BaseStyles.WordWrappedMiniLabel      => new GUIStyle(EditorStyles.wordWrappedMiniLabel),
                BaseStyles.WordWrappedLabel          => new GUIStyle(EditorStyles.wordWrappedLabel),
                BaseStyles.LinkLabel                 => new GUIStyle(EditorStyles.linkLabel),
                BaseStyles.WhiteLabel                => new GUIStyle(EditorStyles.whiteLabel),
                BaseStyles.WhiteMiniLabel            => new GUIStyle(EditorStyles.whiteMiniLabel),
                BaseStyles.WhiteLargeLabel           => new GUIStyle(EditorStyles.whiteLargeLabel),
                BaseStyles.WhiteBoldLabel            => new GUIStyle(EditorStyles.whiteBoldLabel),
                BaseStyles.RadioButton               => new GUIStyle(EditorStyles.radioButton),
                BaseStyles.MiniButton                => new GUIStyle(EditorStyles.miniButton),
                BaseStyles.MiniButtonLeft            => new GUIStyle(EditorStyles.miniButtonLeft),
                BaseStyles.MiniButtonMid             => new GUIStyle(EditorStyles.miniButtonMid),
                BaseStyles.MiniButtonRight           => new GUIStyle(EditorStyles.miniButtonRight),
                BaseStyles.MiniPullDown              => new GUIStyle(EditorStyles.miniPullDown),
                BaseStyles.TextField                 => new GUIStyle(EditorStyles.textField),
                BaseStyles.TextArea                  => new GUIStyle(EditorStyles.textArea),
                BaseStyles.MiniTextField             => new GUIStyle(EditorStyles.miniTextField),
                BaseStyles.NumberField               => new GUIStyle(EditorStyles.numberField),
                BaseStyles.Popup                     => new GUIStyle(EditorStyles.popup),
                BaseStyles.ObjectField               => new GUIStyle(EditorStyles.objectField),
                BaseStyles.ObjectFieldThumb          => new GUIStyle(EditorStyles.objectFieldThumb),
                BaseStyles.ObjectFieldMiniThumb      => new GUIStyle(EditorStyles.objectFieldMiniThumb),
                BaseStyles.ColorField                => new GUIStyle(EditorStyles.colorField),
                BaseStyles.LayerMaskField            => new GUIStyle(EditorStyles.layerMaskField),
                BaseStyles.Toggle                    => new GUIStyle(EditorStyles.toggle),
                BaseStyles.Foldout                   => new GUIStyle(EditorStyles.foldout),
                BaseStyles.FoldoutPreDrop            => new GUIStyle(EditorStyles.foldoutPreDrop),
                BaseStyles.FoldoutHeader             => new GUIStyle(EditorStyles.foldoutHeader),
                BaseStyles.FoldoutHeaderIcon         => new GUIStyle(EditorStyles.foldoutHeaderIcon),
                BaseStyles.ToggleGroup               => new GUIStyle(EditorStyles.toggleGroup),
                BaseStyles.Toolbar                   => new GUIStyle(EditorStyles.toolbar),
                BaseStyles.ToolbarButton             => new GUIStyle(EditorStyles.toolbarButton),
                BaseStyles.ToolbarPopup              => new GUIStyle(EditorStyles.toolbarPopup),
                BaseStyles.ToolbarDropDown           => new GUIStyle(EditorStyles.toolbarDropDown),
                BaseStyles.ToolbarTextField          => new GUIStyle(EditorStyles.toolbarTextField),
                BaseStyles.InspectorDefaultMargins   => new GUIStyle(EditorStyles.inspectorDefaultMargins),
                BaseStyles.InspectorFullWidthMargins => new GUIStyle(EditorStyles.inspectorFullWidthMargins),
                BaseStyles.HelpBox                   => new GUIStyle(EditorStyles.helpBox),
                BaseStyles.ToolbarSearchField        => new GUIStyle(EditorStyles.toolbarSearchField),
                _ => throw new ArgumentOutOfRangeException()
            };
            // @formatter:on
        }

        public OmnixStyleSheet Copy()
        {
            return new OmnixStyleSheet(normal.Copy(), hover.Copy())
            {
                hasCustomStyle = this.hasCustomStyle,
                backgroundAlphaBlend = this.backgroundAlphaBlend,
                backgroundScaleMode = this.backgroundScaleMode,
            };
        }
    }

    [Serializable]
    public class OmnixStyleState
    {
        public Color backColor;
        public Color textColor;
        public Texture2D backTexture;

        // Called by unity for serialization
        public OmnixStyleState()
        {
            this.textColor = Color.white;
            this.backColor = Color.white;
        }

        /// <summary>
        /// Update GUI color to according to this style
        /// </summary>
        public void SetupColors(Rect rect, bool alphaBlend, ScaleMode scaleMode)
        {
            Color color = GUI.color;

            GUI.color = backColor;
            if (backTexture != null) GUI.DrawTexture(rect, backTexture, scaleMode, alphaBlend);
            else GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = color;
            GUI.contentColor = textColor;
        }

        public OmnixStyleState Copy()
        {
            return new OmnixStyleState()
            {
                backColor = this.backColor,
                textColor = this.textColor,
                backTexture = this.backTexture,
            };
        }
    }
}