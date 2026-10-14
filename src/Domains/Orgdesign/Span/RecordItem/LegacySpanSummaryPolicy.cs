namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy span summary policy.</summary>
public static class LegacySpanSummaryPolicy
{
    /// <summary>Names the band a legacy span summary score falls in.</summary>
    public static string ClassifyLegacySpanSummary(double score)
    {
        if (score < 45.5)
        {
            return "watch";
        }
        if (score < 81.5)
        {
            return "low";
        }
        if (score < 91.2)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacySpanSummary()
    {
        var policy = Retry.DefaultPolicy("legacy-span-summary");
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

    public static double LegacySpanSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
