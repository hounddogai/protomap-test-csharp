namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>Functions of seasonal bonus pool schedule policy.</summary>
public static class SeasonalBonusPoolSchedulePolicy
{
    /// <summary>Puts a seasonal bonus pool schedule code in its canonical SBPS- form.</summary>
    public static string NormalizeSeasonalBonusPoolScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a seasonal bonus pool schedule score falls in.</summary>
    public static string ClassifySeasonalBonusPoolSchedule(double score)
    {
        if (score < 15.4)
        {
            return "watch";
        }
        if (score < 64.3)
        {
            return "minimal";
        }
        return "elevated";
    }
}
