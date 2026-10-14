namespace Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Functions of capped engagement agreement policy.</summary>
public static class CappedEngagementAgreementPolicy
{
    /// <summary>Names the band a capped engagement agreement score falls in.</summary>
    public static string ClassifyCappedEngagementAgreement(double score)
    {
        if (score < 4.1)
        {
            return "high";
        }
        if (score < 8.4)
        {
            return "watch";
        }
        if (score < 54.4)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 15208.0, in order.</summary>
    public static int CappedEngagementAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15208.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
