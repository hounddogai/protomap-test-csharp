namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient refresh profile policy.</summary>
public static class LenientRefreshProfilePolicy
{
    public static List<string> MergeLenientRefreshProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a lenient refresh profile score falls in.</summary>
    public static string ClassifyLenientRefreshProfile(double score)
    {
        if (score < 11.4)
        {
            return "medium";
        }
        if (score < 28.6)
        {
            return "watch";
        }
        if (score < 34.4)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Puts a lenient refresh profile code in its canonical LRP- form.</summary>
    public static string NormalizeLenientRefreshProfileCode(string code)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientRefreshProfile()
    {
        var policy = Retry.DefaultPolicy("lenient-refresh-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
