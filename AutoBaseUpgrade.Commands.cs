using System.Linq;

namespace Carbon.Plugins
{
    public partial class AutoBaseUpgrade
    {
        [ChatCommand("baseupgrade")]
        private void OpenUpgradeCommand(BasePlayer player, string command, string[] args)
        {
            OpenUpgradeMenu(player);
        }

        [ProtectedCommand("autobaseupgrade.open")]
        private void OpenMenu(ConsoleSystem.Arg arg)
        {
            BasePlayer player = GetPlayer(arg);
            OpenUpgradeMenu(player);
        }

        [ProtectedCommand("autobaseupgrade.start")]
        private void StartUpgrade(ConsoleSystem.Arg arg)
        {
            BasePlayer player = GetPlayer(arg);
            BuildingPrivlidge cupboard = GetAuthorizedCupboard(player);

            BuildingGrade.Enum targetGrade = (BuildingGrade.Enum)arg.GetInt(0, (int)BuildingGrade.Enum.None);

            if (cupboard == null || !MenuGrades.Contains(targetGrade)) return;

            uint buildingId = cupboard.buildingID;
            if (runningUpgradesByBuilding.ContainsKey(buildingId))
            {
                player.ChatMessage(GetMessage("UpgradeAlreadyRunning", player));
                return;
            }

            ulong playerId = player.userID;
            GradePreview preview = BuildPreview(targetGrade, GetBuildingBlocks(cupboard));
            if (preview.EligibleBlocks.Count == 0)
            {
                player.ChatMessage(GetMessage("NothingToUpgrade", player));

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

            player.ChatMessage(GetMessage("UpgradeStarted", player,
                run.Blocks.Count, GetGradeName(player, targetGrade)));

            UpgradeNextBlock(run);
        }

        [ProtectedCommand("autobaseupgrade.stop")]
        private void StopUpgrade(ConsoleSystem.Arg arg)
        {
            BasePlayer player = GetPlayer(arg);
            BuildingPrivlidge cupboard = GetAuthorizedCupboard(player);

            if (cupboard == null) return;

            uint buildingId = cupboard.buildingID;
            UpgradeRun run;
            if (!runningUpgradesByBuilding.TryGetValue(buildingId, out run))
            {
                player.ChatMessage(GetMessage("NothingToStop", player));
                upgradeUi.ShowAutoUpgradeButton(player, cupboard);
                return;
            }

            run.Cancelled = true;

            BasePlayer owner = BasePlayer.FindByID(run.PlayerId);
            FinishUpgrade(run, owner, "UpgradeStopped");

            if (owner != player)
                player.ChatMessage(GetMessage("UpgradeStopped", player));
        }

        private static BasePlayer GetPlayer(ConsoleSystem.Arg arg)
        {
            return arg?.Connection?.player as BasePlayer;
        }

        private void OpenUpgradeMenu(BasePlayer player)
        {
            BuildingPrivlidge cupboard = GetAuthorizedCupboard(player);
            if (cupboard == null)
                return;

            upgradeUi.ShowUpgradeMenu(player, cupboard);
        }
    }
}
