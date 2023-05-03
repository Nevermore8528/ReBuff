using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReBuff.Helpers;

namespace ReBuff.Config
{
    [JsonObject]
    public class ReBuffConfig : IWidgetGroup, IConfigurable, IPluginDisposable
    {
        public string Name
        {
            get => "ReBuff";
            set { }
        }

        public string Version => Plugin.Version;

        public WidgetListConfig WidgetList { get; set; }

        public GroupConfig GroupConfig { get; set; }

        public VisibilityConfig VisibilityConfig { get; set; }

        public FontConfig FontConfig { get; set; }

        [JsonIgnore]
        private AboutPage AboutPage { get; } = new AboutPage();

        public ReBuffConfig()
        {
            this.WidgetList = new WidgetListConfig();
            this.GroupConfig = new GroupConfig();
            this.VisibilityConfig = new VisibilityConfig();
            this.FontConfig = new FontConfig();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ConfigHelpers.SaveConfig(this);
            }
        }

        public override string ToString() => this.Name;

        public IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.WidgetList;
            yield return this.GroupConfig;
            yield return this.VisibilityConfig;
            yield return this.FontConfig;
            yield return this.AboutPage;
        }

        public void ImportPage(IConfigPage page)
        {
        }
    }
}