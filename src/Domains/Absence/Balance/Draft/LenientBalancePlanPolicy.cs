namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient balance plan policy.</summary>
public static class LenientBalancePlanPolicy
{
    public static string LenientBalancePlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeLenientBalancePlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 91476.0, in order.</summary>
    public static int LenientBalancePlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91476.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LenientBalancePlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
