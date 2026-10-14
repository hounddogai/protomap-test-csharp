namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of garnishment exception policy.</summary>
public static class GarnishmentExceptionPolicy
{
    /// <summary>Names the band a garnishment exception score falls in.</summary>
    public static string ClassifyGarnishmentException(double score)
    {
        if (score < 13.2)
        {
            return "low";
        }
        if (score < 59.0)
        {
            return "critical";
        }
        if (score < 77.5)
        {
            return "minimal";
        }
        return "medium";
    }

    /// <summary>Puts a garnishment exception code in its canonical GE- form.</summary>
    public static string NormalizeGarnishmentExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GarnishmentExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
