namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of accrual override policy.</summary>
public static class AccrualOverridePolicy
{
    public static List<string> MergeAccrualOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a accrual override score falls in.</summary>
    public static string ClassifyAccrualOverride(double score)
    {
        if (score < 74.7)
        {
            return "high";
        }
        if (score < 81.5)
        {
            return "low";
        }
        if (score < 99.9)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 47361.0, in order.</summary>
    public static int AccrualOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47361.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
