namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk entry policy.</summary>
public static class DeskEntryPolicy
{
    public static List<string> MergeDeskEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a desk entry score falls in.</summary>
    public static string ClassifyDeskEntry(double score)
    {
        if (score < 9.2)
        {
            return "medium";
        }
        if (score < 22.1)
        {
            return "watch";
        }
        if (score < 94.2)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Puts a desk entry code in its canonical DE- form.</summary>
    public static string NormalizeDeskEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 15279.0, in order.</summary>
    public static int DeskEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15279.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
