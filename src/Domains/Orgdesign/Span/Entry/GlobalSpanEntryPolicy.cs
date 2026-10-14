namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global span entry policy.</summary>
public static class GlobalSpanEntryPolicy
{
    /// <summary>Names the band a global span entry score falls in.</summary>
    public static string ClassifyGlobalSpanEntry(double score)
    {
        if (score < 8.1)
        {
            return "high";
        }
        if (score < 72.8)
        {
            return "critical";
        }
        if (score < 73.7)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 95852.0, in order.</summary>
    public static int GlobalSpanEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95852.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalSpanEntry()
    {
        var policy = Retry.DefaultPolicy("global-span-entry");
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

    public static string GlobalSpanEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
