namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle batch policy.</summary>
public static class MeritCycleBatchPolicy
{
    /// <summary>Names the band a merit cycle batch score falls in.</summary>
    public static string ClassifyMeritCycleBatch(double score)
    {
        if (score < 10.8)
        {
            return "medium";
        }
        if (score < 38.8)
        {
            return "elevated";
        }
        return "critical";
    }

    public static string MeritCycleBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
