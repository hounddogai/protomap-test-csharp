namespace Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>Functions of vendor preference policy.</summary>
public static class VendorPreferencePolicy
{
    /// <summary>Names the band a vendor preference score falls in.</summary>
    public static string ClassifyVendorPreference(double score)
    {
        if (score < 16.1)
        {
            return "high";
        }
        if (score < 36.2)
        {
            return "minimal";
        }
        if (score < 85.0)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Puts a vendor preference code in its canonical VP- form.</summary>
    public static string NormalizeVendorPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 68813.0, in order.</summary>
    public static int VendorPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68813.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
