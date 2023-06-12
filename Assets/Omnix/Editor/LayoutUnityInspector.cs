using System.Collections.Generic;
using Omnix.Core;
using Omnix.Data;
using UnityEngine;
namespace Omnix.Preview
{
    public class LayoutUnityInspector : LayoutBase
    {
        public override IEnumerable<ElementBase> Children => _children;
        public override int ChildCount => _children.Length;

        private readonly ElementBase[] _children;


        public LayoutUnityInspector(LayoutData layoutData, LayoutBase parent) : base(parent, layoutData)
        {
            _children = new ElementBase[layoutData.children.Length];
            LayoutType = layoutData.elementType;
            int index = 0;
            foreach (string child in layoutData.children)
            {
                if (FirstLayoutWrapper.Current.DrawerData.TryGet(child, out DataClassBase childData))
                {
                    _children[index] = NewUIElement(childData, this);;
                    index++;
                }
            }
        }

        
        public override void DrawOmnixInspector() { }
    }
}