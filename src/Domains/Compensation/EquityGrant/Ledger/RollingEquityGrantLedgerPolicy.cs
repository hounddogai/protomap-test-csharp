namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling equity grant ledger policy.</summary>
public static class RollingEquityGrantLedgerPolicy
{
    public static List<string> MergeRollingEquityGrantLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RollingEquityGrantLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a rolling equity grant ledger code in its canonical REGL- form.</summary>
    public static string NormalizeRollingEquityGrantLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("REGL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"REGL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingEquityGrantLedger()
    {
        var policy = Retry.DefaultPolicy("rolling-equity-grant-ledger");
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
}
