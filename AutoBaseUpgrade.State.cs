using System.Collections.Generic;

namespace Carbon.Plugins
{
    public partial class AutoBaseUpgrade
    {
        private static readonly BuildingGrade.Enum[] MenuGrades =
        {
            BuildingGrade.Enum.Wood,
            BuildingGrade.Enum.Stone,
            BuildingGrade.Enum.Metal,
            BuildingGrade.Enum.TopTier
        };

        private readonly Dictionary<uint, UpgradeRun> runningUpgradesByBuilding =
            new Dictionary<uint, UpgradeRun>();

        private sealed class GradePreview
        {
            public readonly List<BuildingBlock> EligibleBlocks = new List<BuildingBlock>();
            public int CostItemId;
            public int TotalCost;
            public int TotalBlocks;
            public int BlocksBelowTarget;
        }

        private sealed class UpgradeRun
        {
            public ulong PlayerId;
            public uint BuildingId;
            public BuildingPrivlidge Cupboard;
            public BuildingGrade.Enum TargetGrade;
            public List<BuildingBlock> Blocks;
            public int NextBlock;
            public int Upgraded;
            public bool Cancelled;
        }

        private sealed class UiButtonDefinition
        {
            public string Text;
            public string Command;
            public string Color;
            public int FontSize;
        }
    }
}
