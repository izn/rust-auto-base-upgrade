using System.Collections.Generic;
using System.Globalization;

namespace Carbon.Plugins
{
    public partial class AutoBaseUpgrade
    {
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["AutoUpgradeButton"] = "Auto-upgrade",
                ["StopUpgradeButton"] = "Stop upgrade",
                ["UpgradeButton"] = "Upgrade",
                ["CompleteButton"] = "Complete",
                ["Title"] = "Auto Base Upgrade",
                ["Subtitle"] = "Choose a target grade. Each block is paid from this Tool Cupboard when upgraded.",
                ["Footer"] = "Higher-grade blocks are never downgraded.",
                ["BlocksNeedUpgrade"] = "{0:N0} of {1:N0} blocks need upgrade",
                ["NoResourcesRequired"] = "No resources required",
                ["ResourceCost"] = "{0:N0}",
                ["NothingToStop"] = "Nothing to stop.",
                ["UpgradeStopped"] = "Upgrade stopped.",
                ["UpgradeAlreadyRunning"] = "Upgrade already running.",
                ["NothingToUpgrade"] = "Nothing to upgrade.",
                ["UpgradeStarted"] = "Upgrading {0:N0} blocks to {1}.",
                ["NotEnoughResources"] = "Not enough resources.",
                ["UpgradeComplete"] = "Upgrade complete: {0:N0} blocks upgraded.",
                ["GradeWood"] = "Wood",
                ["GradeStone"] = "Stone",
                ["GradeMetal"] = "Sheet Metal",
                ["GradeArmored"] = "Armored"
            }, this, "en");

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["AutoUpgradeButton"] = "Melhorar base",
                ["StopUpgradeButton"] = "Parar melhoria",
                ["UpgradeButton"] = "Melhorar",
                ["CompleteButton"] = "Concluído",
                ["Title"] = "Melhoria automática da base",
                ["Subtitle"] = "Escolha o nível desejado. Cada bloco usa recursos deste armário de ferramentas.",
                ["Footer"] = "Blocos de nível superior nunca são rebaixados.",
                ["BlocksNeedUpgrade"] = "{0:N0} de {1:N0} blocos precisam de melhoria",
                ["NoResourcesRequired"] = "Nenhum recurso necessário",
                ["ResourceCost"] = "{0:N0}",
                ["NothingToStop"] = "Não há melhoria para parar.",
                ["UpgradeStopped"] = "Melhoria interrompida.",
                ["UpgradeAlreadyRunning"] = "Já há uma melhoria em andamento nesta base.",
                ["NothingToUpgrade"] = "Nada para melhorar.",
                ["UpgradeStarted"] = "Melhorando {0:N0} blocos para {1}.",
                ["NotEnoughResources"] = "Recursos insuficientes.",
                ["UpgradeComplete"] = "Melhoria concluída: {0:N0} blocos melhorados.",
                ["GradeWood"] = "Madeira",
                ["GradeStone"] = "Pedra",
                ["GradeMetal"] = "Chapa metálica",
                ["GradeArmored"] = "Blindado"
            }, this, "pt-BR");
        }

        private string GetMessage(string key, BasePlayer player, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture,
                lang.GetMessage(key, this, player.UserIDString), args);
        }

        private string GetGradeName(BasePlayer player, BuildingGrade.Enum grade)
        {
            switch (grade)
            {
                case BuildingGrade.Enum.Wood: return GetMessage("GradeWood", player);
                case BuildingGrade.Enum.Stone: return GetMessage("GradeStone", player);
                case BuildingGrade.Enum.Metal: return GetMessage("GradeMetal", player);
                case BuildingGrade.Enum.TopTier: return GetMessage("GradeArmored", player);
                default: return grade.ToString();
            }
        }

    }
}
