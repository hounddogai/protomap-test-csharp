namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive headcount record policy.</summary>
public static class AdaptiveHeadcountRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 60005.0, in order.</summary>
    public static int AdaptiveHeadcountRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60005.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdaptiveHeadcountRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
