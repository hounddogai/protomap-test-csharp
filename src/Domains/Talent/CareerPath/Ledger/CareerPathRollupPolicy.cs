namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path rollup policy.</summary>
public static class CareerPathRollupPolicy
{
    /// <summary>Names the band a career path rollup score falls in.</summary>
    public static string ClassifyCareerPathRollup(double score)
    {
        if (score < 53.1)
        {
            return "critical";
        }
        if (score < 68.9)
        {
            return "medium";
        }
        if (score < 70.9)
        {
            return "minimal";
        }
        return "low";
    }

    public static string CareerPathRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathRollup()
    {
        var policy = Retry.DefaultPolicy("career-path-rollup");
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

    public static List<string> MergeCareerPathRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
