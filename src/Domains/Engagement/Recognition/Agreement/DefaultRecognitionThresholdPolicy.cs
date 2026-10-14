namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default recognition threshold policy.</summary>
public static class DefaultRecognitionThresholdPolicy
{
    public static List<string> MergeDefaultRecognitionThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultRecognitionThreshold()
    {
        var policy = Retry.DefaultPolicy("default-recognition-threshold");
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

    public static double DefaultRecognitionThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a default recognition threshold code in its canonical DRT- form.</summary>
    public static string NormalizeDefaultRecognitionThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DRT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DRT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
