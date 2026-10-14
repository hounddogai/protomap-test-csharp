namespace Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Functions of scheduled mobility request policy.</summary>
public static class ScheduledMobilityRequestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 98565.0, in order.</summary>
    public static int ScheduledMobilityRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98565.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a scheduled mobility request score falls in.</summary>
    public static string ClassifyScheduledMobilityRequest(double score)
    {
        if (score < 46.8)
        {
            return "critical";
        }
        if (score < 50.6)
        {
            return "medium";
        }
        if (score < 56.1)
        {
            return "watch";
        }
        if (score < 56.6)
        {
            return "ok";
        }
        return "minimal";
    }
}
