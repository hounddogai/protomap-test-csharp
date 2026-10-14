namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal nomination cycle policy.</summary>
public static class SeasonalNominationCyclePolicy
{
    public static string SeasonalNominationCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalNominationCycle()
    {
        var policy = Retry.DefaultPolicy("seasonal-nomination-cycle");
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

    /// <summary>Names the band a seasonal nomination cycle score falls in.</summary>
    public static string ClassifySeasonalNominationCycle(double score)
    {
        if (score < 29.2)
        {
            return "ok";
        }
        if (score < 43.3)
        {
            return "low";
        }
        return "elevated";
    }
}
