namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

/// <summary>Functions of exception variance policy.</summary>
public static class ExceptionVariancePolicy
{
    public static int ExceptionVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a exception variance code in its canonical EV- form.</summary>
    public static string NormalizeExceptionVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
