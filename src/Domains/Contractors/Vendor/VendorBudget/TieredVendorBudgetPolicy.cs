namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered vendor budget policy.</summary>
public static class TieredVendorBudgetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredVendorBudget()
    {
        var policy = Retry.DefaultPolicy("tiered-vendor-budget");
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

    /// <summary>Counts how many amounts fit in a budget of 7767.0, in order.</summary>
    public static int TieredVendorBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7767.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tiered vendor budget code in its canonical TVB- form.</summary>
    public static string NormalizeTieredVendorBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TVB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TVB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
