namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of recognition adjustment policy.</summary>
public static class RecognitionAdjustmentPolicy
{
    public static List<string> MergeRecognitionAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a recognition adjustment code in its canonical RA- form.</summary>
    public static string NormalizeRecognitionAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionAdjustment()
    {
        var policy = Retry.DefaultPolicy("recognition-adjustment");
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
