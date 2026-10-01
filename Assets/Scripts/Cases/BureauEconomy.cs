using System.Collections.Generic;
using UnityEngine;

namespace LostAndFound.Cases
{
    public sealed class BureauUpgrade
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

    public sealed class BureauBooster
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Price { get; }

        public BureauBooster(string id, string name, string description, int price)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
        }
    }

    public static class BureauEconomy
    {
        // Keep the v2 save prefix, so existing coins, room ownership and
        // completed cases survive this progression update.
        private const string Prefix = "LostAndFound.v2.";

        private static readonly BureauUpgrade[] rooms =
        {
            new BureauUpgrade("PhotoLab", "Фотолаборатория",
                "Кадры, детали и сравнение изображений.", 80),
            new BureauUpgrade("ArchiveRoom", "Архив документов",
                "Записи, журналы и хронология событий.", 90),
            new BureauUpgrade("Workshop", "Мастерская находок",
                "Восстановление записок и предметов.", 100)
        };

        // All of these are persistent purchases. For each room, an upgraded
        // instrument opens the in-room analysis aid and themed case variants.
        private static readonly BureauUpgrade[] equipment =
        {
            new BureauUpgrade("PhotoLens", "Лупа фотографа",
                "Подсветка важной детали на кадре.", 140),
            new BureauUpgrade("PhotoFilm", "Стол для плёнок",
                "Подсказка о последовательности снимков.", 260),
            new BureauUpgrade("PhotoCompare", "Стенд сравнения",
                "Помогает сопоставить несколько кадров.", 420),
            new BureauUpgrade("ArchiveIndex", "Каталог карточек",
                "Находит нужную запись по делу.", 160),
            new BureauUpgrade("ArchiveClock", "Хронологический стенд",
                "Показывает порядок событий.", 280),
            new BureauUpgrade("ArchiveCipher", "Шкаф редких документов",
                "Помогает расшифровывать записи.", 440),
            new BureauUpgrade("WorkshopGlue", "Набор реставратора",
                "Подсказывает место фрагмента.", 180),
            new BureauUpgrade("WorkshopTools", "Инструменты мастера",
                "Облегчает восстановление предметов.", 300),
            new BureauUpgrade("WorkshopScanner", "Стол реставрации",
                "Помогает сравнивать фрагменты.", 460)
        };

        private static readonly BureauBooster[] boosters =
        {
            new BureauBooster("Moves5", "+5 ходов",
                "Добавляет пять ходов в текущем Match-3.", 70),
            new BureauBooster("Bomb", "Готовая бомба",
                "Ставит бомбу на выбранную фишку.", 85),
            new BureauBooster("Plane", "Готовый самолётик",
                "Ставит самолётик на выбранную фишку.", 90),
            new BureauBooster("ColorClear", "Удаление цвета",
                "Ставит цветной бонус на выбранную фишку.", 110)
        };

        public static IReadOnlyList<BureauUpgrade> Upgrades => rooms;
        public static IReadOnlyList<BureauUpgrade> Equipment => equipment;
        public static IReadOnlyList<BureauBooster> Boosters => boosters;

        public static int Coins
        {
            get
            {
                MigrateOldDecorationPurchases();
                return PlayerPrefs.GetInt(Prefix + "Coins", 0);
            }
        }

        public static int Reputation
        {
            get
            {
                MigrateLegacyReputation();
                return PlayerPrefs.GetInt(Prefix + "Reputation", 0);
            }
        }

        public static int CompletedUniqueCases
        {
            get
            {
                MigrateLegacyReputation();
                return PlayerPrefs.GetInt(Prefix + "CompletedCases", 0);
            }
        }

        private static int TierForPoints(int reputation)
        {
            if (reputation >= 1400) return 5;
            if (reputation >= 800) return 4;
            if (reputation >= 400) return 3;
            if (reputation >= 150) return 2;
            if (reputation >= 40) return 1;
            return 0;
        }

        public static int ReputationTier => TierForPoints(Reputation);

        public static string ReputationTitle
        {
            get
            {
                switch (ReputationTier)
                {
                    case 1: return "Знакомое бюро";
                    case 2: return "Городское бюро";
                    case 3: return "Бюро с репутацией";
                    case 4: return "Экспертное бюро";
                    case 5: return "Легендарное бюро";
                    default: return "Маленькое бюро";
                }
            }
        }

        public static int NextReputationTarget
        {
            get
            {
                switch (ReputationTier)
                {
                    case 0: return 40;
                    case 1: return 150;
                    case 2: return 400;
                    case 3: return 800;
                    case 4: return 1400;
                    default: return 1400;
                }
            }
        }

        public static bool Owns(string roomId)
        {
            return PlayerPrefs.GetInt(Prefix + "Upgrade." + roomId, 0) == 1;
        }

        public static bool OwnsEquipment(string id)
        {
            return PlayerPrefs.GetInt(Prefix + "Equipment." + id, 0) == 1;
        }

        public static int BoosterCount(string id)
        {
            return PlayerPrefs.GetInt(Prefix + "Booster." + id, 0);
        }

        public static int CaseResult(string caseId)
        {
            return PlayerPrefs.GetInt(Prefix + "Result." + caseId, 0);
        }

        public static bool TryBuy(BureauUpgrade room)
        {
            if (room == null || Owns(room.Id) || Coins < room.Price)
                return false;

            bool valid = false;
            foreach (BureauUpgrade entry in rooms)
                valid |= entry.Id == room.Id;

            if (!valid)
                return false;

            PlayerPrefs.SetInt(Prefix + "Coins", Coins - room.Price);
            PlayerPrefs.SetInt(Prefix + "Upgrade." + room.Id, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool TryBuyEquipment(BureauUpgrade item)
        {
            if (item == null || OwnsEquipment(item.Id) || Coins < item.Price)
                return false;

            bool valid = false;
            foreach (BureauUpgrade entry in equipment)
                valid |= entry.Id == item.Id;

            if (!valid)
                return false;

            PlayerPrefs.SetInt(Prefix + "Coins", Coins - item.Price);
            PlayerPrefs.SetInt(Prefix + "Equipment." + item.Id, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool TryBuyBooster(BureauBooster booster)
        {
            if (booster == null || Coins < booster.Price)
                return false;

            bool valid = false;
            foreach (BureauBooster entry in boosters)
                valid |= entry.Id == booster.Id;

            if (!valid)
                return false;

            PlayerPrefs.SetInt(Prefix + "Coins", Coins - booster.Price);
            PlayerPrefs.SetInt(Prefix + "Booster." + booster.Id,
                BoosterCount(booster.Id) + 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool TryUseBooster(string id)
        {
            if (BoosterCount(id) <= 0)
                return false;

            PlayerPrefs.SetInt(Prefix + "Booster." + id,
                BoosterCount(id) - 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool HasEquipmentForRoom(string roomId)
        {
            foreach (BureauUpgrade item in equipment)
            {
                if (EquipmentRoomId(item.Id) == roomId &&
                    OwnsEquipment(item.Id))
                    return true;
            }

            return false;
        }

        public static int OwnedEquipmentCount(string roomId)
        {
            int count = 0;
            foreach (BureauUpgrade item in equipment)
                if (EquipmentRoomId(item.Id) == roomId &&
                    OwnsEquipment(item.Id))
                    count++;
            return count;
        }

        public static string EquipmentRoomId(string equipmentId)
        {
            if (equipmentId.StartsWith("Photo")) return "PhotoLab";
            if (equipmentId.StartsWith("Archive")) return "ArchiveRoom";
            if (equipmentId.StartsWith("Workshop")) return "Workshop";
            return "";
        }

        public static int CompleteCase(string caseId, bool correct)
        {
            if (CaseResult(caseId) != 0)
                return 0;

            MigrateLegacyReputation();
            int reward = correct ? 150 : 100;
            int reputationGain = correct ? 18 : 8;

            int previousReputation = Reputation;
            int nextReputation = previousReputation + reputationGain;
            int oldTier = TierForPoints(previousReputation);
            int newTier = TierForPoints(nextReputation);

            PlayerPrefs.SetInt(Prefix + "Result." + caseId, correct ? 1 : -1);
            PlayerPrefs.SetInt(Prefix + "Coins", Coins + reward);
            PlayerPrefs.SetInt(Prefix + "Reputation", nextReputation);
            PlayerPrefs.SetInt(Prefix + "CompletedCases", CompletedUniqueCases + 1);

            // Reputation milestones award expendable booster packs, instead of
            // more rooms. One pack per reached rank (including skipped ranks).
            for (int level = oldTier + 1; level <= newTier; level++)
            {
                string[] prizes = { "Moves5", "Bomb", "Plane", "ColorClear" };
                foreach (string prize in prizes)
                {
                    PlayerPrefs.SetInt(Prefix + "Booster." + prize,
                        BoosterCount(prize) + 1);
                }
            }

            PlayerPrefs.Save();
            return reward;
        }

        public static bool IsCollectibleCase(int caseIndex)
        {
            // The four opening cases contain handcrafted story keepsakes.
            // Later on, each fifth case has a collectible evidence card.
            return caseIndex >= 0 && (caseIndex < 4 || (caseIndex + 1) % 5 == 0);
        }

        public static bool IsCollected(int caseIndex)
        {
            CaseDefinition entry = CaseDatabase.GetCase(caseIndex);
            // A mistake must not make a collectible permanently impossible:
            // every completed special case grants its story card.
            return entry != null && IsCollectibleCase(caseIndex) &&
                   CaseResult(entry.Id) != 0;
        }

        public static int CollectedCount
        {
            get
            {
                int result = 0;
                for (int i = 0; i < CaseDatabase.Cases.Count; i++)
                    if (IsCollected(i)) result++;
                return result;
            }
        }

        public static int CollectibleTotal
        {
            get
            {
                int result = 0;
                for (int i = 0; i < CaseDatabase.Cases.Count; i++)
                    if (IsCollectibleCase(i)) result++;
                return result;
            }
        }

        private static void MigrateLegacyReputation()
        {
            if (PlayerPrefs.GetInt(Prefix + "ReputationMigrated", 0) == 1)
                return;

            int rep = 0;
            int completed = 0;

            foreach (CaseDefinition entry in CaseDatabase.Cases)
            {
                int result = CaseResult(entry.Id);
                if (result == 0) continue;
                completed++;
                rep += result > 0 ? 18 : 8;
            }

            PlayerPrefs.SetInt(Prefix + "Reputation", rep);
            PlayerPrefs.SetInt(Prefix + "CompletedCases", completed);
            PlayerPrefs.SetInt(Prefix + "ReputationMigrated", 1);
            PlayerPrefs.Save();
        }

        private static void MigrateOldDecorationPurchases()
        {
            if (PlayerPrefs.GetInt(Prefix + "DecorRefunded", 0) == 1)
                return;

            int refund = 0;
            string[] items = { "Magnifier", "Plant", "Lamp", "Archive" };
            int[] prices = { 80, 120, 180, 240 };

            for (int i = 0; i < items.Length; i++)
            {
                string key = Prefix + "Upgrade." + items[i];
                if (PlayerPrefs.GetInt(key, 0) != 1)
                    continue;

                refund += prices[i];
                PlayerPrefs.DeleteKey(key);
            }

            PlayerPrefs.SetInt(Prefix + "Coins",
                PlayerPrefs.GetInt(Prefix + "Coins", 0) + refund);
            PlayerPrefs.SetInt(Prefix + "DecorRefunded", 1);
            PlayerPrefs.Save();
        }

        public static void ResetEverything()
        {
            foreach (string key in new[]
            {
                "Coins", "DecorRefunded", "Reputation", "CompletedCases",
                "ReputationMigrated"
            })
                PlayerPrefs.DeleteKey(Prefix + key);

            foreach (BureauUpgrade room in rooms)
                PlayerPrefs.DeleteKey(Prefix + "Upgrade." + room.Id);

            foreach (BureauUpgrade item in equipment)
                PlayerPrefs.DeleteKey(Prefix + "Equipment." + item.Id);

            foreach (BureauBooster booster in boosters)
                PlayerPrefs.DeleteKey(Prefix + "Booster." + booster.Id);

            foreach (string obsolete in new[] { "Magnifier", "Plant", "Lamp", "Archive" })
                PlayerPrefs.DeleteKey(Prefix + "Upgrade." + obsolete);

            foreach (CaseDefinition entry in CaseDatabase.Cases)
                PlayerPrefs.DeleteKey(Prefix + "Result." + entry.Id);

            PlayerPrefs.Save();
        }
    }
}
