using System.Collections.Generic;
using System.Numerics;
using ReBuff.Config;
using ReBuff.Helpers;

namespace ReBuff.Widgets
{
    public class WidgetGroup : WidgetListItem, IWidgetGroup
    {
        public override WidgetType Type => WidgetType.Group;

        public WidgetListConfig WidgetList { get; set; }

        public GroupConfig GroupConfig { get; set; }

        public VisibilityConfig VisibilityConfig { get; set; }

        // Constructor for deserialization
        public WidgetGroup() : this(string.Empty) { }

        public WidgetGroup(string name) : base(name)
        {
            this.WidgetList = new WidgetListConfig();
            this.GroupConfig = new GroupConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.WidgetList;
            yield return this.GroupConfig;
            yield return this.VisibilityConfig;
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case WidgetListConfig newPage:
                    this.WidgetList = newPage;
                    break;
                case GroupConfig newPage:
                    this.GroupConfig = newPage;
                    break;
                case VisibilityConfig newPage:
                    this.VisibilityConfig = newPage;
                    break;
            }
        }

        public override void StopPreview()
        {
            base.StopPreview();

            foreach (WidgetListItem widget in this.WidgetList.Widgets)
            {
                widget.StopPreview();
            }
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null, bool parentVisible = true)
        {
            bool visible = this.VisibilityConfig.IsVisible(parentVisible);
            foreach (WidgetListItem widget in this.WidgetList.Widgets)
            {
                if (!this.Preview && this.LastFrameWasPreview)
                {
                    widget.Preview = false;
                }
                else
                {
                    widget.Preview |= this.Preview;
                }

                if (visible || Singletons.Get<PluginManager>().IsConfigOpen())
                {
                    widget.Draw(pos + this.GroupConfig.Position, null, visible);
                }

                if (this.GroupConfig._keepsorted)
                { 
                    SortVisible(GroupConfig._iconPos, GroupConfig._iconPos, GroupConfig._recusiveSort, GroupConfig._conditionsSort, GroupConfig._WidgetCount);
                }
            }

            this.LastFrameWasPreview = this.Preview;
        }

        public void SortVisible(Vector2 position, Vector2 iconposition, bool recurse, bool conditions, int WidgetCount)
        {
            foreach (WidgetListItem item in this.WidgetList.Widgets)
            {
                if (item is WidgetIcon icon)
                {
                    bool istriggered = icon.TriggerConfig.IsTriggered(this.Preview, out DataSource[] datas, out int triggeredIndex);
                    if (istriggered)
                    {
                        WidgetCount++;
                        icon.Reposition(position, iconposition, conditions, WidgetCount);
                    }
                }
                if (item is WidgetBar bar)
                {
                    bool istriggered = bar.TriggerConfig.IsTriggered(this.Preview, out DataSource[] datas, out int triggeredIndex);
                    if (istriggered)
                    {
                        WidgetCount++;
                        bar.Reposition(position, iconposition, conditions, WidgetCount);
                    }
                }
                else if (recurse && item is WidgetGroup group)
                {
                    //group.SortIcons(position, barposition, recurse, conditions, WidgetCount);
                    // don't allow recursive movement on groups, it breaks everything
                }
            }
        }

        public void SortIcons(Vector2 position, Vector2 iconposition, bool recurse, bool conditions, int WidgetCount)
        {
            foreach (WidgetListItem item in this.WidgetList.Widgets)
            {
                WidgetCount++;
                if (item is WidgetIcon icon)
                {
                    icon.Reposition(position, iconposition, conditions, WidgetCount);
                }
                if (item is WidgetBar bar)
                {
                    bar.Reposition(position, iconposition, conditions, WidgetCount);
                }
                else if (recurse && item is WidgetGroup group)
                {
                    //group.SortIcons(position, barposition, recurse, conditions, WidgetCount);
                    // don't allow recursive movement on groups, it breaks everything
                }
            }
        }

        public void UnSortIcons(Vector2 position, Vector2 iconposition, bool recurse, bool conditions, int WidgetCount)
        {
            foreach (WidgetListItem item in this.WidgetList.Widgets)
            {
                WidgetCount++;
                if (item is WidgetIcon icon)
                {
                    icon.UnReposition(position, iconposition, conditions, WidgetCount);
                }
                if (item is WidgetIcon bar)
                {
                    bar.UnReposition(position, iconposition, conditions, WidgetCount);
                }
                else if (recurse && item is WidgetGroup group)
                {
                    //group.UnSortIcons(position, barposition, recurse, conditions, WidgetCount);
                    // don't allow recursive movement on groups, it breaks everything
                }
            }
        }

        public void ResizeIcons(Vector2 size, bool recurse, bool conditions)
        {
            foreach (WidgetListItem item in this.WidgetList.Widgets)
            {
                if (item is WidgetIcon icon)
                {
                    icon.Resize(size, conditions);
                }
                else if (recurse && item is WidgetGroup group)
                {
                    group.ResizeIcons(size, recurse, conditions);
                }
            }
        }

        public void ScaleResolution(Vector2 scaleFactor, bool positionOnly)
        {
            this.GroupConfig.Position *= scaleFactor;
            foreach (WidgetListItem item in this.WidgetList.Widgets)
            {
                if (item is WidgetIcon icon)
                {
                    icon.ScaleResolution(scaleFactor, positionOnly);
                }
                else if (item is WidgetGroup group)
                {
                    group.ScaleResolution(scaleFactor, positionOnly);
                }
            }
        }
    }
}