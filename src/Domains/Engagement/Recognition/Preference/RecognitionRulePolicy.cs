namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of recognition rule policy.</summary>
public static class RecognitionRulePolicy
{
    /// <summary>Puts a recognition rule code in its canonical RR- form.</summary>
    public static string NormalizeRecognitionRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionRule()
    {
        var policy = Retry.DefaultPolicy("recognition-rule");
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

    /// <summary>Names the band a recognition rule score falls in.</summary>
    public static string ClassifyRecognitionRule(double score)
    {
        if (score < 28.4)
        {
            return "low";
        }
        if (score < 55.5)
        {
            return "watch";
        }
        if (score < 73.4)
        {
            return "elevated";
        }
        if (score < 97.2)
        {
            return "critical";
        }
        return "ok";
    }
}
