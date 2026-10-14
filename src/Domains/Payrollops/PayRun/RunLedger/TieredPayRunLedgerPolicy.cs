namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered pay run ledger policy.</summary>
public static class TieredPayRunLedgerPolicy
{
    /// <summary>Puts a tiered pay run ledger code in its canonical TPRL- form.</summary>
    public static string NormalizeTieredPayRunLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TPRL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TPRL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeTieredPayRunLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredPayRunLedger()
    {
        var policy = Retry.DefaultPolicy("tiered-pay-run-ledger");
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

    public static string TieredPayRunLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
