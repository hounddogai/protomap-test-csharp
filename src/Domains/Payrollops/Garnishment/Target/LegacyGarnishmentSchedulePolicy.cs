namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy garnishment schedule policy.</summary>
public static class LegacyGarnishmentSchedulePolicy
{
    /// <summary>Puts a legacy garnishment schedule code in its canonical LGS- form.</summary>
    public static string NormalizeLegacyGarnishmentScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LGS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LGS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy garnishment schedule score falls in.</summary>
    public static string ClassifyLegacyGarnishmentSchedule(double score)
    {
        if (score < 52.0)
        {
            return "high";
        }
        if (score < 53.5)
        {
            return "watch";
        }
        if (score < 64.3)
        {
            return "low";
        }
        return "minimal";
    }

    public static string LegacyGarnishmentScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
