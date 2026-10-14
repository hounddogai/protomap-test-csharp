namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of warranty ledger policy.</summary>
public static class WarrantyLedgerPolicy
{
    /// <summary>Puts a warranty ledger code in its canonical WL- form.</summary>
    public static string NormalizeWarrantyLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double WarrantyLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWarrantyLedger()
    {
        var policy = Retry.DefaultPolicy("warranty-ledger");
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

    public static string WarrantyLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
