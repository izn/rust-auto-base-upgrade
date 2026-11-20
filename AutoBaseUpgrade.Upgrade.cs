using System;
using System.Collections.Generic;
using System.Globalization;
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
                FinishUpgrade(run, player, pluginConfig.Messages.UpgradeStopped);
                return;
            }

            while (run.NextBlock < run.Blocks.Count)
            {
                BuildingBlock block = run.Blocks[run.NextBlock++];
                if (ShouldSkipBlock(block, run.TargetGrade))
                    continue;

                ConstructionGrade grade = GetConstructionGrade(block, run.TargetGrade);
                if (grade == null)
                    continue;

                List<ItemAmount> cost = grade.CostToBuild(block.grade);
                if (!HasAmounts(run.Cupboard, cost))
                {
                    FinishUpgrade(run, player, pluginConfig.Messages.NotEnoughResources);
                    return;
                }

                TakeFromCupboard(run.Cupboard, cost);

                try
                {
                    UpgradeBlock(player, block, run.TargetGrade);
                    run.Upgraded++;
                }
                catch (Exception exception)
                {
                    ReturnToCupboard(run.Cupboard, cost);

                    PrintError("Could not upgrade block {0}: {1}", block.net.ID.ToString(), exception.Message);
                }

                timer.Once(SecondsBetweenUpgrades, () => UpgradeNextBlock(run));
                return;
            }

            FinishUpgrade(run, player);
        }

        private static bool ShouldSkipBlock(BuildingBlock block, BuildingGrade.Enum targetGrade)
        {
            if (block.IsDestroyed)
                return true;

            return block.grade >= targetGrade;
        }

        private void FinishUpgrade(UpgradeRun run, BasePlayer player, string messageOverride = null)
        {
            runningUpgradesByBuilding.Remove(run.BuildingId);
            upgradeUi.RefreshCupboardButtons(run.Cupboard);
            if (player == null || !player.IsConnected) return;

            string completionMessage = string.Format(CultureInfo.InvariantCulture,
                pluginConfig.Messages.UpgradeComplete, run.Upgraded);

            player.ChatMessage(messageOverride ?? completionMessage);
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

                ConstructionGrade grade = GetConstructionGrade(block, targetGrade);
                if (grade == null) continue;

                List<ItemAmount> cost = grade.CostToBuild(block.grade);
                if (cost.Count == 0) continue;

                preview.EligibleBlocks.Add(block);
                AddAmounts(preview.TotalCost, cost);
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

        private static void TakeFromCupboard(BuildingPrivlidge cupboard, List<ItemAmount> cost)
        {
            foreach (ItemAmount amount in cost)
                cupboard.inventory.Take(null, amount.itemid, (int)amount.amount);
        }

        private static void ReturnToCupboard(BuildingPrivlidge cupboard, List<ItemAmount> amounts)
        {
            foreach (ItemAmount amount in amounts)
                cupboard.inventory.AddItem(amount.itemDef, (int)amount.amount);
        }

        private static void AddAmounts(Dictionary<int, int> total, List<ItemAmount> amounts)
        {
            foreach (ItemAmount amount in amounts)
            {
                int itemId = amount.itemid;
                int value = (int)amount.amount;

                if (total.ContainsKey(itemId))
                    total[itemId] += value;
                else
                    total[itemId] = value;
            }
        }

        private static bool HasAmounts(BuildingPrivlidge cupboard, Dictionary<int, int> required)
        {
            foreach (KeyValuePair<int, int> amount in required)
            {
                if (cupboard.inventory.GetAmount(amount.Key) < amount.Value) return false;
            }
            return true;
        }

        private static bool CanPayAtLeastOneBlock(BuildingPrivlidge cupboard,
            List<BuildingBlock> blocks, BuildingGrade.Enum targetGrade)
        {
            foreach (BuildingBlock block in blocks)
            {
                ConstructionGrade grade = GetConstructionGrade(block, targetGrade);
                if (grade == null)
                    continue;

                if (HasAmounts(cupboard, grade.CostToBuild(block.grade)))
                    return true;
            }

            return false;
        }

        private static bool HasAmounts(BuildingPrivlidge cupboard, List<ItemAmount> required)
        {
            foreach (ItemAmount amount in required)
            {
                if (cupboard.inventory.GetAmount(amount.itemid) < amount.amount) return false;
            }
            return true;
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
