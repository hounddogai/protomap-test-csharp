namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary offboarding slot policy.</summary>
public static class PrimaryOffboardingSlotPolicy
{
    public static List<string> MergePrimaryOffboardingSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 86051.0, in order.</summary>
    public static int PrimaryOffboardingSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86051.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a primary offboarding slot code in its canonical POS- form.</summary>
    public static string NormalizePrimaryOffboardingSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("POS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"POS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
