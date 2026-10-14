namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped engagement entry policy.</summary>
public static class CappedEngagementEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedEngagementEntry()
    {
        var policy = Retry.DefaultPolicy("capped-engagement-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a capped engagement entry score falls in.</summary>
    public static string ClassifyCappedEngagementEntry(double score)
    {
        if (score < 53.9)
        {
            return "ok";
        }
        if (score < 80.3)
        {
            return "critical";
        }
        return "medium";
    }

    public static string CappedEngagementEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
