using Omnix.Core;
using UnityEditor;
using UnityEngine;

namespace Omnix.Preview
{
    /// <summary>
    /// Layout Drawer for Paged Layout
    /// </summary>
    public class LdPaged : LayoutDrawerBase
    {
        private int _activeCount;
        private int _selectedIndex;
        
        private ElementBase _selectedLayout;
        private Rect _buttonRect;

        public LdPaged(LayoutBase master) : base(master)
        {
            if (master is LayoutPreview preview)
            {
                preview.DiscardChildren(element => element == null || element.IsProperty);
            }
        }

        public override void DrawerRefreshSize(float totalWidth)
        {
            totalWidth -= Master.Data.padding.horizontal;
            float layoutHeight = EditorGUIUtility.singleLineHeight;
            if (ProcessHeaderSize(ref totalWidth, ref layoutHeight)) return;
            

            if (Master.ChildCount == 0)
            {
                Master.TotalRect.width = totalWidth;
                Master.TotalRect.height = layoutHeight * 2f;
                return;
            }

            _activeCount = 0;
            foreach (ElementBase child in Master.Children)
            {
                if (child.IsSkipped) continue;

                if (_activeCount == _selectedIndex)
                {
                    child.RefreshSize(totalWidth);
                    layoutHeight += child.TotalRect.height + child.Data.padding.vertical;
                    _selectedLayout = child;
                }
                _activeCount++;
            }
            Master.TotalRect.width = totalWidth + HeaderIndents;
            Master.TotalRect.height = layoutHeight;
        }

        public override void DrawerRefreshPosition(float x, float y)
        {
            x += Master.Data.padding.left;
            y += Master.Data.padding.top;
            
            if (ProcessHeaderPos(ref x, ref y)) return;
            if (Master.ChildCount == 0) return;

            float buttonWidth = Master.TotalRect.width / _activeCount;
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            _buttonRect = new Rect(x, y, buttonWidth, buttonHeight);
            _selectedLayout.RefreshPosition(x, y + buttonHeight);
        }

        public override void DrawerDraw()
        {
            if (Master.ChildCount == 0)
            {
                EditorGUI.LabelField(Master.TotalRect, "Empty Paged Layout.\nPaged Layout only accepts another Layout as a child.");
                return;
            }

            if (Master.MyData.showHeader)
            {
                Master.MyData.isExpanded = EditorGUI.Foldout(Master.HeaderRect, Master.MyData.isExpanded, Master.ToString());
                if (!Master.MyData.isExpanded)
                {
                    return;
                }
            }
            
            int index = 0;
            Rect buttonRect = new Rect(_buttonRect);
            foreach (ElementBase child in Master.Children)
            {
                if (child.IsSkipped) continue;
                if (index != _selectedIndex)
                {
                    if (GUI.Button(buttonRect, child.ToString()))
                    {
                        _selectedIndex = index;
                        _selectedLayout = child;
                    }
                }
                else
                {
                    Color color = GUI.backgroundColor;
                    GUI.backgroundColor = Color.blue;
                    GUI.Button(buttonRect, child.ToString());
                    GUI.backgroundColor = color;
                }
                buttonRect.x += buttonRect.width;
                index++;
            }
            if (_selectedLayout != null) _selectedLayout.Draw();
        }
    }
}