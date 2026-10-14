namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal floor plan checkpoint policy.</summary>
public static class SeasonalFloorPlanCheckpointPolicy
{
    public static double SeasonalFloorPlanCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal floor plan checkpoint score falls in.</summary>
    public static string ClassifySeasonalFloorPlanCheckpoint(double score)
    {
        if (score < 6.5)
        {
            return "medium";
        }
        if (score < 46.5)
        {
            return "low";
        }
        if (score < 48.7)
        {
            return "elevated";
        }
        if (score < 52.6)
        {
            return "minimal";
        }
        return "watch";
    }

    /// <summary>Puts a seasonal floor plan checkpoint code in its canonical SFPC- form.</summary>
    public static string NormalizeSeasonalFloorPlanCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalFloorPlanCheckpoint()
    {
        var policy = Retry.DefaultPolicy("seasonal-floor-plan-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
