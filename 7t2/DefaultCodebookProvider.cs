namespace _7t2;

/// <summary>
/// Возвращает таблицу кодов по умолчанию.
/// </summary>
public static class DefaultCodebookProvider
{
    /// <summary>
    /// Создает стандартный набор слов и кодов.
    /// </summary>
    /// <returns>Список строк таблицы кодов.</returns>
    public static List<CodebookEntry> Create()
    {
        return
        [
            new CodebookEntry { SourceWord = "алгоритм", Code = "P14-01" },
            new CodebookEntry { SourceWord = "безопасность", Code = "P14-02" },
            new CodebookEntry { SourceWord = "данные", Code = "P14-03" },
            new CodebookEntry { SourceWord = "дешифрование", Code = "P14-04" },
            new CodebookEntry { SourceWord = "модуль", Code = "P14-05" },
            new CodebookEntry { SourceWord = "ошибка", Code = "P14-06" },
            new CodebookEntry { SourceWord = "пользователь", Code = "P14-07" },
            new CodebookEntry { SourceWord = "проверка", Code = "P14-08" },
            new CodebookEntry { SourceWord = "программа", Code = "P14-09" },
            new CodebookEntry { SourceWord = "сообщение", Code = "P14-10" },
            new CodebookEntry { SourceWord = "тестирование", Code = "P14-11" },
            new CodebookEntry { SourceWord = "шифрование", Code = "P14-12" },
        ];
    }
}
