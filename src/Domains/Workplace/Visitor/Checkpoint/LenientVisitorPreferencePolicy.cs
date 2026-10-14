namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient visitor preference policy.</summary>
public static class LenientVisitorPreferencePolicy
{
    public static string LenientVisitorPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a lenient visitor preference score falls in.</summary>
    public static string ClassifyLenientVisitorPreference(double score)
    {
        if (score < 33.3)
        {
            return "watch";
        }
        if (score < 51.8)
        {
            return "low";
        }
        if (score < 75.0)
        {
            return "high";
        }
        if (score < 90.7)
        {
            return "medium";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientVisitorPreference()
    {
        var policy = Retry.DefaultPolicy("lenient-visitor-preference");
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

    /// <summary>Counts how many amounts fit in a budget of 44268.0, in order.</summary>
    public static int LenientVisitorPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44268.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
