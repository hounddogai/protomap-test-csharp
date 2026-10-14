namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty notice policy.</summary>
public static class WarrantyNoticePolicy
{
    /// <summary>Puts a warranty notice code in its canonical WN- form.</summary>
    public static string NormalizeWarrantyNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 69118.0, in order.</summary>
    public static int WarrantyNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69118.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string WarrantyNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
