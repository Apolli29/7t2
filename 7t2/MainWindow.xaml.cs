using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace _7t2;

/// <summary>
/// Содержит обработчики элементов главного окна приложения.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ObservableCollection<CodebookEntry> codebookEntries;
    private readonly PortaCipher portaCipher;

    /// <summary>
    /// Создает главное окно и подготавливает данные для интерфейса.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        portaCipher = new PortaCipher();
        codebookEntries = new ObservableCollection<CodebookEntry>();
        CodebookDataGrid.ItemsSource = codebookEntries;

        LoadDefaultCodebook();
    }

    /// <summary>
    /// Выполняет шифрование текста.
    /// </summary>
    private void EncryptButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Окно] Нажата кнопка шифрования.");
        ExecuteOperation("Шифрование выполнено успешно.", text => portaCipher.Encrypt(text, GetCurrentCodebook()));
    }

    /// <summary>
    /// Выполняет дешифрование текста.
    /// </summary>
    private void DecryptButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Окно] Нажата кнопка дешифрования.");
        ExecuteOperation("Дешифрование выполнено успешно.", text => portaCipher.Decrypt(text, GetCurrentCodebook()));
    }

    /// <summary>
    /// Загружает таблицу кодов по умолчанию.
    /// </summary>
    private void LoadDefaultTableButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Окно] Загружена таблица кодов по умолчанию.");
        LoadDefaultCodebook();
        StatusTextBlock.Text = "Загружена таблица кодов по умолчанию.";
        ErrorTextBox.Text = string.Empty;
    }

    /// <summary>
    /// Добавляет пустую строку в таблицу кодов.
    /// </summary>
    private void AddRowButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Окно] Добавлена новая строка таблицы.");
        codebookEntries.Add(new CodebookEntry());
        StatusTextBlock.Text = "Добавлена новая строка таблицы кодов.";
    }

    /// <summary>
    /// Удаляет выбранную строку таблицы кодов.
    /// </summary>
    private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
    {
        if (CodebookDataGrid.SelectedItem is not CodebookEntry selectedEntry)
        {
            ErrorTextBox.Text = "Сначала выберите строку таблицы кодов для удаления.";
            StatusTextBlock.Text = "Удаление строки не выполнено.";
            return;
        }

        Debug.WriteLine("[Окно] Удалена выбранная строка таблицы.");
        codebookEntries.Remove(selectedEntry);
        StatusTextBlock.Text = "Выбранная строка удалена.";
        ErrorTextBox.Text = string.Empty;
    }

    /// <summary>
    /// Очищает текстовые поля.
    /// </summary>
    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Окно] Выполнена очистка полей.");
        SourceTextBox.Text = string.Empty;
        ResultTextBox.Text = string.Empty;
        ErrorTextBox.Text = string.Empty;
        StatusTextBlock.Text = "Поля очищены.";
    }

    private void LoadDefaultCodebook()
    {
        codebookEntries.Clear();

        foreach (var entry in DefaultCodebookProvider.Create())
        {
            codebookEntries.Add(entry);
        }
    }

    private List<CodebookEntry> GetCurrentCodebook()
    {
        return codebookEntries
            .Select(entry => new CodebookEntry
            {
                SourceWord = entry.SourceWord,
                Code = entry.Code,
            })
            .ToList();
    }

    private void ExecuteOperation(string successMessage, Func<string, string> action)
    {
        try
        {
            ErrorTextBox.Text = string.Empty;
            ResultTextBox.Text = action(SourceTextBox.Text);
            StatusTextBlock.Text = successMessage;
        }
        catch (Exception exception) when (exception is ArgumentNullException or CodebookValidationException)
        {
            ResultTextBox.Text = string.Empty;
            ErrorTextBox.Text = BuildErrorText(exception);
            StatusTextBlock.Text = "Операция не выполнена.";
        }
    }

    private static string BuildErrorText(Exception exception)
    {
        if (exception is ArgumentNullException)
        {
            return "Получено пустое значение вместо текста.";
        }

        if (exception is not CodebookValidationException validationException)
        {
            return exception.Message;
        }

        var builder = new StringBuilder();
        builder.AppendLine(validationException.Message);

        foreach (var validationError in validationException.ValidationErrors)
        {
            builder.AppendLine($"- {validationError}");
        }

        foreach (var token in validationException.ProblemTokens)
        {
            builder.AppendLine($"- Проблемный токен: {token}");
        }

        return builder.ToString().TrimEnd();
    }
}
