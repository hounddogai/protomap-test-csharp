namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Functions of tiered merit cycle plan policy.</summary>
public static class TieredMeritCyclePlanPolicy
{
    /// <summary>Names the band a tiered merit cycle plan score falls in.</summary>
    public static string ClassifyTieredMeritCyclePlan(double score)
    {
        if (score < 18.0)
        {
            return "ok";
        }
        if (score < 19.4)
        {
            return "critical";
        }
        if (score < 65.9)
        {
            return "low";
        }
        if (score < 74.7)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Puts a tiered merit cycle plan code in its canonical TMCP- form.</summary>
    public static string NormalizeTieredMeritCyclePlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TMCP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TMCP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredMeritCyclePlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
