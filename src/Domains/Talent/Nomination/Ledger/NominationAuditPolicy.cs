namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination audit policy.</summary>
public static class NominationAuditPolicy
{
    public static double NominationAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a nomination audit score falls in.</summary>
    public static string ClassifyNominationAudit(double score)
    {
        if (score < 14.0)
        {
            return "ok";
        }
        if (score < 46.8)
        {
            return "watch";
        }
        if (score < 71.5)
        {
            return "medium";
        }
        if (score < 93.1)
        {
            return "low";
        }
        return "high";
    }
}
