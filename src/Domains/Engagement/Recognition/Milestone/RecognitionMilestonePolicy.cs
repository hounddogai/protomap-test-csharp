namespace Acme.Workforce.Engagement.Recognition.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of recognition milestone policy.</summary>
public static class RecognitionMilestonePolicy
{
    public static List<string> MergeRecognitionMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RecognitionMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a recognition milestone code in its canonical RM- form.</summary>
    public static string NormalizeRecognitionMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RecognitionMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
