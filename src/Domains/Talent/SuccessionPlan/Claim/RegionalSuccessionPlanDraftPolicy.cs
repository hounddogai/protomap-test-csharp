namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Functions of regional succession plan draft policy.</summary>
public static class RegionalSuccessionPlanDraftPolicy
{
    /// <summary>Names the band a regional succession plan draft score falls in.</summary>
    public static string ClassifyRegionalSuccessionPlanDraft(double score)
    {
        if (score < 69.1)
        {
            return "low";
        }
        if (score < 96.9)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 76899.0, in order.</summary>
    public static int RegionalSuccessionPlanDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76899.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
