namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle ledger policy.</summary>
public static class MeritCycleLedgerPolicy
{
    public static List<string> MergeMeritCycleLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double MeritCycleLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.03;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a merit cycle ledger score falls in.</summary>
    public static string ClassifyMeritCycleLedger(double score)
    {
        if (score < 4.1)
        {
            return "medium";
        }
        if (score < 23.5)
        {
            return "ok";
        }
        if (score < 58.0)
        {
            return "minimal";
        }
        return "low";
    }
}
