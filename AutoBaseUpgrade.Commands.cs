using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Carbon.Plugins
{
    public partial class AutoBaseUpgrade
    {
        [ProtectedCommand("autobaseupgrade.open")]
        private void OpenMenu(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Connection?.player as BasePlayer;
            BuildingPrivlidge cupboard = GetUsableCupboard(player);

            if (cupboard == null) return;

            upgradeUi.ShowUpgradeMenu(player, cupboard);
        }

        [ProtectedCommand("autobaseupgrade.stop")]
        private void StopUpgrade(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Connection?.player as BasePlayer;
            BuildingPrivlidge cupboard = GetUsableCupboard(player);

            if (cupboard == null) return;

            uint buildingId = cupboard.buildingID;
            if (!runningUpgradesByBuilding.ContainsKey(buildingId))
            {
                player.ChatMessage(pluginConfig.Messages.NothingToStop);
                upgradeUi.ShowAutoUpgradeButton(player, cupboard);
                return;
            }

            UpgradeRun run = runningUpgradesByBuilding[buildingId];
            run.Cancelled = true;

            BasePlayer owner = BasePlayer.FindByID(run.PlayerId);
            FinishUpgrade(run, owner, pluginConfig.Messages.UpgradeStopped);

            if (owner != player)
                player.ChatMessage(pluginConfig.Messages.UpgradeStopped);
        }

        [ProtectedCommand("autobaseupgrade.start")]
        private void StartUpgrade(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Connection?.player as BasePlayer;
            BuildingPrivlidge cupboard = GetUsableCupboard(player);

            BuildingGrade.Enum targetGrade = (BuildingGrade.Enum)arg.GetInt(0, (int)BuildingGrade.Enum.None);

            if (cupboard == null || !MenuGrades.Contains(targetGrade)) return;

            uint buildingId = cupboard.buildingID;
            if (runningUpgradesByBuilding.ContainsKey(buildingId))
            {
                player.ChatMessage(pluginConfig.Messages.UpgradeAlreadyRunning);
                return;
            }

            ulong playerId = player.userID;
            if (runningUpgradesByBuilding.Values.Any(run => run.PlayerId == playerId))
            {
                player.ChatMessage(pluginConfig.Messages.PlayerAlreadyUpgrading);
                return;
            }

            GradePreview preview = BuildPreview(targetGrade, GetBuildingBlocks(cupboard));
            if (preview.EligibleBlocks.Count == 0)
            {
                player.ChatMessage(pluginConfig.Messages.NothingToUpgrade);

                upgradeUi.ShowUpgradeMenu(player, cupboard);
                return;
            }

            UpgradeRun run = new UpgradeRun
            {
                PlayerId = playerId,
                BuildingId = buildingId,
                Cupboard = cupboard,
                TargetGrade = targetGrade,
                Blocks = preview.EligibleBlocks
            };

            runningUpgradesByBuilding[buildingId] = run;
            upgradeUi.DestroyMenu(player);
            upgradeUi.RefreshCupboardButtons(cupboard);

            player.ChatMessage(string.Format(CultureInfo.InvariantCulture,
                pluginConfig.Messages.UpgradeStarted,
                run.Blocks.Count, UpgradeUi.GradeName(targetGrade)));

            UpgradeNextBlock(run);
        }
    }
}
