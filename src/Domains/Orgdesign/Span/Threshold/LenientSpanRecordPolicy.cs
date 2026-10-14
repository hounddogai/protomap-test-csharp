namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient span record policy.</summary>
public static class LenientSpanRecordPolicy
{
    public static int LenientSpanRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientSpanRecord()
    {
        var policy = Retry.DefaultPolicy("lenient-span-record");
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

    public static double LenientSpanRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.45;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a lenient span record score falls in.</summary>
    public static string ClassifyLenientSpanRecord(double score)
    {
        if (score < 45.2)
        {
            return "elevated";
        }
        if (score < 99.9)
        {
            return "watch";
        }
        return "high";
    }
}
