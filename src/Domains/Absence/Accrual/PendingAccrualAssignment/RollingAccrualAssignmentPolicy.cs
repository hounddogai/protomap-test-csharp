namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling accrual assignment policy.</summary>
public static class RollingAccrualAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 77474.0, in order.</summary>
    public static int RollingAccrualAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77474.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rolling accrual assignment score falls in.</summary>
    public static string ClassifyRollingAccrualAssignment(double score)
    {
        if (score < 22.3)
        {
            return "critical";
        }
        if (score < 38.4)
        {
            return "minimal";
        }
        if (score < 89.2)
        {
            return "elevated";
        }
        return "low";
    }

    public static List<string> MergeRollingAccrualAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RollingAccrualAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
