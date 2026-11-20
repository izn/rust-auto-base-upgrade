using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Carbon.Components;
using UnityEngine;

namespace Carbon.Plugins
{
    public partial class AutoBaseUpgrade
    {
        private UpgradeUi upgradeUi;
        private const string ButtonUi = "AutoBaseUpgrade.Button";
        private const string ButtonTextUi = ButtonUi + ".Text";
        private const string MenuUi = "AutoBaseUpgrade.Menu";
        private readonly HashSet<ulong> visibleButtonPlayers = new HashSet<ulong>();

        private string UiColor(string hex)
        {
            return CUI.HexToRustColor(hex);
        }

        private sealed class UiButtonFactory
        {
            private readonly AutoBaseUpgrade plugin;

            public UiButtonFactory(AutoBaseUpgrade plugin)
            {
                this.plugin = plugin;
            }

            public UiButtonDefinition Cupboard(BuildingPrivlidge cupboard)
            {
                bool isRunning = plugin.runningUpgradesByBuilding.ContainsKey(cupboard.buildingID);
                if (isRunning)
                    return Create(plugin.pluginConfig.Text.StopUpgradeButton, "autobaseupgrade.stop",
                        plugin.pluginConfig.Colors.StopButton, 10);

                return Create(plugin.pluginConfig.Text.AutoUpgradeButton, "autobaseupgrade.open",
                    plugin.pluginConfig.Colors.Button, 10);
            }

            public UiButtonDefinition Upgrade(BuildingGrade.Enum grade, bool hasBlocksToUpgrade,
                bool canPayAll, bool canPayAny)
            {
                if (!hasBlocksToUpgrade)
                    return Create(plugin.pluginConfig.Text.CompleteButton, null,
                        plugin.pluginConfig.Colors.DisabledButton, 11);

                if (!canPayAny)
                    return Create(plugin.pluginConfig.Text.NoResources, null,
                        plugin.pluginConfig.Colors.DisabledButton, 11);

                return Create(GetUpgradeText(canPayAll), "autobaseupgrade.start " + (int)grade,
                    plugin.pluginConfig.Colors.Button, 11);
            }

            private string GetUpgradeText(bool canPay)
            {
                if (canPay)
                    return plugin.pluginConfig.Text.UpgradeButton;

                return plugin.pluginConfig.Text.PartialUpgradeButton;
            }
            private UiButtonDefinition Create(string text, string command, string color, int fontSize)
            {
                return new UiButtonDefinition
                {
                    Text = text,
                    Command = command,
                    Color = plugin.UiColor(color),
                    FontSize = fontSize
                };
            }
        }

        private sealed class UpgradeUi
        {
            private const float GradeRowLeft = 0.035f;
            private const float GradeRowRight = 0.965f;
            private const float GradeRowFirstTop = 0.80f;
            private const float GradeRowSpacing = 0.185f;
            private const float GradeRowHeight = 0.155f;

            private readonly AutoBaseUpgrade plugin;
            private readonly UiButtonFactory buttons;

            public UpgradeUi(AutoBaseUpgrade plugin)
            {
                this.plugin = plugin;
                buttons = new UiButtonFactory(plugin);
            }

            public void ShowAutoUpgradeButton(BasePlayer player, BuildingPrivlidge cupboard)
            {
                UiButtonDefinition button = buttons.Cupboard(cupboard);

                using (CUI cui = plugin.CreateCUI())
                {
                    if (plugin.visibleButtonPlayers.Contains(player.userID))
                    {
                        UpdateCupboardButton(cui, player, button);
                        return;
                    }

                    AddButton(cui, "Overlay", button, LuiPosition.LowerCenter,
                        new LuiOffset(476.5f, 599.5f, 572.5f, 620.5f), ButtonUi, ButtonTextUi);
                    cui.v2.SendUi(player);
                    plugin.visibleButtonPlayers.Add(player.userID);
                }
            }

            private static void UpdateCupboardButton(CUI cui, BasePlayer player,
                UiButtonDefinition button)
            {
                cui.v2.UpdateColor(ButtonUi, button.Color);
                cui.v2.UpdateButtonCommand(ButtonUi, button.Command);
                cui.v2.UpdateText(ButtonTextUi, button.Text, button.FontSize);
                cui.v2.SendUi(player);
            }

            public void RefreshCupboardButtons(BuildingPrivlidge cupboard)
            {
                foreach (BasePlayer player in BasePlayer.activePlayerList)
                {
                    if (AutoBaseUpgrade.IsLooting(player, cupboard))
                        ShowAutoUpgradeButton(player, cupboard);
                }
            }

            public void ShowUpgradeMenu(BasePlayer player, BuildingPrivlidge cupboard)
            {
                List<BuildingBlock> buildingBlocks = AutoBaseUpgrade.GetBuildingBlocks(cupboard);

                using (CUI cui = plugin.CreateCUI())
                {
                    cui.Destroy(MenuUi, player);

                    CreateMenuPanel(cui);
                    AddMenuHeader(cui);
                    AddGradeRows(cui, buildingBlocks, cupboard);
                    AddMenuFooter(cui);
                    cui.v2.SendUi(player);
                }
            }

            private void CreateMenuPanel(CUI cui)
            {
                LUI.LuiContainer panel = cui.v2.CreatePanel(
                    "Overlay", LuiPosition.MiddleCenter,
                    new LuiOffset(-390, -245, 390, 245),
                    plugin.UiColor(plugin.pluginConfig.Colors.Panel), MenuUi);
                panel.AddCursor();
            }

            private void AddMenuHeader(CUI cui)
            {
                AddMenuTitle(cui);
                AddMenuSubtitle(cui);
                AddMenuCloseButton(cui);
            }

            private void AddMenuTitle(CUI cui)
            {
                AddText(cui, MenuUi, plugin.pluginConfig.Text.Title, 20,
                    plugin.UiColor(plugin.pluginConfig.Colors.Text), TextAnchor.MiddleLeft,
                    new LuiPosition(0.035f, 0.895f, 0.80f, 0.98f));
            }

            private void AddMenuSubtitle(CUI cui)
            {
                AddText(cui, MenuUi, plugin.pluginConfig.Text.Subtitle, 12,
                    plugin.UiColor(plugin.pluginConfig.Colors.MutedText), TextAnchor.MiddleLeft,
                    new LuiPosition(0.035f, 0.835f, 0.90f, 0.90f));
            }

            private void AddMenuCloseButton(CUI cui)
            {
                AddCloseButton(cui, MenuUi, "X", plugin.UiColor(plugin.pluginConfig.Colors.StopButton),
                    new LuiPosition(0.925f, 0.91f, 0.972f, 0.975f), 13, MenuUi);
            }

            private void AddGradeRows(CUI cui, List<BuildingBlock> buildingBlocks,
                BuildingPrivlidge cupboard)
            {
                for (int index = 0; index < MenuGrades.Length; index++)
                {
                    BuildingGrade.Enum grade = MenuGrades[index];
                    GradePreview preview = plugin.BuildPreview(grade, buildingBlocks);
                    AddGradeRow(cui, index, grade, preview, cupboard);
                }
            }

            private void AddMenuFooter(CUI cui)
            {
                AddFooterText(cui, MenuUi, plugin.pluginConfig.Text.Footer);
            }

            private void AddFooterText(CUI cui, string parent, string text)
            {
                AddText(cui, parent, text, 10,
                    plugin.UiColor(plugin.pluginConfig.Colors.MutedText), TextAnchor.MiddleCenter,
                    new LuiPosition(0.04f, 0.018f, 0.96f, 0.065f));
            }

            private void AddGradeRow(CUI cui, int index, BuildingGrade.Enum grade,
                GradePreview preview, BuildingPrivlidge cupboard)
            {
                string row = MenuUi + ".Row." + index;
                bool hasBlocksToUpgrade = preview.EligibleBlocks.Count > 0;
                bool canPayAll = AutoBaseUpgrade.HasAmounts(cupboard, preview.TotalCost);
                bool canPayAnyBlock = AutoBaseUpgrade.CanPayAtLeastOneBlock(cupboard,
                    preview.EligibleBlocks, grade);

                CreateGradeRowPanel(cui, row, index);
                AddGradeLabels(cui, row, grade, preview, cupboard, canPayAll);
                AddGradeActions(cui, row, grade, hasBlocksToUpgrade, canPayAll, canPayAnyBlock);
            }

            private void CreateGradeRowPanel(CUI cui, string row, int index)
            {
                AddPanel(cui, MenuUi, row, plugin.pluginConfig.Colors.Row,
                    GetGradeRowPosition(index));
            }

            private static LuiPosition GetGradeRowPosition(int index)
            {
                float top = GradeRowFirstTop - index * GradeRowSpacing;
                float bottom = top - GradeRowHeight;
                return new LuiPosition(GradeRowLeft, bottom, GradeRowRight, top);
            }

            private void AddGradeLabels(CUI cui, string row, BuildingGrade.Enum grade,
                GradePreview preview, BuildingPrivlidge cupboard, bool canPay)
            {
                AddGradeName(cui, row, grade);
                AddUpgradeCount(cui, row, preview);
                AddResourceSummary(cui, row, preview, cupboard, canPay);
            }

            private void AddGradeName(CUI cui, string row, BuildingGrade.Enum grade)
            {
                AddText(cui, row, GradeName(grade), 16, GradeColor(grade), TextAnchor.MiddleLeft,
                    new LuiPosition(0.025f, 0.54f, 0.20f, 0.94f));
            }

            private void AddUpgradeCount(CUI cui, string row, GradePreview preview)
            {
                string text = string.Format(CultureInfo.InvariantCulture,
                    plugin.pluginConfig.Text.BlocksNeedUpgrade,
                    preview.BlocksBelowTarget, preview.TotalBlocks);
                AddText(cui, row, text, 11, plugin.UiColor(plugin.pluginConfig.Colors.MutedText),
                    TextAnchor.MiddleLeft, new LuiPosition(0.025f, 0.08f, 0.30f, 0.50f));
            }

            private void AddResourceSummary(CUI cui, string row, GradePreview preview,
                BuildingPrivlidge cupboard, bool canPay)
            {
                AddText(cui, row, FormatResources(preview.TotalCost, cupboard), 12,
                    GetResourceColor(canPay), TextAnchor.MiddleLeft,
                    new LuiPosition(0.31f, 0.16f, 0.66f, 0.84f));
            }

            private void AddGradeActions(CUI cui, string row, BuildingGrade.Enum grade,
                bool hasBlocksToUpgrade, bool canPayAll, bool canPayAny)
            {
                AddButton(cui, row, buttons.Upgrade(grade, hasBlocksToUpgrade, canPayAll, canPayAny),
                    new LuiPosition(0.68f, 0.18f, 0.975f, 0.82f), LuiOffset.None);
            }

            private string FormatResources(Dictionary<int, int> cost, BuildingPrivlidge cupboard)
            {
                if (cost.Count == 0) return plugin.pluginConfig.Text.NoResourcesRequired;
                return string.Join("   ", cost.OrderBy(entry => entry.Key)
                    .Select(entry => FormatResourceLine(entry.Key, entry.Value, cupboard)));
            }

            private string FormatResourceLine(int itemId, int requiredAmount, BuildingPrivlidge cupboard)
            {
                string name = ItemManager.FindItemDefinition(itemId).displayName.english;
                int storedAmount = cupboard.inventory.GetAmount(itemId);
                int remainingAmount = storedAmount - requiredAmount;

                if (remainingAmount >= 0)
                    return string.Format(CultureInfo.InvariantCulture, plugin.pluginConfig.Text.ResourceAvailable,
                        name, requiredAmount, storedAmount, remainingAmount);

                return string.Format(CultureInfo.InvariantCulture, plugin.pluginConfig.Text.ResourceMissing,
                    name, requiredAmount, storedAmount, -remainingAmount);
            }

            private string GetResourceColor(bool canPay)
            {
                if (canPay)
                    return plugin.UiColor(plugin.pluginConfig.Colors.SuccessText);

                return plugin.UiColor(plugin.pluginConfig.Colors.ErrorText);
            }

            public static string GradeName(BuildingGrade.Enum grade)
            {
                switch (grade)
                {
                    case BuildingGrade.Enum.Wood: return "Wood";
                    case BuildingGrade.Enum.Stone: return "Stone";
                    case BuildingGrade.Enum.Metal: return "Sheet Metal";
                    case BuildingGrade.Enum.TopTier: return "Armored";
                    default: return grade.ToString();
                }
            }

            private string GradeColor(BuildingGrade.Enum grade)
            {
                switch (grade)
                {
                    case BuildingGrade.Enum.Wood: return plugin.UiColor(plugin.pluginConfig.Colors.Wood);
                    case BuildingGrade.Enum.Stone: return plugin.UiColor(plugin.pluginConfig.Colors.Stone);
                    case BuildingGrade.Enum.Metal: return plugin.UiColor(plugin.pluginConfig.Colors.Metal);
                    case BuildingGrade.Enum.TopTier: return plugin.UiColor(plugin.pluginConfig.Colors.Armored);
                    default: return plugin.UiColor("#FFFFFFFF");
                }
            }

            private static void AddText(CUI cui, string parent, string text, int size, string color,
                TextAnchor alignment, LuiPosition position)
            {
                cui.v2.CreateText(parent, position, LuiOffset.None, size, color, text, alignment);
            }

            private static void AddText(CUI cui, LUI.LuiContainer parent, string text, int size,
                string color, TextAnchor alignment, LuiPosition position)
            {
                cui.v2.CreateText(parent, position, LuiOffset.None, size, color, text, alignment);
            }

            private void AddPanel(CUI cui, string parent, string name, string color,
                LuiPosition position)
            {
                cui.v2.CreatePanel(parent, position, LuiOffset.None, plugin.UiColor(color), name);
            }

            private static void AddButton(CUI cui, string parent, UiButtonDefinition button,
                LuiPosition position, LuiOffset offset, string name = "", string textName = "")
            {
                LUI.LuiContainer element = cui.v2.CreateButton(parent, position, offset,
                    button.Command, button.Color, name: name);
                cui.v2.CreateText(element, LuiPosition.Full, LuiOffset.None, button.FontSize,
                    "1 1 1 1", button.Text, TextAnchor.MiddleCenter, textName);
            }

            private static void AddCloseButton(CUI cui, string parent, string text, string color,
                LuiPosition position, int size, string closeUi)
            {
                LUI.LuiContainer button = cui.v2.CreateButton(parent, position, LuiOffset.None, null, color);
                button.SetButtonClose(closeUi);
                cui.v2.CreateText(button, LuiPosition.Full, LuiOffset.None, size, "1 1 1 1", text,
                    TextAnchor.MiddleCenter);
            }

            public void DestroyMenu(BasePlayer player)
            {
                using (CUI cui = plugin.CreateCUI())
                    cui.Destroy(MenuUi, player);
            }


            public void DestroyAllUi(BasePlayer player)
            {
                plugin.visibleButtonPlayers.Remove(player.userID);
                using (CUI cui = plugin.CreateCUI())
                {
                    cui.Destroy(MenuUi, player);
                    cui.Destroy(ButtonUi, player);
                }
            }
        }
    }
}
