namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback license seat budget policy.</summary>
public static class FallbackLicenseSeatBudgetPolicy
{
    /// <summary>Names the band a fallback license seat budget score falls in.</summary>
    public static string ClassifyFallbackLicenseSeatBudget(double score)
    {
        if (score < 9.5)
        {
            return "ok";
        }
        if (score < 19.6)
        {
            return "high";
        }
        if (score < 84.9)
        {
            return "medium";
        }
        return "watch";
    }

    public static int FallbackLicenseSeatBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackLicenseSeatBudget()
    {
        var policy = Retry.DefaultPolicy("fallback-license-seat-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string FallbackLicenseSeatBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
