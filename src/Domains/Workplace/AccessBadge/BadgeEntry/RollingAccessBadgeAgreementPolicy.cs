namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling access badge agreement policy.</summary>
public static class RollingAccessBadgeAgreementPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 38042.0, in order.</summary>
    public static int RollingAccessBadgeAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38042.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rolling access badge agreement score falls in.</summary>
    public static string ClassifyRollingAccessBadgeAgreement(double score)
    {
        if (score < 9.7)
        {
            return "medium";
        }
        if (score < 16.9)
        {
            return "minimal";
        }
        if (score < 24.7)
        {
            return "elevated";
        }
        if (score < 50.6)
        {
            return "high";
        }
        return "ok";
    }

    public static double RollingAccessBadgeAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.72;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RollingAccessBadgeAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
