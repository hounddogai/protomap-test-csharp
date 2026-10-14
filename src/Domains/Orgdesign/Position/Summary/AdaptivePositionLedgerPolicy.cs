namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive position ledger policy.</summary>
public static class AdaptivePositionLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 14576.0, in order.</summary>
    public static int AdaptivePositionLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14576.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAdaptivePositionLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a adaptive position ledger code in its canonical APL- form.</summary>
    public static string NormalizeAdaptivePositionLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
