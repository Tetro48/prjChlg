/*
    Project Challenger, a challenging block stacking game.
    Copyright (C) 2022-2023, Aymir

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
//This might be the most messy of a language system.
public enum LangArray { mainMenu, settingsMenu, inputsMenu, notifications }
public static class LanguageList
{
    public static string Extract(LangArray array, Language language, int select)
    {
        return LangString[(int)array][(int)language, select];
    }
    public static string[][,] LangString =
    {
        new string[,]
        {
            {"Play!", "Settings", "Quit"},
            {"Играть!", "Настройки", "Выйти"},
            {"プレイ!", "セッチング", "終了する"}
        },
        new string[,]
        {
            {"Resolution:", "Inputs", "Rotation Systems", "Custom Mode settings", "Preferences settings", "Tuning", "< Back"},
            {"Разрешение:", "Вводы", "Системы вращения", "Настройки режима", "Настройки предпотчении", "Тьюнинг", "< Назад"},
            {"解像度", "入力", "回転システム", "カスタムモードの設定", "プリファレンス設定", "チューニング", "< バック"}
        },
        new string[,]
        {
            {"", "", "", "", "", "", ""},
            {"", "", "", "", "", "", ""},
            {"", "", "", "", "", "", ""},
        },
        new string[,]
        {
            //
            {"Singles: ", "Doubles: ", "Triples: ", "Quadruples: ", "lines: ", "Total ", "Pieces: ", "Grade: ", "Total grade score:", "Level: ", "Gravity: ", "Time: ", "500 level part complete!", "Controller is swapped", "Grade score: ", "Starting up!", "All Clears: "},
            {"Одиночные: ", "Двойные: ", "Тройные: ", "Четверные: ", "линии: ", "Всего ", "Фигур: ", "Оценка: ", "Общий счет оценки:", "Уровень: ", "Гравитация: ", "Время: ", "Достигнуто часть 500 уровней!", "Контроллер заменен", "Счет оценки: ", "Начинаем!", "Полные очистки: "},
            {"シングル：", "ダブル：", "トリプル", "クアドラプル：", "行：", "合計", "ピース", "成績：", "総合成績スコア", "レベル：", "時間", "", "", "", "", "", ""},
        }
    };
}