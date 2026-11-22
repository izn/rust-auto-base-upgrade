using System;
using Newtonsoft.Json;

namespace Carbon.Plugins
{
    public partial class AutoBaseUpgrade
    {
        private PluginConfig pluginConfig;

        private sealed class PluginConfig
        {
            [JsonProperty("UI colors")]
            public UiColors Colors = new UiColors();

        }

        private sealed class UiColors
        {
            public string Panel = "#0E1114FA";
            public string Row = "#182021FA";
            public string Button = "#527A33FA";
            public string StopButton = "#8C2E24F2";
            public string DisabledButton = "#383D40E6";
            public string Text = "#E0EDF5FF";
            public string MutedText = "#9EAEB7FF";
            public string ErrorText = "#ED7A6BFF";
            public string SuccessText = "#B8D1B3FF";
            public string Wood = "#C2854DFF";
            public string Stone = "#ADB2B7FF";
            public string Metal = "#7AAECBFF";
            public string Armored = "#D1D1C2FF";
        }

        protected override void LoadDefaultConfig()
        {
            pluginConfig = new PluginConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                pluginConfig = Config.ReadObject<PluginConfig>();
                if (pluginConfig == null)
                    throw new InvalidOperationException("Configuration is empty.");
            }
            catch (Exception exception)
            {
                PrintError("Failed to load configuration: " + exception.Message);
                LoadDefaultConfig();
            }

            SaveConfig();
        }


        protected override void SaveConfig()
        {
            Config.WriteObject(pluginConfig, true);
        }
    }
}
