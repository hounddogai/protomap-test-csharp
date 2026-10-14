namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Functions of merit cycle group policy.</summary>
public static class MeritCycleGroupPolicy
{
    /// <summary>Puts a merit cycle group code in its canonical MCG- form.</summary>
    public static string NormalizeMeritCycleGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a merit cycle group score falls in.</summary>
    public static string ClassifyMeritCycleGroup(double score)
    {
        if (score < 18.8)
        {
            return "minimal";
        }
        if (score < 44.7)
        {
            return "high";
        }
        return "elevated";
    }
}
