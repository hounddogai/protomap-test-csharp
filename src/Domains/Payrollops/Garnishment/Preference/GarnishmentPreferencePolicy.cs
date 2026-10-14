namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of garnishment preference policy.</summary>
public static class GarnishmentPreferencePolicy
{
    public static int GarnishmentPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string GarnishmentPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGarnishmentPreference()
    {
        var policy = Retry.DefaultPolicy("garnishment-preference");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a garnishment preference score falls in.</summary>
    public static string ClassifyGarnishmentPreference(double score)
    {
        if (score < 40.8)
        {
            return "critical";
        }
        if (score < 64.8)
        {
            return "low";
        }
        if (score < 81.1)
        {
            return "minimal";
        }
        if (score < 90.5)
        {
            return "elevated";
        }
        return "high";
    }
}
