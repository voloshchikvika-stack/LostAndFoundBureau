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
            new BureauUpgrade("PhotoLab", "Фотолаборатория", "Исследовать снимки и восстановить последовательность кадров.", 80),
            new BureauUpgrade("ArchiveRoom", "Архив документов", "Сопоставлять записи и находить пропавшие документы.", 90),
            new BureauUpgrade("Workshop", "Мастерская находок", "Восстанавливать повреждённые предметы и записи.", 100)
        };

        public static IReadOnlyList<BureauUpgrade> Upgrades => upgrades;

        public static int Coins
        {
            get
            {
                MigrateOldDecorationPurchases();
                return PlayerPrefs.GetInt(Prefix + "Coins", 0);
            }
        }

        // Players who already bought furniture should not lose those coins
        // when the prototype shop is replaced by functional departments.
        private static void MigrateOldDecorationPurchases()
        {
            if (PlayerPrefs.GetInt(Prefix + "DecorRefunded", 0) == 1)
                return;

            int refund = 0;
            string[] oldItems = { "Magnifier", "Plant", "Lamp", "Archive" };
            int[] oldPrices = { 80, 120, 180, 240 };

            for (int i = 0; i < oldItems.Length; i++)
            {
                string key = Prefix + "Upgrade." + oldItems[i];
                if (PlayerPrefs.GetInt(key, 0) == 1)
                {
                    refund += oldPrices[i];
                    PlayerPrefs.DeleteKey(key);
                }
            }

            PlayerPrefs.SetInt(Prefix + "Coins",
                PlayerPrefs.GetInt(Prefix + "Coins", 0) + refund);
            PlayerPrefs.SetInt(Prefix + "DecorRefunded", 1);
            PlayerPrefs.Save();
        }
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

            int reward = correct ? 150 : 100;
            PlayerPrefs.SetInt(Prefix + "Result." + caseId, correct ? 1 : -1);
            PlayerPrefs.SetInt(Prefix + "Coins", Coins + reward);
            PlayerPrefs.Save();
            return reward;
        }

        public static void ResetEverything()
        {
            PlayerPrefs.DeleteKey(Prefix + "Coins");
            PlayerPrefs.DeleteKey(Prefix + "DecorRefunded");
            foreach (BureauUpgrade upgrade in upgrades)
                PlayerPrefs.DeleteKey(Prefix + "Upgrade." + upgrade.Id);

            // Remove purchases from the old decoration-only prototype as well.
            foreach (string obsolete in new[] { "Magnifier", "Plant", "Lamp", "Archive" })
                PlayerPrefs.DeleteKey(Prefix + "Upgrade." + obsolete);

            foreach (CaseDefinition currentCase in CaseDatabase.Cases)
                PlayerPrefs.DeleteKey(Prefix + "Result." + currentCase.Id);

            PlayerPrefs.Save();
        }
    }
}
