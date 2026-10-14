namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped pay run ledger policy.</summary>
public static class CappedPayRunLedgerPolicy
{
    public static List<string> MergeCappedPayRunLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a capped pay run ledger score falls in.</summary>
    public static string ClassifyCappedPayRunLedger(double score)
    {
        if (score < 33.6)
        {
            return "elevated";
        }
        if (score < 67.6)
        {
            return "low";
        }
        if (score < 81.0)
        {
            return "medium";
        }
        if (score < 82.5)
        {
            return "ok";
        }
        return "minimal";
    }

    public static string CappedPayRunLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedPayRunLedger()
    {
        var policy = Retry.DefaultPolicy("capped-pay-run-ledger");
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
