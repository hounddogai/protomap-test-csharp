namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted career path override policy.</summary>
public static class WeightedCareerPathOverridePolicy
{
    public static double WeightedCareerPathOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a weighted career path override code in its canonical WCPO- form.</summary>
    public static string NormalizeWeightedCareerPathOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WCPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WCPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedCareerPathOverride()
    {
        var policy = Retry.DefaultPolicy("weighted-career-path-override");
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
