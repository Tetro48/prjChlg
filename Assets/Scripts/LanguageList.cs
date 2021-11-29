using System;

public enum LangArray {mainMenu, settingsMenu, inputsMenu, notifications}
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
            {"Singles: ", "Doubles: ", "Triples: ", "Tetrises: ", "lines: ", "Total ", "Pieces: ", "Grade: ", "Total grade score:", "Level: ", "Gravity: ", "Time: ", "500 level part complete!", "Controller is swapped", "Grade score: ", "Starting up!", "All Clears: "},
            {"Одиночные: ", "Двойные: ", "Тройные: ", "Тетрисы: ", "линии: ", "Всего ", "Фигур: ", "Оценка: ", "Общий счет оценки:", "Уровень: ", "Гравитация: ", "Время: ", "Достигнуто часть 500 уровней!", "Контроллер заменен", "Счет оценки: ", "Начинаем!", "Полные очистки: "},
            {"シングル：", "ダブル：", "トリプル", "テトリス：", "行：", "合計", "ピース", "成績：", "総合成績スコア", "レベル：", "時間", "", "", "", "", "", ""},
        }
    };
}