namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary vendor profile policy.</summary>
public static class PrimaryVendorProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 62121.0, in order.</summary>
    public static int PrimaryVendorProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62121.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a primary vendor profile code in its canonical PVP- form.</summary>
    public static string NormalizePrimaryVendorProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PVP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PVP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PrimaryVendorProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a primary vendor profile score falls in.</summary>
    public static string ClassifyPrimaryVendorProfile(double score)
    {
        if (score < 35.9)
        {
            return "watch";
        }
        if (score < 78.0)
        {
            return "high";
        }
        return "critical";
    }
}
