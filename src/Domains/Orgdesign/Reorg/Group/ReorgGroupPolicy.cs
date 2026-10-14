namespace Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Functions of reorg group policy.</summary>
public static class ReorgGroupPolicy
{
    /// <summary>Names the band a reorg group score falls in.</summary>
    public static string ClassifyReorgGroup(double score)
    {
        if (score < 10.9)
        {
            return "medium";
        }
        if (score < 21.2)
        {
            return "critical";
        }
        if (score < 44.7)
        {
            return "elevated";
        }
        if (score < 66.1)
        {
            return "minimal";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 45046.0, in order.</summary>
    public static int ReorgGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45046.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
