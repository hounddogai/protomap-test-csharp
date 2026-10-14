namespace Acme.Workforce.Talent.Pool.Group;

/// <summary>Functions of pool group policy.</summary>
public static class PoolGroupPolicy
{
    /// <summary>Names the band a pool group score falls in.</summary>
    public static string ClassifyPoolGroup(double score)
    {
        if (score < 12.9)
        {
            return "watch";
        }
        if (score < 78.7)
        {
            return "low";
        }
        if (score < 95.2)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 36761.0, in order.</summary>
    public static int PoolGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36761.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
