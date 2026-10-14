namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path claim policy.</summary>
public static class CareerPathClaimPolicy
{
    public static List<string> MergeCareerPathClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a career path claim score falls in.</summary>
    public static string ClassifyCareerPathClaim(double score)
    {
        if (score < 5.4)
        {
            return "medium";
        }
        if (score < 48.4)
        {
            return "low";
        }
        if (score < 56.5)
        {
            return "elevated";
        }
        if (score < 77.0)
        {
            return "critical";
        }
        return "high";
    }

    public static double CareerPathClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathClaim()
    {
        var policy = Retry.DefaultPolicy("career-path-claim");
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
