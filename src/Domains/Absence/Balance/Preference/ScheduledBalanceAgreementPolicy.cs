namespace Acme.Workforce.Absence.Balance.Preference;

/// <summary>Functions of scheduled balance agreement policy.</summary>
public static class ScheduledBalanceAgreementPolicy
{
    public static int ScheduledBalanceAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a scheduled balance agreement score falls in.</summary>
    public static string ClassifyScheduledBalanceAgreement(double score)
    {
        if (score < 44.8)
        {
            return "high";
        }
        if (score < 54.7)
        {
            return "low";
        }
        if (score < 67.9)
        {
            return "medium";
        }
        return "watch";
    }
}
