namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Functions of global succession plan quota policy.</summary>
public static class GlobalSuccessionPlanQuotaPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 70447.0, in order.</summary>
    public static int GlobalSuccessionPlanQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70447.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a global succession plan quota score falls in.</summary>
    public static string ClassifyGlobalSuccessionPlanQuota(double score)
    {
        if (score < 70.6)
        {
            return "medium";
        }
        if (score < 79.1)
        {
            return "low";
        }
        if (score < 84.6)
        {
            return "minimal";
        }
        return "ok";
    }
}
