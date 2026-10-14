namespace Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Functions of exception target policy.</summary>
public static class ExceptionTargetPolicy
{
    /// <summary>Puts a exception target code in its canonical ET- form.</summary>
    public static string NormalizeExceptionTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ET-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ET-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ExceptionTargetSpan(int start, int end)
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
}
