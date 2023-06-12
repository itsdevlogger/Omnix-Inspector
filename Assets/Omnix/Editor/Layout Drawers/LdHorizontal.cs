using System;
using Omnix.Core;
using Omnix.Data;

namespace Omnix.Preview
{
    /// <summary>
    /// Layout Drawer for Horizontal Layout
    /// </summary>
    public class LdHorizontal : LayoutDrawerBase
    {
        public LdHorizontal(LayoutBase master) : base(master) { }
        protected override void UpdateXY(ref float x, ref float y, ElementBase element)
        {
            x += element.TotalRect.width + element.Data.padding.horizontal;
        }

        public override void DrawerRefreshSize(float totalWidth)
        {
            totalWidth -= Master.Data.padding.horizontal;
            float layoutHeight = 0f;
            if (ProcessHeaderSize(ref totalWidth, ref layoutHeight)) return;

            int autoWidthCount = 0;
            float autoWidthSpace = totalWidth;

            foreach (ElementBase element in Master.Children)
            {
                if (element.IsSkipped) continue;

                if (!element.IsProperty)
                {
                    autoWidthCount++;
                    continue;
                }

                PropertyBase prop = (PropertyBase)element;
                switch (prop.MyData.widthType)
                {
                    case SizeType.Auto:
                        autoWidthCount++;
                        break;
                    case SizeType.Pixels:
                        autoWidthSpace -= prop.MyData.widthValue;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            float oneWidth = autoWidthSpace / autoWidthCount;
            foreach (ElementBase element in Master.Children)
            {
                if (element.IsSkipped) continue;

                element.RefreshSize(oneWidth);
                float eleHeight = element.TotalRect.height + element.Data.padding.vertical;
                
                if (eleHeight > layoutHeight)
                {
                    layoutHeight = eleHeight;
                }
            }

            Master.TotalRect.width = totalWidth + HeaderIndents;
            Master.TotalRect.height = layoutHeight;
        }
    }
}