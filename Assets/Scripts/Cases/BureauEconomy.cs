using System.Collections.Generic;
using UnityEngine;

namespace LostAndFound.Cases
{
    public class BureauUpgrade
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Price { get; }

        public BureauUpgrade(string id, string name, string description, int price)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
        }
    }

    public static class BureauEconomy
    {
        private const string Prefix = "LostAndFound.v2.";
        private static readonly BureauUpgrade[] upgrades =
        {
            new BureauUpgrade("Magnifier", "Винтажная лупа", "Украшение рабочего стола.", 80),
            new BureauUpgrade("Plant", "Комнатное растение", "Немного уюта для бюро.", 120),
            new BureauUpgrade("Lamp", "Настольная лампа", "Тёплый свет для расследований.", 180),
            new BureauUpgrade("Archive", "Архивная коробка", "Место для памятных дел.", 240)
        };

        public static IReadOnlyList<BureauUpgrade> Upgrades => upgrades;
        public static int Coins => PlayerPrefs.GetInt(Prefix + "Coins", 0);
        public static bool Owns(string id) => PlayerPrefs.GetInt(Prefix + "Upgrade." + id, 0) == 1;
        public static int CaseResult(string caseId) => PlayerPrefs.GetInt(Prefix + "Result." + caseId, 0);

        public static bool TryBuy(BureauUpgrade upgrade)
        {
            if (upgrade == null || Owns(upgrade.Id) || Coins < upgrade.Price)
                return false;

            PlayerPrefs.SetInt(Prefix + "Coins", Coins - upgrade.Price);
            PlayerPrefs.SetInt(Prefix + "Upgrade." + upgrade.Id, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static int CompleteCase(string caseId, bool correct)
        {
            if (CaseResult(caseId) != 0)
                return 0;

            int reward = correct ? 150 : 30;
            PlayerPrefs.SetInt(Prefix + "Result." + caseId, correct ? 1 : -1);
            PlayerPrefs.SetInt(Prefix + "Coins", Coins + reward);
            PlayerPrefs.Save();
            return reward;
        }

        public static void ResetEverything()
        {
            PlayerPrefs.DeleteKey(Prefix + "Coins");
            foreach (BureauUpgrade upgrade in upgrades)
                PlayerPrefs.DeleteKey(Prefix + "Upgrade." + upgrade.Id);

            foreach (CaseDefinition currentCase in CaseDatabase.Cases)
                PlayerPrefs.DeleteKey(Prefix + "Result." + currentCase.Id);

            PlayerPrefs.Save();
        }
    }
}
