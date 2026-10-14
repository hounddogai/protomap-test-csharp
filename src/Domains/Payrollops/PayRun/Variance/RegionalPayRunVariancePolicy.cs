namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional pay run variance policy.</summary>
public static class RegionalPayRunVariancePolicy
{
    public static List<string> MergeRegionalPayRunVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string RegionalPayRunVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalPayRunVariance()
    {
        var policy = Retry.DefaultPolicy("regional-pay-run-variance");
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

    /// <summary>Names the band a regional pay run variance score falls in.</summary>
    public static string ClassifyRegionalPayRunVariance(double score)
    {
        if (score < 26.0)
        {
            return "ok";
        }
        if (score < 83.5)
        {
            return "minimal";
        }
        return "critical";
    }
}
