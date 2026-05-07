namespace _7t2;

/// <summary>
/// Описывает одну строку таблицы кодов.
/// </summary>
public class CodebookEntry
{
    /// <summary>
    /// Создает пустую строку таблицы кодов.
    /// </summary>
    public CodebookEntry()
    {
    }

    /// <summary>
    /// Создает строку таблицы кодов со словом и кодом.
    /// </summary>
    /// <param name="sourceWord">Исходное слово.</param>
    /// <param name="code">Код для замены слова.</param>
    public CodebookEntry(string sourceWord, string code)
    {
        SourceWord = sourceWord;
        Code = code;
    }

    /// <summary>
    /// Получает или задает исходное слово.
    /// </summary>
    public string SourceWord { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает код, который заменяет слово.
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
