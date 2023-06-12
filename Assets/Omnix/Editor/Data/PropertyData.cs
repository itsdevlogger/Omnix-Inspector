using System;
using UnityEngine;
namespace Omnix.Data
{
    /// <summary>
    /// Data Class for a Property
    /// </summary>
    [Serializable]
    public class PropertyData : DataClassBase
    {
        public SizeType widthType;
        public SizeType heightType;
        public float widthValue;
        public float heightValue;
        public string target;
        public GUIContent content;

        public string extraData;

        /// <summary> Used by Unity for deserialization </summary>
        public PropertyData() : base(false)
        {
            this.content = new GUIContent("");
        }

        public PropertyData(ElementType propertyType, string name, string target) : base(false)
        {
            this.elementType = propertyType;
            this.nameInKsHierarchy = name;
            this.content = new GUIContent(name);
            this.target = target;
        }
        
        
        public override DataClassBase Copy()
        {
            return new PropertyData(this.elementType, this.nameInKsHierarchy + " Copy", this.target)
            {
                showIfTargetType  = this.showIfTargetType,
                styleSheet        = this.styleSheet.Copy(),
                padding           = this.padding,
                callbackTarget    = this.callbackTarget,
                showIfTarget      = this.showIfTarget,
                hideDontDisable   = this.hideDontDisable,
                widthType         = this.widthType,
                heightType        = this.heightType,
                widthValue        = this.widthValue,
                heightValue       = this.heightValue,
                target            = this.target,
                content           = this.content,
                extraData         = this.extraData,
            };
        }
    }
}