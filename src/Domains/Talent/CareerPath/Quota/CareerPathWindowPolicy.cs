namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path window policy.</summary>
public static class CareerPathWindowPolicy
{
    /// <summary>Names the band a career path window score falls in.</summary>
    public static string ClassifyCareerPathWindow(double score)
    {
        if (score < 14.0)
        {
            return "high";
        }
        if (score < 16.3)
        {
            return "ok";
        }
        if (score < 49.7)
        {
            return "critical";
        }
        if (score < 69.8)
        {
            return "low";
        }
        return "watch";
    }

    /// <summary>Puts a career path window code in its canonical CPW- form.</summary>
    public static string NormalizeCareerPathWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCareerPathWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathWindow()
    {
        var policy = Retry.DefaultPolicy("career-path-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
