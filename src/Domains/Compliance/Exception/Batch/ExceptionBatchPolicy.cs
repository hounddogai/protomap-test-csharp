namespace Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Functions of exception batch policy.</summary>
public static class ExceptionBatchPolicy
{
    public static int ExceptionBatchSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a exception batch code in its canonical EB- form.</summary>
    public static string NormalizeExceptionBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
