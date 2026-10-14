namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback bonus pool request policy.</summary>
public static class FallbackBonusPoolRequestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 71454.0, in order.</summary>
    public static int FallbackBonusPoolRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71454.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeFallbackBonusPoolRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
