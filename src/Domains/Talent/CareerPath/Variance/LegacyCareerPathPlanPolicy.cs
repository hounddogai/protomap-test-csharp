namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy career path plan policy.</summary>
public static class LegacyCareerPathPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyCareerPathPlan()
    {
        var policy = Retry.DefaultPolicy("legacy-career-path-plan");
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

    /// <summary>Names the band a legacy career path plan score falls in.</summary>
    public static string ClassifyLegacyCareerPathPlan(double score)
    {
        if (score < 31.9)
        {
            return "high";
        }
        if (score < 39.9)
        {
            return "watch";
        }
        if (score < 44.1)
        {
            return "medium";
        }
        return "ok";
    }

    public static List<string> MergeLegacyCareerPathPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a legacy career path plan code in its canonical LCPP- form.</summary>
    public static string NormalizeLegacyCareerPathPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LCPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LCPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
