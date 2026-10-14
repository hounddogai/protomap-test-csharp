namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling pay run exception policy.</summary>
public static class RollingPayRunExceptionPolicy
{
    public static double RollingPayRunExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingPayRunException()
    {
        var policy = Retry.DefaultPolicy("rolling-pay-run-exception");
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

    /// <summary>Puts a rolling pay run exception code in its canonical RPRE- form.</summary>
    public static string NormalizeRollingPayRunExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPRE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPRE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRollingPayRunExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
