namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped pay run record policy.</summary>
public static class CappedPayRunRecordPolicy
{
    public static List<string> MergeCappedPayRunRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedPayRunRecord()
    {
        var policy = Retry.DefaultPolicy("capped-pay-run-record");
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

    /// <summary>Names the band a capped pay run record score falls in.</summary>
    public static string ClassifyCappedPayRunRecord(double score)
    {
        if (score < 52.6)
        {
            return "ok";
        }
        if (score < 55.8)
        {
            return "low";
        }
        return "critical";
    }
}
