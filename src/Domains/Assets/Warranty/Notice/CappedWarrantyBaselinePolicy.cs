namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped warranty baseline policy.</summary>
public static class CappedWarrantyBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 40890.0, in order.</summary>
    public static int CappedWarrantyBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 40890.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a capped warranty baseline code in its canonical CWB- form.</summary>
    public static string NormalizeCappedWarrantyBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CWB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CWB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CappedWarrantyBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
