namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception record policy.</summary>
public static class ExceptionRecordPolicy
{
    /// <summary>Names the band a exception record score falls in.</summary>
    public static string ClassifyExceptionRecord(double score)
    {
        if (score < 7.6)
        {
            return "medium";
        }
        if (score < 31.9)
        {
            return "high";
        }
        if (score < 59.8)
        {
            return "elevated";
        }
        if (score < 93.7)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Puts a exception record code in its canonical ER- form.</summary>
    public static string NormalizeExceptionRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ER-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ER-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ExceptionRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
