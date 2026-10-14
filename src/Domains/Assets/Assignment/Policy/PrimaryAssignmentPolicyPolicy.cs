namespace Acme.Workforce.Assets.Assignment.Policy;

/// <summary>Functions of primary assignment policy policy.</summary>
public static class PrimaryAssignmentPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 90725.0, in order.</summary>
    public static int PrimaryAssignmentPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90725.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a primary assignment policy score falls in.</summary>
    public static string ClassifyPrimaryAssignmentPolicy(double score)
    {
        if (score < 46.7)
        {
            return "ok";
        }
        if (score < 55.5)
        {
            return "low";
        }
        if (score < 65.6)
        {
            return "medium";
        }
        return "high";
    }
}
