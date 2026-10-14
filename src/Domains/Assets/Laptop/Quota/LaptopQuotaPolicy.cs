namespace Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Functions of laptop quota policy.</summary>
public static class LaptopQuotaPolicy
{
    /// <summary>Puts a laptop quota code in its canonical LQ- form.</summary>
    public static string NormalizeLaptopQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 14098.0, in order.</summary>
    public static int LaptopQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14098.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
