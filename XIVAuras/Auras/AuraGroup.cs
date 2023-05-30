using System.Collections.Generic;
using System.Numerics;
using XIVAuras.Auras;
using XIVAuras.Config;
using XIVAuras.Helpers;
using XIVAuras;

namespace XIVAuras.Auras
{
    public class AuraGroup : AuraListItem, IAuraGroup
    {
        public override AuraType Type => AuraType.Group;

        public AuraListConfig AuraList { get; set; }

        public GroupConfig GroupConfig { get; set; }

        public VisibilityConfig VisibilityConfig { get; set; }

        // Constructor for deserialization
        public AuraGroup() : this(string.Empty) { }

        public AuraGroup(string name) : base(name)
        {
            this.AuraList = new AuraListConfig();
            this.GroupConfig = new GroupConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.AuraList;
            yield return this.GroupConfig;
            yield return this.VisibilityConfig;
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case AuraListConfig newPage:
                    this.AuraList = newPage;
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

            foreach (AuraListItem aura in this.AuraList.Auras)
            {
                aura.StopPreview();
            }
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null, bool parentVisible = true)
        {
            bool visible = this.VisibilityConfig.IsVisible(parentVisible);
            foreach (AuraListItem aura in this.AuraList.Auras)
            {
                if (!this.Preview && this.LastFrameWasPreview)
                {
                    aura.Preview = false;
                }
                else
                {
                    aura.Preview |= this.Preview;
                }

                if (visible || Singletons.Get<PluginManager>().IsConfigOpen())
                {
                    aura.Draw(pos + this.GroupConfig.Position, null, visible);
                }

                if (this.GroupConfig._keepsorted)
                {
                    SortVisible(GroupConfig._iconPos, GroupConfig._iconPos, GroupConfig._recusiveSort, GroupConfig._conditionsSort, GroupConfig._AuraCount);
                }
            }

            this.LastFrameWasPreview = this.Preview;
        }

        public void SortVisible(Vector2 position, Vector2 iconposition, bool recurse, bool conditions, int AuraCount)
        {
            foreach (AuraListItem item in this.AuraList.Auras)
            {
                if (item is AuraIcon icon)
                {
                    bool istriggered = icon.TriggerConfig.IsTriggered(this.Preview, out DataSource[] datas, out int triggeredIndex);
                    if (istriggered)
                    {
                        AuraCount++;
                        icon.Reposition(position, iconposition, conditions, AuraCount);
                    }
                }
                if (item is AuraBar bar)
                {
                    bool istriggered = bar.TriggerConfig.IsTriggered(this.Preview, out DataSource[] datas, out int triggeredIndex);
                    if (istriggered)
                    {
                        AuraCount++;
                        bar.Reposition(position, iconposition, conditions, AuraCount);
                    }
                }
                else if (recurse && item is AuraGroup group)
                {
                    //group.SortIcons(position, barposition, recurse, conditions, AuraCount);
                    // don't allow recursive movement on groups, it breaks everything
                }
            }
        }

        public void SortIcons(Vector2 position, Vector2 iconposition, bool recurse, bool conditions, int AuraCount)
        {
            foreach (AuraListItem item in this.AuraList.Auras)
            {
                AuraCount++;
                if (item is AuraIcon icon)
                {
                    icon.Reposition(position, iconposition, conditions, AuraCount);
                }
                if (item is AuraBar bar)
                {
                    bar.Reposition(position, iconposition, conditions, AuraCount);
                }
                else if (recurse && item is AuraGroup group)
                {
                    //group.SortIcons(position, barposition, recurse, conditions, AuraCount);
                    // don't allow recursive movement on groups, it breaks everything
                }
            }
        }

        public void UnSortIcons(Vector2 position, Vector2 iconposition, bool recurse, bool conditions, int AuraCount)
        {
            foreach (AuraListItem item in this.AuraList.Auras)
            {
                AuraCount++;
                if (item is AuraIcon icon)
                {
                    icon.UnReposition(position, iconposition, conditions, AuraCount);
                }
                if (item is AuraIcon bar)
                {
                    bar.UnReposition(position, iconposition, conditions, AuraCount);
                }
                else if (recurse && item is AuraGroup group)
                {
                    //group.UnSortIcons(position, barposition, recurse, conditions, AuraCount);
                    // don't allow recursive movement on groups, it breaks everything
                }
            }
        }

        public void ResizeIcons(Vector2 size, bool recurse, bool conditions)
        {
            foreach (AuraListItem item in this.AuraList.Auras)
            {
                if (item is AuraIcon icon)
                {
                    icon.Resize(size, conditions);
                }
                else if (recurse && item is AuraGroup group)
                {
                    group.ResizeIcons(size, recurse, conditions);
                }
            }
        }

        public void ScaleResolution(Vector2 scaleFactor, bool positionOnly)
        {
            this.GroupConfig.Position *= scaleFactor;
            foreach (AuraListItem item in this.AuraList.Auras)
            {
                if (item is AuraIcon icon)
                {
                    icon.ScaleResolution(scaleFactor, positionOnly);
                }
                else if (item is AuraGroup group)
                {
                    group.ScaleResolution(scaleFactor, positionOnly);
                }
            }
        }
    }
}