using System.Collections.Generic;

namespace Carbon.Plugins
{
    [Info("Auto Base Upgrade", "Pretend", "2.0.1")]
    [Description("Upgrades a connected base one block at a time using resources from its Tool Cupboard.")]
    public partial class AutoBaseUpgrade : CarbonPlugin
    {
        private readonly Dictionary<BuildingPrivlidge, HashSet<ulong>> cupboardViewers =
            new Dictionary<BuildingPrivlidge, HashSet<ulong>>();

        private void Init()
        {
            upgradeUi = new UpgradeUi(this);
        }

        private void Unload()
        {
            runningUpgradesByBuilding.Clear();
            cupboardViewers.Clear();
            foreach (BasePlayer player in BasePlayer.activePlayerList)
                upgradeUi.DestroyAllUi(player);
        }

        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            BuildingPrivlidge cupboard = entity as BuildingPrivlidge;

            NextTick(() =>
            {
                if (IsLooting(player, cupboard))
                {
                    RegisterCupboardViewer(player, cupboard);
                    upgradeUi.ShowAutoUpgradeButton(player, cupboard);
                }
            });
        }

        private void OnLootEntityEnd(BasePlayer player, BaseCombatEntity entity)
        {
            BuildingPrivlidge cupboard = entity as BuildingPrivlidge;
            if (cupboard != null)
                UnregisterCupboardViewer(player.userID, cupboard);
            upgradeUi.DestroyAllUi(player);
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            BuildingPrivlidge cupboard = player.inventory.loot.entitySource as BuildingPrivlidge;
            if (cupboard != null)
                UnregisterCupboardViewer(player.userID, cupboard);
            visibleButtonPlayers.Remove(player.userID);
        }

        private void RegisterCupboardViewer(BasePlayer player, BuildingPrivlidge cupboard)
        {
            HashSet<ulong> viewers;
            if (!cupboardViewers.TryGetValue(cupboard, out viewers))
            {
                viewers = new HashSet<ulong>();
                cupboardViewers[cupboard] = viewers;
            }

            viewers.Add(player.userID);
        }

        private void UnregisterCupboardViewer(ulong playerId, BuildingPrivlidge cupboard)
        {
            HashSet<ulong> viewers;
            if (cupboardViewers.TryGetValue(cupboard, out viewers))
                viewers.Remove(playerId);
        }

        private void SendChatMessage(BasePlayer player, string key, params object[] args)
        {
            if (player == null || !player.IsConnected)
                return;

            player.ChatMessage(GetMessage(key, player, args));
        }
    }
}
