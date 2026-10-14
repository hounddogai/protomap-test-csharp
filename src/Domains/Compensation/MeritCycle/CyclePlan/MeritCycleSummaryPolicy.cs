namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Functions of merit cycle summary policy.</summary>
public static class MeritCycleSummaryPolicy
{
    public static int MeritCycleSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 96582.0, in order.</summary>
    public static int MeritCycleSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96582.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a merit cycle summary score falls in.</summary>
    public static string ClassifyMeritCycleSummary(double score)
    {
        if (score < 25.8)
        {
            return "low";
        }
        if (score < 50.1)
        {
            return "high";
        }
        if (score < 84.1)
        {
            return "ok";
        }
        return "minimal";
    }
}
