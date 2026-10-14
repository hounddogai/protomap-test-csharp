namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback garnishment exception policy.</summary>
public static class FallbackGarnishmentExceptionPolicy
{
    public static List<string> MergeFallbackGarnishmentExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a fallback garnishment exception score falls in.</summary>
    public static string ClassifyFallbackGarnishmentException(double score)
    {
        if (score < 22.3)
        {
            return "critical";
        }
        if (score < 79.9)
        {
            return "watch";
        }
        if (score < 90.6)
        {
            return "medium";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackGarnishmentException()
    {
        var policy = Retry.DefaultPolicy("fallback-garnishment-exception");
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
