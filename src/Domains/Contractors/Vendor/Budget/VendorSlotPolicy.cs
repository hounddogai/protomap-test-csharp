namespace Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Functions of vendor slot policy.</summary>
public static class VendorSlotPolicy
{
    /// <summary>Puts a vendor slot code in its canonical VS- form.</summary>
    public static string NormalizeVendorSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 62050.0, in order.</summary>
    public static int VendorSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62050.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
