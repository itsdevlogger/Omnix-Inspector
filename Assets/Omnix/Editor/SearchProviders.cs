using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Omnix.Core;
using Omnix.Data;
using Omnix.InputFields;
using Omnix.Preview;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ElementBase = Omnix.Core.ElementBase;

namespace Omnix.Windows.SearchProviders
{
    public class SearchProviderOmnix : ScriptableObject, ISearchWindowProvider
    {
        public enum TreeType
        {
            ValueChangedCallback,
            PropertyTarget,
            AddElement,
            ShowIf,
        }

        #region Statics
        private static readonly List<SearchTreeEntry> PropertyTargetTree = new List<SearchTreeEntry>();
        private static readonly List<SearchTreeEntry> AddElementTree = new List<SearchTreeEntry>();
        private static readonly List<SearchTreeEntry> ValueChangedTree = new List<SearchTreeEntry>();
        private static readonly List<SearchTreeEntry> ShowIfTree = new List<SearchTreeEntry>();
        #endregion

        #region Fields
        private TreeType _treeType;
        private ElementBase _targetElement;
        private int _index;
        private Action _extraCallback;
        #endregion

        #region Private
        private bool OnShowIf(SearchTreeEntry entry)
        {
            _targetElement.Data.showIfTarget = entry.name;
            _targetElement.Data.showIfTargetType = (ShowIfTargetType)entry.userData;
            if (_targetElement.Data.showIfTargetType == ShowIfTargetType.AlwaysShow || _targetElement.Data.showIfTargetType == ShowIfTargetType.AlwaysHide)
            {
                _targetElement.Data.showIfTarget = "";
            }
            Helpers.SetReadonlyField(_targetElement, "IsHiddenCheck", FirstLayoutWrapper.Current.GetIsHiddenCheckerFunction(_targetElement.Data));
            Helpers.RepaintAllWindows();
            return true;
        }

        private bool OnAddElement(SearchTreeEntry entry)
        {
            LayoutPreview parent = (LayoutPreview)_targetElement;

            if (!(entry.userData is ElementType elementType)) return false;
            if (!parent.AcceptsChild(elementType)) return false;

            ElementBase.NewPreviewElement(elementType.NewDataInstance(entry.name), parent, _index);
            Helpers.RepaintAllWindows();
            return false;
        }

        private bool OnPropertyTarget(SearchTreeEntry entry)
        {
            InputFieldBase.ChangePropertyTarget((InputFieldBase)_targetElement, entry.name);
            Helpers.RepaintAllWindows();
            return true;
        }

        private bool OnValueChangedCallback(SearchTreeEntry entry)
        {
            if (entry.name == "None")
            {
                Helpers.SetReadonlyField(_targetElement, "OnValueChangedCallback", null);
                _targetElement.NoCheckChange = true;
                _targetElement.Data.callbackTarget = DataClassBase.CallbackTargetForNone;
            }
            else
            {
                Action cc = FirstLayoutWrapper.Current.Method(entry.name);
                Helpers.SetReadonlyField(_targetElement, "OnValueChangedCallback", cc);
                _targetElement.NoCheckChange = (cc == null);
                _targetElement.Data.callbackTarget = entry.name;
            }
            Helpers.RepaintAllWindows();
            return true;
        }

        private static SearchTreeEntry CreateEntry(string name, int level, object data)
        {
            return new SearchTreeEntry(new GUIContent(name))
            {
                level = level,
                userData = data
            };
        }
        #endregion

        #region Interface
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return _treeType switch
            {
                TreeType.ShowIf => ShowIfTree,
                TreeType.AddElement => AddElementTree,
                TreeType.PropertyTarget => PropertyTargetTree,
                TreeType.ValueChangedCallback => ValueChangedTree,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            bool ret = _treeType switch
            {
                TreeType.ShowIf => OnShowIf(entry),
                TreeType.AddElement => OnAddElement(entry),
                TreeType.PropertyTarget => OnPropertyTarget(entry),
                TreeType.ValueChangedCallback => OnValueChangedCallback(entry),
                _ => throw new ArgumentOutOfRangeException()
            };

            _extraCallback?.Invoke();
            return ret;
        }
        #endregion

        #region Exposed Methods
        public static SearchProviderOmnix Create(ElementBase target, TreeType treeType, Action extraCallback = null)
        {
            SearchProviderOmnix current = CreateInstance<SearchProviderOmnix>();
            current._treeType = treeType;
            current._targetElement = target;
            current._extraCallback = extraCallback;
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), current);
            return current;
        }

        public static SearchProviderOmnix Create(ElementBase target, int index, Action extraCallback = null)
        {
            SearchProviderOmnix current = CreateInstance<SearchProviderOmnix>();
            current._treeType = TreeType.AddElement;
            current._index = index;
            current._targetElement = target;
            current._extraCallback = extraCallback;
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), current);
            return current;
        }

        public static void PopulateAllTrees(Type classType)
        {
            Type boolType = typeof(bool);

            ValueChangedTree.Clear();
            ShowIfTree.Clear();
            AddElementTree.Clear();
            PropertyTargetTree.Clear();

            ShowIfTree.Capacity = 5;
            AddElementTree.Capacity = 11;

            ShowIfTree.Add(new SearchTreeGroupEntry(new GUIContent("Conditions"), 0));
            ShowIfTree.Add(CreateEntry("Show Always", 1, ShowIfTargetType.AlwaysShow));
            ShowIfTree.Add(CreateEntry("Hide Always", 1, ShowIfTargetType.AlwaysHide));

            #region Layouts & Innert Elements
            AddElementTree.Add(new SearchTreeGroupEntry(new GUIContent("Add Element"), 0));
            AddElementTree.Add(new SearchTreeGroupEntry(new GUIContent("Layouts"), 1));
            AddElementTree.Add(CreateEntry("Vertical", 2, ElementType.VerticalLayout));
            AddElementTree.Add(CreateEntry("Horizontal", 2, ElementType.HorizontalLayout));
            AddElementTree.Add(CreateEntry("Paged", 2, ElementType.PagedLayout));
            AddElementTree.Add(new SearchTreeGroupEntry(new GUIContent("Elements"), 1));
            AddElementTree.Add(CreateEntry("Label", 2, ElementType.Label));
            AddElementTree.Add(CreateEntry("Button", 2, ElementType.Button));
            AddElementTree.Add(CreateEntry("HelpBox", 2, ElementType.HelpBox));
            AddElementTree.Add(CreateEntry("Spacer", 2, ElementType.Spacer));
            #endregion

            #region Properties & Fields
            PropertyTargetTree.Add(new SearchTreeGroupEntry(new GUIContent("Serialized Fields"), 1));
            AddElementTree.Add(new SearchTreeGroupEntry(new GUIContent("Serialized Fields"), 1));
            ShowIfTree.Add(new SearchTreeGroupEntry(new GUIContent("Fields & Properties"), 1));

            foreach (KeyValuePair<string, ClassMember> pair in FirstLayoutWrapper.Current.Serialized)
            {
                if (pair.Value.CanWrite)
                {
                    PropertyTargetTree.Add(CreateEntry(pair.Key, 2, ElementType.InputField));
                    AddElementTree.Add(CreateEntry(pair.Key, 2, ElementType.InputField));
                }

                if (pair.Value.ValueType == boolType)
                {
                    ShowIfTree.Add(CreateEntry(pair.Value.Name, 2, ShowIfTargetType.Field));
                }
            }

            PropertyTargetTree.Add(new SearchTreeGroupEntry(new GUIContent("Properties & Fields"), 1));
            AddElementTree.Add(new SearchTreeGroupEntry(new GUIContent("Properties & Fields"), 1));
            foreach (KeyValuePair<string, ClassMember> pair in FirstLayoutWrapper.Current.Serialized)
            {
                if (!pair.Value.CanWrite)
                {
                    PropertyTargetTree.Add(CreateEntry(pair.Key, 2, ElementType.InputField));
                    AddElementTree.Add(CreateEntry(pair.Key, 2, ElementType.InputField));
                }

                if (pair.Value.ValueType == typeof(bool))
                {
                    ShowIfTree.Add(CreateEntry(pair.Value.Name, 2, ShowIfTargetType.Property));
                }
            }
            #endregion

            #region Methods
            ShowIfTree.Add(new SearchTreeGroupEntry(new GUIContent("Methods"), 1));
            AddElementTree.Add(new SearchTreeGroupEntry(new GUIContent("Methods"), 1));
            ValueChangedTree.Add(new SearchTreeGroupEntry(new GUIContent("Callbacks"), 0));
            ValueChangedTree.Add(CreateEntry("None", 1, null));
            foreach (MethodInfo method in classType.GetMethods(Helpers.Flags))
            {
                if (method.GetParameters().Length != 0) continue;

                ValueChangedTree.Add(CreateEntry(method.Name, 1, null));
                AddElementTree.Add(CreateEntry(method.Name, 2, ElementType.Button));

                if (method.ReturnType == boolType)
                {
                    ShowIfTree.Add(CreateEntry(method.Name, 2, ShowIfTargetType.Method));
                }
            }
            #endregion

        }

        public static void ClearTrees()
        {
            PropertyTargetTree.Clear();
            AddElementTree.Clear();
            ValueChangedTree.Clear();
            ShowIfTree.Clear();
        }
        #endregion

    }
}