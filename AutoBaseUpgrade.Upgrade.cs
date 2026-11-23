using System;
using System.Collections.Generic;
using System.Linq;

namespace Carbon.Plugins
{
    public partial class AutoBaseUpgrade
    {
        private const float SecondsBetweenUpgrades = 1f;

        private void UpgradeNextBlock(UpgradeRun run)
        {
            if (run.Cancelled) return;

            BasePlayer player = BasePlayer.FindByID(run.PlayerId);

            if (!CanUseCupboard(player, run.Cupboard))
            {
                FinishUpgrade(run, player, "UpgradeStopped");
                return;
            }

            while (run.NextBlock < run.Blocks.Count)
            {
                BuildingBlock block = run.Blocks[run.NextBlock++];
                if (ShouldSkipBlock(block, run.TargetGrade))
                    continue;

                ItemAmount cost = GetBlockCost(GetConstructionGrade(block, run.TargetGrade), block);
                if (cost == null) continue;

                if (run.Cupboard.inventory.GetAmount(cost.itemid) < cost.amount)
                {
                    FinishUpgrade(run, player, "NotEnoughResources");
                    return;
                }

                int costAmount = (int)cost.amount;
                TakeFromCupboard(run.Cupboard, cost.itemid, costAmount);

                try
                {
                    UpgradeBlock(player, block, run.TargetGrade);
                    run.Upgraded++;
                }
                catch (Exception exception)
                {
                    ReturnToCupboard(run.Cupboard, cost.itemid, costAmount);

                    PrintError("Could not upgrade block {0}: {1}", block.net.ID.ToString(), exception.Message);
                }

                timer.Once(SecondsBetweenUpgrades, () => UpgradeNextBlock(run));
                return;
            }

            FinishUpgrade(run, player, "UpgradeComplete", run.Upgraded);
        }

        private static bool ShouldSkipBlock(BuildingBlock block, BuildingGrade.Enum targetGrade)
        {
            if (block.IsDestroyed)
                return true;

            return block.grade >= targetGrade;
        }

        private void FinishUpgrade(UpgradeRun run, BasePlayer player, string messageKey,
            params object[] args)
        {
            runningUpgradesByBuilding.Remove(run.BuildingId);
            upgradeUi.RefreshCupboardButtons(run.Cupboard);
            SendChatMessage(player, messageKey, args);
        }

        private static void UpgradeBlock(BasePlayer player, BuildingBlock block,
            BuildingGrade.Enum targetGrade)
        {
            ConstructionGrade grade = GetConstructionGrade(block, targetGrade);

            block.ClientRPC(RpcTarget.NetworkGroup("DoUpgradeEffect"),
                (int)targetGrade, grade.gradeBase.skin);

            block.ChangeGradeAndSkin(targetGrade, grade.gradeBase.skin, playEffect: true);
        }

        private GradePreview BuildPreview(BuildingGrade.Enum targetGrade, List<BuildingBlock> buildingBlocks)
        {
            GradePreview preview = new GradePreview { TotalBlocks = buildingBlocks.Count };

            foreach (BuildingBlock block in buildingBlocks)
            {
                if (block.IsDestroyed || block.grade >= targetGrade) continue;

                preview.BlocksBelowTarget++;

                ItemAmount cost = GetBlockCost(GetConstructionGrade(block, targetGrade), block);
                if (cost == null) continue;
                preview.CostItemId = cost.itemid;
                preview.TotalCost += (int)cost.amount;
                preview.EligibleBlocks.Add(block);
            }

            return preview;
        }

        private static List<BuildingBlock> GetBuildingBlocks(BuildingPrivlidge cupboard)
        {
            BuildingManager.Building building = BuildingManager.server.GetBuilding(cupboard.buildingID);

            return building.buildingBlocks.ToList();
        }

        private static ConstructionGrade GetConstructionGrade(BuildingBlock block, BuildingGrade.Enum targetGrade)
        {
            return block.blockDefinition.GetGrade(targetGrade, 0UL);
        }

        private static ItemAmount GetBlockCost(ConstructionGrade grade, BuildingBlock block)
        {
            if (grade == null || block == null) return null;
            return grade.CostToBuild(block.grade).FirstOrDefault();
        }

        private static void TakeFromCupboard(BuildingPrivlidge cupboard, int itemId, int amount)
        {
            cupboard.inventory.Take(null, itemId, amount);
        }

        private static void ReturnToCupboard(BuildingPrivlidge cupboard, int itemId, int amount)
        {
            ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemId);
            if (itemDefinition != null) cupboard.inventory.AddItem(itemDefinition, amount);
        }

        private static BuildingPrivlidge GetAuthorizedCupboard(BasePlayer player)
        {
            if (player == null) return null;

            BuildingPrivlidge cupboard = player.GetBuildingPrivilege();
            if (!CanUseCupboard(player, cupboard))
                return null;

            return cupboard;
        }


        private static bool CanUseCupboard(BasePlayer player, BuildingPrivlidge cupboard)
        {
            return player != null && player.IsConnected && cupboard != null && !cupboard.IsDestroyed &&
                cupboard.IsAuthed(player);
        }

        private static bool IsLooting(BasePlayer player, BuildingPrivlidge cupboard)
        {
            return player != null && player.inventory.loot.entitySource == cupboard;
        }

    }
}
