namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary headcount ledger policy.</summary>
public static class PrimaryHeadcountLedgerPolicy
{
    public static double PrimaryHeadcountLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary headcount ledger score falls in.</summary>
    public static string ClassifyPrimaryHeadcountLedger(double score)
    {
        if (score < 6.1)
        {
            return "medium";
        }
        if (score < 6.7)
        {
            return "critical";
        }
        if (score < 86.0)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 43612.0, in order.</summary>
    public static int PrimaryHeadcountLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43612.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergePrimaryHeadcountLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
