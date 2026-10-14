namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span cycle policy.</summary>
public static class SpanCyclePolicy
{
    public static List<string> MergeSpanCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSpanCycle()
    {
        var policy = Retry.DefaultPolicy("span-cycle");
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

    public static int SpanCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a span cycle score falls in.</summary>
    public static string ClassifySpanCycle(double score)
    {
        if (score < 16.7)
        {
            return "high";
        }
        if (score < 27.2)
        {
            return "watch";
        }
        if (score < 94.8)
        {
            return "low";
        }
        return "elevated";
    }
}
