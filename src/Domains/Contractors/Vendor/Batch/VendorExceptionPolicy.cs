namespace Acme.Workforce.Contractors.Vendor.Batch;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of vendor exception policy.</summary>
public static class VendorExceptionPolicy
{
    /// <summary>Puts a vendor exception code in its canonical VE- form.</summary>
    public static string NormalizeVendorExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVendorException()
    {
        var policy = Retry.DefaultPolicy("vendor-exception");
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

    /// <summary>Counts how many amounts fit in a budget of 34728.0, in order.</summary>
    public static int VendorExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 34728.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
