using System;
using System.Collections.Generic;
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

            [JsonProperty("UI text")]
            public UiText Text = new UiText();

            [JsonProperty("Messages")]
            public Messages Messages = new Messages();

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

        private sealed class UiText
        {
            public string Title = "Auto Base Upgrade";
            public string AutoUpgradeButton = "Auto-upgrade";
            public string StopUpgradeButton = "Stop upgrade";
            public string Subtitle = "Choose a target grade. Each block is paid from this Tool Cupboard when upgraded.";
            public string Footer = "Higher-grade blocks are never downgraded.";
            public string UpgradeButton = "Upgrade";
            public string PartialUpgradeButton = "Upgrade partial";
            public string CompleteButton = "Complete";
            public string BlocksNeedUpgrade = "{0:N0} of {1:N0} blocks need upgrade";
            public string NoResourcesRequired = "No resources required";
            public string ResourceAvailable = "{0}: Required {1:N0} | In TC {2:N0} | After upgrade {3:N0}";
            public string ResourceMissing = "{0}: Required {1:N0} | In TC {2:N0} | Missing {3:N0}";
        }

        private sealed class Messages
        {
            public string NothingToStop = "Nothing to stop.";
            public string UpgradeStopped = "Upgrade stopped.";
            public string UpgradeAlreadyRunning = "Upgrade already running.";
            public string PlayerAlreadyUpgrading = "You already have an upgrade running.";
            public string NothingToUpgrade = "Nothing to upgrade.";
            public string UpgradeStarted = "Upgrading {0:N0} blocks to {1}.";
            public string NotEnoughResources = "Not enough resources.";
            public string UpgradeComplete = "Upgrade complete: {0:N0} blocks upgraded.";
        }
        protected override void LoadDefaultConfig()
        {
            pluginConfig = new PluginConfig();
            SaveConfig();
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
