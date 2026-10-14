namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive mobility proposal policy.</summary>
public static class AdaptiveMobilityProposalPolicy
{
    /// <summary>Puts a adaptive mobility proposal code in its canonical AMP- form.</summary>
    public static string NormalizeAdaptiveMobilityProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AMP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AMP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AdaptiveMobilityProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeAdaptiveMobilityProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 74409.0, in order.</summary>
    public static int AdaptiveMobilityProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74409.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
