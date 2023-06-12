using Omnix.Core;

namespace Omnix.Preview
{
    /// <summary>
    /// Layout Drawer for Vertical Layout
    /// </summary>
    public class LdVertical : LayoutDrawerBase
    {
        public LdVertical(LayoutBase master) : base(master) { }
        protected override void UpdateXY(ref float x, ref float y, ElementBase element)
        {
            y += element.TotalRect.height + element.Data.padding.vertical;
        }

        public override void DrawerRefreshSize(float totalWidth)
        {
            totalWidth -= Master.Data.padding.horizontal;
            float height = 0f;
            if (ProcessHeaderSize(ref totalWidth, ref height)) return;
            
            foreach (ElementBase element in Master.Children)
            {
                if (element.IsSkipped) continue;
                element.RefreshSize(totalWidth);
                height += element.TotalRect.height + element.Data.padding.vertical;
            }
            
            Master.TotalRect.width = totalWidth + HeaderIndents;
            Master.TotalRect.height = height;
        }
    }
}