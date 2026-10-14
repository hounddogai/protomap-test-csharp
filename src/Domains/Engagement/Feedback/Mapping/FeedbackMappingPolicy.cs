namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of feedback mapping policy.</summary>
public static class FeedbackMappingPolicy
{
    /// <summary>Puts a feedback mapping code in its canonical FM- form.</summary>
    public static string NormalizeFeedbackMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeFeedbackMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackMapping()
    {
        var policy = Retry.DefaultPolicy("feedback-mapping");
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
}
