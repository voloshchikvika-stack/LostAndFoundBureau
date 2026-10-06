КАРТИНКИ ДЛЯ MATCH-3

Фон:
Assets/Resources/Match3/Background.png

Фишки:
Assets/Resources/Match3/Pieces/Footprints.png
Assets/Resources/Match3/Pieces/Key.png
Assets/Resources/Match3/Pieces/Map.png
Assets/Resources/Match3/Pieces/Note.png
Assets/Resources/Match3/Pieces/Tag.png
Assets/Resources/Match3/Pieces/Magnifier.png

Для каждого PNG в Unity:
Texture Type = Sprite (2D and UI)
Sprite Mode = Single
Apply

Если какого-то файла нет, игра автоматически покажет встроенную временную иконку.


БОНУСЫ НА ПОЛЕ

Если картинки не добавлять, игра сама нарисует временные бонусы.

Можно заменить их своими PNG:
Assets/Resources/Match3/Bonuses/RocketHorizontal.png
Assets/Resources/Match3/Bonuses/RocketVertical.png
Assets/Resources/Match3/Bonuses/Bomb.png
Assets/Resources/Match3/Bonuses/Plane.png
Assets/Resources/Match3/Bonuses/ColorClear.png

Создание:
4 в линию -> ракета
5 в T/L -> бомба
5 в линию -> радужный шар
квадрат 2x2 -> самолётик

Комбинации:
ракета + ракета -> строка + столбец
бомба + бомба -> удвоенный радиус
бомба + ракета -> 3 строки + 3 столбца
радужный шар + ракета/бомба/самолётик -> самый частый тип фишки
превращается в соответствующий бонус и активируется
радужный шар + радужный шар -> очищение поля
самолётик + ракета/бомба -> перенос бонуса к цели
самолётик + самолётик -> три цели
