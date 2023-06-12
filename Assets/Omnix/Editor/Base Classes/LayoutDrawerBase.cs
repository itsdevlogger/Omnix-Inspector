using System;
using Omnix.Data;
using UnityEditor;

namespace Omnix.Core
{
    public abstract class LayoutDrawerBase
    {
        protected static readonly float HeaderHeight = 18f;
        protected static readonly float HeaderIndents = 18f;

        protected LayoutBase Master;
        protected LayoutDrawerBase(LayoutBase master) => Master = master;

        /// <summary>
        /// Same as <see cref="ElementBase.RefreshSize"/>
        /// </summary>
        public abstract void DrawerRefreshSize(float totalWidth);

        /// <summary>
        /// Same as <see cref="ElementBase.RefreshPosition"/>
        /// </summary>
        public virtual void DrawerRefreshPosition(float x, float y)
        {
            x += Master.Data.padding.left;
            y += Master.Data.padding.top;

            if (ProcessHeaderPos(ref x, ref y)) return;

            foreach (ElementBase element in Master.Children)
            {
                if (element.IsSkipped) continue;
                element.RefreshPosition(x, y);
                UpdateXY(ref x, ref y, element);
            }
        }

        /// <returns> true if this layout accept the child of provided type false otherwise</returns>
        public virtual bool AcceptsChild(ElementType elementType) => true;

        /// <summary>
        /// Draw this layout
        /// </summary>
        public virtual void DrawerDraw()
        {
            if (Master.MyData.showHeader)
            {
                Master.MyData.isExpanded = EditorGUI.Foldout(Master.HeaderRect, Master.MyData.isExpanded, Master.ToString());
                if (!Master.MyData.isExpanded)
                {
                    return;
                }
            }

            foreach (ElementBase child in Master.Children)
            {
                child.Draw();
            }
        }

        /// <summary>
        /// Update the variables x and y based on the given element's width and height. 
        /// These (updated) x and y will be used to draw the next element in the children's list
        /// </summary>
        protected virtual void UpdateXY(ref float x, ref float y, ElementBase element)
        {
            throw new Exception("If you are not implementing DrawerRefreshPosition, you must implement UpdateRect");
        }

        /// <summary> Updates layout width and height based on Layout Header Size. </summary>
        /// <returns> true if the header is collapsed </returns>
        protected bool ProcessHeaderSize(ref float totalWidth, ref float totalHeight)
        {
            if (!Master.MyData.showHeader) return false;

            Master.HeaderRect.width = totalWidth;
            Master.HeaderRect.height = HeaderHeight;
            totalWidth -= HeaderIndents;
            totalHeight += HeaderHeight;

            if (Master.MyData.isExpanded) return false;
            
            Master.TotalRect.width = Master.HeaderRect.width;
            Master.TotalRect.height = Master.HeaderRect.height;
            return true;
        }

        /// <summary> Updates layout width and height based on Layout Header Position. </summary>
        /// <returns>true if the header is collapsed</returns>
        protected bool ProcessHeaderPos(ref float x, ref float y)
        {
            if (!Master.MyData.showHeader) return false;
            
            Master.HeaderRect.x = Master.TotalRect.x = x;
            Master.HeaderRect.y = Master.TotalRect.y = y;
            x += HeaderIndents;
            y += HeaderHeight;

            return !Master.MyData.isExpanded;
        }
    }
}