namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy ledger policy.</summary>
public static class OccupancyLedgerPolicy
{
    public static double OccupancyLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a occupancy ledger code in its canonical OL- form.</summary>
    public static string NormalizeOccupancyLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyLedger()
    {
        var policy = Retry.DefaultPolicy("occupancy-ledger");
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
