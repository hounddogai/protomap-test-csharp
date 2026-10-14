namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global laptop draft policy.</summary>
public static class GlobalLaptopDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72048.0, in order.</summary>
    public static int GlobalLaptopDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72048.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeGlobalLaptopDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a global laptop draft code in its canonical GLD- form.</summary>
    public static string NormalizeGlobalLaptopDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GLD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GLD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
