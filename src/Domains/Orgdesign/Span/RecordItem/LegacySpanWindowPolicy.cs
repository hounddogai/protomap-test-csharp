namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy span window policy.</summary>
public static class LegacySpanWindowPolicy
{
    /// <summary>Puts a legacy span window code in its canonical LSW- form.</summary>
    public static string NormalizeLegacySpanWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy span window score falls in.</summary>
    public static string ClassifyLegacySpanWindow(double score)
    {
        if (score < 49.2)
        {
            return "medium";
        }
        if (score < 63.3)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacySpanWindow()
    {
        var policy = Retry.DefaultPolicy("legacy-span-window");
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

    public static double LegacySpanWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
