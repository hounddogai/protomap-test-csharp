namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled recognition threshold policy.</summary>
public static class ScheduledRecognitionThresholdPolicy
{
    public static int ScheduledRecognitionThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledRecognitionThreshold()
    {
        var policy = Retry.DefaultPolicy("scheduled-recognition-threshold");
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

    /// <summary>Names the band a scheduled recognition threshold score falls in.</summary>
    public static string ClassifyScheduledRecognitionThreshold(double score)
    {
        if (score < 75.4)
        {
            return "high";
        }
        if (score < 78.1)
        {
            return "critical";
        }
        if (score < 96.2)
        {
            return "low";
        }
        return "minimal";
    }

    public static List<string> MergeScheduledRecognitionThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
