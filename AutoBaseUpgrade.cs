namespace Carbon.Plugins
{
    [Info("Auto Base Upgrade", "Pretend", "1.1.0")]
    [Description("Upgrades a connected base one block at a time using resources from its Tool Cupboard.")]
    public partial class AutoBaseUpgrade : CarbonPlugin
    {

        private void Init()
        {
            upgradeUi = new UpgradeUi(this);
        }

        private void Unload()
        {
            runningUpgradesByBuilding.Clear();
            foreach (BasePlayer player in BasePlayer.activePlayerList)
                upgradeUi.DestroyAllUi(player);
        }

        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            BuildingPrivlidge cupboard = entity as BuildingPrivlidge;

            NextTick(() =>
            {
                if (IsLooting(player, cupboard))
                    upgradeUi.ShowAutoUpgradeButton(player, cupboard);
            });
        }

        private void OnLootEntityEnd(BasePlayer player, BaseCombatEntity entity)
        {
            upgradeUi.DestroyAllUi(player);
        }
    }
}
