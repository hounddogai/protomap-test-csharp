namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of garnishment budget policy.</summary>
public static class GarnishmentBudgetPolicy
{
    public static string GarnishmentBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a garnishment budget code in its canonical GB- form.</summary>
    public static string NormalizeGarnishmentBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int GarnishmentBudgetSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a garnishment budget score falls in.</summary>
    public static string ClassifyGarnishmentBudget(double score)
    {
        if (score < 59.0)
        {
            return "critical";
        }
        if (score < 69.4)
        {
            return "low";
        }
        return "minimal";
    }
}
