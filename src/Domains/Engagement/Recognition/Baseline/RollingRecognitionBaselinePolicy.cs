namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling recognition baseline policy.</summary>
public static class RollingRecognitionBaselinePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingRecognitionBaseline()
    {
        var policy = Retry.DefaultPolicy("rolling-recognition-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a rolling recognition baseline score falls in.</summary>
    public static string ClassifyRollingRecognitionBaseline(double score)
    {
        if (score < 20.2)
        {
            return "watch";
        }
        if (score < 78.6)
        {
            return "critical";
        }
        return "low";
    }

    public static string RollingRecognitionBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int RollingRecognitionBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}
