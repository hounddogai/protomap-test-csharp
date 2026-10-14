namespace Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Functions of position plan policy.</summary>
public static class PositionPlanPolicy
{
    public static int PositionPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a position plan score falls in.</summary>
    public static string ClassifyPositionPlan(double score)
    {
        if (score < 15.5)
        {
            return "critical";
        }
        if (score < 41.7)
        {
            return "high";
        }
        if (score < 57.3)
        {
            return "elevated";
        }
        if (score < 89.9)
        {
            return "medium";
        }
        return "minimal";
    }
}
