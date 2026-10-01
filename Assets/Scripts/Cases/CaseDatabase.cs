using System.Collections.Generic;
using UnityEngine;

namespace LostAndFound.Cases
{
    public static class CaseDatabase
    {
        // Keep the four fully scripted introductory stories unchanged.
        private static readonly CaseDefinition[] authoredCases =
        {
            new CaseDefinition(
                id: "Case01",
                clientName: "Анна",
                lostItemName: "Серебряный медальон",
                story:
                    "Здравствуйте... Я потеряла серебряный медальон моей бабушки. " +
                    "Сегодня утром я надела его впервые за несколько лет — хотела отнести фотографию из него в мастерскую. " +
                    "По дороге я выпила кофе, прошла через городской сквер и потом зашла в библиотеку. " +
                    "Уже там я заметила, что цепочка висит пустая. Я вернулась тем же маршрутом, но медальона нигде не было.\n\n" +
                    "Есть одна деталь, которая не даёт мне покоя: в сквере мой шарф резко за что-то зацепился. " +
                    "Я спешила, освободила его и пошла дальше, даже не посмотрев, за что именно. " +
                    "Этот медальон — единственная вещь, которая осталась у меня от бабушки. Поможете его найти?",
                clues: new[]
                {
                    "На внутренней стороне шарфа обнаружились свежая зелёная краска и тонкая древесная щепка. " +
                    "Утром в городском сквере как раз перекрашивали деревянные скамейки."
                },
                levels: new[]
                {
                    new Match3LevelDefinition(moves: 20, targetType: 0, targetCount: 15)
                },
                answerOptions: new[]
                {
                    "Кофейня",
                    "Городской сквер",
                    "Библиотека",
                    "Автобусная остановка"
                },
                correctAnswerIndex: 1,
                correctResponse:
                    "Да! Он действительно был там — медальон зацепился за край скамейки вместе с шарфом. " +
                    "Спасибо вам огромное. Я уже думала, что больше никогда его не увижу.",
                wrongResponse:
                    "Нет... там его не оказалось. Я ещё раз всё проверила, но медальона там точно нет.",
                correctAnswerExplanation:
                    "Правильный ответ: городской сквер. Шарф зацепился за свежеокрашенную деревянную скамейку, " +
                    "и медальон сорвался именно в этот момент.",
                inquiryQuestion: "Что именно случилось с вашим шарфом?",
                inquiryAnswer: "Я задела им что-то деревянное. Было совсем некогда останавливаться, а вечером я заметила на ткани маленькое зелёное пятно.",
                clueResponses: new[]
                {
                    "Неужели эта маленькая зацепка поможет? Посмотрите всё в деле и скажите, где мне поискать медальон."
                },
                shortIntro:
                    "Я потеряла медальон моей бабушки — единственную память о ней. " +
                    "Утром он был на мне, а после прогулки по городу исчез. Поможете мне его найти?"
            ),

            new CaseDefinition(
                id: "Case02",
                clientName: "Никита",
                lostItemName: "Плёночный фотоаппарат",
                story:
                    "Мне очень нужна ваша помощь. Я потерял старый плёночный фотоаппарат отца. " +
                    "Он давно не работает идеально, но отец снимал на него всю мою детскую жизнь, поэтому для меня он важнее любой новой камеры.\n\n" +
                    "Сегодня я нёс его в фотолабораторию. Сначала зашёл на блошиный рынок за плёнкой, потом выпил кофе, " +
                    "сел на трамвай №6 и уже возле лаборатории заметил, что кожаный чехол на плече почти невесомый. " +
                    "Самое странное — застёжка была закрыта, а ремешок оказался оборван. " +
                    "Я боюсь, что кто-то его забрал, но очень хочу понять, где он исчез.",
                clues: new[]
                {
                    "Камера у кофейни в 17:08 сняла Никиту на выходе. Фотоаппарат всё ещё висел у него на ремне — значит, на рынке он его не потерял."
                },
                levels: new[]
                {
                    new Match3LevelDefinition(
                        moves: 20,
                        targetType: 1,
                        targetCount: 14,
                        bombsEnabled: true,
                        planesEnabled: true,
                        colorClearEnabled: true)
                },
                answerOptions: new[]
                {
                    "Блошиный рынок",
                    "Кофейня",
                    "Трамвай №6",
                    "Фотолаборатория"
                },
                correctAnswerIndex: 2,
                correctResponse:
                    "Нашёлся! В депо подтвердили номер вагона, и фотоаппарат всё ещё был под сиденьем. " +
                    "Я даже не знаю, как вас благодарить. Для меня это не просто камера.",
                wrongResponse:
                    "Нет, там его не оказалось. Я проверил это место ещё раз — след ведёт куда-то в другую сторону.",
                correctAnswerExplanation:
                    "Правильный ответ: трамвай №6. После кофейни фотоаппарат был при Никите, " +
                    "а в вагоне нашли деталь от оборванного ремешка и позже зарегистрировали его чехол.",
                inquiryQuestion: "Когда вы в последний раз держали камеру в руках?",
                inquiryAnswer: "Возле кофейни я проверял, закрыта ли крышка объектива, и снова повесил чехол на плечо. После этого помню только дорогу и поездку.",
                clueResponses: new[]
                {
                    "Значит, у кофейни камера ещё была со мной. Давайте восстановим маршрут по времени — возможно, этого уже хватит, чтобы понять, где она исчезла."
                },
                miniGameAfterClue: 1,
                miniGameTitle: "Изучить три фотокадра",
                miniGameInstruction:
                    "Сопоставьте три кадра и выберите тот, на котором видна " +
                    "вероятная причина пропажи фотоаппарата.",
                miniGameCards: new[]
                {
                    "Кадр А · кофейня, 17:08 — фотоаппарат ещё на ремне",
                    "Кадр Б · трамвай №6, 17:19 — ремень резко натянут у задних дверей",
                    "Кадр В · фотолаборатория, 17:31 — чехол уже пуст"
                },
                miniGameCorrectOrder: new[] { 1 },
                miniGameResult:
                    "Фотолаборатория: на кадре из трамвая №6 в 17:19 ремешок " +
                    "фотоаппарата резко натянулся у поручня задних дверей. " +
                    "К 17:31 чехол уже пуст. Вероятнее всего, камера выпала в трамвае.",
                shortIntro:
                    "Я потерял старый фотоаппарат отца. Сегодня вёз его в фотолабораторию, " +
                    "но по дороге он исчез прямо из чехла. Поможете разобраться?",
                roomId: "PhotoLab",
                miniGameMode: "Spot"
            ),

            new CaseDefinition(
                id: "Case03",
                clientName: "Мария",
                lostItemName: "Посылка с семейным альбомом",
                story:
                    "Я отправила сестре семейный фотоальбом, но посылку так и не доставили. " +
                    "В приложении значится «вручено», хотя сестра весь день была дома. " +
                    "Я сохранила номер квитанции: 314. Помогите выяснить, куда попал альбом.",
                clues: new[]
                {
                    "На квитанции №314 стоит пометка «перенаправлено». " +
                    "Рядом указано время 16:40, но адрес замазан штампом. " +
                    "Точную историю перемещений можно проверить по журналу архива."
                },
                levels: new[]
                {
                    new Match3LevelDefinition(moves: 20, targetType: 2, targetCount: 14,
                        bombsEnabled: true, planesEnabled: true, colorClearEnabled: true)
                },
                answerOptions: new[]
                {
                    "Курьерская машина",
                    "Подъезд сестры",
                    "Пункт выдачи на Садовой",
                    "Почтовый ящик"
                },
                correctAnswerIndex: 2,
                correctResponse:
                    "Альбом действительно лежал в пункте выдачи! Его перенаправили туда после ошибки курьера. " +
                    "Спасибо, теперь наши семейные фотографии в безопасности.",
                wrongResponse:
                    "Там не оказалось альбома. Похоже, надо было внимательнее сверить архивные записи.",
                correctAnswerExplanation:
                    "Правильный ответ: пункт выдачи на Садовой. В архивном журнале записано, " +
                    "что посылку №314 перенаправили туда и оставили на хранении.",
                inquiryQuestion: "Номер квитанции точно сохранился?",
                inquiryAnswer: "Да, я сфотографировала её перед отправкой. На ней ясно видно №314 и дату.",
                clueResponses: new[]
                {
                    "Видите? На квитанции есть штамп, которого раньше не было. " +
                    "Давайте проверим документы в архиве."
                },
                miniGameAfterClue: 1,
                miniGameTitle: "Сверить путь посылки №314",
                miniGameInstruction:
                    "Архивные записи перепутаны. Нажмите карточки по времени — " +
                    "от отправления до последнего места хранения.",
                miniGameCards: new[]
                {
                    "16:40 — посылку приняли на хранение на Садовой",
                    "09:15 — посылка №314 отправлена со склада",
                    "14:20 — курьер отметил неудачную доставку"
                },
                miniGameCorrectOrder: new[] { 1, 2, 0 },
                miniGameResult:
                    "Архив подтверждает: последняя запись по №314 — пункт выдачи на Садовой, хранение в ячейке 7.",
                shortIntro:
                    "Моя сестра не получила посылку с нашим семейным альбомом. " +
                    "В системе она отмечена как вручённая. Поможете разобраться?",
                roomId: "ArchiveRoom"
            ),

            new CaseDefinition(
                id: "Case04",
                clientName: "Вера",
                lostItemName: "Старинная музыкальная шкатулка",
                story:
                    "Я обещала подарить маме её старую музыкальную шкатулку. " +
                    "Неделю назад отдала её знакомому часовщику на ремонт и получила бумажную расписку. " +
                    "Теперь расписка порвалась, а я не помню, в какую из мастерских города ходила. " +
                    "Боюсь, что подарок не успею вернуть.",
                clues: new[]
                {
                    "В кармане пальто нашли три кусочка расписки. На первом различима буква «С», " +
                    "на втором слово «часовщик», а на третьем цифра «8». " +
                    "Края бумаги совпадают, фрагменты можно восстановить в мастерской бюро."
                },
                levels: new[]
                {
                    new Match3LevelDefinition(moves: 22, targetType: 3, targetCount: 15,
                        bombsEnabled: true, planesEnabled: true, colorClearEnabled: true)
                },
                answerOptions: new[]
                {
                    "Антикварный магазин",
                    "Мастерская часовщика на Садовой, 8",
                    "Библиотека",
                    "Пункт выдачи"
                },
                correctAnswerIndex: 1,
                correctResponse:
                    "Да, это именно мастерская на Садовой! Шкатулку уже починили и сохранили для меня. " +
                    "Мама будет так рада. Спасибо!",
                wrongResponse:
                    "В этом месте шкатулки не оказалось. Надо было восстановить адрес из обрывков расписки.",
                correctAnswerExplanation:
                    "Правильный ответ: мастерская часовщика на Садовой, 8. " +
                    "Восстановленная расписка сохранила и название улицы, и номер дома.",
                inquiryQuestion: "Как выглядела расписка?",
                inquiryAnswer: "Она была на плотной кремовой бумаге, с коричневой печатью и адресом внизу.",
                clueResponses: new[]
                {
                    "Если восстановить расписку по кусочкам, мы узнаем точный адрес мастерской."
                },
                miniGameAfterClue: 1,
                miniGameTitle: "Собрать порванную расписку",
                miniGameInstruction:
                    "Восстановите адрес мастерской: выберите обрывок бумаги " +
                    "и поместите его в подходящую часть расписки.",
                miniGameCards: new[]
                {
                    "ДОМ 8",
                    "ЧАСОВЩИК · УЛ.",
                    "САДОВАЯ,"
                },
                miniGameCorrectOrder: new[] { 1, 2, 0 },
                miniGameResult:
                    "Расписка восстановлена: «Часовщик. ул. Садовая, дом 8».",
                shortIntro:
                    "Я отдала музыкальную шкатулку в ремонт, но потеряла адрес мастерской. " +
                    "Остались только обрывки расписки. Поможете?",
                roomId: "Workshop",
                miniGameMode: "Assembly"
            )
        };

        public const int TotalLevels = 200;

        // Later slots are deterministic content prototypes, not 196 separately
        // written stories. They can be replaced by hand-authored CaseDefinitions
        // without changing saves, scene transitions or puzzle progression.
        private static readonly CaseDefinition[] cases = BuildCases();

        private sealed class StoryPattern
        {
            public readonly string ObjectName;
            public readonly string Place;
            public readonly string OtherA;
            public readonly string OtherB;
            public readonly string OtherC;
            public readonly string Hint;
            public readonly string RoomHint;

            public StoryPattern(string objectName, string place,
                string otherA, string otherB, string otherC,
                string hint, string roomHint)
            {
                ObjectName = objectName;
                Place = place;
                OtherA = otherA;
                OtherB = otherB;
                OtherC = otherC;
                Hint = hint;
                RoomHint = roomHint;
            }
        }

        private static readonly string[] Names =
        {
            "Лиза", "Денис", "Алина", "Егор", "София", "Илья",
            "Дарья", "Артём", "Катя", "Михаил", "Юля", "Полина",
            "Кирилл", "Олеся", "Вадим", "Наташа", "Павел", "Женя"
        };

        private static readonly StoryPattern[] Patterns =
        {
            new StoryPattern("письмо бабушки", "книжный магазин", "сквер", "кофейня", "почта",
                "На конверте заметили книжную пыль и синюю наклейку с номером полки.",
                "В учётных записях обнаружилась та же наклейка книжного магазина."),
            new StoryPattern("ключ от мастерской", "автобус №4", "банк", "рынок", "пекарня",
                "К ключам прикреплена красная лента, замеченная у сиденья автобуса №4.",
                "В материалах проверки маршрут и время связаны с автобусом №4."),
            new StoryPattern("сшитый вручную шарф", "пекарня", "трамвай", "музей", "остановка",
                "К волокнам шарфа прилипли крошки и этикетка пекарни.",
                "Запись из пекарни подтверждает, что шарф остался на спинке стула."),
            new StoryPattern("блокнот с рисунками", "городская библиотека", "почта", "сквер", "ателье",
                "Внутри обнаружен библиотечный талон с сегодняшней датой.",
                "В журнале выдачи найден номер стола, за которым забыли блокнот."),
            new StoryPattern("карманные часы", "часовая мастерская", "вокзал", "рынок", "парк",
                "На цепочке осталась квитанция со штампом часовой мастерской.",
                "Снимок витрины позволяет уточнить, где остались часы."),
            new StoryPattern("подарочную открытку", "цветочный магазин", "кофейня", "студия", "парк",
                "На бумаге сохранились лепесток и ценник цветочного магазина.",
                "Журнал заказов указывает на цветочный магазин."),
            new StoryPattern("альбом с фотографиями", "фотоателье", "библиотека", "школа", "переход",
                "К обложке прилипла бумажная метка фотоателье.",
                "На обработанном изображении виден альбом возле приёмной стойки фотоателье."),
            new StoryPattern("браслет", "сквер у фонтана", "магазин", "трамвай", "театр",
                "Между звеньями нашли крупицу голубой краски с ограды фонтана.",
                "Сопоставление материалов указывает на сквер у фонтана."),
            new StoryPattern("записку с адресом", "ателье", "кафе", "автобус", "кинотеатр",
                "На клочке бумаги осталась нитка с характерной этикеткой ателье.",
                "Из восстановленной записи удалось прочитать адрес ателье."),
            new StoryPattern("коробочку с кулоном", "музейный гардероб", "парк", "аптека", "ярмарка",
                "На упаковке сохранился жетон музейного гардероба.",
                "Журнал находок подтверждает номер ячейки музейного гардероба."),
            new StoryPattern("стопку старых фотографий", "трамвай №8", "кофейня", "галерея", "вокзал",
                "На одной фотографии виден зелёный поручень трамвая №8.",
                "Обработка кадра позволяет установить номер вагона трамвая №8."),
            new StoryPattern("заводную игрушку", "ремонтную мастерскую", "почта", "библиотека", "театр",
                "В коробке лежит обрывок квитанции ремонтной мастерской.",
                "Склеенные фрагменты квитанции указывают на ремонтную мастерскую.")
        };

        private static CaseDefinition[] BuildCases()
        {
            List<CaseDefinition> generated = new List<CaseDefinition>(TotalLevels);
            generated.AddRange(authoredCases);

            for (int index = authoredCases.Length; index < TotalLevels; index++)
            {
                int levelNumber = index + 1;
                StoryPattern pattern = Patterns[(index - authoredCases.Length) % Patterns.Length];
                string client = Names[(index * 7 + 3) % Names.Length];
                string caseTag = levelNumber.ToString("000");
                string tracking = "Н-" + (2000 + index * 37).ToString();
                string room = index % 3 == 0 ? "PhotoLab" :
                              index % 3 == 1 ? "ArchiveRoom" : "Workshop";
                int challenge = (index / 3) % 4;
                string mode = room == "PhotoLab"
                    ? new[] { "Spot", "Compare", "Focus", "Sequence" }[challenge]
                    : room == "ArchiveRoom"
                        ? new[] { "Sequence", "Catalog", "CrossCheck", "Code" }[challenge]
                        : new[] { "Assembly", "Repair", "Pair", "Restore" }[challenge];

                bool selectOne = mode == "Spot" || mode == "Compare" ||
                                 mode == "Focus" || mode == "Catalog" ||
                                 mode == "CrossCheck" || mode == "Repair" ||
                                 mode == "Pair";
                string taskTitle = room == "PhotoLab"
                    ? new[] { "Изучить кадры", "Сравнить снимки",
                              "Найти деталь при увеличении", "Разложить кадры" }[challenge]
                    : room == "ArchiveRoom"
                        ? new[] { "Сверить хронологию", "Найти запись в каталоге",
                                  "Сопоставить два документа", "Восстановить код записи" }[challenge]
                        : new[] { "Собрать документ", "Выбрать подходящий фрагмент",
                                  "Найти совпадающие детали", "Восстановить записку" }[challenge];

                string[] puzzleOptions = selectOne
                    ? new[]
                    {
                        "Запись А · след ведёт к месту «" + pattern.OtherA + "».",
                        "Запись Б · " + pattern.RoomHint,
                        "Запись В · свидетель упомянул «" + pattern.OtherB + "»."
                    }
                    : new[]
                    {
                        "Часть III · Подтверждение: " + pattern.Place + ".",
                        "Часть I · Обращение №" + tracking + ".",
                        "Часть II · " + pattern.RoomHint
                    };

                int correctIndex = (index * 3 + 1) % 4;
                string[] answerOptions =
                {
                    pattern.OtherA, pattern.OtherB, pattern.OtherC, pattern.Place
                };
                string temp = answerOptions[3];
                answerOptions[3] = answerOptions[correctIndex];
                answerOptions[correctIndex] = temp;

                generated.Add(new CaseDefinition(
                    id: "Case" + caseTag,
                    clientName: client,
                    lostItemName: pattern.ObjectName,
                    story:
                        "У меня пропали " + pattern.ObjectName +
                        ". После нескольких остановок я заметил(а) пропажу. " +
                        "Я успел(а) заглянуть в разные места, поэтому очень хочу восстановить маршрут. " +
                        "Номер обращения: " + tracking + ".",
                    clues: new[]
                    {
                        "Улика из поиска №" + tracking + ": " + pattern.Hint +
                        " Для подтверждения нужно исследование в соответствующем отделе."
                    },
                    levels: new[]
                    {
                        new Match3LevelDefinition(
                            moves: 20 + index % 7,
                            targetType: index % 6,
                            targetCount: 12 + index % 8,
                            bombsEnabled: true,
                            planesEnabled: true,
                            colorClearEnabled: true)
                    },
                    answerOptions: answerOptions,
                    correctAnswerIndex: correctIndex,
                    correctResponse:
                        "Спасибо! " + pattern.ObjectName +
                        " нашлись в нужном месте. Теперь я смогу их забрать!",
                    wrongResponse:
                        "К сожалению, там их не оказалось. Давайте сверим собранные материалы.",
                    correctAnswerExplanation:
                        "Правильный ответ: " + pattern.Place +
                        ". " + pattern.Hint + " " + pattern.RoomHint,
                    inquiryQuestion: "Что вы помните о последнем месте?",
                    inquiryAnswer: "Я сохранил(а) маршрут и номер обращения " +
                        tracking + ". Возможно, в документах найдётся ещё одна деталь.",
                    clueResponses: new[]
                    {
                        "Это полезная находка. Давайте проверим её в отделе расследований."
                    },
                    miniGameAfterClue: 1,
                    miniGameTitle: taskTitle + " · дело №" + caseTag,
                    miniGameInstruction: selectOne
                        ? "Выберите материал, который подтверждает место пропажи."
                        : "Расположите три фрагмента в логической последовательности.",
                    miniGameCards: puzzleOptions,
                    miniGameCorrectOrder: selectOne
                        ? new[] { 1 }
                        : new[] { 1, 2, 0 },
                    miniGameResult: room == "PhotoLab"
                        ? "Фотолаборатория. " + pattern.RoomHint
                        : room == "ArchiveRoom"
                            ? "Архив документов. " + pattern.RoomHint
                            : "Мастерская находок. " + pattern.RoomHint,
                    shortIntro: "У меня пропали " + pattern.ObjectName +
                        ". Я очень хочу их вернуть. Поможете разобраться?",
                    roomId: room,
                    miniGameMode: mode
                ));
            }

            return generated.ToArray();
        }

        public static IReadOnlyList<CaseDefinition> Cases => cases;

        public static CaseDefinition GetCase(int index)
        {
            if (index < 0 || index >= cases.Length)
                return null;

            return cases[index];
        }
    }
}
