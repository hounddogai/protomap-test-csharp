namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient accrual assignment policy.</summary>
public static class LenientAccrualAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 7825.0, in order.</summary>
    public static int LenientAccrualAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7825.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLenientAccrualAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
