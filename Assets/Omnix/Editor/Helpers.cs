using System;
using System.Collections.Generic;
using System.Reflection;
using Omnix.Data;
using Omnix.Preview;
using Omnix.Windows.SearchProviders;
using UnityEditor;
using UnityEngine;

namespace Omnix.Core
{
    public static class Helpers
    {
        public delegate void SaveDataDelegate(bool closingWindows);

        public static event Action OnRepaint;
        public static event SaveDataDelegate OnSaveData;

        public static readonly BindingFlags Flags = BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        public static readonly HashSet<Type> SupportedTypes = new HashSet<Type>()
        {
            typeof(Color),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Rect),
            typeof(AnimationCurve),
            typeof(Bounds),
            typeof(Vector2Int),
            typeof(Vector3Int),
            typeof(RectInt),
            typeof(BoundsInt),
        };

        public static void SetReadonlyField(object obj, string fieldName, object nawValue) => obj.GetType().GetField(fieldName, Flags)?.SetValue(obj, nawValue);
        public static bool AreDifferent(this Rect r1, Rect r2) => Mathf.Abs(r1.width - r2.width) > 1f || Mathf.Abs(r1.y - r2.y) > 1f;
        public static ElementBase NewPreviewInstance(this ElementType elementType, LayoutPreview parent, int insertIndex) => ElementBase.NewPreviewElement(NewDataInstance(elementType, elementType.ToString()), parent, insertIndex);


        // %Element% means every Element must be registered in this method
        // %Property% means every Property must be registered in this method
        public static DataClassBase NewDataInstance(this ElementType elementType, string name)
        {
            switch (elementType)
            {
                case ElementType.VerticalLayout:
                case ElementType.HorizontalLayout:
                case ElementType.PagedLayout: return new LayoutData($"{elementType}", elementType);
                case ElementType.InputField:
                {
                    if (string.IsNullOrEmpty(name) || !FirstLayoutWrapper.Current.Serialized.ContainsKey(name)) return new PropertyData(ElementType.InputField, PropertyBase.UnknownPropertyName, "");

                    string propName = FirstLayoutWrapper.Current.Serialized[name].DisplayName;
                    return new PropertyData(ElementType.InputField, propName, name);
                }
                case ElementType.Button:
                    Action method = FirstLayoutWrapper.Current.Method(name);
                    if (method == null) return new PropertyData(elementType, name, "");
                    return new PropertyData(elementType, name, "") { callbackTarget = name };
                case ElementType.Label:
                case ElementType.HelpBox:
                case ElementType.Spacer:
                    return new PropertyData(elementType, name, "");
                default:
                    throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null);
            }
        }

        public static void RepaintAllWindows()
        {
            FirstLayoutWrapper.Current.RefreshLayouts();
            OnRepaint?.Invoke();
        }

        public static void CallSaveData(bool closingAllWindows) => OnSaveData?.Invoke(closingAllWindows);

        public static void ClearCallbacks()
        {
            OnRepaint = null;
            OnSaveData = null;
        }

        public static void GetInternalClasses(out Type hierarchy, out Type inspector, out Type gameView, out Type console)
        {
            hierarchy = null;
            inspector = null;
            gameView = null;
            console = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == "SceneHierarchyWindow") hierarchy = type;
                    else if (type.Name == "InspectorWindow") inspector = type;
                    else if (type.Name == "SceneView") gameView = type;
                    else if (type.Name == "ConsoleWindow") console = type;

                    if (hierarchy != null && inspector != null && gameView != null && console != null) return;
                }
            }
        }
        public static Rect AddPadding(Rect original, RectOffset padding)
        {
            return new Rect(
                x: original.x - padding.left,
                y: original.y - padding.top,
                width: original.width + padding.horizontal,
                height: original.height + padding.vertical
            );
        }
    }

    public static class HelpersDraw
    {
        private static Dictionary<ElementType, int> IndentsNeeded = new Dictionary<ElementType, int>()
        {
            { ElementType.InputField, 5 },
            { ElementType.HelpBox, 4 },
            { ElementType.Button, 4 },
            { ElementType.Label, 4 },
            { ElementType.HorizontalLayout, 4 },
            { ElementType.VerticalLayout, 4 },
            { ElementType.PagedLayout, 4 },
            { ElementType.Spacer, 3 },
        };

        private static bool[] _dropDowns = null;
        private static int _currentDDIndex;
        private static readonly GUIContent HideModesLabel = new GUIContent("Hide Mode");
        private static readonly GUIContent[] HideModes = new GUIContent[]
        {
            new GUIContent("Dont Draw"),
            new GUIContent("Disable"),
        };
        public static readonly GUIContent[] LayoutTypes = new GUIContent[]
        {
            new GUIContent("Vertical Layout"),
            new GUIContent("Horizontal Layout"),
            new GUIContent("Paged Layout"),
        };

        public static void Paddings(DataClassBase data)
        {
            data.padding.left = EditorGUILayout.IntField("Pad Left", data.padding.left);
            data.padding.right = EditorGUILayout.IntField("Pad Right", data.padding.right);
            data.padding.top = EditorGUILayout.IntField("Pad Top", data.padding.top);
            data.padding.bottom = EditorGUILayout.IntField("Pad Down", data.padding.bottom);
        }

        public static void ScriptPickupToggle()
        {
            bool showPickup = FirstLayoutWrapper.Current.DrawerData.showScriptPickup;
            float width = EditorGUIUtility.currentViewWidth - 50f;
            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Color color = GUI.color;
            GUI.color = showPickup ? Color.green : Color.red;
            if (GUILayout.Button("Show Pickup", GUILayout.Width(width), GUILayout.Height(50)))
            {
                FirstLayoutWrapper.Current.DrawerData.showScriptPickup = !showPickup;
            }
            
            GUI.color = color;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public static void Visibility(ElementBase element)
        {
            if (!DropDrown("Visibility")) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Check");
            if (GUILayout.Button(ShowIfTargetText(element), EditorStyles.popup))
            {
                SearchProviderOmnix.Create(element, SearchProviderOmnix.TreeType.ShowIf);
            }
            EditorGUILayout.EndHorizontal();

            int hideModeIndex = element.Data.hideDontDisable ? 0 : 1;
            element.Data.hideDontDisable = (0 == EditorGUILayout.Popup(HideModesLabel, hideModeIndex, HideModes));
            EditorGUI.indentLevel--;
        }

        public static string ShowIfTargetText(ElementBase element)
        {
            return element.Data.showIfTargetType switch
            {
                ShowIfTargetType.AlwaysShow => "(Show Always)",
                ShowIfTargetType.AlwaysHide => "(Always Hide)",
                _ => $"({element.Data.showIfTargetType}) {element.Data.showIfTarget}"
            };
        }

        public static void ValueChangedCallback(ElementBase element, string content)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(content);
            if (GUILayout.Button(element.Data.callbackTarget, EditorStyles.popup))
            {
                SearchProviderOmnix.Create(element, SearchProviderOmnix.TreeType.ValueChangedCallback);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void ContentFull(PropertyData proData)
        {
            if (!DropDrown("Content")) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            {
                float height = EditorGUIUtility.singleLineHeight * 5f;
                float width = EditorGUIUtility.singleLineHeight * 3f;
                proData.content.image = (Texture)EditorGUILayout.ObjectField(proData.content.image, typeof(Texture), allowSceneObjects: false, GUILayout.Width(height), GUILayout.Height(height));

                EditorGUILayout.BeginVertical();
                {
                    height *= 0.25f;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Text", GUILayout.Width(width), GUILayout.Height(height));
                    proData.content.text = EditorGUILayout.TextField(GUIContent.none, proData.content.text, GUILayout.Height(height));
                    EditorGUILayout.EndHorizontal();

                    height *= 3f;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Tooltip", GUILayout.Width(width));
                    proData.content.tooltip = EditorGUILayout.TextField(GUIContent.none, proData.content.tooltip, GUILayout.Height(height));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }


        public static void DrawStyleSheet(OmnixStyleSheet styleSheet)
        {
            styleSheet.hasCustomStyle = EditorGUILayout.Toggle("Has Custom Style", styleSheet.hasCustomStyle);
            if (!styleSheet.hasCustomStyle) return;

            styleSheet.backgroundAlphaBlend = EditorGUILayout.Toggle("Back Alpha Blend", styleSheet.backgroundAlphaBlend);
            styleSheet.backgroundScaleMode = (ScaleMode)EditorGUILayout.EnumPopup("Back Scale Mode", styleSheet.backgroundScaleMode);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("    ");
            GUILayout.Label("Texture");
            GUILayout.Label("Back");
            GUILayout.Label("Text");
            EditorGUILayout.EndHorizontal();

            float oneHeight = EditorGUIUtility.singleLineHeight * 2.5f;
            DrawStyleSheetState(styleSheet.normal, oneHeight);
            DrawStyleSheetState(styleSheet.hover, oneHeight);
        }

        public static void DrawStyleSheetState(OmnixStyleState state, float oneHeight)
        {
            EditorGUILayout.BeginHorizontal();
            state.backTexture = (Texture2D)EditorGUILayout.ObjectField("Normal", state.backTexture, typeof(Texture2D), allowSceneObjects: false, GUILayout.Height(oneHeight));
            GUILayout.FlexibleSpace();
            state.backColor = EditorGUILayout.ColorField(GUIContent.none, state.backColor, GUILayout.Width(oneHeight), GUILayout.Height(oneHeight));
            GUILayout.FlexibleSpace();
            state.textColor = EditorGUILayout.ColorField(GUIContent.none, state.textColor, GUILayout.Width(oneHeight), GUILayout.Height(oneHeight));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawGuiStyle(GUIStyle drawerStyle)
        {
            GUILayout.Space(10f);
            GUILayout.Label("Font", EditorStyles.boldLabel);
            drawerStyle.font = (Font)EditorGUILayout.ObjectField("Font", drawerStyle.font, typeof(Font), allowSceneObjects: false);
            drawerStyle.fontSize = EditorGUILayout.IntField("Font Size", drawerStyle.fontSize);
            drawerStyle.fontStyle = (FontStyle)EditorGUILayout.EnumPopup("Font Style", drawerStyle.fontStyle);

            GUILayout.Space(10f);
            GUILayout.Label("Content", EditorStyles.boldLabel);
            drawerStyle.wordWrap = EditorGUILayout.Toggle("Word Wrap", drawerStyle.wordWrap);
            drawerStyle.contentOffset = EditorGUILayout.Vector2Field("contentOffset", drawerStyle.contentOffset);
            drawerStyle.alignment = (TextAnchor)EditorGUILayout.EnumPopup("alignment", drawerStyle.alignment);
            drawerStyle.clipping = (TextClipping)EditorGUILayout.EnumPopup("clipping", drawerStyle.clipping);
            drawerStyle.imagePosition = (ImagePosition)EditorGUILayout.EnumPopup("imagePosition", drawerStyle.imagePosition);
        }


        public static bool DropDrown(string content)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);

            bool ddVal = _dropDowns[_currentDDIndex];
            float butHeight = EditorGUIUtility.singleLineHeight * 1.25f;
            Rect rect = EditorGUILayout.GetControlRect(false, butHeight);

            DataHub.CurrentSkin.dropDown.StartArea(rect);
            {
                Color backColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.clear;
                if (GUI.Button(rect, content))
                {
                    _dropDowns[_currentDDIndex] = !ddVal;
                }
                GUI.backgroundColor = backColor;
            }
            DataHub.CurrentSkin.dropDown.EndArea();
            _currentDDIndex++;
            return ddVal;
        }

        public static void PrepareDropDowns(ElementType elementType)
        {
            _dropDowns = new bool[IndentsNeeded[elementType]];
            _currentDDIndex = 0;
        }

        public static void ResetDdIndex()
        {
            _currentDDIndex = 0;
        }
    }
}