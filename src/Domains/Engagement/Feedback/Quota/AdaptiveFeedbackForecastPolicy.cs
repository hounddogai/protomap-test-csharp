namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive feedback forecast policy.</summary>
public static class AdaptiveFeedbackForecastPolicy
{
    public static List<string> MergeAdaptiveFeedbackForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveFeedbackForecast()
    {
        var policy = Retry.DefaultPolicy("adaptive-feedback-forecast");
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

    /// <summary>Puts a adaptive feedback forecast code in its canonical AFF- form.</summary>
    public static string NormalizeAdaptiveFeedbackForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AFF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AFF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
