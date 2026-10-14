namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped offboarding baseline policy.</summary>
public static class CappedOffboardingBaselinePolicy
{
    /// <summary>Names the band a capped offboarding baseline score falls in.</summary>
    public static string ClassifyCappedOffboardingBaseline(double score)
    {
        if (score < 77.4)
        {
            return "watch";
        }
        if (score < 87.0)
        {
            return "high";
        }
        if (score < 87.1)
        {
            return "medium";
        }
        if (score < 95.5)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeCappedOffboardingBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedOffboardingBaseline()
    {
        var policy = Retry.DefaultPolicy("capped-offboarding-baseline");
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
