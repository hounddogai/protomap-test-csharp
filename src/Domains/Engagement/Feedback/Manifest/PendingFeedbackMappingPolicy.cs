namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending feedback mapping policy.</summary>
public static class PendingFeedbackMappingPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingFeedbackMapping()
    {
        var policy = Retry.DefaultPolicy("pending-feedback-mapping");
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

    public static int PendingFeedbackMappingSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergePendingFeedbackMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a pending feedback mapping code in its canonical PFM- form.</summary>
    public static string NormalizePendingFeedbackMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PFM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PFM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
