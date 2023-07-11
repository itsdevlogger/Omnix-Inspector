using System;
using System.Collections.Generic;
using Omnix.Core;
using UnityEngine;

namespace Omnix.Data
{
    [Serializable]
    public class LayoutData : DataClassBase
    {
        public bool showHeader;
        public bool isExpanded;
        [SerializeField] public string[] children;


        /// <summary> Used by Unity for deserialization </summary>
        public LayoutData() : base(false)
        {
            this.hideDontDisable = true;
            this.children = Array.Empty<string>();
        }

        public LayoutData(string name, ElementType layoutType) : base(false)
        {
            this.nameInKsHierarchy = name;
            this.hideDontDisable = true;
            this.showIfTargetType = ShowIfTargetType.AlwaysShow;
            this.elementType = layoutType;
            this.children = Array.Empty<string>();
        }

        public override DataClassBase Copy()
        {
            List<string> childrenOfCopy = new List<string>();
            foreach (string child in this.children)
            {
                if (FirstLayoutWrapper.Current.DrawerData.TryGet(child, out DataClassBase childData))
                {
                    childrenOfCopy.Add(childData.Copy().GUID);
                }
            }

            // @formatter:off
            LayoutData copy = new LayoutData($"{nameInKsHierarchy} Copy", elementType)
            {
                showIfTargetType  = this.showIfTargetType,
                styleSheet        = this.styleSheet.Copy(),
                padding           = this.padding,
                callbackTarget    = this.callbackTarget,
                showIfTarget      = this.showIfTarget,
                hideDontDisable   = this.hideDontDisable,
                showHeader        = this.showHeader,
                isExpanded        = this.isExpanded,
                children          = childrenOfCopy.ToArray(),
            };
            
            return copy;
            // @formatter:on
        }
    }
}