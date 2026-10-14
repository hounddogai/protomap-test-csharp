namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending exception quota policy.</summary>
public static class PendingExceptionQuotaPolicy
{
    public static string PendingExceptionQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a pending exception quota score falls in.</summary>
    public static string ClassifyPendingExceptionQuota(double score)
    {
        if (score < 9.8)
        {
            return "high";
        }
        if (score < 99.6)
        {
            return "critical";
        }
        return "low";
    }
}
