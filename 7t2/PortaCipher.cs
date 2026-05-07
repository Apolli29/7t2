using System.Diagnostics;
using System.Text;

namespace _7t2;

/// <summary>
/// Выполняет шифрование и дешифрование по таблице кодов.
/// </summary>
public class PortaCipher
{
    /// <summary>
    /// Шифрует текст по таблице кодов.
    /// </summary>
    /// <param name="text">Исходный текст.</param>
    /// <param name="codebook">Текущая таблица кодов.</param>
    /// <returns>Зашифрованный текст.</returns>
    public string Encrypt(string text, IReadOnlyCollection<CodebookEntry> codebook)
    {
        ArgumentNullException.ThrowIfNull(text);

        Debug.WriteLine("[Шифрование] Запуск операции.");
        CheckText(text);
        var preparedCodebook = PrepareCodebook(codebook);

        var result = new StringBuilder();
        var missingWords = new List<string>();
        var index = 0;

        while (index < text.Length)
        {
            if (char.IsLetterOrDigit(text[index]))
            {
                var startIndex = index;

                while (index < text.Length && char.IsLetterOrDigit(text[index]))
                {
                    index++;
                }

                var token = text[startIndex..index];
                if (preparedCodebook.Words.TryGetValue(token, out var code))
                {
                    result.Append(code);
                }
                else
                {
                    missingWords.Add(token);
                }

                continue;
            }

            result.Append(text[index]);
            index++;
        }

        if (missingWords.Count > 0)
        {
            throw new CodebookValidationException(
                "В таблице кодов отсутствуют слова для шифрования.",
                problemTokens: missingWords);
        }

        return result.ToString();
    }

    /// <summary>
    /// Дешифрует текст по таблице кодов.
    /// </summary>
    /// <param name="text">Текст с кодами.</param>
    /// <param name="codebook">Текущая таблица кодов.</param>
    /// <returns>Расшифрованный текст.</returns>
    public string Decrypt(string text, IReadOnlyCollection<CodebookEntry> codebook)
    {
        ArgumentNullException.ThrowIfNull(text);

        Debug.WriteLine("[Дешифрование] Запуск операции.");
        CheckText(text);
        var preparedCodebook = PrepareCodebook(codebook);

        var result = new StringBuilder();
        var missingCodes = new List<string>();
        var index = 0;

        while (index < text.Length)
        {
            if (char.IsWhiteSpace(text[index]))
            {
                result.Append(text[index]);
                index++;
                continue;
            }

            if (TryReadKnownCode(text, index, preparedCodebook, out var knownCode, out var sourceWord))
            {
                result.Append(sourceWord);
                index += knownCode.Length;
                continue;
            }

            if (IsCodeCharacter(text[index], preparedCodebook.CodeCharacters))
            {
                var startIndex = index;

                while (index < text.Length && IsCodeCharacter(text[index], preparedCodebook.CodeCharacters))
                {
                    index++;
                }

                missingCodes.Add(text[startIndex..index]);
                continue;
            }

            result.Append(text[index]);
            index++;
        }

        if (missingCodes.Count > 0)
        {
            throw new CodebookValidationException(
                "В таблице кодов отсутствуют коды для дешифрования.",
                problemTokens: missingCodes);
        }

        return result.ToString();
    }

    private static void CheckText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new CodebookValidationException(
                "Текст не должен быть пустым.",
                validationErrors: ["Введите текст в поле ввода."]);
        }
    }

    private static PreparedCodebook PrepareCodebook(IReadOnlyCollection<CodebookEntry> codebook)
    {
        ArgumentNullException.ThrowIfNull(codebook);

        if (codebook.Count == 0)
        {
            throw new CodebookValidationException(
                "Таблица кодов не должна быть пустой.",
                validationErrors: ["Добавьте хотя бы одну строку в таблицу кодов."]);
        }

        var errors = new List<string>();
        var words = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var codes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var codeCharacters = new HashSet<char>();

        for (var i = 0; i < codebook.Count; i++)
        {
            var entry = codebook.ElementAt(i);
            var rowNumber = i + 1;
            var sourceWord = entry.SourceWord?.Trim() ?? string.Empty;
            var code = entry.Code?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sourceWord))
            {
                errors.Add($"Строка {rowNumber}: слово не должно быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                errors.Add($"Строка {rowNumber}: код не должен быть пустым.");
            }

            if (!string.IsNullOrWhiteSpace(sourceWord) && !sourceWord.All(char.IsLetterOrDigit))
            {
                errors.Add($"Строка {rowNumber}: слово \"{sourceWord}\" должно содержать только буквы и цифры.");
            }

            if (!string.IsNullOrWhiteSpace(code) && code.Any(char.IsWhiteSpace))
            {
                errors.Add($"Строка {rowNumber}: код \"{code}\" не должен содержать пробелы.");
            }

            if (string.IsNullOrWhiteSpace(sourceWord) ||
                string.IsNullOrWhiteSpace(code) ||
                !sourceWord.All(char.IsLetterOrDigit) ||
                code.Any(char.IsWhiteSpace))
            {
                continue;
            }

            if (!words.TryAdd(sourceWord, code))
            {
                errors.Add($"Дублирующееся слово в таблице: {sourceWord}.");
            }

            if (!codes.TryAdd(code, sourceWord))
            {
                errors.Add($"Дублирующийся код в таблице: {code}.");
            }

            foreach (var symbol in code)
            {
                codeCharacters.Add(symbol);
            }
        }

        if (errors.Count > 0)
        {
            throw new CodebookValidationException("Таблица кодов заполнена с ошибками.", errors);
        }

        return new PreparedCodebook
        {
            Words = words,
            Codes = codes,
            CodeCharacters = codeCharacters,
            OrderedCodes = codes.Keys.OrderByDescending(item => item.Length).ToList(),
        };
    }

    private static bool TryReadKnownCode(
        string text,
        int startIndex,
        PreparedCodebook preparedCodebook,
        out string code,
        out string word)
    {
        foreach (var currentCode in preparedCodebook.OrderedCodes)
        {
            if (!text.AsSpan(startIndex).StartsWith(currentCode, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var nextIndex = startIndex + currentCode.Length;
            if (nextIndex < text.Length && IsCodeCharacter(text[nextIndex], preparedCodebook.CodeCharacters))
            {
                continue;
            }

            code = currentCode;
            word = preparedCodebook.Codes[currentCode];
            return true;
        }

        code = string.Empty;
        word = string.Empty;
        return false;
    }

    private static bool IsCodeCharacter(char symbol, HashSet<char> codeCharacters)
    {
        return char.IsLetterOrDigit(symbol) || codeCharacters.Contains(symbol);
    }

    /// <summary>
    /// Хранит подготовленные словари для поиска.
    /// </summary>
    private class PreparedCodebook
    {
        /// <summary>
        /// Получает или задает словарь слово-код.
        /// </summary>
        public Dictionary<string, string> Words { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Получает или задает словарь код-слово.
        /// </summary>
        public Dictionary<string, string> Codes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Получает или задает список кодов по убыванию длины.
        /// </summary>
        public List<string> OrderedCodes { get; set; } = [];

        /// <summary>
        /// Получает или задает набор символов, которые встречаются в кодах.
        /// </summary>
        public HashSet<char> CodeCharacters { get; set; } = [];
    }
}
