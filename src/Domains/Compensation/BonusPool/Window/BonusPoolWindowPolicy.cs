namespace Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Functions of bonus pool window policy.</summary>
public static class BonusPoolWindowPolicy
{
    /// <summary>Names the band a bonus pool window score falls in.</summary>
    public static string ClassifyBonusPoolWindow(double score)
    {
        if (score < 22.3)
        {
            return "medium";
        }
        if (score < 38.0)
        {
            return "ok";
        }
        if (score < 58.3)
        {
            return "minimal";
        }
        if (score < 64.0)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 99652.0, in order.</summary>
    public static int BonusPoolWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99652.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
