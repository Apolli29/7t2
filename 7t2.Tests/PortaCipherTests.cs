namespace _7t2.Tests;

[TestClass]
public sealed class PortaCipherTests
{
    private readonly PortaCipher portaCipher = new();

    [TestMethod]
    public void Encrypt_UsesDefaultCodebook_AndPreservesPunctuation()
    {
        var text = "Алгоритм, шифрование, тестирование.";

        var encrypted = portaCipher.Encrypt(text, DefaultCodebookProvider.Create());

        Assert.AreEqual("P14-01, P14-12, P14-11.", encrypted);
    }

    [TestMethod]
    public void Decrypt_UsesDefaultCodebook_AndPreservesPunctuation()
    {
        var encrypted = "P14-01, P14-12, P14-11.";

        var decrypted = portaCipher.Decrypt(encrypted, DefaultCodebookProvider.Create());

        Assert.AreEqual("алгоритм, шифрование, тестирование.", decrypted);
    }

    [TestMethod]
    public void Encrypt_UsesCustomCodebook()
    {
        var codebook = new[]
        {
            new CodebookEntry("alpha", "C1"),
            new CodebookEntry("beta", "C2"),
        };

        var encrypted = portaCipher.Encrypt("alpha beta alpha", codebook);

        Assert.AreEqual("C1 C2 C1", encrypted);
    }

    [TestMethod]
    public void Encrypt_IsCaseInsensitiveForSourceWords()
    {
        var encrypted = portaCipher.Encrypt("АлГоРиТм", DefaultCodebookProvider.Create());

        Assert.AreEqual("P14-01", encrypted);
    }

    [TestMethod]
    public void Encrypt_ThrowsArgumentNullException_WhenTextIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => portaCipher.Encrypt(null!, DefaultCodebookProvider.Create()));
    }

    [TestMethod]
    public void Encrypt_ThrowsValidationException_WhenTextIsEmpty()
    {
        var exception = Assert.ThrowsException<CodebookValidationException>(
            () => portaCipher.Encrypt("   ", DefaultCodebookProvider.Create()));

        StringAssert.Contains(exception.Message, "Текст не должен быть пустым");
    }

    [TestMethod]
    public void Encrypt_ThrowsValidationException_WhenCodebookIsEmpty()
    {
        var exception = Assert.ThrowsException<CodebookValidationException>(
            () => portaCipher.Encrypt("алгоритм", Array.Empty<CodebookEntry>()));

        StringAssert.Contains(exception.Message, "Таблица кодов не должна быть пустой");
    }

    [TestMethod]
    public void Encrypt_ThrowsValidationException_WhenSourceWordsAreDuplicated()
    {
        var codebook = new[]
        {
            new CodebookEntry("alpha", "C1"),
            new CodebookEntry("alpha", "C2"),
        };

        var exception = Assert.ThrowsException<CodebookValidationException>(
            () => portaCipher.Encrypt("alpha", codebook));

        CollectionAssert.Contains(exception.ValidationErrors.ToArray(), "Дублирующееся слово в таблице: alpha.");
    }

    [TestMethod]
    public void Decrypt_ThrowsValidationException_WhenCodesAreDuplicated()
    {
        var codebook = new[]
        {
            new CodebookEntry("alpha", "C1"),
            new CodebookEntry("beta", "C1"),
        };

        var exception = Assert.ThrowsException<CodebookValidationException>(
            () => portaCipher.Decrypt("C1", codebook));

        CollectionAssert.Contains(exception.ValidationErrors.ToArray(), "Дублирующийся код в таблице: C1.");
    }

    [TestMethod]
    public void Encrypt_ThrowsValidationException_WhenSourceWordIsMissing()
    {
        var exception = Assert.ThrowsException<CodebookValidationException>(
            () => portaCipher.Encrypt("неизвестно", DefaultCodebookProvider.Create()));

        CollectionAssert.Contains(exception.ProblemTokens.ToArray(), "неизвестно");
    }

    [TestMethod]
    public void Decrypt_ThrowsValidationException_WhenCodeIsMissing()
    {
        var exception = Assert.ThrowsException<CodebookValidationException>(
            () => portaCipher.Decrypt("P14-99", DefaultCodebookProvider.Create()));

        CollectionAssert.Contains(exception.ProblemTokens.ToArray(), "P14-99");
    }

    [TestMethod]
    public void Encrypt_ThrowsValidationException_WhenCodebookContainsEmptyCells()
    {
        var codebook = new[]
        {
            new CodebookEntry(string.Empty, "C1"),
            new CodebookEntry("beta", string.Empty),
        };

        var exception = Assert.ThrowsException<CodebookValidationException>(
            () => portaCipher.Encrypt("beta", codebook));

        Assert.AreEqual(2, exception.ValidationErrors.Count);
    }
}
