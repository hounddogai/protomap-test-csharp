namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal succession plan preference policy.</summary>
public static class SeasonalSuccessionPlanPreferencePolicy
{
    public static string SeasonalSuccessionPlanPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalSuccessionPlanPreference()
    {
        var policy = Retry.DefaultPolicy("seasonal-succession-plan-preference");
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
}
