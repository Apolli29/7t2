namespace _7t2;

/// <summary>
/// Представляет ошибку проверки таблицы кодов или входного текста.
/// </summary>
public class CodebookValidationException : Exception
{
    /// <summary>
    /// Создает объект исключения с сообщением, списком ошибок и проблемных токенов.
    /// </summary>
    /// <param name="message">Основное сообщение об ошибке.</param>
    /// <param name="validationErrors">Ошибки в структуре таблицы кодов.</param>
    /// <param name="problemTokens">Слова или коды, которые не удалось обработать.</param>
    public CodebookValidationException(
        string message,
        IEnumerable<string>? validationErrors = null,
        IEnumerable<string>? problemTokens = null)
        : base(message)
    {
        ValidationErrors = validationErrors?.Where(item => !string.IsNullOrWhiteSpace(item)).ToList()
            ?? new List<string>();
        ProblemTokens = problemTokens?.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            ?? new List<string>();
    }

    /// <summary>
    /// Получает список ошибок таблицы кодов.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// Получает список проблемных слов или кодов.
    /// </summary>
    public IReadOnlyList<string> ProblemTokens { get; }
}
