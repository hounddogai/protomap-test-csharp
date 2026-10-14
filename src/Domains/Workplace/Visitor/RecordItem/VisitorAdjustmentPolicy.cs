namespace Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Functions of visitor adjustment policy.</summary>
public static class VisitorAdjustmentPolicy
{
    /// <summary>Names the band a visitor adjustment score falls in.</summary>
    public static string ClassifyVisitorAdjustment(double score)
    {
        if (score < 57.7)
        {
            return "critical";
        }
        if (score < 61.1)
        {
            return "medium";
        }
        if (score < 84.0)
        {
            return "low";
        }
        if (score < 98.0)
        {
            return "high";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 37686.0, in order.</summary>
    public static int VisitorAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37686.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
