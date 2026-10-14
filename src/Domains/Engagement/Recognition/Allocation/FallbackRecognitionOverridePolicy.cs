namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback recognition override policy.</summary>
public static class FallbackRecognitionOverridePolicy
{
    public static List<string> MergeFallbackRecognitionOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a fallback recognition override score falls in.</summary>
    public static string ClassifyFallbackRecognitionOverride(double score)
    {
        if (score < 35.1)
        {
            return "minimal";
        }
        if (score < 91.0)
        {
            return "elevated";
        }
        if (score < 94.9)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackRecognitionOverride()
    {
        var policy = Retry.DefaultPolicy("fallback-recognition-override");
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

    public static double FallbackRecognitionOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
