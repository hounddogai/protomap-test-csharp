namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan ledger policy.</summary>
public static class FloorPlanLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanLedger()
    {
        var policy = Retry.DefaultPolicy("floor-plan-ledger");
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

    /// <summary>Puts a floor plan ledger code in its canonical FPL- form.</summary>
    public static string NormalizeFloorPlanLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FloorPlanLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
