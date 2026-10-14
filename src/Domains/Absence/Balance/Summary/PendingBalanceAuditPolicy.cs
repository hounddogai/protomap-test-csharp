namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending balance audit policy.</summary>
public static class PendingBalanceAuditPolicy
{
    /// <summary>Names the band a pending balance audit score falls in.</summary>
    public static string ClassifyPendingBalanceAudit(double score)
    {
        if (score < 14.6)
        {
            return "high";
        }
        if (score < 61.7)
        {
            return "low";
        }
        return "elevated";
    }

    public static string PendingBalanceAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double PendingBalanceAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
