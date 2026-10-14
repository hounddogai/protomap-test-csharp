namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop ledger policy.</summary>
public static class LaptopLedgerPolicy
{
    public static List<string> MergeLaptopLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LaptopLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a laptop ledger code in its canonical LL- form.</summary>
    public static string NormalizeLaptopLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
