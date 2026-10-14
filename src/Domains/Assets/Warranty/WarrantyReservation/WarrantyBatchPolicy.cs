namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty batch policy.</summary>
public static class WarrantyBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 4042.0, in order.</summary>
    public static int WarrantyBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4042.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double WarrantyBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.45;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a warranty batch code in its canonical WB- form.</summary>
    public static string NormalizeWarrantyBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
