namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback position record policy.</summary>
public static class FallbackPositionRecordPolicy
{
    /// <summary>Names the band a fallback position record score falls in.</summary>
    public static string ClassifyFallbackPositionRecord(double score)
    {
        if (score < 13.1)
        {
            return "critical";
        }
        if (score < 72.9)
        {
            return "ok";
        }
        if (score < 85.3)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static string FallbackPositionRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackPositionRecord()
    {
        var policy = Retry.DefaultPolicy("fallback-position-record");
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

    /// <summary>Counts how many amounts fit in a budget of 46884.0, in order.</summary>
    public static int FallbackPositionRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46884.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
