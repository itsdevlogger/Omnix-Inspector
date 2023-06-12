using System;
using System.Collections.Generic;
using Omnix.Data;
using Omnix.Preview;
using UnityEngine;

namespace Omnix.Core
{
    /// <summary>
    /// Base class for Layouts.
    /// There are only two types of Layout, Preview Layout and Unity Inspector Layout.
    /// No need to derive from this class, derive from <see cref="LayoutDrawerBase"/> instead.
    /// </summary>
    public abstract class LayoutBase : ElementBase
    {
        public override sealed bool IsProperty => false;
        public readonly LayoutData MyData;
        
        /// <summary> List of all the childrens in this layouts </summary>
        public abstract IEnumerable<ElementBase> Children { get; }

        /// <summary> How many children this Layout Has. </summary>
        public abstract int ChildCount { get; }
        
        /// <summary> Drawer that will be used to actually draw this layout </summary>
        protected LayoutDrawerBase Drawer;
        
        /// <summary> Rect used to draw layout header </summary>
        public Rect HeaderRect;


        /// <summary>
        /// Not abstract but its assumed that every Layout sets this variable in their constructor.
        /// </summary>
        public ElementType LayoutType
        {
            get => Data.elementType;
            set
            {
                Drawer = GetDrawer(value);
                DrawAction = Drawer.DrawerDraw;
                Data.elementType = value;
            }
        }

        protected LayoutBase(LayoutBase parent, DataClassBase data) : base(data, parent)
        {
            this.MyData = (LayoutData) this.Data;
        }
        
        // %Layout% means every Layout must be registered in this method
        private LayoutDrawerBase GetDrawer(ElementType layoutType)
        {
            return layoutType switch
            {
                ElementType.VerticalLayout => new LdVertical(this),
                ElementType.HorizontalLayout => new LdHorizontal(this),
                ElementType.PagedLayout => new LdPaged(this),
                _ => throw new ArgumentOutOfRangeException(nameof(layoutType), layoutType, null)
            };
        }

        public override void RefreshSize(float width) => Drawer.DrawerRefreshSize(width);
        public override void RefreshPosition(float x, float y) => Drawer.DrawerRefreshPosition(x, y);

        public override void PrintInfo(string indentLevel)
        {
            Debug.Log($"{indentLevel}{Data.elementType} {Data.GUID} Children:");
            indentLevel = $"{indentLevel}    ";
            foreach (var child in Children)
            {
                child.PrintInfo(indentLevel);
            }
        }
    }
}