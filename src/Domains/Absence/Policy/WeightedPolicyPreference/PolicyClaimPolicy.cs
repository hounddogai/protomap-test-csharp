namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy claim policy.</summary>
public static class PolicyClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99353.0, in order.</summary>
    public static int PolicyClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99353.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PolicyClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePolicyClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a policy claim code in its canonical PC- form.</summary>
    public static string NormalizePolicyClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
