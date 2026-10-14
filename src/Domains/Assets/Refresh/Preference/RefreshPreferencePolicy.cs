namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of refresh preference policy.</summary>
public static class RefreshPreferencePolicy
{
    public static string RefreshPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 49508.0, in order.</summary>
    public static int RefreshPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49508.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a refresh preference score falls in.</summary>
    public static string ClassifyRefreshPreference(double score)
    {
        if (score < 38.8)
        {
            return "medium";
        }
        if (score < 58.6)
        {
            return "watch";
        }
        if (score < 69.8)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRefreshPreference()
    {
        var policy = Retry.DefaultPolicy("refresh-preference");
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
}
