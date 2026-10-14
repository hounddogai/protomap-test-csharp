namespace Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Functions of strict pay band audit policy.</summary>
public static class StrictPayBandAuditPolicy
{
    /// <summary>Names the band a strict pay band audit score falls in.</summary>
    public static string ClassifyStrictPayBandAudit(double score)
    {
        if (score < 14.6)
        {
            return "high";
        }
        if (score < 41.2)
        {
            return "minimal";
        }
        if (score < 57.4)
        {
            return "elevated";
        }
        if (score < 78.4)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 38362.0, in order.</summary>
    public static int StrictPayBandAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38362.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
