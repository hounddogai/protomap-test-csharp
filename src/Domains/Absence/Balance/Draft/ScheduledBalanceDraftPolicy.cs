namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled balance draft policy.</summary>
public static class ScheduledBalanceDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 92804.0, in order.</summary>
    public static int ScheduledBalanceDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92804.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeScheduledBalanceDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a scheduled balance draft code in its canonical SBD- form.</summary>
    public static string NormalizeScheduledBalanceDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
