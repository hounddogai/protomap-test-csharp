namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional exception preference policy.</summary>
public static class RegionalExceptionPreferencePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalExceptionPreference()
    {
        var policy = Retry.DefaultPolicy("regional-exception-preference");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 53283.0, in order.</summary>
    public static int RegionalExceptionPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53283.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a regional exception preference score falls in.</summary>
    public static string ClassifyRegionalExceptionPreference(double score)
    {
        if (score < 15.3)
        {
            return "ok";
        }
        if (score < 58.8)
        {
            return "high";
        }
        return "watch";
    }

    public static int RegionalExceptionPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
