using System;
using System.Collections.Generic;
using System.Linq;
using Omnix.Core;
using Omnix.Data;
using Omnix.Preview;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace Omnix.Windows
{
    public class OmnixTreeView : TreeView
    {
        private int _countdownToMenu;

        public OmnixTreeView(TreeViewState state) : base(state)
        {
            Reload();
        }

        protected override bool CanStartDrag(CanStartDragArgs args) => true;
        protected override bool CanRename(TreeViewItem item) => true;
        protected override bool CanMultiSelect(TreeViewItem item) => true;
        protected override bool CanBeParent(TreeViewItem item) => !FirstLayoutWrapper.GetElementFromId(item.id).IsProperty;

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem { id = 0, depth = -1 };
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            IList<TreeViewItem> rows = GetRows() ?? new List<TreeViewItem>(200);
            rows.Clear();
            LayoutPreview rootElement = (LayoutPreview)FirstLayoutWrapper.Current.FirstLayout;
            TreeViewItem item = new TreeViewItem(rootElement.MyID, -1, rootElement.ToString());
            root.AddChild(item);
            rows.Add(item);
            SetExpanded(item.id, true);
            AddChildrenRecursive(rootElement, item, rows);
            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }

        protected override IList<int> GetAncestors(int id)
        {
            ElementBase element = FirstLayoutWrapper.GetElementFromId(id);

            List<int> ancestors = new List<int>();
            while (element.Parent != null)
            {
                ancestors.Add(element.Parent.MyID);
                element = element.Parent;
            }
            return ancestors;
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            Stack<ElementBase> stack = new Stack<ElementBase>();

            ElementBase start = FirstLayoutWrapper.GetElementFromId(id);
            stack.Push(start);

            List<int> parents = new List<int>();
            while (stack.Count > 0)
            {
                ElementBase current = stack.Pop();
                if (!(current is LayoutPreview layout) || layout.ChildCount == 0) continue;
                parents.Add(layout.MyID);

                foreach (ElementBase child in layout.Children)
                {
                    stack.Push(child);
                }
            }

            return parents;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count != 0)
                OmnixHierarchyWindow.__OnSelectionChanged(FirstLayoutWrapper.GetElementFromId(selectedIds[0]));
            else
                OmnixHierarchyWindow.__OnSelectionChanged(null);
            Helpers.RepaintAllWindows();
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            IList<int> sortedDraggedIDs = SortItemIDsInRowOrder(args.draggedItemIDs);

            List<ElementBase> objList = new List<ElementBase>(sortedDraggedIDs.Count);
            foreach (int id in sortedDraggedIDs)
            {
                ElementBase obj = FirstLayoutWrapper.GetElementFromId(id);
                if (obj != null)
                    objList.Add(obj);
            }
            DragAndDrop.SetGenericData("OmnixD&D", objList); // = objList.ToArray();
            string title = objList.Count > 1 ? "<Multiple>" : objList[0].ToString();
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (!args.performDrop) return DragAndDropVisualMode.Move;

            object genericData = DragAndDrop.GetGenericData("OmnixET");
            if (genericData is ElementType elementType)
            {
                switch (args.dragAndDropPosition)
                {
                    case DragAndDropPosition.UponItem:
                    case DragAndDropPosition.BetweenItems:
                        ElementBase parent = args.parentItem != null ? FirstLayoutWrapper.GetElementFromId(args.parentItem.id) : null;
                        if (!(parent is LayoutPreview parentLayout)) return DragAndDropVisualMode.None;

                        if (args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
                        {
                            elementType.NewPreviewInstance(parentLayout, args.insertAtIndex);
                        }
                        else
                        {
                            elementType.NewPreviewInstance(parentLayout, -1);
                        }
                        break;

                    case DragAndDropPosition.OutsideItems:
                        elementType.NewPreviewInstance(OmnixWindowBase.Actives.FirstLayoutPreview, -1);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
                OmnixWindowBase.Actives.Preview.ObjectDropped();
                Helpers.RepaintAllWindows();
                return DragAndDropVisualMode.Move;
            }


            List<ElementBase> draggedObjects = (List<ElementBase>)DragAndDrop.GetGenericData("OmnixD&D");
            bool wasAccepted;
            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                    ElementBase parent = args.parentItem != null ? FirstLayoutWrapper.GetElementFromId(args.parentItem.id) : null;
                    if (!(parent is LayoutPreview parentLayout)) return DragAndDropVisualMode.None;
                    wasAccepted = DropTo(parentLayout);
                    break;

                case DragAndDropPosition.OutsideItems:
                    wasAccepted = DropTo(OmnixWindowBase.Actives.FirstLayoutPreview);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!wasAccepted) return DragAndDropVisualMode.Rejected;

            Helpers.RepaintAllWindows();
            return DragAndDropVisualMode.Move;

            // returns true if the drop is accepted
            bool DropTo(LayoutPreview parentLayout)
            {
                draggedObjects.RemoveAll((child => IsParentChildOfDragger(child, parentLayout)));
                if (draggedObjects.Count == 0) return false;
                foreach (ElementBase element in draggedObjects)
                {
                    ((LayoutPreview)element.Parent).RemoveChild(element);
                    parentLayout.AppendChild(element);
                }

                if (args.dragAndDropPosition != DragAndDropPosition.BetweenItems) return true;

                int insertIndex = args.insertAtIndex;
                foreach (ElementBase element in draggedObjects)
                {
                    ((LayoutPreview)element.Parent).ChangeChildIndex(element, insertIndex);
                    insertIndex++;
                }
                return true;
            }
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            ElementBase elementBase = FirstLayoutWrapper.GetElementFromId(args.itemID);
            elementBase.Data.nameInKsHierarchy = args.newName;
            Helpers.RepaintAllWindows();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            Event current = Event.current;

            if (_countdownToMenu != -1)
            {
                if (_countdownToMenu == 0)
                {
                    _countdownToMenu = -1;
                    TreeMenu.CreateMenu(this);
                }
                else _countdownToMenu--;
            }

            if (current.isMouse && current.button == 1 && args.rowRect.Contains(current.mousePosition))
            {
                _countdownToMenu = 3;
                SelectionClick(args.item, true);
                Repaint();
            }
        }

        private void AddChildrenRecursive(LayoutPreview element, TreeViewItem item, IList<TreeViewItem> rows)
        {
            item.children = new List<TreeViewItem>(element.ChildCount);
            foreach (ElementBase child in element.Children)
            {
                TreeViewItem childItem = new TreeViewItem(child.MyID, -1, child.ToString());
                item.AddChild(childItem);
                rows.Add(childItem);
                if (!(child is LayoutPreview childLayout) || childLayout.ChildCount <= 0) continue;

                if (IsExpanded(childItem.id))
                    AddChildrenRecursive(childLayout, childItem, rows);
                else
                    childItem.children = CreateChildListForCollapsedParent();
            }
        }

        private static bool IsParentChildOfDragger(ElementBase dragged, LayoutPreview parent)
        {
            ElementBase t = parent.Parent;
            while (t != null)
            {
                if (t.MyID == dragged.MyID)
                    return true;
                t = t.Parent;
            }
            return false;
        }

        public void BeginRenameOf(ElementBase element) => BeginRename(FindRows(new List<int>() { element.MyID })[0]);

        public ElementBase[] GetSelectionElements()
        {
            if (!HasSelection()) return null;

            IList<int> selection = GetSelection();
            ElementBase[] elements = new ElementBase[selection.Count];
            int index = 0;
            foreach (int id in selection)
            {
                ElementBase element = FirstLayoutWrapper.GetElementFromId(id);
                if (element.Parent == null) continue;
                elements[index] = element;
                index++;
            }
            return elements;
        }
    }


    public static class TreeMenu
    {
        private static OmnixTreeView _tree;
        private static GenericMenu _menu;
        private static IList<int> _selection;
        private static bool _hasExactlyOneSelected;

        private static ElementBase FirstElementInSelection => FirstLayoutWrapper.GetElementFromId(_selection[0]);

        public static void CreateMenu(OmnixTreeView omnixTreeView)
        {
            _tree = omnixTreeView;
            _menu = new GenericMenu();
            _selection = _tree.GetSelection();
            _hasExactlyOneSelected = (_selection.Count == 1);
            PopulateMenu();
            _menu.ShowAsContext();
        }

        #region Populate Menu
        // ReSharper disable once PossibleNullReferenceException
        private static void PopulateMenu()
        {
            bool isCutCopyEnabled = _selection.Count > 0;

            if (isCutCopyEnabled) Pop_CutCopy(CutOrCopy);
            if (_hasExactlyOneSelected && TreeFunc.CanPaste)
            {
                ElementBase selected = FirstElementInSelection;
                if (selected != OmnixWindowBase.Actives.FirstLayoutPreview) _menu.AddItem(new GUIContent("Paste as Neighbour"), on: false, func: PasteAsNeighbour);
                if (!selected.IsProperty) _menu.AddItem(new GUIContent("Paste as Child"), on: false, func: PasteAsChild);
            }

            _menu.AddSeparator("");
            if (_hasExactlyOneSelected) _menu.AddItem(new GUIContent("Rename"), on: false, func: Rename);
            if (_selection.Count > 0)
            {
                _menu.AddItem(new GUIContent("Duplicate"), on: false, func: Duplicate);
                _menu.AddItem(new GUIContent("Delete"), on: false, func: Delete);
            }
        }

        private static void Pop_CutCopy(GenericMenu.MenuFunction2 callback)
        {
            _menu.AddItem(new GUIContent("Cut"), on: false, func: callback, "cut");
            _menu.AddItem(new GUIContent("Copy"), on: false, func: callback, "copy");
        }
        #endregion

        #region Funcitonality
        private static void Rename() => _tree.BeginRenameOf(FirstElementInSelection);
        private static void PasteAsChild() => TreeFunc.PasteAsChild((LayoutPreview)FirstElementInSelection);
        private static void PasteAsNeighbour() => TreeFunc.PasteAsNeighbour(FirstElementInSelection);
        private static void Duplicate() => TreeFunc.Duplicate(_tree.GetSelectionElements());
        private static void Delete() => TreeFunc.Delete(_tree.GetSelectionElements());
        private static void CutOrCopy(object meta)
        {
            if ((string)meta == "cut") TreeFunc.Cut(_tree.GetSelectionElements());
            else TreeFunc.Copy(_tree.GetSelectionElements());
        }
        #endregion

    }

    public static class TreeFunc
    {
        private static ElementBase[] _elementsToPaste;
        private static bool _wasCopied;

        public static bool CanPaste => (_elementsToPaste != null) && (_elementsToPaste.Length > 0);

        public static void PasteAsChild(LayoutPreview newParent, int insertAt = -1)
        {
            if (!CanPaste) return;
            if (_wasCopied)
            {
                Duplicate(_elementsToPaste);
            }
            else
            {
                foreach (ElementBase elementBase in _elementsToPaste)
                {
                    newParent.InsertChild(elementBase, insertAt);
                    insertAt++;
                }
                _elementsToPaste = null;
            }

            Helpers.CallSaveData(false);
            Helpers.RepaintAllWindows();
        }

        public static void PasteAsNeighbour(ElementBase neighbourOf)
        {
            if (!CanPaste) return;
            if (neighbourOf.Parent == null) return;

            LayoutPreview parent = (LayoutPreview)neighbourOf.Parent;
            int currentIndex = parent.IndexOf(neighbourOf);
            PasteAsChild(parent, currentIndex);
        }

        public static void Copy(params ElementBase[] elements)
        {
            _wasCopied = true;
            _elementsToPaste = Filter(elements).ToArray();
        }

        public static void Cut(params ElementBase[] elements)
        {
            Copy(elements);
            Delete(elements);
            _wasCopied = false;
            Helpers.RepaintAllWindows();
        }

        public static ElementBase[] Duplicate(params ElementBase[] elements)
        {
            Helpers.CallSaveData(closingAllWindows: false);
            List<ElementBase> duplicates = new List<ElementBase>();

            foreach (ElementBase element in Filter(elements))
            {
                DataClassBase dupData = element.Data.Copy();
                ElementBase dupEl = ElementBase.NewPreviewElement(dupData, (LayoutPreview)element.Parent);
                duplicates.Add(dupEl);
            }
            Helpers.CallSaveData(false);
            Helpers.RepaintAllWindows();
            return duplicates.ToArray();
        }

        private static IEnumerable<ElementBase> Filter(ElementBase[] elements)
        {
            HashSet<string> allIDs = new HashSet<string>();
            foreach (ElementBase element in elements)
            {
                allIDs.Add(element.Data.GUID);
            }
            return elements.Where(element => !HasParentSelected(element) && element.Parent != null);

            bool HasParentSelected(ElementBase el)
            {
                LayoutBase parent = el.Parent;
                while (parent != null)
                {
                    if (allIDs.Contains(parent.Data.GUID)) return true;
                    parent = parent.Parent;
                }
                return false;
            }
        }

        public static void Delete(params ElementBase[] elements)
        {
            foreach (ElementBase element in elements)
            {
                if (element?.Parent is LayoutPreview parent) // same as element != null && element.Parent != null && element.Parent.GetType() == typeof(LayoutPreview)
                    parent.RemoveChild(element);
            }
            Helpers.CallSaveData(false);
            Helpers.RepaintAllWindows();
        }
    }
}