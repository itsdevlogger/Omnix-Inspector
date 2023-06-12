using System;
using System.Diagnostics.CodeAnalysis;
using Omnix.Data;
using Omnix.InputFields;
using Omnix.Preview;
using Omnix.Properties;
using UnityEditor;
using UnityEngine;

namespace Omnix.Core
{
    /// <summary> Base class for InputFields, Inert Elements (Button, Label, Spaced, HelpBox) and Layouts </summary>
    public abstract class ElementBase
    {

        #region Static Fields
        /// <summary>
        /// Its a process heavy task to check everytime "if the data passed to constructor is valid or not"
        /// So this variable makes sure that user can't create new instance of any element using new keyword.
        /// Must use ElementBase.NewPreviewElement to create new element.
        /// </summary>
        private static bool _acceptInstance = false;

        /// <summary> Is GUI Enabled </summary>
        protected static bool GuiEnabled = true;
        #endregion

        #region Abstract
        /// <summary> ture if this element is a InputField or Inert Element (Button, Label, Spaced, HelpBox, etc) false if its a Layout or any other element </summary>
        public abstract bool IsProperty { get; }

        /// <summary>
        /// Temporary. Prints the child info of this element after given indents
        /// </summary>
        public abstract void PrintInfo(string indentLevel);

        /// <summary> Draw Properties of this Element. Called while drawing Omnix Inspector </summary>
        public abstract void DrawOmnixInspector();

        /// <summary>
        /// Update size of this element (and its children).
        /// Cache the Height and Width value in CurrentRect.
        /// Called whenever something changes in the inspector.
        /// </summary>
        /// <param name="width">Maximum Width assigned to this element</param>
        public abstract void RefreshSize(float width);

        /// <summary>
        /// Refresh position of this element (and its children).
        /// Cache the X and Y value in CurrentRect.
        /// Called right after updating sizes of all elements.
        /// </summary>
        /// <param name="x"> Assigned x-position of top-left corner of this element. </param>
        /// <param name="y"> Assigned y-position of top-left corner of this element. </param>
        public abstract void RefreshPosition(float x, float y);

        /// <summary>
        /// Not abstract but we assume this variable is set by every child.
        /// Draw the element, without having to worry about things like is element disabled and gui color. 
        /// </summary>
        protected Action DrawAction;
        #endregion

        #region Fields
        /// <summary> True if OnValueChangeCallback is null </summary>
        public bool NoCheckChange;

        /// <summary> Unique ID of this element. Unlike DataBase.GUID this ID is not serialized and generated every time when the element is created. </summary>
        public readonly int MyID;

        /// <summary> Callback when the value of this element is changed, not serialized. </summary>
        public readonly Action OnValueChangedCallback;

        /// <summary> Callback to check if this element is hidden in Unity Inspector, not serialized. </summary>
        public readonly Func<bool> IsHiddenCheck;

        /// <summary> Serializable data for this element. This data determines how the element is drawn </summary>
        public readonly DataClassBase Data;

        /// <summary> Parent Layout of this element </summary>
        public readonly LayoutBase Parent;

        /// <summary> Rect which defines how this element is being drawn </summary>
        [NonSerialized] public Rect TotalRect;

        /// <summary> is this element hidden in the Unity Inspector. Hidden means this will not show at all or this will show up as disabled based on Data.hideDontDisable variable </summary>
        [NonSerialized] public bool IsHidden;

        /// <summary> Should this element not be drawn in current frame in Unity Inspector. </summary>
        public bool IsSkipped => this.IsHidden && this.Data.hideDontDisable;
        #endregion

        #region Constructor
        protected ElementBase(DataClassBase data, LayoutBase parent)
        {
            if (!_acceptInstance)
            {
                _acceptInstance = false;
                throw new Exception("Cannot create instance of an Element, Use ElementBase.GetPreviewElement or ElementBase.GetUnityInspectorElement");
            }
            _acceptInstance = false;

            this.Data = data;
            this.Parent = parent;
            this.TotalRect = Rect.zero;
            this.MyID = FirstLayoutWrapper.Current.GetUniqueElementID(this);
            this.IsHiddenCheck = FirstLayoutWrapper.Current.GetIsHiddenCheckerFunction(data);
            this.OnValueChangedCallback = FirstLayoutWrapper.Current.Method(data.callbackTarget);
            this.NoCheckChange = (OnValueChangedCallback == null);
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            if (this.Data == null) return "Null Data";
            return this.Data.nameInKsHierarchy;
        }

        /// <summary>
        /// Draw this element in Unity Inspector.
        /// </summary>
        public void Draw()
        {
            if (this.IsSkipped)
            {
                Debug.Log($"Skipping this: {Data.GUID}");
                return;
            }

            // Update stylesheet (Colors)
            Data.styleSheet.StartArea(Helpers.AddPadding(TotalRect, Data.padding));
            {
                // Draw
                GuiEnabled = GUI.enabled;
                GUI.enabled = GuiEnabled && !IsHidden;
                if (NoCheckChange || !GUI.enabled)
                {
                    DrawAction();
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    DrawAction();
                    if (EditorGUI.EndChangeCheck()) OnValueChangedCallback.Invoke();
                }
                GUI.enabled = GuiEnabled;
            }
            Data.styleSheet.EndArea();
        }
        #endregion

        #region Static Methods
        /// <summary> Create New InputField or Inert Element (Button, Label, Spaced, HelpBox) via Element Data </summary>
        /// <param name="data"> Element Data that the new Element will use as its signature.</param>
        /// <param name="parent"> Parent of the new element, should not be null. </param>
        /// 
        /// <returns>New Property</returns>
        /// <exception cref="ArgumentOutOfRangeException">When the given data does not map to an InputField nor Inert Element</exception>
        // %Property% means every Property must be registered in this method
        protected static PropertyBase NewProperty(PropertyData data, LayoutBase parent)
        {
            _acceptInstance = true;
            PropertyBase newProp;

            switch (data.elementType)
            {
                case ElementType.InputField:
                {
                    if (!FirstLayoutWrapper.Current.Serialized.ContainsKey(data.target))
                    {
                        newProp = new IfUnknown(data, parent);
                        break;
                    }

                    ClassMember member = FirstLayoutWrapper.Current.Serialized[data.target];
                    if (member.ValueType == typeof(float) || member.ValueType == typeof(int)) newProp = new IfNumber(member, data, parent);
                    else if (member.ValueType == typeof(bool)) newProp = new IfBool(member, data, parent);
                    else newProp = new IfDefault(member, data, parent);
                    break;
                }
                case ElementType.Button:
                    newProp = new OmnixButton(data, parent);
                    break;
                case ElementType.Label:
                    newProp = new OmnixLabel(data, parent);
                    break;
                case ElementType.Spacer:
                    newProp = new OmnixSpacer(data, parent);
                    break;
                case ElementType.HelpBox:
                    newProp = new OmnixHelpBox(data, parent);
                    break;

                case ElementType.VerticalLayout:
                case ElementType.HorizontalLayout:
                case ElementType.PagedLayout:
                default: throw new ArgumentOutOfRangeException($"Not implemented for {data.elementType}");
            }

            return newProp;
        }

        /// <summary> Create New Layout, InputField or Inert Element (Button, Label, Spaced, HelpBox) via Element Data </summary>
        /// <param name="data"> Element Data that the new Element will use as its signature. </param>
        /// <param name="parent"> Parent of the new element, should not be null. </param>
        /// <param name="insertIndex"> What index this child should be placed at. <see cref="LayoutPreview.InsertChild"/> </param>
        /// <returns>New Element</returns>
        /// <exception cref="ArgumentOutOfRangeException">When the given data does not map to an InputField nor Inert Element</exception>
        // %Element% means every Element must be registered in this method
        public static ElementBase NewPreviewElement(DataClassBase data, LayoutPreview parent, int insertIndex = -1)
        {
            _acceptInstance = true;
            ElementBase element;

            switch (data)
            {
                case LayoutData ld:
                    element = new LayoutPreview(ld, parent);
                    parent?.InsertChild(element, insertIndex);
                    break;
                case PropertyData pd:
                    element = NewProperty(pd, parent);
                    parent?.InsertChild(element, insertIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(data.elementType), data.elementType, null);
            }
            return element;
        }

        // %Element% means every Element must be registered in this method
        // UI stands for Unity Inspector, as there are 2 inspectors, Omnix and Unity's
        public static ElementBase NewUIElement(DataClassBase data, LayoutUnityInspector parent, int insertIndex = -1)
        {
            _acceptInstance = true;
            return data switch
            {
                LayoutData ld => new LayoutUnityInspector(ld, parent),
                PropertyData pd => NewProperty(pd, parent),
                _ => throw new ArgumentOutOfRangeException(nameof(data.elementType), data.elementType, null)
            };
        }
        #endregion

    }
}