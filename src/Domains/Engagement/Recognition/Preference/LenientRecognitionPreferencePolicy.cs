namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient recognition preference policy.</summary>
public static class LenientRecognitionPreferencePolicy
{
    /// <summary>Puts a lenient recognition preference code in its canonical LRP- form.</summary>
    public static string NormalizeLenientRecognitionPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientRecognitionPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientRecognitionPreference()
    {
        var policy = Retry.DefaultPolicy("lenient-recognition-preference");
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
}
