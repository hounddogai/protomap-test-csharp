namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk summary policy.</summary>
public static class DeskSummaryPolicy
{
    public static List<string> MergeDeskSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a desk summary score falls in.</summary>
    public static string ClassifyDeskSummary(double score)
    {
        if (score < 75.2)
        {
            return "elevated";
        }
        if (score < 86.8)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 2666.0, in order.</summary>
    public static int DeskSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2666.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double DeskSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
