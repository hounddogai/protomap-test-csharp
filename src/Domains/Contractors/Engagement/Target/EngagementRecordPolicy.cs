namespace Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Functions of engagement record policy.</summary>
public static class EngagementRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26896.0, in order.</summary>
    public static int EngagementRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26896.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a engagement record score falls in.</summary>
    public static string ClassifyEngagementRecord(double score)
    {
        if (score < 66.6)
        {
            return "ok";
        }
        if (score < 80.7)
        {
            return "elevated";
        }
        if (score < 83.5)
        {
            return "minimal";
        }
        return "watch";
    }
}
