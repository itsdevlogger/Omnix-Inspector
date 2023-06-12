using System.Collections.Generic;
using System.Linq;
using Omnix.Core;
using Omnix.Preview;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace Omnix.Windows
{
    public class OmnixHierarchyWindow : OmnixWindowBase
    {
        public ElementBase SelectedElement { get; private set; }
        public bool HasMultipleSelected => _treeView.GetSelection().Count > 1;
        public IEnumerable<ElementBase> LoopSelection() => _treeView.GetSelection().Select(FirstLayoutWrapper.GetElementFromId);

        [SerializeField] private TreeViewState mTreeViewState;
        private OmnixTreeView _treeView;

        private void OnEnable()
        {
            if (Actives.Preview == null)
            {
                this.Close();
                return;
            }
            SelectedElement = null;
            mTreeViewState ??= new TreeViewState();
            _treeView = new OmnixTreeView(mTreeViewState);
            _treeView.SetSelection(new List<int>());
            Helpers.OnRepaint += _treeView.Reload;
        }

        private void OnGUI()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, position.height);
            _treeView.OnGUI(rect);
            CheckKeyBindings();
        }

        private void CheckKeyBindings()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyUp) return;


            ElementBase[] selection = _treeView.GetSelectionElements();
            if (selection != null)
            {
                switch (current.modifiers, current.keyCode)
                {
                    case (_, KeyCode.Delete):
                        TreeFunc.Delete(selection);
                        return;
                    case (EventModifiers.Control, KeyCode.D):
                    case (EventModifiers.Command, KeyCode.D):
                        TreeFunc.Duplicate(selection);
                        return;
                    case (EventModifiers.Control, KeyCode.C):
                    case (EventModifiers.Command, KeyCode.C):
                        TreeFunc.Copy(selection);
                        return;
                    case (EventModifiers.Control, KeyCode.X):
                    case (EventModifiers.Command, KeyCode.X):
                        TreeFunc.Cut(selection);
                        return;
                }
            }


            if (TreeFunc.CanPaste && (current.modifiers == EventModifiers.Control || current.modifiers == EventModifiers.Command) && current.keyCode == KeyCode.V)
            {
                LayoutPreview parent = null;
                if (selection != null && selection.Length != 0)
                {
                    ElementBase firstSel = selection[0];
                    if (firstSel?.Parent != null)
                        parent = (LayoutPreview)firstSel.Parent;
                }

                if (parent?.Parent == null) // same as parent == null || parent.Parent == null
                    parent = (LayoutPreview)FirstLayoutWrapper.Current.FirstLayout;
                TreeFunc.PasteAsChild(parent);
            }
        }

        public static void SelectElement(ElementBase element)
        {
            if (Actives.Hierarchy == null) return;

            Actives.Hierarchy.SelectedElement = element;
            if (element != null)
            {
                Actives.Hierarchy._treeView.SetSelection(new List<int>() { element.MyID });
                Actives.Hierarchy._treeView.FrameItem(element.MyID);
            }
            else
            {
                Actives.Hierarchy._treeView.SetSelection(new List<int>());
            }
            Helpers.RepaintAllWindows();
        }

        public static void __OnSelectionChanged(ElementBase elementBase)
        {
            if (Actives.Hierarchy == null) return;

            Actives.Hierarchy.SelectedElement = elementBase;
        }
        
        
        
        /*private class LastEventInfo
        {
            private EventType _type;
            private EventModifiers _modifiers;
            private KeyCode _keyCode;
            private ElementBase[] _selection;

            public bool Compare(Event ev, ElementBase[] selection)
            {
                bool areSame = ev != null
                               && this._type == ev.type
                               && this._modifiers == ev.modifiers
                               && this._keyCode == ev.keyCode
                               && this._selection == selection;
                
                Debug.Log(message:
                    "LastEvInfo:\n"
                    + $"    type {_type}\n"
                    + $"    modifiers {_modifiers}\n"
                    + $"    keyCode {_keyCode}\n"
                    + $"    selection {SerSel(_selection)}\n \n"
                    + "This Event Info:\n"
                    + $"    type {ev.type}\n"
                    + $"    modifiers {ev.modifiers}\n"
                    + $"    keyCode {ev.keyCode}\n"
                    + $"    selection {SerSel(selection)}\n \n"
                    + $"Are Same: {areSame}"
                );
                
                return areSame;
            }

            private static string SerSel(ElementBase[] selection)
            {
                if (selection == null) return "NULLLLLLLLLLSel";
                StringBuilder val = new StringBuilder();
                foreach (ElementBase element in selection)
                {
                    if (element == null) val.Append($"=>NULL<=");
                    else val.Append($"=>{element.Data.ElementGuid}<=");
                }
                return val.ToString();
            }
            
            public void Update(Event ev, ElementBase[] selection)
            {
                _type = ev.type;
                _modifiers = ev.modifiers;
                _keyCode = ev.keyCode;
                _selection = selection;
            }
        }*/
    }
}