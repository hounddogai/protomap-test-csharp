namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending deduction exception policy.</summary>
public static class PendingDeductionExceptionPolicy
{
    /// <summary>Names the band a pending deduction exception score falls in.</summary>
    public static string ClassifyPendingDeductionException(double score)
    {
        if (score < 16.3)
        {
            return "low";
        }
        if (score < 27.1)
        {
            return "ok";
        }
        if (score < 37.7)
        {
            return "critical";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingDeductionException()
    {
        var policy = Retry.DefaultPolicy("pending-deduction-exception");
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

    public static string PendingDeductionExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergePendingDeductionExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
