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

        // Fictional city leaderboard. The player's bureau replaces the entry
        // at its current ranking position and climbs after successful special cases.
        private static readonly string[] cityBureaus =
        {
            "Агентство «Маяк»",
            "Бюро «Северная звезда»",
            "Детективная служба «Аргус»",
            "Бюро «Красная нить»",
            "Агентство «Ключ»",
            "Бюро «Точный след»",
            "Служба «Фонарь»",
            "Агентство «Седьмой адрес»",
            "Бюро «Городской след»",
            "Служба «Верный маршрут»",
            "Агентство «Параллель»",
            "Бюро «Старый квартал»",
            "Служба «Компас»",
            "Бюро «Тихий свидетель»",
            "Агентство «Ориентир»",
            "Бюро «Дальний свет»",
            "Служба «Перекрёсток»",
            "Бюро «Знак на карте»",
            "Агентство «Последний адрес»",
            "Бюро «Час пик»",
            "Служба «Найдено»",
            "Бюро «Линия маршрута»",
            "Агентство «Городские истории»",
            "Бюро «Второй ключ»",
            "Служба «Пятый переулок»",
            "Бюро «Белая метка»",
            "Агентство «Старая карта»",
            "Бюро «Точка возврата»",
            "Служба «Открытая дверь»",
            "Бюро «След на мостовой»",
            "Агентство «Забытый адрес»",
            "Бюро «Ночной фонарь»",
            "Служба «Поворот»",
            "Бюро «Семь улиц»",
            "Агентство «Потерянный маршрут»",
            "Бюро «Городская записка»",
            "Служба «Улика»",
            "Бюро «Домой»",
            "Агентство «Старый ключ»",
            "Бюро «Третий звонок»",
            "Служба «Возвращение»",
            "Бюро «По следу»",
            "Агентство «Нужный адрес»",
            "Бюро «Находка»",
            "Служба «Добрый знак»",
            "Бюро «Зелёная дверь»",
            "Агентство «Соседний квартал»",
            "Бюро «Память вещей»",
            "Служба «Вернуть домой»",
            "Бюро «Последняя улика»"
        };

        public const string PlayerBureauName = "Бюро потерянных вещей";
        public static int RankingSize => cityBureaus.Length;

        public static string BureauNameAtPosition(int position)
        {
            int clamped = Mathf.Clamp(position, 1, cityBureaus.Length);

            if (clamped == BureauRatingPosition)
                return PlayerBureauName;

            return cityBureaus[clamped - 1];
        }

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
                MigrateProgressionV3();
                return PlayerPrefs.GetInt(Prefix + "Reputation", 0);
            }
        }

        public static int CompletedUniqueCases
        {
            get
            {
                MigrateProgressionV3();
                return PlayerPrefs.GetInt(Prefix + "CompletedCases", 0);
            }
        }

        public static int SpecialCasesCompleted
        {
            get
            {
                MigrateProgressionV3();
                return PlayerPrefs.GetInt(Prefix + "SpecialCasesCompleted", 0);
            }
        }

        public static int BureauRatingPosition
        {
            get
            {
                MigrateProgressionV3();
                return PlayerPrefs.GetInt(Prefix + "RatingPosition", 50);
            }
        }

        public static bool SpecialCasePending
        {
            get
            {
                MigrateProgressionV3();
                return PlayerPrefs.GetInt(Prefix + "SpecialPending", 0) == 1;
            }
        }

        public static bool SpecialCaseActive
        {
            get
            {
                MigrateProgressionV3();
                return PlayerPrefs.GetInt(Prefix + "SpecialActive", 0) == 1;
            }
        }

        // Reputation is now a repeating progress bar. After every successful
        // special case the next bar becomes slightly longer, up to a sane cap.
        public static int ReputationTarget =>
            Mathf.Min(120, 40 + SpecialCasesCompleted * 15);

        public static float ReputationProgress01 =>
            ReputationTarget <= 0
                ? 0f
                : Mathf.Clamp01(Reputation / (float)ReputationTarget);

        // Kept for older UI helpers: tiers now represent how many ranking
        // milestones the bureau has passed, not lifetime reputation points.
        public static int ReputationTier =>
            Mathf.Clamp(SpecialCasesCompleted, 0, 5);

        public static string ReputationTitle
        {
            get
            {
                int rating = BureauRatingPosition;
                if (rating <= 5) return "Одно из лучших бюро";
                if (rating <= 15) return "Известное городское бюро";
                if (rating <= 30) return "Бюро с репутацией";
                if (rating <= 40) return "Набирающее известность бюро";
                return "Небольшое городское бюро";
            }
        }

        public static int NextReputationTarget => ReputationTarget;

        // Called only after CaseSession has moved to the next client.
        // A filled reputation bar turns that next client into a special case.
        public static void PrepareNextCaseAfterAdvance()
        {
            MigrateProgressionV3();

            if (!SpecialCasePending || SpecialCaseActive)
                return;

            PlayerPrefs.SetInt(Prefix + "SpecialPending", 0);
            PlayerPrefs.SetInt(Prefix + "SpecialActive", 1);
            PlayerPrefs.Save();
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

            MigrateProgressionV3();

            bool special = SpecialCaseActive;
            int reward = special
                ? (correct ? 260 : 140)
                : (correct ? 150 : 100);

            PlayerPrefs.SetInt(Prefix + "Result." + caseId, correct ? 1 : -1);
            PlayerPrefs.SetInt(Prefix + "Coins", Coins + reward);
            PlayerPrefs.SetInt(Prefix + "CompletedCases", CompletedUniqueCases + 1);

            if (special)
            {
                // A special case closes the current reputation cycle.
                // Only a successful conclusion improves the city ranking.
                if (correct)
                {
                    int previousPosition = BureauRatingPosition;
                    int improvement = previousPosition > 25 ? 5 :
                                      previousPosition > 10 ? 3 : 1;
                    PlayerPrefs.SetInt(
                        Prefix + "RatingPosition",
                        Mathf.Max(1, previousPosition - improvement));
                    PlayerPrefs.SetInt(
                        Prefix + "SpecialCasesCompleted",
                        SpecialCasesCompleted + 1);

                    // Special clients award a small booster pack as a clear
                    // celebration reward.
                    foreach (string prize in new[] { "Moves5", "Bomb", "Plane", "ColorClear" })
                    {
                        PlayerPrefs.SetInt(
                            Prefix + "Booster." + prize,
                            BoosterCount(prize) + 1);
                    }
                }

                PlayerPrefs.SetInt(Prefix + "Reputation", 0);
                PlayerPrefs.SetInt(Prefix + "SpecialActive", 0);
                PlayerPrefs.SetInt(Prefix + "SpecialPending", 0);
            }
            else
            {
                int gain = correct ? 18 : 8;
                int target = ReputationTarget;
                int next = Mathf.Min(target, Reputation + gain);

                PlayerPrefs.SetInt(Prefix + "Reputation", next);

                if (next >= target)
                    PlayerPrefs.SetInt(Prefix + "SpecialPending", 1);
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

        private static void MigrateProgressionV3()
        {
            if (PlayerPrefs.GetInt(Prefix + "ProgressionV3Migrated", 0) == 1)
                return;

            // Preserve existing case results/coins, but convert the old
            // lifetime-reputation total into the first repeating bar.
            int completed = 0;
            int oldLifetimeRep = 0;

            foreach (CaseDefinition entry in CaseDatabase.Cases)
            {
                int result = CaseResult(entry.Id);
                if (result == 0)
                    continue;

                completed++;
                oldLifetimeRep += result > 0 ? 18 : 8;
            }

            int initialTarget = 40;
            int convertedProgress = Mathf.Min(initialTarget, oldLifetimeRep);

            PlayerPrefs.SetInt(Prefix + "CompletedCases", completed);
            PlayerPrefs.SetInt(Prefix + "Reputation", convertedProgress);
            PlayerPrefs.SetInt(Prefix + "RatingPosition",
                PlayerPrefs.GetInt(Prefix + "RatingPosition", 50));
            PlayerPrefs.SetInt(Prefix + "SpecialCasesCompleted",
                PlayerPrefs.GetInt(Prefix + "SpecialCasesCompleted", 0));

            if (convertedProgress >= initialTarget &&
                PlayerPrefs.GetInt(Prefix + "SpecialActive", 0) == 0)
            {
                PlayerPrefs.SetInt(Prefix + "SpecialPending", 1);
            }

            PlayerPrefs.SetInt(Prefix + "ProgressionV3Migrated", 1);
            // Keep this old flag too so older builds cannot recompute the value.
            PlayerPrefs.SetInt(Prefix + "ReputationMigrated", 1);
            PlayerPrefs.Save();
        }

        private static void MigrateLegacyReputation()
        {
            // Compatibility alias for older call sites.
            MigrateProgressionV3();
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
                "ReputationMigrated", "ProgressionV3Migrated",
                "RatingPosition", "SpecialCasesCompleted",
                "SpecialPending", "SpecialActive"
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
